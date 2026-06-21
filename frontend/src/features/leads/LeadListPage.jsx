import { useEffect, useState, useMemo } from "react";
import leadApi from "../../api/leadApi";
import authApi from "../../api/authApi";
import useAuthStore from "../auth/authStore";
import Pagination from "../../components/common/Pagination";
import {
  ROLES,
  LEAD_TINH_TRANG_OPTIONS,
  LEAD_TINH_TRANG_COLOR,
} from "../../utils/constants";
import { formatDateTime } from "../../utils/formatters";

const emptyForm = {
  tenLead: "",
  tenCongTy: "",
  soDienThoai: "",
  email: "",
  tinhTrang: "",
  nhanVienPhuTrachId: "",
};

function Badge({ label, colorClass }) {
  return (
    <span
      className={`px-2 py-0.5 rounded-full text-xs font-semibold ${colorClass}`}
    >
      {label}
    </span>
  );
}

function ConvertModal({ lead, onClose, onConverted, nhanVienList }) {
  const [form, setForm] = useState({
    tenKhachHang: lead.tenLead,
    loaiKhachHangId: "",
    tinhTrangId: "",
    email: lead.email ?? "",
    soDienThoai: lead.soDienThoai ?? "",
    maSoThue: "",
    nhanVienPhuTrachId: lead.nhanVienPhuTrachId ?? "",
  });
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState("");

  const handleSubmit = async (e) => {
    e.preventDefault();
    setLoading(true);
    setError("");
    const toInt = (v) => (v === "" || v == null ? null : Number(v));
    try {
      await leadApi.convert(lead.id, {
        tenKhachHang: form.tenKhachHang.trim(),
        loaiKhachHangId: toInt(form.loaiKhachHangId),
        tinhTrangId: toInt(form.tinhTrangId),
        email: form.email || null,
        soDienThoai: form.soDienThoai || null,
        maSoThue: form.maSoThue || null,
        nhanVienPhuTrachId: toInt(form.nhanVienPhuTrachId),
      });
      onConverted();
    } catch (err) {
      setError(err?.message || "Chuyển đổi thất bại");
    } finally {
      setLoading(false);
    }
  };

  return (
    <div className="fixed inset-0 z-50 flex items-center justify-center bg-black/40">
      <form
        onSubmit={handleSubmit}
        className="bg-white rounded-xl shadow-xl p-6 w-full max-w-md space-y-4"
      >
        <h3 className="font-bold text-lg text-gray-800">
          Chuyển đổi Lead → Khách hàng
        </h3>
        <div>
          <label className="block text-sm font-medium mb-1">
            Tên KH <span className="text-red-500">*</span>
          </label>
          <input
            value={form.tenKhachHang}
            onChange={(e) =>
              setForm((f) => ({ ...f, tenKhachHang: e.target.value }))
            }
            className="w-full border rounded-lg px-3 py-2 text-sm"
            required
          />
        </div>
        <div className="grid grid-cols-2 gap-3">
          <div>
            <label className="block text-sm font-medium mb-1">Loại KH</label>
            <select
              value={form.loaiKhachHangId}
              onChange={(e) =>
                setForm((f) => ({ ...f, loaiKhachHangId: e.target.value }))
              }
              className="w-full border rounded-lg px-3 py-2 text-sm"
            >
              <option value="">-- Chọn --</option>
              <option value="1">VIP</option>
              <option value="2">B2B</option>
              <option value="3">B2C</option>
            </select>
          </div>
          <div>
            <label className="block text-sm font-medium mb-1">Tình trạng</label>
            <select
              value={form.tinhTrangId}
              onChange={(e) =>
                setForm((f) => ({ ...f, tinhTrangId: e.target.value }))
              }
              className="w-full border rounded-lg px-3 py-2 text-sm"
            >
              <option value="">-- Chọn --</option>
              <option value="1">Đang giao dịch</option>
              <option value="2">Tiềm năng</option>
              <option value="3">Ngừng giao dịch</option>
            </select>
          </div>
        </div>
        {nhanVienList.length > 0 && (
          <div>
            <label className="block text-sm font-medium mb-1">
              NV phụ trách
            </label>
            <select
              value={form.nhanVienPhuTrachId}
              onChange={(e) =>
                setForm((f) => ({ ...f, nhanVienPhuTrachId: e.target.value }))
              }
              className="w-full border rounded-lg px-3 py-2 text-sm"
            >
              <option value="">-- Chọn --</option>
              {nhanVienList.map((nv) => (
                <option key={nv.id} value={nv.id}>
                  {nv.hoTen ?? `NV #${nv.id}`}
                </option>
              ))}
            </select>
          </div>
        )}
        {error && (
          <div className="text-sm text-red-600 bg-red-50 rounded p-2">
            {error}
          </div>
        )}
        <div className="flex gap-2 pt-2">
          <button
            type="submit"
            disabled={loading}
            className="flex-1 bg-green-600 text-white rounded-lg py-2 text-sm font-medium hover:bg-green-700 disabled:opacity-50"
          >
            {loading ? "Đang xử lý..." : "Xác nhận chuyển đổi"}
          </button>
          <button
            type="button"
            onClick={onClose}
            className="px-4 border rounded-lg py-2 text-sm hover:bg-gray-50"
          >
            Hủy
          </button>
        </div>
      </form>
    </div>
  );
}

export default function LeadListPage() {
  const { user } = useAuthStore();
  // ✅ Khớp với backend: Delete chỉ ManagerOnly; Convert là nghiệp vụ Sale (SalesTeam policy)
  const canDelete = user?.role === ROLES.Manager;
  const canConvert = [ROLES.Sale, ROLES.Manager].includes(user?.role);

  const [items, setItems] = useState([]);
  const [nhanVienList, setNhanVienList] = useState([]);
  const [form, setForm] = useState(emptyForm);
  const [editingId, setEditingId] = useState(null);
  const [convertLead, setConvertLead] = useState(null);
  const [loading, setLoading] = useState(false);
  const [submitting, setSubmitting] = useState(false);
  const [error, setError] = useState("");
  const [success, setSuccess] = useState("");
  const [search, setSearch] = useState("");
  const [pageNumber, setPageNumber] = useState(1);
  const [totalPages, setTotalPages] = useState(1);
  const [totalCount, setTotalCount] = useState(0);
  const pageSize = 10;

  const nhanVienMap = useMemo(
    () =>
      new Map(
        nhanVienList.map((nv) => [String(nv.id), nv.hoTen ?? `NV #${nv.id}`]),
      ),
    [nhanVienList],
  );

  const loadLeads = async () => {
    setLoading(true);
    setError("");
    try {
      const res = await leadApi.getAll({
        pageNumber,
        pageSize,
        search: search.trim() || undefined,
      });
      const paged = res.data;
      setItems(paged?.items ?? []);
      setTotalPages(paged?.totalPages ?? 1);
      setTotalCount(paged?.totalCount ?? 0);
    } catch (err) {
      setError(err?.message || "Tải danh sách thất bại");
    } finally {
      setLoading(false);
    }
  };

  const loadNhanVien = async () => {
    try {
      const res = await authApi.getStaffList();
      setNhanVienList(res.data ?? []);
    } catch {}
  };

  useEffect(() => {
    loadLeads();
  }, [pageNumber]);
  useEffect(() => {
    loadNhanVien();
  }, []);

  const resetForm = () => {
    setForm(emptyForm);
    setEditingId(null);
    setError("");
    setSuccess("");
  };
  const handleChange = (e) =>
    setForm((f) => ({ ...f, [e.target.name]: e.target.value }));

  const handleSubmit = async (e) => {
    e.preventDefault();
    if (!form.tenLead.trim()) {
      setError("Tên lead không được để trống");
      return;
    }
    setSubmitting(true);
    setError("");
    setSuccess("");
    const toInt = (v) => (v === "" || v == null ? null : Number(v));
    const payload = {
      tenLead: form.tenLead.trim(),
      tenCongTy: form.tenCongTy.trim() || null,
      soDienThoai: form.soDienThoai.trim() || null,
      email: form.email.trim() || null,
      tinhTrang: form.tinhTrang || null,
      nhanVienPhuTrachId: toInt(form.nhanVienPhuTrachId),
    };
    try {
      if (editingId) {
        await leadApi.update(editingId, payload);
        setSuccess("Cập nhật lead thành công");
      } else {
        await leadApi.create(payload);
        setSuccess("Thêm lead thành công");
      }
      await loadLeads();
      resetForm();
    } catch (err) {
      setError(err?.message || "Không thể lưu lead");
    } finally {
      setSubmitting(false);
    }
  };

  const handleEdit = (item) => {
    setEditingId(item.id);
    setForm({
      tenLead: item.tenLead ?? "",
      tenCongTy: item.tenCongTy ?? "",
      soDienThoai: item.soDienThoai ?? "",
      email: item.email ?? "",
      tinhTrang: item.tinhTrang ?? "",
      nhanVienPhuTrachId: item.nhanVienPhuTrachId ?? "",
    });
    window.scrollTo({ top: 0, behavior: "smooth" });
  };

  const handleDelete = async (id) => {
    if (!window.confirm("Xóa lead này?")) return;
    try {
      await leadApi.delete(id);
      setSuccess("Xóa lead thành công");
      await loadLeads();
      if (editingId === id) resetForm();
    } catch (err) {
      setError(err?.message || "Không thể xóa lead");
    }
  };

  return (
    <div className="space-y-6">
      {convertLead && (
        <ConvertModal
          lead={convertLead}
          nhanVienList={nhanVienList}
          onClose={() => setConvertLead(null)}
          onConverted={() => {
            setConvertLead(null);
            setSuccess("Chuyển đổi thành công!");
            loadLeads();
          }}
        />
      )}

      <div className="flex flex-col sm:flex-row sm:items-center sm:justify-between gap-4">
        <div>
          <p className="text-xs text-gray-500 uppercase tracking-wide">
            CRM / Lead
          </p>
          <h1 className="text-2xl font-bold text-gray-800">Quản lý Lead</h1>
        </div>
        <div className="flex gap-2">
          <input
            type="search"
            placeholder="Tìm theo tên, email, SĐT..."
            value={search}
            onChange={(e) => setSearch(e.target.value)}
            onKeyDown={(e) => {
              if (e.key === "Enter") {
                setPageNumber(1);
                loadLeads();
              }
            }}
            className="border rounded-lg px-3 py-2 text-sm w-64 focus:outline-none focus:ring-2 focus:ring-blue-400"
          />
          <button
            onClick={() => {
              setPageNumber(1);
              loadLeads();
            }}
            className="border rounded-lg px-4 py-2 text-sm hover:bg-gray-50"
          >
            Tải lại
          </button>
        </div>
      </div>

      <div className="grid grid-cols-1 lg:grid-cols-3 gap-6">
        {/* Form */}
        <form
          onSubmit={handleSubmit}
          className="bg-white rounded-xl border shadow-sm p-6 space-y-4 lg:col-span-1"
        >
          <div className="flex items-center justify-between">
            <div>
              <h2 className="font-semibold text-gray-800">
                {editingId ? "Cập nhật Lead" : "Thêm Lead mới"}
              </h2>
              <p className="text-xs text-gray-400">
                Điền thông tin và nhấn lưu.
              </p>
            </div>
            {editingId && (
              <button
                type="button"
                onClick={resetForm}
                className="text-xs text-gray-500 hover:text-gray-700"
              >
                Hủy
              </button>
            )}
          </div>

          {[
            {
              name: "tenLead",
              label: "Tên Lead *",
              placeholder: "Nguyễn Văn A",
              required: true,
            },
            { name: "tenCongTy", label: "Công ty", placeholder: "Công ty ABC" },
            {
              name: "email",
              label: "Email",
              placeholder: "email@example.com",
              type: "email",
            },
            {
              name: "soDienThoai",
              label: "Số điện thoại",
              placeholder: "0901234567",
            },
          ].map((f) => (
            <div key={f.name}>
              <label className="block text-sm font-medium text-gray-700 mb-1">
                {f.label}
              </label>
              <input
                name={f.name}
                type={f.type ?? "text"}
                value={form[f.name]}
                onChange={handleChange}
                placeholder={f.placeholder}
                className="w-full border rounded-lg px-3 py-2 text-sm focus:outline-none focus:ring-2 focus:ring-blue-400"
              />
            </div>
          ))}

          <div>
            <label className="block text-sm font-medium text-gray-700 mb-1">
              Tình trạng
            </label>
            <select
              name="tinhTrang"
              value={form.tinhTrang}
              onChange={handleChange}
              className="w-full border rounded-lg px-3 py-2 text-sm focus:outline-none focus:ring-2 focus:ring-blue-400"
            >
              <option value="">-- Chọn --</option>
              {LEAD_TINH_TRANG_OPTIONS.map((o) => (
                <option key={o.value} value={o.value}>
                  {o.label}
                </option>
              ))}
            </select>
          </div>

          <div>
            <label className="block text-sm font-medium text-gray-700 mb-1">
              Nhân viên phụ trách
            </label>
            {nhanVienList.length > 0 ? (
              <select
                name="nhanVienPhuTrachId"
                value={form.nhanVienPhuTrachId}
                onChange={handleChange}
                className="w-full border rounded-lg px-3 py-2 text-sm focus:outline-none focus:ring-2 focus:ring-blue-400"
              >
                <option value="">-- Chọn --</option>
                {nhanVienList.map((nv) => (
                  <option key={nv.id} value={nv.id}>
                    {nv.hoTen ?? `NV #${nv.id}`}
                  </option>
                ))}
              </select>
            ) : (
              <input
                name="nhanVienPhuTrachId"
                type="number"
                min="1"
                value={form.nhanVienPhuTrachId}
                onChange={handleChange}
                placeholder="ID nhân viên"
                className="w-full border rounded-lg px-3 py-2 text-sm focus:outline-none focus:ring-2 focus:ring-blue-400"
              />
            )}
          </div>

          {error && (
            <div className="text-sm text-red-600 bg-red-50 rounded-lg p-2">
              {error}
            </div>
          )}
          {success && (
            <div className="text-sm text-green-700 bg-green-50 rounded-lg p-2">
              {success}
            </div>
          )}

          <div className="flex gap-2 pt-2">
            <button
              type="submit"
              disabled={submitting}
              className="flex-1 bg-blue-600 text-white rounded-lg py-2 text-sm font-medium hover:bg-blue-700 disabled:opacity-50"
            >
              {submitting ? "Đang lưu..." : editingId ? "Cập nhật" : "Thêm mới"}
            </button>
            <button
              type="button"
              onClick={resetForm}
              className="px-4 border rounded-lg py-2 text-sm hover:bg-gray-50"
            >
              Làm mới
            </button>
          </div>
        </form>

        {/* Table */}
        <div className="bg-white rounded-xl border shadow-sm lg:col-span-2 overflow-hidden">
          <div className="px-6 py-4 border-b flex items-center justify-between">
            <div>
              <h2 className="font-semibold text-gray-800">Danh sách Lead</h2>
              <p className="text-xs text-gray-400">
                Trang {pageNumber} / {totalPages} — {totalCount} lead
              </p>
            </div>
            {loading && (
              <span className="text-xs text-gray-400 animate-pulse">
                Đang tải...
              </span>
            )}
          </div>
          <div className="overflow-x-auto">
            <table className="w-full text-sm">
              <thead className="bg-gray-50 text-xs text-gray-500 uppercase">
                <tr>
                  <th className="px-4 py-3 text-left">Tên Lead</th>
                  <th className="px-4 py-3 text-left">Liên hệ</th>
                  <th className="px-4 py-3 text-left">Công ty</th>
                  <th className="px-4 py-3 text-left">Tình trạng</th>
                  <th className="px-4 py-3 text-left">NV phụ trách</th>
                  <th className="px-4 py-3 text-left">Ngày tạo</th>
                  <th className="px-4 py-3 text-left">Hành động</th>
                </tr>
              </thead>
              <tbody className="divide-y divide-gray-100">
                {items.length === 0 ? (
                  <tr>
                    <td colSpan="7" className="text-center py-10 text-gray-400">
                      {loading ? "Đang tải..." : "Không có dữ liệu"}
                    </td>
                  </tr>
                ) : (
                  items.map((item) => (
                    <tr key={item.id} className="hover:bg-gray-50">
                      <td className="px-4 py-3 font-medium text-gray-900">
                        {item.tenLead}
                      </td>
                      <td className="px-4 py-3">
                        <div>{item.email || "—"}</div>
                        {item.soDienThoai && (
                          <div className="text-xs text-gray-400">
                            {item.soDienThoai}
                          </div>
                        )}
                      </td>
                      <td className="px-4 py-3 text-gray-600">
                        {item.tenCongTy || "—"}
                      </td>
                      <td className="px-4 py-3">
                        {item.tinhTrang ? (
                          <Badge
                            label={item.tinhTrang}
                            colorClass={
                              LEAD_TINH_TRANG_COLOR[item.tinhTrang] ??
                              "bg-gray-100 text-gray-600"
                            }
                          />
                        ) : (
                          "—"
                        )}
                      </td>
                      <td className="px-4 py-3 text-gray-700">
                        {item.nhanVienPhuTrachId
                          ? (nhanVienMap.get(String(item.nhanVienPhuTrachId)) ??
                            `NV #${item.nhanVienPhuTrachId}`)
                          : "—"}
                      </td>
                      <td className="px-4 py-3 text-xs text-gray-400">
                        {formatDateTime(item.createdAt)}
                      </td>
                      <td className="px-4 py-3">
                        <div className="flex gap-2 flex-wrap">
                          <button
                            onClick={() => handleEdit(item)}
                            className="text-blue-600 hover:underline text-xs font-medium"
                          >
                            Sửa
                          </button>
                          {canConvert && item.tinhTrang !== "Đã chuyển đổi" && (
                            <button
                              onClick={() => setConvertLead(item)}
                              className="text-green-600 hover:underline text-xs font-medium"
                            >
                              Chuyển đổi
                            </button>
                          )}
                          {canDelete && (
                            <button
                              onClick={() => handleDelete(item.id)}
                              className="text-red-500 hover:underline text-xs font-medium"
                            >
                              Xóa
                            </button>
                          )}
                        </div>
                      </td>
                    </tr>
                  ))
                )}
              </tbody>
            </table>
          </div>
          <div className="px-6 py-4 border-t">
            <Pagination
              pageNumber={pageNumber}
              totalPages={totalPages}
              onPageChange={setPageNumber}
            />
          </div>
        </div>
      </div>
    </div>
  );
}
