import { useState, useEffect } from "react";
import {
  Settings,
  ChevronRight,
  Pencil,
  Trash2,
  Plus,
  Check,
  X,
} from "lucide-react";
import danhMucApi from "../../api/danhMucApi";
import PageHeader from "../../components/common/PageHeader";
import Button from "../../components/common/Button";
import Modal from "../../components/common/Modal";
import { useForm } from "react-hook-form";
import useDanhMucStore from "../../stores/danhMucStore";

// ── Generic CRUD table cho danh mục đơn giản ────────────────────────────────
function DanhMucTable({
  title,
  items,
  loading,
  onAdd,
  onEdit,
  onDelete,
  columns,
  canDelete = true,
}) {
  return (
    <div className="bg-surface rounded-card border border-ink-100 overflow-hidden">
      <div className="px-5 py-3.5 border-b border-ink-100 flex items-center justify-between">
        <h3 className="font-semibold text-ink-900">{title}</h3>
        <Button size="sm" icon={Plus} onClick={onAdd}>
          Thêm
        </Button>
      </div>
      {loading ? (
        <div className="py-8 text-center text-ink-400 text-sm">Đang tải...</div>
      ) : items.length === 0 ? (
        <div className="py-8 text-center text-ink-400 text-sm">
          Chưa có dữ liệu
        </div>
      ) : (
        <table className="w-full text-sm">
          <thead>
            <tr className="bg-surface-alt">
              {columns.map((c) => (
                <th
                  key={c.key}
                  className="px-5 py-3 text-left text-xs font-medium text-ink-400 uppercase tracking-wide"
                >
                  {c.label}
                </th>
              ))}
              <th className="w-20"></th>
            </tr>
          </thead>
          <tbody className="divide-y divide-ink-100">
            {items.map((item) => (
              <tr
                key={item.id}
                className="hover:bg-surface-alt transition-colors"
              >
                {columns.map((c) => (
                  <td key={c.key} className="px-5 py-3 text-ink-700">
                    {c.render ? c.render(item) : (item[c.key] ?? "—")}
                  </td>
                ))}
                <td className="px-3 py-3 text-right">
                  <div className="flex items-center justify-end gap-1">
                    <button
                      onClick={() => onEdit(item)}
                      className="p-1.5 rounded-lg text-ink-400 hover:text-accent-600 hover:bg-accent-50 transition-colors"
                    >
                      <Pencil size={14} />
                    </button>
                    {canDelete && (
                      <button
                        onClick={() => onDelete(item.id)}
                        className="p-1.5 rounded-lg text-ink-400 hover:text-danger-600 hover:bg-danger-50 transition-colors"
                      >
                        <Trash2 size={14} />
                      </button>
                    )}
                  </div>
                </td>
              </tr>
            ))}
          </tbody>
        </table>
      )}
    </div>
  );
}

// ── Modal chung cho danh mục đơn giản ────────────────────────────────────────
function SimpleModal({
  title,
  fields,
  defaultValues,
  onClose,
  onSubmit: onSubmitProp,
}) {
  const {
    register,
    handleSubmit,
    formState: { errors, isSubmitting },
  } = useForm({ defaultValues });
  const [apiError, setApiError] = useState("");

  const onSubmit = async (data) => {
    setApiError("");
    try {
      await onSubmitProp(data);
      onClose();
    } catch (err) {
      setApiError(err?.message || "Thao tác thất bại");
    }
  };

  return (
    <Modal isOpen title={title} onClose={onClose} size="sm">
      <form onSubmit={handleSubmit(onSubmit)} className="space-y-4">
        {apiError && (
          <div className="text-sm text-danger-600 bg-danger-50 rounded-lg p-3">
            {apiError}
          </div>
        )}
        {fields.map((f) => (
          <div key={f.name}>
            <label className="block text-sm font-medium text-ink-700 mb-1">
              {f.label}{" "}
              {f.required && <span className="text-danger-500">*</span>}
            </label>
            {f.type === "checkbox" ? (
              <label className="flex items-center gap-2 cursor-pointer">
                <input
                  type="checkbox"
                  {...register(f.name)}
                  className="rounded"
                />
                <span className="text-sm text-ink-600">Đang hoạt động</span>
              </label>
            ) : f.type === "select" ? (
              <select
                {...register(f.name, f.rules)}
                className="w-full border border-ink-200 rounded-lg px-3 py-2 text-sm focus:outline-none focus:ring-2 focus:ring-accent-400/40 focus:border-accent-400"
              >
                {f.options.map((o) => (
                  <option key={String(o.value)} value={o.value}>
                    {o.label}
                  </option>
                ))}
              </select>
            ) : f.type === "textarea" ? (
              <textarea
                {...register(f.name, f.rules)}
                rows={3}
                className="w-full border border-ink-200 rounded-lg px-3 py-2 text-sm focus:outline-none focus:ring-2 focus:ring-accent-400/40 focus:border-accent-400"
                placeholder={f.placeholder}
              />
            ) : (
              <input
                type={f.type || "text"}
                {...register(f.name, f.rules)}
                className="w-full border border-ink-200 rounded-lg px-3 py-2 text-sm focus:outline-none focus:ring-2 focus:ring-accent-400/40 focus:border-accent-400"
                placeholder={f.placeholder}
                step={f.step}
                min={f.min}
                max={f.max}
              />
            )}
            {errors[f.name] && (
              <p className="text-xs text-danger-600 mt-1">
                {errors[f.name].message}
              </p>
            )}
          </div>
        ))}
        <div className="flex justify-end gap-2 pt-2">
          <Button variant="secondary" onClick={onClose} type="button">
            Huỷ
          </Button>
          <Button type="submit" disabled={isSubmitting}>
            {isSubmitting ? "Đang lưu..." : "Lưu"}
          </Button>
        </div>
      </form>
    </Modal>
  );
}

// ── XepHang Section ───────────────────────────────────────────────────────────
function XepHangSection() {
  const [items, setItems] = useState([]);
  const [loading, setLoading] = useState(true);
  const [editing, setEditing] = useState(null);
  const [error, setError] = useState("");

  const load = async () => {
    try {
      const res = await danhMucApi.getXepHang();
      setItems(res.data ?? []);
    } catch {
      setError("Không tải được hạng khách hàng");
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => {
    load();
  }, []);

  const handleUpdate = async (data) => {
    await danhMucApi.updateXepHang(editing.id, {
      diemToiThieu: Number(data.diemToiThieu),
      soLanThuToiThieu: Number(data.soLanThuToiThieu),
      phanTramGiamVoucher: Number(data.phanTramGiamVoucher),
      moTaQuyenLoi: data.moTaQuyenLoi,
      isActive: data.isActive,
    });
    setEditing(null);
    load();
  };

  return (
    <div className="bg-surface rounded-card border border-ink-100 overflow-hidden">
      {editing && (
        <SimpleModal
          title={`Cập nhật hạng: ${editing.tenHang}`}
          defaultValues={{
            diemToiThieu: editing.diemToiThieu,
            soLanThuToiThieu: editing.soLanThuToiThieu,
            phanTramGiamVoucher: editing.phanTramGiamVoucher,
            moTaQuyenLoi: editing.moTaQuyenLoi,
            isActive: editing.isActive,
          }}
          fields={[
            {
              name: "diemToiThieu",
              label: "Mốc điểm tối thiểu (12 tháng)",
              type: "number",
              min: "0",
              rules: {
                required: "Bắt buộc",
                min: { value: 0, message: "≥ 0" },
              },
            },
            {
              name: "soLanThuToiThieu",
              label: "Số lần thu tối thiểu (12 tháng)",
              type: "number",
              min: "0",
              rules: { required: "Bắt buộc" },
            },
            {
              name: "phanTramGiamVoucher",
              label: "% giảm voucher khi thăng hạng",
              type: "number",
              min: "0",
              max: "100",
              step: "0.5",
              rules: {
                required: "Bắt buộc",
                min: { value: 0, message: "≥ 0" },
                max: { value: 100, message: "≤ 100" },
              },
            },
            {
              name: "moTaQuyenLoi",
              label: "Mô tả quyền lợi (chèn vào email)",
              type: "textarea",
            },
            { name: "isActive", label: "Trạng thái", type: "checkbox" },
          ]}
          onClose={() => setEditing(null)}
          onSubmit={handleUpdate}
        />
      )}

      <div className="px-5 py-3.5 border-b border-ink-100">
        <h3 className="font-semibold text-ink-900">
          Hạng khách hàng & Tích điểm
        </h3>
        <p className="text-xs text-ink-400 mt-0.5">
          Điểm tính theo rolling 12 tháng gần nhất. 100.000 VNĐ = 1 điểm. Chỉ
          chỉnh tiêu chí, không tạo/xóa hạng.
        </p>
      </div>

      {error && <div className="text-sm text-danger-600 p-4">{error}</div>}
      {loading ? (
        <div className="py-8 text-center text-ink-400 text-sm">Đang tải...</div>
      ) : (
        <table className="w-full text-sm">
          <thead>
            <tr className="bg-surface-alt">
              {[
                "Thứ tự",
                "Hạng",
                "Mốc điểm",
                "Số lần thu",
                "% Voucher",
                "Trạng thái",
                "",
              ].map((h) => (
                <th
                  key={h}
                  className="px-5 py-3 text-left text-xs font-medium text-ink-400 uppercase tracking-wide"
                >
                  {h}
                </th>
              ))}
            </tr>
          </thead>
          <tbody className="divide-y divide-ink-100">
            {items.map((item) => (
              <tr
                key={item.id}
                className="hover:bg-surface-alt transition-colors"
              >
                <td className="px-5 py-3 text-ink-400">{item.thuTu}</td>
                <td className="px-5 py-3">
                  <span className="font-semibold text-ink-900">
                    {item.tenHang}
                  </span>
                  <span className="ml-1.5 text-xs text-ink-400 font-mono">
                    ({item.maHang})
                  </span>
                </td>
                <td className="px-5 py-3 text-ink-700">
                  {item.diemToiThieu.toLocaleString()} điểm
                </td>
                <td className="px-5 py-3 text-ink-700">
                  {item.soLanThuToiThieu} lần
                </td>
                <td className="px-5 py-3">
                  {item.phanTramGiamVoucher > 0 ? (
                    <span className="font-medium text-success-600">
                      {item.phanTramGiamVoucher}%
                    </span>
                  ) : (
                    <span className="text-ink-400">—</span>
                  )}
                </td>
                <td className="px-5 py-3">
                  {item.isActive ? (
                    <span className="flex items-center gap-1 text-success-600 text-xs">
                      <Check size={13} />
                      Hoạt động
                    </span>
                  ) : (
                    <span className="flex items-center gap-1 text-ink-400 text-xs">
                      <X size={13} />
                      Tắt
                    </span>
                  )}
                </td>
                <td className="px-3 py-3 text-right">
                  <button
                    onClick={() => setEditing(item)}
                    className="p-1.5 rounded-lg text-ink-400 hover:text-accent-600 hover:bg-accent-50 transition-colors"
                  >
                    <Pencil size={14} />
                  </button>
                </td>
              </tr>
            ))}
          </tbody>
        </table>
      )}
    </div>
  );
}

// ── NgayLe Section ────────────────────────────────────────────────────────────
function NgayLeSection() {
  const [items, setItems] = useState([]);
  const [loading, setLoading] = useState(true);
  const [modal, setModal] = useState(null); // null | { mode: 'create' | 'edit', item? }
  const { xepHang, load: loadDanhMuc } = useDanhMucStore();

  const load = async () => {
    const res = await danhMucApi.getNgayLe();
    setItems(res.data ?? []);
    setLoading(false);
  };

  useEffect(() => {
    load();
    loadDanhMuc();
  }, []);

  const fields = [
    {
      name: "tenNgayLe",
      label: "Tên ngày lễ",
      required: true,
      rules: { required: "Bắt buộc" },
    },
    {
      name: "thang",
      label: "Tháng (1-12)",
      type: "number",
      min: "1",
      max: "12",
      required: true,
      rules: { required: "Bắt buộc" },
    },
    {
      name: "ngay",
      label: "Ngày (1-31)",
      type: "number",
      min: "1",
      max: "31",
      required: true,
      rules: { required: "Bắt buộc" },
    },
    {
      name: "soNgayGuiTruoc",
      label: "Gửi email trước (ngày)",
      type: "number",
      min: "1",
      max: "30",
      required: true,
      rules: { required: "Bắt buộc" },
    },
    {
      name: "apDungChoLoaiKH",
      label: "Áp dụng cho",
      type: "select",
      options: [
        { value: "TatCa", label: "Tất cả khách hàng" },
        { value: "B2C", label: "Chỉ cá nhân (B2C)" },
        { value: "B2B", label: "Chỉ doanh nghiệp (B2B)" },
      ],
    },
    {
      name: "hangToiThieuApDung",
      label: "Hạng tối thiểu áp dụng",
      type: "select",
      options: [
        { value: "", label: "-- Áp dụng mọi hạng --" },
        ...xepHang.map((h) => ({ value: h.id, label: h.tenHang })),
      ],
    },
    { name: "isActive", label: "Trạng thái", type: "checkbox" },
  ];

  const LOAI_KH_LABEL = {
    B2C: "Cá nhân (B2C)",
    B2B: "Doanh nghiệp (B2B)",
    TatCa: "Tất cả",
  };

  return (
    <div className="bg-surface rounded-card border border-ink-100 overflow-hidden">
      {modal && (
        <SimpleModal
          title={
            modal.mode === "create"
              ? "Thêm ngày lễ"
              : `Sửa: ${modal.item?.tenNgayLe}`
          }
          defaultValues={
            modal.item
              ? {
                  ...modal.item,
                  hangToiThieuApDung: modal.item.hangToiThieuApDung ?? "",
                }
              : {
                  soNgayGuiTruoc: 5,
                  isActive: true,
                  apDungChoLoaiKH: "TatCa",
                  hangToiThieuApDung: "",
                }
          }
          fields={fields}
          onClose={() => setModal(null)}
          onSubmit={async (data) => {
            const payload = {
              ...data,
              thang: Number(data.thang),
              ngay: Number(data.ngay),
              soNgayGuiTruoc: Number(data.soNgayGuiTruoc),
              apDungChoLoaiKH: data.apDungChoLoaiKH,
              hangToiThieuApDung:
                data.hangToiThieuApDung === ""
                  ? null
                  : Number(data.hangToiThieuApDung),
            };
            if (modal.mode === "create") await danhMucApi.createNgayLe(payload);
            else await danhMucApi.updateNgayLe(modal.item.id, payload);
            load();
          }}
        />
      )}
      <div className="px-5 py-3.5 border-b border-ink-100 flex items-center justify-between">
        <h3 className="font-semibold text-ink-900">Ngày lễ / Sự kiện ưu đãi</h3>
        <Button
          size="sm"
          icon={Plus}
          onClick={() => setModal({ mode: "create" })}
        >
          Thêm
        </Button>
      </div>
      {loading ? (
        <div className="py-8 text-center text-ink-400 text-sm">Đang tải...</div>
      ) : (
        <table className="w-full text-sm">
          <thead>
            <tr className="bg-surface-alt">
              {[
                "Tên ngày lễ",
                "Ngày",
                "Gửi trước",
                "Áp dụng cho",
                "Trạng thái",
                "",
              ].map((h) => (
                <th
                  key={h}
                  className="px-5 py-3 text-left text-xs font-medium text-ink-400 uppercase tracking-wide"
                >
                  {h}
                </th>
              ))}
            </tr>
          </thead>
          <tbody className="divide-y divide-ink-100">
            {items.map((item) => (
              <tr
                key={item.id}
                className="hover:bg-surface-alt transition-colors"
              >
                <td className="px-5 py-3 font-medium text-ink-900">
                  {item.tenNgayLe}
                </td>
                <td className="px-5 py-3 text-ink-700">
                  {String(item.ngay).padStart(2, "0")}/
                  {String(item.thang).padStart(2, "0")}
                </td>
                <td className="px-5 py-3 text-ink-700">
                  {item.soNgayGuiTruoc} ngày
                </td>
                <td className="px-5 py-3 text-ink-600">
                  {LOAI_KH_LABEL[item.apDungChoLoaiKH] ?? item.apDungChoLoaiKH}
                </td>
                <td className="px-5 py-3">
                  {item.isActive ? (
                    <span className="text-success-600 text-xs flex items-center gap-1">
                      <Check size={13} />
                      Hoạt động
                    </span>
                  ) : (
                    <span className="text-ink-400 text-xs flex items-center gap-1">
                      <X size={13} />
                      Tắt
                    </span>
                  )}
                </td>
                <td className="px-3 py-3 text-right">
                  <div className="flex items-center justify-end gap-1">
                    <button
                      onClick={() => setModal({ mode: "edit", item })}
                      className="p-1.5 rounded-lg text-ink-400 hover:text-accent-600 hover:bg-accent-50 transition-colors"
                    >
                      <Pencil size={14} />
                    </button>
                    <button
                      onClick={async () => {
                        if (confirm("Xóa ngày lễ này?")) {
                          await danhMucApi.deleteNgayLe(item.id);
                          load();
                        }
                      }}
                      className="p-1.5 rounded-lg text-ink-400 hover:text-danger-600 hover:bg-danger-50 transition-colors"
                    >
                      <Trash2 size={14} />
                    </button>
                  </div>
                </td>
              </tr>
            ))}
          </tbody>
        </table>
      )}
    </div>
  );
}

// ── SimpleDanhMucSection (dùng cho LoaiKhachHang, TinhTrang, LoaiTicket, LoaiSanPham) ───
function SimpleDanhMucSection({
  title,
  fetchFn,
  createFn,
  updateFn,
  deleteFn,
  columns,
  fields,
  checkActive = true,
}) {
  const [items, setItems] = useState([]);
  const [loading, setLoading] = useState(true);
  const [modal, setModal] = useState(null);

  const load = async () => {
    try {
      const res = await fetchFn();
      setItems(res.data ?? []);
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => {
    load();
  }, []);

  return (
    <div className="bg-surface rounded-card border border-ink-100 overflow-hidden">
      {modal && (
        <SimpleModal
          title={
            modal.mode === "create"
              ? `Thêm ${title}`
              : `Sửa: ${modal.item?.tenLoai || modal.item?.tenTinhTrang}`
          }
          defaultValues={modal.item ?? { isActive: true }}
          fields={fields}
          onClose={() => setModal(null)}
          onSubmit={async (data) => {
            if (modal.mode === "create") await createFn(data);
            else await updateFn(modal.item.id, data);
            load();
          }}
        />
      )}
      <div className="px-5 py-3.5 border-b border-ink-100 flex items-center justify-between">
        <h3 className="font-semibold text-ink-900">{title}</h3>
        <Button
          size="sm"
          icon={Plus}
          onClick={() => setModal({ mode: "create" })}
        >
          Thêm
        </Button>
      </div>
      {loading ? (
        <div className="py-8 text-center text-ink-400 text-sm">Đang tải...</div>
      ) : items.length === 0 ? (
        <div className="py-8 text-center text-ink-400 text-sm">
          Chưa có dữ liệu
        </div>
      ) : (
        <table className="w-full text-sm">
          <thead>
            <tr className="bg-surface-alt">
              {columns.map((c) => (
                <th
                  key={c.key}
                  className="px-5 py-3 text-left text-xs font-medium text-ink-400 uppercase tracking-wide"
                >
                  {c.label}
                </th>
              ))}
              <th className="w-20"></th>
            </tr>
          </thead>
          <tbody className="divide-y divide-ink-100">
            {items.map((item) => (
              <tr
                key={item.id}
                className="hover:bg-surface-alt transition-colors"
              >
                {columns.map((c) => (
                  <td key={c.key} className="px-5 py-3 text-ink-700">
                    {c.render ? c.render(item) : (item[c.key] ?? "—")}
                  </td>
                ))}
                <td className="px-3 py-3 text-right">
                  <div className="flex items-center justify-end gap-1">
                    <button
                      onClick={() => setModal({ mode: "edit", item })}
                      className="p-1.5 rounded-lg text-ink-400 hover:text-accent-600 hover:bg-accent-50 transition-colors"
                    >
                      <Pencil size={14} />
                    </button>
                    <button
                      onClick={async () => {
                        if (
                          confirm(`Xóa "${item.tenLoai || item.tenTinhTrang}"?`)
                        ) {
                          try {
                            await deleteFn(item.id);
                            load();
                          } catch (err) {
                            alert(err?.message || "Không thể xóa");
                          }
                        }
                      }}
                      className="p-1.5 rounded-lg text-ink-400 hover:text-danger-600 hover:bg-danger-50 transition-colors"
                    >
                      <Trash2 size={14} />
                    </button>
                  </div>
                </td>
              </tr>
            ))}
          </tbody>
        </table>
      )}
    </div>
  );
}

// ── TABS ──────────────────────────────────────────────────────────────────────
const TABS = [
  { key: "khachhang", label: "Khách hàng" },
  { key: "ticket", label: "Ticket" },
  { key: "sanpham", label: "Sản phẩm" },
  { key: "xephang", label: "Xếp hạng & Điểm" },
  { key: "ngayle", label: "Ngày lễ / Ưu đãi" },
];

export default function SettingsPage() {
  const [activeTab, setActiveTab] = useState("khachhang");

  const activeClass = "bg-accent-500/15 text-accent-700 font-medium";
  const inactiveClass = "text-ink-500 hover:bg-ink-100 hover:text-ink-900";

  const commonActiveField = {
    name: "isActive",
    label: "Trạng thái",
    type: "checkbox",
  };

  return (
    <div className="space-y-5">
      <PageHeader
        breadcrumb="CRM / Admin"
        title="Cài đặt hệ thống"
        icon={Settings}
      />

      <div className="bg-info-50 border border-info-100 rounded-lg p-3 text-sm text-info-700">
        Chỉ Admin mới có thể chỉnh sửa danh mục. Các thay đổi sẽ ảnh hưởng trực
        tiếp đến toàn bộ dữ liệu hệ thống.
      </div>

      {/* Tabs */}
      <div className="flex gap-1 bg-surface border border-ink-100 rounded-xl p-1.5">
        {TABS.map((t) => (
          <button
            key={t.key}
            onClick={() => setActiveTab(t.key)}
            className={`flex-1 px-4 py-2 text-sm rounded-lg transition-colors ${activeTab === t.key ? activeClass : inactiveClass}`}
          >
            {t.label}
          </button>
        ))}
      </div>

      {activeTab === "khachhang" && (
        <div className="space-y-4">
          <SimpleDanhMucSection
            title="Loại khách hàng"
            fetchFn={danhMucApi.getLoaiKhachHang}
            createFn={danhMucApi.createLoaiKhachHang}
            updateFn={danhMucApi.updateLoaiKhachHang}
            deleteFn={danhMucApi.deleteLoaiKhachHang}
            columns={[
              { key: "tenLoai", label: "Tên loại" },
              { key: "moTa", label: "Mô tả" },
              {
                key: "isActive",
                label: "Trạng thái",
                render: (i) =>
                  i.isActive ? (
                    <span className="text-success-600 text-xs">Hoạt động</span>
                  ) : (
                    <span className="text-ink-400 text-xs">Tắt</span>
                  ),
              },
            ]}
            fields={[
              {
                name: "tenLoai",
                label: "Tên loại",
                required: true,
                rules: { required: "Bắt buộc" },
              },
              { name: "moTa", label: "Mô tả", type: "textarea" },
              commonActiveField,
            ]}
          />
          <SimpleDanhMucSection
            title="Tình trạng khách hàng"
            fetchFn={danhMucApi.getTinhTrang}
            createFn={danhMucApi.createTinhTrang}
            updateFn={danhMucApi.updateTinhTrang}
            deleteFn={danhMucApi.deleteTinhTrang}
            columns={[
              { key: "tenTinhTrang", label: "Tên tình trạng" },
              {
                key: "isActive",
                label: "Trạng thái",
                render: (i) =>
                  i.isActive ? (
                    <span className="text-success-600 text-xs">Hoạt động</span>
                  ) : (
                    <span className="text-ink-400 text-xs">Tắt</span>
                  ),
              },
            ]}
            fields={[
              {
                name: "tenTinhTrang",
                label: "Tên tình trạng",
                required: true,
                rules: { required: "Bắt buộc" },
              },
              commonActiveField,
            ]}
          />
        </div>
      )}

      {activeTab === "ticket" && (
        <SimpleDanhMucSection
          title="Loại ticket"
          fetchFn={danhMucApi.getLoaiTicket}
          createFn={danhMucApi.createLoaiTicket}
          updateFn={danhMucApi.updateLoaiTicket}
          deleteFn={danhMucApi.deleteLoaiTicket}
          columns={[
            { key: "tenLoai", label: "Tên loại" },
            { key: "moTa", label: "Mô tả" },
            {
              key: "isActive",
              label: "Trạng thái",
              render: (i) =>
                i.isActive ? (
                  <span className="text-success-600 text-xs">Hoạt động</span>
                ) : (
                  <span className="text-ink-400 text-xs">Tắt</span>
                ),
            },
          ]}
          fields={[
            {
              name: "tenLoai",
              label: "Tên loại ticket",
              required: true,
              rules: { required: "Bắt buộc" },
            },
            { name: "moTa", label: "Mô tả", type: "textarea" },
            commonActiveField,
          ]}
        />
      )}

      {activeTab === "sanpham" && (
        <SimpleDanhMucSection
          title="Loại sản phẩm / dịch vụ"
          fetchFn={danhMucApi.getLoaiSanPham}
          createFn={danhMucApi.createLoaiSanPham}
          updateFn={danhMucApi.updateLoaiSanPham}
          deleteFn={danhMucApi.deleteLoaiSanPham}
          columns={[
            { key: "tenLoai", label: "Tên loại" },
            { key: "moTa", label: "Mô tả" },
          ]}
          fields={[
            {
              name: "tenLoai",
              label: "Tên loại sản phẩm",
              required: true,
              rules: { required: "Bắt buộc" },
            },
            { name: "moTa", label: "Mô tả", type: "textarea" },
          ]}
        />
      )}

      {activeTab === "xephang" && <XepHangSection />}
      {activeTab === "ngayle" && <NgayLeSection />}
    </div>
  );
}
