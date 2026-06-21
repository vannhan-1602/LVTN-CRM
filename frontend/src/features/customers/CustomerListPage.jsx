import { useEffect, useMemo, useState } from "react";
import customerApi from "../../api/customerApi";
import authApi from "../../api/authApi";
import useAuthStore from "../auth/authStore";
import Pagination from "../../components/common/Pagination";
import {
  ROLES,
  LOAI_KHACH_HANG_OPTIONS,
  TINH_TRANG_KHACH_HANG_OPTIONS,
  LOAI_BADGE_COLOR,
  TINH_TRANG_BADGE_COLOR,
} from "../../utils/constants";
import { formatDateTime } from "../../utils/formatters";

const emptyForm = {
  tenKhachHang: "",
  email: "",
  soDienThoai: "",
  loaiKhachHangId: "",
  tinhTrangId: "",
  maSoThue: "",
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

export default function CustomerListPage() {
  const { user } = useAuthStore();
  // ✅ Khớp với backend: Delete chỉ Policies.ManagerOnly (Admin không có quyền nghiệp vụ)
  const canDelete = user?.role === ROLES.Manager;

  const [items, setItems] = useState([]);
  const [nhanVienList, setNhanVienList] = useState([]);
  const [form, setForm] = useState(emptyForm);
  const [editingId, setEditingId] = useState(null);
  const [loading, setLoading] = useState(false);
  const [submitting, setSubmitting] = useState(false);
  const [error, setError] = useState("");
  const [success, setSuccess] = useState("");

  const [search, setSearch] = useState("");
  const [filterLoai, setFilterLoai] = useState("");
  const [filterTinhTrang, setFilterTinhTrang] = useState("");
  const [pageNumber, setPageNumber] = useState(1);
  const [totalPages, setTotalPages] = useState(1);
  const [totalCount, setTotalCount] = useState(0);
  const pageSize = 10;

  // nhanVienMap để dùng trong form select
  const nhanVienMap = useMemo(
    () =>
      new Map(
        nhanVienList.map((nv) => [String(nv.id), nv.hoTen ?? `NV #${nv.id}`]),
      ),
    [nhanVienList],
  );

  // ── fetch danh sách khách hàng ──────────────────────────────────────────
  const loadKhachHang = async () => {
    setLoading(true);
    setError("");
    try {
      const res = await customerApi.getAll({
        pageNumber,
        pageSize,
        search: search.trim() || undefined,
        loaiKhachHangId: filterLoai || undefined,
        tinhTrangId: filterTinhTrang || undefined,
      });
      // axiosClient trả về ApiResponse<PagedResult<CustomerDto>>
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

  // ── fetch danh sách nhân viên (Admin API, dùng cho form select) ────────
  const loadNhanVien = async () => {
    try {
      const res = await authApi.getStaffList();
      setNhanVienList(res.data ?? []);
    } catch {
      /* Admin-only, sale không có quyền — bỏ qua */
    }
  };

  useEffect(() => {
    loadKhachHang();
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [pageNumber, filterLoai, filterTinhTrang]);

  useEffect(() => {
    loadNhanVien();
  }, []);

  // ── stats từ data trang hiện tại ────────────────────────────────────────
  const stats = useMemo(
    () => ({
      total: totalCount,
      vip: items.filter((i) => i.loaiKhachHangId === 1).length,
      b2b: items.filter((i) => i.loaiKhachHangId === 2).length,
      b2c: items.filter((i) => i.loaiKhachHangId === 3).length,
    }),
    [items, totalCount],
  );

  // ── form handlers ────────────────────────────────────────────────────────
  const resetForm = () => {
    setForm(emptyForm);
    setEditingId(null);
    setError("");
    setSuccess("");
  };
  const handleChange = (e) => {
    const { name, value } = e.target;
    setForm((prev) => ({ ...prev, [name]: value }));
  };

  const handleSubmit = async (e) => {
    e.preventDefault();
    if (!form.tenKhachHang.trim()) {
      setError("Tên khách hàng không được để trống");
      return;
    }
    setSubmitting(true);
    setError("");
    setSuccess("");
    const toInt = (v) => (v === "" || v == null ? null : Number(v));
    const payload = {
      tenKhachHang: form.tenKhachHang.trim(),
      email: form.email.trim() || null,
      soDienThoai: form.soDienThoai.trim() || null,
      loaiKhachHangId: toInt(form.loaiKhachHangId),
      tinhTrangId: toInt(form.tinhTrangId),
      maSoThue: form.maSoThue.trim() || null,
      nhanVienPhuTrachId: toInt(form.nhanVienPhuTrachId),
    };
    try {
      if (editingId) {
        await customerApi.update(editingId, payload);
        setSuccess("Cập nhật khách hàng thành công");
      } else {
        await customerApi.create(payload);
        setSuccess("Thêm khách hàng thành công");
      }
      await loadKhachHang();
      resetForm();
    } catch (err) {
      setError(err?.message || "Không thể lưu khách hàng");
    } finally {
      setSubmitting(false);
    }
  };

  const handleEdit = (item) => {
    setEditingId(item.id);
    setForm({
      tenKhachHang: item.tenKhachHang ?? "",
      email: item.email ?? "",
      soDienThoai: item.soDienThoai ?? "",
      loaiKhachHangId: item.loaiKhachHangId ?? "",
      tinhTrangId: item.tinhTrangId ?? "",
      maSoThue: item.maSoThue ?? "",
      nhanVienPhuTrachId: item.nhanVienPhuTrachId ?? "",
    });
    setError("");
    setSuccess("");
    window.scrollTo({ top: 0, behavior: "smooth" });
  };

  const handleDelete = async (id) => {
    if (!window.confirm("Bạn có chắc muốn xóa khách hàng này không?")) return;
    try {
      await customerApi.delete(id);
      setSuccess("Xóa khách hàng thành công");
      await loadKhachHang();
      if (editingId === id) resetForm();
    } catch (err) {
      setError(err?.message || "Không thể xóa khách hàng");
    }
  };

  // ── render ───────────────────────────────────────────────────────────────
  return (
    <div className="space-y-6">
      {/* Header */}
      <div className="flex flex-col sm:flex-row sm:items-center sm:justify-between gap-4">
        <div>
          <p className="text-xs text-gray-500 uppercase tracking-wide">
            CRM / Khách hàng
          </p>
          <h1 className="text-2xl font-bold text-gray-800">
            Quản lý khách hàng
          </h1>
        </div>
        {/* Toolbar */}
        <div className="flex flex-wrap gap-2">
          <input
            type="search"
            placeholder="Tìm theo mã, tên, email, SĐT..."
            value={search}
            onChange={(e) => setSearch(e.target.value)}
            onKeyDown={(e) => {
              if (e.key === "Enter") {
                setPageNumber(1);
                loadKhachHang();
              }
            }}
            className="border rounded-lg px-3 py-2 text-sm w-64 focus:outline-none focus:ring-2 focus:ring-blue-400"
          />
          <select
            value={filterLoai}
            onChange={(e) => {
              setFilterLoai(e.target.value);
              setPageNumber(1);
            }}
            className="border rounded-lg px-3 py-2 text-sm focus:outline-none focus:ring-2 focus:ring-blue-400"
          >
            <option value="">Tất cả loại</option>
            {LOAI_KHACH_HANG_OPTIONS.map((o) => (
              <option key={o.value} value={o.value}>
                {o.label}
              </option>
            ))}
          </select>
          <select
            value={filterTinhTrang}
            onChange={(e) => {
              setFilterTinhTrang(e.target.value);
              setPageNumber(1);
            }}
            className="border rounded-lg px-3 py-2 text-sm focus:outline-none focus:ring-2 focus:ring-blue-400"
          >
            <option value="">Tất cả tình trạng</option>
            {TINH_TRANG_KHACH_HANG_OPTIONS.map((o) => (
              <option key={o.value} value={o.value}>
                {o.label}
              </option>
            ))}
          </select>
          <button
            onClick={() => {
              setPageNumber(1);
              loadKhachHang();
            }}
            className="border rounded-lg px-4 py-2 text-sm hover:bg-gray-50"
          >
            Tải lại
          </button>
        </div>
      </div>

      {/* Stats */}
      <div className="grid grid-cols-2 sm:grid-cols-4 gap-4">
        {[
          { label: "Tổng khách hàng", value: stats.total },
          { label: "VIP", value: stats.vip },
          { label: "B2B", value: stats.b2b },
          { label: "B2C", value: stats.b2c },
        ].map((s) => (
          <div
            key={s.label}
            className="bg-white rounded-xl border p-4 shadow-sm"
          >
            <p className="text-xs text-gray-500">{s.label}</p>
            <p className="text-2xl font-bold text-gray-800 mt-1">{s.value}</p>
          </div>
        ))}
      </div>

      {/* Content grid */}
      <div className="grid grid-cols-1 lg:grid-cols-3 gap-6">
        {/* Form */}
        <form
          onSubmit={handleSubmit}
          className="bg-white rounded-xl border shadow-sm p-6 space-y-4 lg:col-span-1"
        >
          <div className="flex items-center justify-between">
            <div>
              <h2 className="font-semibold text-gray-800">
                {editingId ? "Cập nhật khách hàng" : "Thêm khách hàng mới"}
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
                Hủy sửa
              </button>
            )}
          </div>

          <div>
            <label className="block text-sm font-medium text-gray-700 mb-1">
              Tên khách hàng <span className="text-red-500">*</span>
            </label>
            <input
              name="tenKhachHang"
              value={form.tenKhachHang}
              onChange={handleChange}
              placeholder="Nguyễn Văn A"
              className="w-full border rounded-lg px-3 py-2 text-sm focus:outline-none focus:ring-2 focus:ring-blue-400"
            />
          </div>

          <div className="grid grid-cols-2 gap-3">
            <div>
              <label className="block text-sm font-medium text-gray-700 mb-1">
                Email
              </label>
              <input
                name="email"
                type="email"
                value={form.email}
                onChange={handleChange}
                placeholder="example@mail.com"
                className="w-full border rounded-lg px-3 py-2 text-sm focus:outline-none focus:ring-2 focus:ring-blue-400"
              />
            </div>
            <div>
              <label className="block text-sm font-medium text-gray-700 mb-1">
                SĐT
              </label>
              <input
                name="soDienThoai"
                value={form.soDienThoai}
                onChange={handleChange}
                placeholder="0901234567"
                className="w-full border rounded-lg px-3 py-2 text-sm focus:outline-none focus:ring-2 focus:ring-blue-400"
              />
            </div>
          </div>

          <div className="grid grid-cols-2 gap-3">
            <div>
              <label className="block text-sm font-medium text-gray-700 mb-1">
                Loại KH
              </label>
              <select
                name="loaiKhachHangId"
                value={form.loaiKhachHangId}
                onChange={handleChange}
                className="w-full border rounded-lg px-3 py-2 text-sm focus:outline-none focus:ring-2 focus:ring-blue-400"
              >
                <option value="">-- Chọn --</option>
                {LOAI_KHACH_HANG_OPTIONS.map((o) => (
                  <option key={o.value} value={o.value}>
                    {o.label}
                  </option>
                ))}
              </select>
            </div>
            <div>
              <label className="block text-sm font-medium text-gray-700 mb-1">
                Tình trạng
              </label>
              <select
                name="tinhTrangId"
                value={form.tinhTrangId}
                onChange={handleChange}
                className="w-full border rounded-lg px-3 py-2 text-sm focus:outline-none focus:ring-2 focus:ring-blue-400"
              >
                <option value="">-- Chọn --</option>
                {TINH_TRANG_KHACH_HANG_OPTIONS.map((o) => (
                  <option key={o.value} value={o.value}>
                    {o.label}
                  </option>
                ))}
              </select>
            </div>
          </div>

          <div>
            <label className="block text-sm font-medium text-gray-700 mb-1">
              Mã số thuế
            </label>
            <input
              name="maSoThue"
              value={form.maSoThue}
              onChange={handleChange}
              placeholder="0123456789"
              className="w-full border rounded-lg px-3 py-2 text-sm focus:outline-none focus:ring-2 focus:ring-blue-400"
            />
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
                <option value="">-- Chọn nhân viên --</option>
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
              <h2 className="font-semibold text-gray-800">
                Danh sách khách hàng
              </h2>
              <p className="text-xs text-gray-400">
                Trang {pageNumber} / {totalPages} — {totalCount} khách hàng
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
                  <th className="px-4 py-3 text-left">Mã KH</th>
                  <th className="px-4 py-3 text-left">Tên khách hàng</th>
                  <th className="px-4 py-3 text-left">Liên hệ</th>
                  <th className="px-4 py-3 text-left">Loại</th>
                  <th className="px-4 py-3 text-left">Tình trạng</th>
                  <th className="px-4 py-3 text-left">NV phụ trách</th>
                  <th className="px-4 py-3 text-left">Cập nhật</th>
                  <th className="px-4 py-3 text-left">Hành động</th>
                </tr>
              </thead>
              <tbody className="divide-y divide-gray-100">
                {items.length === 0 ? (
                  <tr>
                    <td colSpan="8" className="text-center py-10 text-gray-400">
                      {loading
                        ? "Đang tải dữ liệu..."
                        : "Không có dữ liệu phù hợp"}
                    </td>
                  </tr>
                ) : (
                  items.map((item) => (
                    <tr
                      key={item.id}
                      className="hover:bg-gray-50 transition-colors"
                    >
                      <td className="px-4 py-3">
                        <span className="font-mono text-blue-600 text-xs font-semibold">
                          {item.maKhachHang}
                        </span>
                      </td>
                      <td className="px-4 py-3">
                        <div className="font-medium text-gray-900">
                          {item.tenKhachHang}
                        </div>
                        {item.maSoThue && (
                          <div className="text-xs text-gray-400">
                            MST: {item.maSoThue}
                          </div>
                        )}
                      </td>
                      <td className="px-4 py-3">
                        <div>{item.email || "—"}</div>
                        {item.soDienThoai && (
                          <div className="text-xs text-gray-400">
                            {item.soDienThoai}
                          </div>
                        )}
                      </td>
                      <td className="px-4 py-3">
                        {item.loaiKhachHangId ? (
                          <Badge
                            label={
                              item.tenLoaiKhachHang ??
                              `Loại ${item.loaiKhachHangId}`
                            }
                            colorClass={
                              LOAI_BADGE_COLOR[item.loaiKhachHangId] ??
                              "bg-gray-100 text-gray-600"
                            }
                          />
                        ) : (
                          "—"
                        )}
                      </td>
                      <td className="px-4 py-3">
                        {item.tinhTrangId ? (
                          <Badge
                            label={
                              item.tenTinhTrang ??
                              `Tình trạng ${item.tinhTrangId}`
                            }
                            colorClass={
                              TINH_TRANG_BADGE_COLOR[item.tinhTrangId] ??
                              "bg-gray-100 text-gray-600"
                            }
                          />
                        ) : (
                          "—"
                        )}
                      </td>
                      <td className="px-4 py-3 text-gray-700">
                        {item.tenNhanVienPhuTrach
                          ? item.tenNhanVienPhuTrach
                          : item.nhanVienPhuTrachId
                            ? (nhanVienMap.get(
                                String(item.nhanVienPhuTrachId),
                              ) ?? `NV #${item.nhanVienPhuTrachId}`)
                            : "—"}
                      </td>
                      <td className="px-4 py-3 text-xs text-gray-400">
                        {formatDateTime(item.updatedAt)}
                      </td>
                      <td className="px-4 py-3">
                        <div className="flex gap-2">
                          <button
                            onClick={() => handleEdit(item)}
                            className="text-blue-600 hover:underline text-xs font-medium"
                          >
                            Sửa
                          </button>
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
