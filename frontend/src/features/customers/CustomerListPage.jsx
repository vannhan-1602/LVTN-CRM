import { useEffect, useMemo, useState } from "react";
import customerApi from "../../api/customerApi";
import Pagination from "../../components/common/Pagination"; // Import Pagination
import "./KhachHang.css";

const LOAI_KHACH_HANG_OPTIONS = [
  { value: 1, label: "Cá nhân" },
  { value: 2, label: "Doanh nghiệp" },
  { value: 3, label: "VIP" },
];

const TINH_TRANG_OPTIONS = [
  { value: 1, label: "Đang hoạt động" },
  { value: 2, label: "Tiềm năng" },
  { value: 3, label: "Không hoạt động" },
];

const LOAI_BADGE_MAP = {
  1: "badge-canhan",
  2: "badge-doanhnghiep",
  3: "badge-vip",
};

const TINH_TRANG_BADGE_MAP = {
  1: "badge-hoatdong",
  2: "badge-tiemnang",
  3: "badge-khonghoatdong",
};

const emptyForm = {
  tenKhachHang: "",
  email: "",
  soDienThoai: "",
  loaiKhachHangId: "",
  tinhTrangId: "",
  maSoThue: "",
  nhanVienPhuTrachId: "",
};

function formatDiaChi(diaChiList) {
  if (!Array.isArray(diaChiList) || diaChiList.length === 0) return null;
  const dc = diaChiList.find((d) => d.isDefault) ?? diaChiList[0];
  return [dc.diaChiChiTiet, dc.phuongXa, dc.quanHuyen, dc.tinhThanh]
    .filter(Boolean)
    .join(", ");
}

function formatDateTime(value) {
  if (!value) return "-";
  const date = new Date(value);
  if (Number.isNaN(date.getTime())) return value;
  return new Intl.DateTimeFormat("vi-VN", {
    dateStyle: "short",
    timeStyle: "short",
  }).format(date);
}

// ─── component ──────────────────────────────────────────────────────────────
export default function CustomerListPage() {
  const [items, setItems] = useState([]);
  const [nhanVienList, setNhanVienList] = useState([]);
  const [form, setForm] = useState(emptyForm);
  const [editingId, setEditingId] = useState(null);
  const [loading, setLoading] = useState(false);
  const [submitting, setSubmitting] = useState(false);
  const [error, setError] = useState("");
  const [success, setSuccess] = useState("");

  // States lọc & Phân trang (Sử dụng cho Pagination component)
  const [search, setSearch] = useState("");
  const [filterLoai, setFilterLoai] = useState("");
  const [filterTinhTrang, setFilterTinhTrang] = useState("");
  const [pageNumber, setPageNumber] = useState(1);
  const [totalPages, setTotalPages] = useState(1);
  const pageSize = 10;

  // map id → họ tên nhanh
  const nhanVienMap = useMemo(
    () =>
      new Map(
        nhanVienList.map((nv) => [
          String(nv.id),
          nv.hoTen ?? nv.tenNhanVien ?? `NV #${nv.id}`,
        ]),
      ),
    [nhanVienList],
  );

  const tenNhanVien = (id) =>
    id != null ? (nhanVienMap.get(String(id)) ?? `NV #${id}`) : "—";

  // ── fetch ──
  const loadKhachHang = async () => {
    setLoading(true);
    setError("");
    try {
      const res = await customerApi.getAll({
        pageNumber,
        pageSize,
        search: search || null,
        loaiKhachHangId: filterLoai || null,
        tinhTrangId: filterTinhTrang || null,
      });
      // Lấy dữ liệu phân trang từ Backend
      const data = res.data.data.items || [];
      const fetchedTotalPages = res.data.data.totalPages || 1;

      setItems(Array.isArray(data) ? data : []);
      setTotalPages(fetchedTotalPages);
    } catch (err) {
      setError(err.response?.data?.message || "Tải danh sách thất bại");
    } finally {
      setLoading(false);
    }
  };

  const loadNhanVien = async () => {
    try {
      const API_BASE_URL =
        import.meta.env.VITE_API_BASE_URL ?? "https://localhost:7071";
      const res = await fetch(`${API_BASE_URL}/api/Auth/users`);
      if (!res.ok) return;
      const data = await res.json();
      setNhanVienList(Array.isArray(data.data) ? data.data : []);
    } catch {
      /* silent */
    }
  };

  // Gọi API mỗi khi pageNumber hoặc bộ lọc thay đổi
  useEffect(() => {
    loadKhachHang();
    loadNhanVien();
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [pageNumber, filterLoai, filterTinhTrang]);

  // Handle Search Trigger (Nhấn Enter hoặc nút Tải lại)
  const handleSearchSubmit = () => {
    setPageNumber(1);
    loadKhachHang();
  };

  // ── stats ──
  const stats = useMemo(
    () => ({
      total: items.length,
      doanhnghiep: items.filter((i) => i.loaiKhachHangId === 2).length,
      canhan: items.filter((i) => i.loaiKhachHangId === 1).length,
      vip: items.filter((i) => i.loaiKhachHangId === 3).length,
    }),
    [items],
  );

  // ── form handlers ──
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

  const validateForm = () => {
    if (!form.tenKhachHang.trim()) return "Tên khách hàng không được rỗng";
    if (form.email && !/^[^\s@]+@[^\s@]+\.[^\s@]+$/.test(form.email))
      return "Email không đúng định dạng";
    if (form.soDienThoai && !/^\d{10,11}$/.test(form.soDienThoai))
      return "Số điện thoại phải có 10–11 chữ số";
    return "";
  };

  const handleSubmit = async (e) => {
    e.preventDefault();
    const msg = validateForm();
    if (msg) {
      setError(msg);
      setSuccess("");
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
      setError(err.response?.data?.message || "Không thể lưu khách hàng");
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
    setError("");
    setSuccess("");
    try {
      await customerApi.delete(id);
      await loadKhachHang();
      if (editingId === id) resetForm();
      setSuccess("Xóa khách hàng thành công");
    } catch (err) {
      setError(err.response?.data?.message || "Không thể xóa khách hàng");
    }
  };

  // ─── render ─────────────────────────────────────────────────────────────
  return (
    <main className="kh-page">
      {/* HEADER */}
      <section className="kh-header">
        <div>
          <p className="eyebrow">CRM / Khách hàng</p>
          <h1>Quản lý khách hàng</h1>
        </div>
        <div className="toolbar">
          <input
            className="search"
            type="search"
            placeholder="Tìm theo mã, tên, email, SĐT..."
            value={search}
            onChange={(e) => setSearch(e.target.value)}
            onKeyDown={(e) => e.key === "Enter" && handleSearchSubmit()}
          />
          <select
            className="search kh-select-sm"
            value={filterLoai}
            onChange={(e) => {
              setFilterLoai(e.target.value);
              setPageNumber(1);
            }}
          >
            <option value="">Tất cả loại</option>
            {LOAI_KHACH_HANG_OPTIONS.map((o) => (
              <option key={o.value} value={o.value}>
                {o.label}
              </option>
            ))}
          </select>
          <select
            className="search kh-select-sm"
            value={filterTinhTrang}
            onChange={(e) => {
              setFilterTinhTrang(e.target.value);
              setPageNumber(1);
            }}
          >
            <option value="">Tất cả trạng thái</option>
            {TINH_TRANG_OPTIONS.map((o) => (
              <option key={o.value} value={o.value}>
                {o.label}
              </option>
            ))}
          </select>
          <button
            className="secondary-btn"
            type="button"
            onClick={handleSearchSubmit}
          >
            Tải lại
          </button>
        </div>
      </section>

      {/* STATS */}
      <section className="stats-row">
        <article className="stat-card">
          <span>Khách hàng (Trang hiện tại)</span>
          <strong>{stats.total}</strong>
        </article>
        <article className="stat-card">
          <span>Cá nhân</span>
          <strong>{stats.canhan}</strong>
        </article>
        <article className="stat-card">
          <span>Doanh nghiệp</span>
          <strong>{stats.doanhnghiep}</strong>
        </article>
        <article className="stat-card">
          <span>VIP</span>
          <strong>{stats.vip}</strong>
        </article>
      </section>

      {/* CONTENT */}
      <section className="content-grid">
        {/* FORM */}
        <form className="panel form-panel" onSubmit={handleSubmit}>
          <div className="panel-head">
            <div>
              <h2>
                {editingId ? "Cập nhật khách hàng" : "Thêm khách hàng mới"}
              </h2>
              <p>Điền thông tin và nhấn lưu.</p>
            </div>
            {editingId ? (
              <button className="ghost-btn" type="button" onClick={resetForm}>
                Hủy sửa
              </button>
            ) : null}
          </div>

          <label>
            Tên khách hàng <span className="kh-req">*</span>
            <input
              name="tenKhachHang"
              value={form.tenKhachHang}
              onChange={handleChange}
              placeholder="Nguyễn Văn A"
            />
          </label>

          <div className="two-col">
            <label>
              Email
              <input
                name="email"
                type="email"
                value={form.email}
                onChange={handleChange}
                placeholder="example@mail.com"
              />
            </label>
            <label>
              Số điện thoại
              <input
                name="soDienThoai"
                value={form.soDienThoai}
                onChange={handleChange}
                placeholder="0901234567"
              />
            </label>
          </div>

          <div className="two-col">
            <label>
              Loại khách hàng
              <select
                name="loaiKhachHangId"
                value={form.loaiKhachHangId}
                onChange={handleChange}
              >
                <option value="">-- Chọn loại --</option>
                {LOAI_KHACH_HANG_OPTIONS.map((o) => (
                  <option key={o.value} value={o.value}>
                    {o.label}
                  </option>
                ))}
              </select>
            </label>
            <label>
              Tình trạng
              <select
                name="tinhTrangId"
                value={form.tinhTrangId}
                onChange={handleChange}
              >
                <option value="">-- Chọn tình trạng --</option>
                {TINH_TRANG_OPTIONS.map((o) => (
                  <option key={o.value} value={o.value}>
                    {o.label}
                  </option>
                ))}
              </select>
            </label>
          </div>

          <label>
            Mã số thuế
            <input
              name="maSoThue"
              value={form.maSoThue}
              onChange={handleChange}
              placeholder="0123456789"
            />
          </label>

          <label>
            Nhân viên phụ trách
            {nhanVienList.length > 0 ? (
              <select
                name="nhanVienPhuTrachId"
                value={form.nhanVienPhuTrachId}
                onChange={handleChange}
              >
                <option value="">-- Chọn nhân viên --</option>
                {nhanVienList.map((nv) => (
                  <option key={nv.id} value={nv.id}>
                    {nv.hoTen ?? nv.tenNhanVien ?? `NV #${nv.id}`}
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
              />
            )}
          </label>

          {error ? <div className="message error">{error}</div> : null}
          {success ? <div className="message success">{success}</div> : null}

          <div className="actions">
            <button className="primary-btn" type="submit" disabled={submitting}>
              {submitting ? "Đang lưu..." : editingId ? "Cập nhật" : "Thêm mới"}
            </button>
            <button className="secondary-btn" type="button" onClick={resetForm}>
              Làm mới form
            </button>
          </div>
        </form>

        {/* TABLE */}
        <section className="panel table-panel">
          <div className="panel-head">
            <div>
              <h2>Danh sách khách hàng</h2>
              <p>
                Trang {pageNumber} / {totalPages}
              </p>
            </div>
            {loading ? (
              <span className="text-gray-500 text-sm">Đang tải...</span>
            ) : null}
          </div>

          <div className="table-wrap">
            <table>
              <thead>
                <tr>
                  <th>Mã KH</th>
                  <th>Tên khách hàng</th>
                  <th>Liên hệ</th>
                  <th>Địa chỉ</th>
                  <th>Loại</th>
                  <th>Tình trạng</th>
                  <th>NV phụ trách</th>
                  <th>Cập nhật</th>
                  <th>Hành động</th>
                </tr>
              </thead>
              <tbody>
                {items.length === 0 ? (
                  <tr>
                    <td colSpan="9" className="empty-row">
                      {loading
                        ? "Đang tải dữ liệu..."
                        : "Không có dữ liệu phù hợp"}
                    </td>
                  </tr>
                ) : (
                  items.map((item) => (
                    <tr key={item.id}>
                      <td>
                        <span className="kh-ma">
                          {item.maKhachHang || `#${item.id}`}
                        </span>
                      </td>
                      <td>
                        <div className="stacked-cell">
                          <strong>{item.tenKhachHang}</strong>
                          {item.maSoThue && (
                            <span style={{ color: "#6d7c91", fontSize: 12 }}>
                              MST: {item.maSoThue}
                            </span>
                          )}
                        </div>
                      </td>
                      <td>
                        <div className="stacked-cell">
                          {item.email && <span>{item.email}</span>}
                          {item.soDienThoai && (
                            <span style={{ color: "#6d7c91", fontSize: 12 }}>
                              {item.soDienThoai}
                            </span>
                          )}
                          {!item.email && !item.soDienThoai && (
                            <span style={{ color: "#aaa" }}>—</span>
                          )}
                        </div>
                      </td>
                      <td>
                        <div className="stacked-cell">
                          {formatDiaChi(item.diaChiList) ? (
                            <span>{formatDiaChi(item.diaChiList)}</span>
                          ) : (
                            <span style={{ color: "#aaa" }}>—</span>
                          )}
                          {item.diaChiList?.length > 1 && (
                            <span style={{ color: "#6d7c91", fontSize: 12 }}>
                              +{item.diaChiList.length - 1} địa chỉ khác
                            </span>
                          )}
                        </div>
                      </td>
                      <td>
                        {item.loaiKhachHangId ? (
                          <span
                            className={`badge ${LOAI_BADGE_MAP[item.loaiKhachHangId] ?? "badge"}`}
                          >
                            {LOAI_KHACH_HANG_OPTIONS.find(
                              (o) => o.value === item.loaiKhachHangId,
                            )?.label ?? `Loại ${item.loaiKhachHangId}`}
                          </span>
                        ) : (
                          "—"
                        )}
                      </td>
                      <td>
                        {item.tinhTrangId ? (
                          <span
                            className={`badge ${TINH_TRANG_BADGE_MAP[item.tinhTrangId] ?? "badge"}`}
                          >
                            {TINH_TRANG_OPTIONS.find(
                              (o) => o.value === item.tinhTrangId,
                            )?.label ?? `Trạng thái ${item.tinhTrangId}`}
                          </span>
                        ) : (
                          "—"
                        )}
                      </td>
                      <td>
                        <span className="kh-nv">
                          {item.tenNhanVienPhuTrach
                            ? item.tenNhanVienPhuTrach
                            : tenNhanVien(item.nhanVienPhuTrachId)}
                        </span>
                      </td>
                      <td>{formatDateTime(item.updatedAt)}</td>
                      <td>
                        <div className="row-actions">
                          <button
                            type="button"
                            className="ghost-btn"
                            onClick={() => handleEdit(item)}
                          >
                            Sửa
                          </button>
                          <button
                            type="button"
                            className="danger-btn"
                            onClick={() => handleDelete(item.id)}
                          >
                            Xóa
                          </button>
                        </div>
                      </td>
                    </tr>
                  ))
                )}
              </tbody>
            </table>
          </div>

          {/* SỬ DỤNG PAGINATION ĐÃ IMPORT Ở ĐÂY */}
          <div style={{ padding: "16px 24px", borderTop: "1px solid #e5ebf3" }}>
            <Pagination
              pageNumber={pageNumber}
              totalPages={totalPages}
              onPageChange={setPageNumber}
            />
          </div>
        </section>
      </section>
    </main>
  );
}
