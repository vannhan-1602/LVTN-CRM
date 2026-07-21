import { useEffect, useState } from "react";
import { useNavigate, useParams } from "react-router-dom";
import { Pencil, Trash2, MapPin, Plus, Star, X } from "lucide-react";
import customerApi from "../../api/customerApi";
import addressApi from "../../api/addressApi";
import useAuthStore from "../auth/authStore";
import PageHeader from "../../components/common/PageHeader";
import Card, { Field } from "../../components/common/Card";
import Badge from "../../components/common/Badge";
import Button from "../../components/common/Button";
import EmptyState from "../../components/common/EmptyState";
import CustomerFormModal from "./CustomerFormModal";
import ActivitySection from "../activities/ActivitySection";
import CustomerLoyaltySection from "./CustomerLoyaltySection";
import CustomerExpenseSection from "./CustomerExpenseSection";
import { ROLES } from "../../utils/constants";
import { formatDateTime, badgeToneForId } from "../../utils/formatters";

const LOAI_DIA_CHI_OPTIONS = [
  { value: "Office", label: "Văn phòng" },
  { value: "Billing", label: "Thanh toán" },
  { value: "Shipping", label: "Giao hàng" },
];

const emptyAddrForm = {
  loaiDiaChi: "Office",
  diaChiChiTiet: "",
  tinhThanh: "",
  quanHuyen: "",
  phuongXa: "",
  isDefault: false,
};

// ── Khối quản lý địa chỉ — gộp ngay trong trang chi tiết khách hàng ─────────
function AddressSection({ customerId, canEdit }) {
  const [addresses, setAddresses] = useState([]);
  const [loading, setLoading] = useState(true);
  const [showForm, setShowForm] = useState(false);
  const [form, setForm] = useState(emptyAddrForm);
  const [editingId, setEditingId] = useState(null);
  const [submitting, setSubmitting] = useState(false);
  const [error, setError] = useState("");

  const load = async () => {
    setLoading(true);
    try {
      const res = await addressApi.getByCustomer(customerId);
      setAddresses(res.data ?? []);
    } catch {
      setError("Không thể tải danh sách địa chỉ");
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => {
    load();
  }, [customerId]);

  const resetForm = () => {
    setForm(emptyAddrForm);
    setEditingId(null);
    setShowForm(false);
    setError("");
  };

  const handleEdit = (addr) => {
    setEditingId(addr.id);
    setForm({
      loaiDiaChi: addr.loaiDiaChi ?? "Office",
      diaChiChiTiet: addr.diaChiChiTiet ?? "",
      tinhThanh: addr.tinhThanh ?? "",
      quanHuyen: addr.quanHuyen ?? "",
      phuongXa: addr.phuongXa ?? "",
      isDefault: addr.isDefault ?? false,
    });
    setShowForm(true);
  };

  const handleSubmit = async (e) => {
    e.preventDefault();
    if (!form.diaChiChiTiet.trim()) {
      setError("Địa chỉ chi tiết không được để trống");
      return;
    }
    setSubmitting(true);
    setError("");
    try {
      if (editingId) await addressApi.update(editingId, form);
      else await addressApi.create(customerId, form);
      await load();
      resetForm();
    } catch (err) {
      setError(err?.message || "Không thể lưu địa chỉ");
    } finally {
      setSubmitting(false);
    }
  };

  const handleDelete = async (id) => {
    if (!window.confirm("Xóa địa chỉ này?")) return;
    try {
      await addressApi.delete(id);
      await load();
    } catch (err) {
      setError(err?.message || "Không thể xóa");
    }
  };

  return (
    <Card
      title={`Địa chỉ (${addresses.length})`}
      action={
        canEdit &&
        !showForm && (
          <Button
            size="sm"
            variant="secondary"
            icon={Plus}
            onClick={() => setShowForm(true)}
          >
            Thêm địa chỉ
          </Button>
        )
      }
    >
      {showForm && (
        <form
          onSubmit={handleSubmit}
          className="border border-ink-100 rounded-lg p-4 bg-surface-alt space-y-3 mb-4"
        >
          <div className="flex items-center justify-between">
            <h4 className="text-sm font-medium text-ink-900">
              {editingId ? "Sửa địa chỉ" : "Thêm địa chỉ mới"}
            </h4>
            <button
              type="button"
              onClick={resetForm}
              className="text-ink-400 hover:text-ink-700"
            >
              <X size={16} />
            </button>
          </div>

          <div className="grid grid-cols-2 gap-3">
            <div>
              <label className="block text-xs font-medium text-ink-500 mb-1">
                Loại địa chỉ
              </label>
              <select
                value={form.loaiDiaChi}
                onChange={(e) =>
                  setForm((f) => ({ ...f, loaiDiaChi: e.target.value }))
                }
                className="w-full border border-ink-200 rounded-lg px-3 py-2 text-sm"
              >
                {LOAI_DIA_CHI_OPTIONS.map((o) => (
                  <option key={o.value} value={o.value}>
                    {o.label}
                  </option>
                ))}
              </select>
            </div>
            <div className="flex items-end pb-1.5">
              <label className="flex items-center gap-2 text-sm cursor-pointer text-ink-700">
                <input
                  type="checkbox"
                  checked={form.isDefault}
                  onChange={(e) =>
                    setForm((f) => ({ ...f, isDefault: e.target.checked }))
                  }
                  className="w-4 h-4 rounded accent-accent-500"
                />
                Đặt làm mặc định
              </label>
            </div>
          </div>

          <div>
            <label className="block text-xs font-medium text-ink-500 mb-1">
              Địa chỉ chi tiết <span className="text-danger-500">*</span>
            </label>
            <input
              value={form.diaChiChiTiet}
              onChange={(e) =>
                setForm((f) => ({ ...f, diaChiChiTiet: e.target.value }))
              }
              placeholder="Số nhà, tên đường, tòa nhà..."
              className="w-full border border-ink-200 rounded-lg px-3 py-2 text-sm"
            />
          </div>

          <div className="grid grid-cols-3 gap-3">
            <div>
              <label className="block text-xs font-medium text-ink-500 mb-1">
                Phường/Xã
              </label>
              <input
                value={form.phuongXa}
                onChange={(e) =>
                  setForm((f) => ({ ...f, phuongXa: e.target.value }))
                }
                placeholder="Phường/Xã"
                className="w-full border border-ink-200 rounded-lg px-3 py-2 text-sm"
              />
            </div>
            <div>
              <label className="block text-xs font-medium text-ink-500 mb-1">
                Quận/Huyện
              </label>
              <input
                value={form.quanHuyen}
                onChange={(e) =>
                  setForm((f) => ({ ...f, quanHuyen: e.target.value }))
                }
                placeholder="Quận/Huyện"
                className="w-full border border-ink-200 rounded-lg px-3 py-2 text-sm"
              />
            </div>
            <div>
              <label className="block text-xs font-medium text-ink-500 mb-1">
                Tỉnh/Thành phố
              </label>
              <input
                value={form.tinhThanh}
                onChange={(e) =>
                  setForm((f) => ({ ...f, tinhThanh: e.target.value }))
                }
                placeholder="Tỉnh/Thành phố"
                className="w-full border border-ink-200 rounded-lg px-3 py-2 text-sm"
              />
            </div>
          </div>

          {error && (
            <div className="text-sm text-danger-600 bg-danger-50 rounded-lg p-2">
              {error}
            </div>
          )}

          <div className="flex gap-2">
            <Button type="submit" size="sm" disabled={submitting}>
              {submitting
                ? "Đang lưu..."
                : editingId
                  ? "Cập nhật"
                  : "Thêm địa chỉ"}
            </Button>
            <Button
              type="button"
              size="sm"
              variant="secondary"
              onClick={resetForm}
            >
              Hủy
            </Button>
          </div>
        </form>
      )}

      {loading ? (
        <p className="text-sm text-ink-400 text-center py-4">Đang tải...</p>
      ) : addresses.length === 0 ? (
        <EmptyState icon={MapPin} title="Chưa có địa chỉ nào" />
      ) : (
        <div className="space-y-2">
          {addresses.map((addr) => (
            <div
              key={addr.id}
              className={`border rounded-lg p-3.5 flex items-start justify-between gap-3 ${addr.isDefault ? "border-accent-300 bg-accent-50/40" : "border-ink-100"}`}
            >
              <div className="flex-1 min-w-0">
                <div className="flex items-center gap-2 flex-wrap mb-1">
                  <Badge
                    label={
                      LOAI_DIA_CHI_OPTIONS.find(
                        (o) => o.value === addr.loaiDiaChi,
                      )?.label ?? addr.loaiDiaChi
                    }
                    tone="info"
                  />
                  {addr.isDefault && (
                    <span className="inline-flex items-center gap-1 text-xs text-accent-700 font-medium">
                      <Star
                        size={12}
                        className="fill-accent-500 text-accent-500"
                      />{" "}
                      Mặc định
                    </span>
                  )}
                </div>
                <p className="text-sm text-ink-900">{addr.diaChiChiTiet}</p>
                {(addr.phuongXa || addr.quanHuyen || addr.tinhThanh) && (
                  <p className="text-xs text-ink-400 mt-0.5">
                    {[addr.phuongXa, addr.quanHuyen, addr.tinhThanh]
                      .filter(Boolean)
                      .join(", ")}
                  </p>
                )}
              </div>
              {canEdit && (
                <div className="flex gap-3 shrink-0">
                  <button
                    onClick={() => handleEdit(addr)}
                    className="text-xs font-medium text-info-600 hover:underline"
                  >
                    Sửa
                  </button>
                  <button
                    onClick={() => handleDelete(addr.id)}
                    className="text-xs font-medium text-danger-600 hover:underline"
                  >
                    Xóa
                  </button>
                </div>
              )}
            </div>
          ))}
        </div>
      )}
    </Card>
  );
}

export default function CustomerDetailPage() {
  const { id } = useParams();
  const navigate = useNavigate();
  const { user } = useAuthStore();
  const canEdit = [ROLES.Sale, ROLES.Manager].includes(user?.role);
  const canDelete = user?.role === ROLES.Manager;
  const canViewExpense = [ROLES.Manager, ROLES.Accountant].includes(user?.role);

  const [customer, setCustomer] = useState(null);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState("");
  const [showEditModal, setShowEditModal] = useState(false);

  const load = async () => {
    setLoading(true);
    setError("");
    try {
      const res = await customerApi.getById(id);
      setCustomer(res.data ?? null);
    } catch (err) {
      setError(err?.message || "Không thể tải thông tin khách hàng");
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => {
    load();
  }, [id]);

  const handleDelete = async () => {
    if (!window.confirm("Xóa khách hàng này? Hành động không thể hoàn tác."))
      return;
    try {
      await customerApi.delete(id);
      navigate("/customers");
    } catch (err) {
      setError(err?.message || "Không thể xóa khách hàng");
    }
  };

  if (loading)
    return (
      <div className="text-sm text-ink-400 py-10 text-center">Đang tải...</div>
    );

  if (error || !customer) {
    return (
      <div className="space-y-4">
        <PageHeader
          breadcrumb="CRM / Khách hàng"
          title="Khách hàng"
          onBack={() => navigate("/customers")}
        />
        <div className="text-sm text-danger-600 bg-danger-50 rounded-lg p-4">
          {error || "Không tìm thấy khách hàng."}
        </div>
      </div>
    );
  }

  return (
    <div className="space-y-5">
      {showEditModal && (
        <CustomerFormModal
          customer={customer}
          onClose={() => setShowEditModal(false)}
          onSaved={() => {
            setShowEditModal(false);
            load();
          }}
        />
      )}

      <PageHeader
        breadcrumb="Khách hàng"
        title={customer.tenKhachHang}
        onBack={() => navigate("/customers")}
        badge={
          customer.tinhTrangId && (
            <Badge
              label={
                customer.tenTinhTrang ?? `Tình trạng ${customer.tinhTrangId}`
              }
              tone={badgeToneForId(customer.tinhTrangId)}
            />
          )
        }
        actions={
          <>
            {canEdit && (
              <Button
                variant="secondary"
                icon={Pencil}
                onClick={() => setShowEditModal(true)}
              >
                Sửa
              </Button>
            )}
            {canDelete && (
              <Button variant="danger" icon={Trash2} onClick={handleDelete}>
                Xóa
              </Button>
            )}
          </>
        }
      />

      {error && (
        <div className="text-sm text-danger-600 bg-danger-50 rounded-lg p-3">
          {error}
        </div>
      )}

      <div className="grid grid-cols-1 lg:grid-cols-3 gap-4">
        <div className="lg:col-span-2 space-y-4">
          <Card title="Thông tin khách hàng">
            <div className="grid grid-cols-2 gap-5">
              <Field
                label="Mã khách hàng"
                value={
                  <span className="font-mono">{customer.maKhachHang}</span>
                }
              />
              <Field
                label="Loại khách hàng"
                value={
                  customer.loaiKhachHangId ? (
                    <Badge
                      label={
                        customer.tenLoaiKhachHang ??
                        `Loại ${customer.loaiKhachHangId}`
                      }
                      tone={badgeToneForId(customer.loaiKhachHangId)}
                    />
                  ) : (
                    "—"
                  )
                }
              />
              <Field label="Email" value={customer.email} />
              <Field label="Số điện thoại" value={customer.soDienThoai} />
              <Field label="Mã số thuế" value={customer.maSoThue} />
              <Field
                label="Nhân viên phụ trách"
                value={customer.tenNhanVienPhuTrach}
              />
              <Field
                label="Ngày tạo"
                value={formatDateTime(customer.createdAt)}
              />
              <Field
                label="Cập nhật gần nhất"
                value={formatDateTime(customer.updatedAt)}
              />
            </div>
          </Card>

          <AddressSection customerId={customer.id} canEdit={canEdit} />

          <ActivitySection khachHangId={customer.id} canEdit={canEdit} />
        </div>

        <div className="space-y-4">
          <CustomerLoyaltySection khachHangId={customer.id} />

          {canViewExpense && (
            <CustomerExpenseSection khachHangId={customer.id} />
          )}

          <Card title="Liên kết nhanh">
            <div className="space-y-2">
              <Button
                variant="secondary"
                size="sm"
                className="w-full"
                onClick={() =>
                  navigate(`/opportunities?khachHangId=${customer.id}`)
                }
              >
                Cơ hội bán hàng
              </Button>
              <Button
                variant="secondary"
                size="sm"
                className="w-full"
                onClick={() => navigate(`/quotes?khachHangId=${customer.id}`)}
              >
                Báo giá
              </Button>
              <Button
                variant="secondary"
                size="sm"
                className="w-full"
                onClick={() =>
                  navigate(`/contracts?khachHangId=${customer.id}`)
                }
              >
                Hợp đồng
              </Button>
              <Button
                variant="secondary"
                size="sm"
                className="w-full"
                onClick={() => navigate(`/tickets?khachHangId=${customer.id}`)}
              >
                Yêu cầu hỗ trợ
              </Button>
            </div>
          </Card>
        </div>
      </div>
    </div>
  );
}
