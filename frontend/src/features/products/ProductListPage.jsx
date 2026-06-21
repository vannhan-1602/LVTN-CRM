import { useEffect, useMemo, useState } from "react";
import productApi from "../../api/productApi";
import useAuthStore from "../auth/authStore";
import Pagination from "../../components/common/Pagination";
import { ROLES, STOCK_TRANSACTION_TYPE_OPTIONS, STOCK_TRANSACTION_TYPE_LABEL } from "../../utils/constants";
import { formatDateTime } from "../../utils/formatters";

function Badge({ label, colorClass }) {
  return <span className={`px-2 py-0.5 rounded-full text-xs font-semibold ${colorClass}`}>{label}</span>;
}

const emptyForm = { loaiSanPhamId: "", maSP: "", tenSP: "", donVi: "", giaBan: "", soLuongTonBanDau: "0" };

// ── Modal: lịch sử kho + điều chỉnh tồn ─────────────────────────────────────
function StockModal({ product, onClose, onUpdated, canManage }) {
  const [history, setHistory] = useState([]);
  const [loading, setLoading] = useState(true);
  const [form, setForm] = useState({ loaiGiaoDich: "NhapMua", soLuong: "", maChungTu: "", ghiChu: "" });
  const [submitting, setSubmitting] = useState(false);
  const [error, setError] = useState("");
  const [tonHienTai, setTonHienTai] = useState(product.soLuongTon);

  const loadDetail = async () => {
    setLoading(true);
    try {
      const res = await productApi.getById(product.id);
      setHistory(res.data?.lichSuKho ?? []);
      setTonHienTai(res.data?.soLuongTon ?? product.soLuongTon);
    } catch { setError("Không thể tải lịch sử kho"); }
    finally { setLoading(false); }
  };

  useEffect(() => { loadDetail(); }, []);

  const handleSubmit = async (e) => {
    e.preventDefault();
    if (!form.soLuong || Number(form.soLuong) <= 0) { setError("Số lượng phải lớn hơn 0"); return; }
    setSubmitting(true); setError("");
    try {
      await productApi.adjustStock(product.id, {
        loaiGiaoDich: form.loaiGiaoDich,
        soLuong: Number(form.soLuong),
        maChungTu: form.maChungTu || null,
        ghiChu: form.ghiChu || null,
      });
      setForm({ loaiGiaoDich: "NhapMua", soLuong: "", maChungTu: "", ghiChu: "" });
      await loadDetail();
      onUpdated();
    } catch (err) {
      setError(err?.message || "Cập nhật tồn kho thất bại");
    } finally { setSubmitting(false); }
  };

  return (
    <div className="fixed inset-0 z-50 flex items-center justify-center bg-black/40 p-4">
      <div className="bg-white rounded-xl shadow-xl w-full max-w-2xl max-h-[90vh] overflow-y-auto">
        <div className="flex items-center justify-between px-6 py-4 border-b sticky top-0 bg-white">
          <div>
            <h3 className="font-bold text-lg text-gray-800">Quản lý kho — {product.tenSP}</h3>
            <p className="text-xs text-gray-400">Mã: {product.maSP} · Tồn hiện tại: <strong>{tonHienTai}</strong></p>
          </div>
          <button onClick={onClose} className="text-gray-400 hover:text-gray-700 text-xl font-bold">×</button>
        </div>

        <div className="p-6 space-y-5">
          {canManage && (
            <form onSubmit={handleSubmit} className="space-y-3 border-b pb-5">
              <h4 className="font-semibold text-gray-700">Tạo phiếu nhập/xuất kho</h4>
              <div className="grid grid-cols-2 gap-3">
                <div>
                  <label className="block text-xs font-medium text-gray-600 mb-1">Loại giao dịch</label>
                  <select value={form.loaiGiaoDich} onChange={(e) => setForm(f => ({ ...f, loaiGiaoDich: e.target.value }))}
                    className="w-full border rounded-lg px-3 py-2 text-sm">
                    {STOCK_TRANSACTION_TYPE_OPTIONS.map(o => <option key={o.value} value={o.value}>{o.label}</option>)}
                  </select>
                </div>
                <div>
                  <label className="block text-xs font-medium text-gray-600 mb-1">Số lượng</label>
                  <input type="number" min="1" value={form.soLuong}
                    onChange={(e) => setForm(f => ({ ...f, soLuong: e.target.value }))}
                    className="w-full border rounded-lg px-3 py-2 text-sm" />
                </div>
                <div>
                  <label className="block text-xs font-medium text-gray-600 mb-1">Mã chứng từ</label>
                  <input value={form.maChungTu} onChange={(e) => setForm(f => ({ ...f, maChungTu: e.target.value }))}
                    placeholder="VD: PNK001" className="w-full border rounded-lg px-3 py-2 text-sm" />
                </div>
                <div>
                  <label className="block text-xs font-medium text-gray-600 mb-1">Ghi chú</label>
                  <input value={form.ghiChu} onChange={(e) => setForm(f => ({ ...f, ghiChu: e.target.value }))}
                    className="w-full border rounded-lg px-3 py-2 text-sm" />
                </div>
              </div>
              {error && <div className="text-sm text-red-600 bg-red-50 rounded p-2">{error}</div>}
              <button type="submit" disabled={submitting}
                className="bg-blue-600 text-white px-4 py-2 rounded-lg text-sm font-medium hover:bg-blue-700 disabled:opacity-50">
                {submitting ? "Đang lưu..." : "Ghi nhận giao dịch"}
              </button>
            </form>
          )}

          <div>
            <h4 className="font-semibold text-gray-700 mb-3">Lịch sử giao dịch kho</h4>
            <div className="space-y-2 max-h-64 overflow-y-auto">
              {loading ? (
                <p className="text-gray-400 text-sm">Đang tải...</p>
              ) : history.length === 0 ? (
                <p className="text-gray-400 text-sm">Chưa có giao dịch nào</p>
              ) : history.map((h) => (
                <div key={h.id} className="bg-gray-50 rounded-lg p-3 text-sm flex items-center justify-between">
                  <div>
                    <span className="font-medium">{STOCK_TRANSACTION_TYPE_LABEL[h.loaiGiaoDich] ?? h.loaiGiaoDich}</span>
                    <span className={`ml-2 font-mono ${h.soLuongThayDoi > 0 ? "text-green-600" : "text-red-600"}`}>
                      {h.soLuongThayDoi > 0 ? "+" : ""}{h.soLuongThayDoi}
                    </span>
                    {h.maChungTu && <span className="text-xs text-gray-400 ml-2">({h.maChungTu})</span>}
                    {h.ghiChu && <p className="text-xs text-gray-400 mt-1">{h.ghiChu}</p>}
                  </div>
                  <div className="text-right text-xs text-gray-400">
                    <div>Tồn sau: <strong className="text-gray-700">{h.tonCuoi}</strong></div>
                    <div>{formatDateTime(h.ngayGiaoDich)}</div>
                  </div>
                </div>
              ))}
            </div>
          </div>
        </div>
      </div>
    </div>
  );
}

export default function ProductListPage() {
  const { user } = useAuthStore();
  // Theo backend: chỉ Manager được thêm/sửa/khóa sản phẩm + điều chỉnh kho
  const canManage = user?.role === ROLES.Manager;

  const [items, setItems] = useState([]);
  const [types, setTypes] = useState([]);
  const [form, setForm] = useState(emptyForm);
  const [editingId, setEditingId] = useState(null);
  const [stockModalProduct, setStockModalProduct] = useState(null);
  const [loading, setLoading] = useState(false);
  const [submitting, setSubmitting] = useState(false);
  const [error, setError] = useState("");
  const [success, setSuccess] = useState("");

  const [search, setSearch] = useState("");
  const [filterType, setFilterType] = useState("");
  const [filterActive, setFilterActive] = useState("");
  const [pageNumber, setPageNumber] = useState(1);
  const [totalPages, setTotalPages] = useState(1);
  const [totalCount, setTotalCount] = useState(0);
  const pageSize = 10;

  const loadProducts = async () => {
    setLoading(true); setError("");
    try {
      const res = await productApi.getAll({
        pageNumber, pageSize, search: search.trim() || undefined,
        loaiSanPhamId: filterType || undefined,
        dangKinhDoanh: filterActive === "" ? undefined : filterActive === "true",
      });
      setItems(res.data?.items ?? []);
      setTotalPages(res.data?.totalPages ?? 1);
      setTotalCount(res.data?.totalCount ?? 0);
    } catch (err) { setError(err?.message || "Tải danh sách thất bại"); }
    finally { setLoading(false); }
  };

  const loadTypes = async () => {
    try { const res = await productApi.getTypes(); setTypes(res.data ?? []); } catch {}
  };

  useEffect(() => { loadProducts(); }, [pageNumber, filterType, filterActive]);
  useEffect(() => { loadTypes(); }, []);

  const resetForm = () => { setForm(emptyForm); setEditingId(null); setError(""); setSuccess(""); };

  const handleSubmit = async (e) => {
    e.preventDefault();
    if (!form.maSP.trim() || !form.tenSP.trim()) { setError("Mã và tên sản phẩm là bắt buộc"); return; }
    setSubmitting(true); setError("");
    try {
      if (editingId) {
        await productApi.update(editingId, {
          loaiSanPhamId: form.loaiSanPhamId ? Number(form.loaiSanPhamId) : null,
          tenSP: form.tenSP.trim(),
          donVi: form.donVi.trim() || null,
          giaBan: Number(form.giaBan) || 0,
          dangKinhDoanh: true,
        });
        setSuccess("Cập nhật sản phẩm thành công");
      } else {
        await productApi.create({
          loaiSanPhamId: form.loaiSanPhamId ? Number(form.loaiSanPhamId) : null,
          maSP: form.maSP.trim(),
          tenSP: form.tenSP.trim(),
          donVi: form.donVi.trim() || null,
          giaBan: Number(form.giaBan) || 0,
          soLuongTonBanDau: Number(form.soLuongTonBanDau) || 0,
        });
        setSuccess("Thêm sản phẩm thành công");
      }
      await loadProducts(); resetForm();
    } catch (err) { setError(err?.message || "Không thể lưu sản phẩm"); }
    finally { setSubmitting(false); }
  };

  const handleEdit = (item) => {
    setEditingId(item.id);
    setForm({
      loaiSanPhamId: item.loaiSanPhamId ?? "", maSP: item.maSP, tenSP: item.tenSP,
      donVi: item.donVi ?? "", giaBan: item.giaBan, soLuongTonBanDau: "0",
    });
    window.scrollTo({ top: 0, behavior: "smooth" });
  };

  const handleDeactivate = async (id) => {
    if (!window.confirm("Khóa kinh doanh sản phẩm này? Sản phẩm sẽ không xuất hiện khi lập báo giá mới.")) return;
    try { await productApi.delete(id); setSuccess("Đã khóa kinh doanh sản phẩm"); await loadProducts(); }
    catch (err) { setError(err?.message || "Không thể khóa sản phẩm"); }
  };

  return (
    <div className="space-y-6">
      {stockModalProduct && (
        <StockModal product={stockModalProduct} canManage={canManage}
          onClose={() => setStockModalProduct(null)}
          onUpdated={loadProducts} />
      )}

      <div className="flex flex-col sm:flex-row sm:items-center sm:justify-between gap-4">
        <div>
          <p className="text-xs text-gray-500 uppercase tracking-wide">CRM / Sản phẩm</p>
          <h1 className="text-2xl font-bold text-gray-800">Quản lý Sản phẩm / Dịch vụ</h1>
        </div>
        <div className="flex flex-wrap gap-2">
          <input type="search" placeholder="Tìm theo mã, tên..."
            value={search} onChange={(e) => setSearch(e.target.value)}
            onKeyDown={(e) => { if (e.key === "Enter") { setPageNumber(1); loadProducts(); } }}
            className="border rounded-lg px-3 py-2 text-sm w-56" />
          <select value={filterType} onChange={(e) => { setFilterType(e.target.value); setPageNumber(1); }}
            className="border rounded-lg px-3 py-2 text-sm">
            <option value="">Tất cả loại</option>
            {types.map(t => <option key={t.id} value={t.id}>{t.tenLoai}</option>)}
          </select>
          <select value={filterActive} onChange={(e) => { setFilterActive(e.target.value); setPageNumber(1); }}
            className="border rounded-lg px-3 py-2 text-sm">
            <option value="">Tất cả trạng thái</option>
            <option value="true">Đang kinh doanh</option>
            <option value="false">Ngừng kinh doanh</option>
          </select>
        </div>
      </div>

      <div className={`grid grid-cols-1 ${canManage ? "lg:grid-cols-3" : ""} gap-6`}>
        {canManage && (
        <form onSubmit={handleSubmit} className="bg-white rounded-xl border shadow-sm p-6 space-y-4 lg:col-span-1">
          <div className="flex items-center justify-between">
            <h2 className="font-semibold text-gray-800">{editingId ? "Cập nhật sản phẩm" : "Thêm sản phẩm mới"}</h2>
            {editingId && <button type="button" onClick={resetForm} className="text-xs text-gray-500">Hủy</button>}
          </div>

          <div>
            <label className="block text-sm font-medium mb-1">Mã sản phẩm {!editingId && <span className="text-red-500">*</span>}</label>
            <input value={form.maSP} disabled={!!editingId} onChange={(e) => setForm(f => ({ ...f, maSP: e.target.value }))}
              placeholder="VD: SP0001" className="w-full border rounded-lg px-3 py-2 text-sm disabled:bg-gray-100" />
          </div>
          <div>
            <label className="block text-sm font-medium mb-1">Tên sản phẩm <span className="text-red-500">*</span></label>
            <input value={form.tenSP} onChange={(e) => setForm(f => ({ ...f, tenSP: e.target.value }))}
              className="w-full border rounded-lg px-3 py-2 text-sm" />
          </div>
          <div className="grid grid-cols-2 gap-3">
            <div>
              <label className="block text-sm font-medium mb-1">Loại</label>
              <select value={form.loaiSanPhamId} onChange={(e) => setForm(f => ({ ...f, loaiSanPhamId: e.target.value }))}
                className="w-full border rounded-lg px-3 py-2 text-sm">
                <option value="">-- Chọn --</option>
                {types.map(t => <option key={t.id} value={t.id}>{t.tenLoai}</option>)}
              </select>
            </div>
            <div>
              <label className="block text-sm font-medium mb-1">Đơn vị</label>
              <input value={form.donVi} onChange={(e) => setForm(f => ({ ...f, donVi: e.target.value }))}
                placeholder="cái, gói, tháng..." className="w-full border rounded-lg px-3 py-2 text-sm" />
            </div>
          </div>
          <div className="grid grid-cols-2 gap-3">
            <div>
              <label className="block text-sm font-medium mb-1">Giá bán (VNĐ)</label>
              <input type="number" min="0" value={form.giaBan} onChange={(e) => setForm(f => ({ ...f, giaBan: e.target.value }))}
                className="w-full border rounded-lg px-3 py-2 text-sm" />
            </div>
            {!editingId && (
              <div>
                <label className="block text-sm font-medium mb-1">Tồn ban đầu</label>
                <input type="number" min="0" value={form.soLuongTonBanDau}
                  onChange={(e) => setForm(f => ({ ...f, soLuongTonBanDau: e.target.value }))}
                  className="w-full border rounded-lg px-3 py-2 text-sm" />
              </div>
            )}
          </div>

          {error && <div className="text-sm text-red-600 bg-red-50 rounded p-2">{error}</div>}
          {success && <div className="text-sm text-green-700 bg-green-50 rounded p-2">{success}</div>}

          <button type="submit" disabled={submitting}
            className="w-full bg-blue-600 text-white rounded-lg py-2 text-sm font-medium hover:bg-blue-700 disabled:opacity-50">
            {submitting ? "Đang lưu..." : editingId ? "Cập nhật" : "Thêm mới"}
          </button>
        </form>
        )}

        <div className={`bg-white rounded-xl border shadow-sm overflow-hidden ${canManage ? "lg:col-span-2" : ""}`}>
          <div className="px-6 py-4 border-b flex items-center justify-between">
            <div>
              <h2 className="font-semibold text-gray-800">Danh sách sản phẩm</h2>
              <p className="text-xs text-gray-400">Trang {pageNumber}/{totalPages} — {totalCount} sản phẩm</p>
            </div>
            {loading && <span className="text-xs text-gray-400 animate-pulse">Đang tải...</span>}
          </div>
          <div className="overflow-x-auto">
            <table className="w-full text-sm">
              <thead className="bg-gray-50 text-xs text-gray-500 uppercase">
                <tr>
                  <th className="px-4 py-3 text-left">Mã SP</th>
                  <th className="px-4 py-3 text-left">Tên sản phẩm</th>
                  <th className="px-4 py-3 text-left">Loại</th>
                  <th className="px-4 py-3 text-left">Giá bán</th>
                  <th className="px-4 py-3 text-left">Tồn kho</th>
                  <th className="px-4 py-3 text-left">Trạng thái</th>
                  <th className="px-4 py-3 text-left">Hành động</th>
                </tr>
              </thead>
              <tbody className="divide-y divide-gray-100">
                {items.length === 0 ? (
                  <tr><td colSpan="7" className="text-center py-10 text-gray-400">{loading ? "Đang tải..." : "Không có dữ liệu"}</td></tr>
                ) : items.map((item) => (
                  <tr key={item.id} className="hover:bg-gray-50">
                    <td className="px-4 py-3 font-mono text-blue-600 text-xs font-semibold">{item.maSP}</td>
                    <td className="px-4 py-3 font-medium text-gray-900">{item.tenSP}</td>
                    <td className="px-4 py-3 text-gray-600">{item.tenLoai || "—"}</td>
                    <td className="px-4 py-3 text-gray-700">{Number(item.giaBan).toLocaleString("vi-VN")} đ{item.donVi ? `/${item.donVi}` : ""}</td>
                    <td className="px-4 py-3">
                      <span className={item.soLuongTon <= 0 ? "text-red-500 font-semibold" : "text-gray-700"}>
                        {item.soLuongTon}
                      </span>
                    </td>
                    <td className="px-4 py-3">
                      <Badge label={item.dangKinhDoanh ? "Đang kinh doanh" : "Ngừng kinh doanh"}
                        colorClass={item.dangKinhDoanh ? "bg-green-100 text-green-700" : "bg-gray-100 text-gray-500"} />
                    </td>
                    <td className="px-4 py-3">
                      <div className="flex gap-2 flex-wrap">
                        <button onClick={() => setStockModalProduct(item)} className="text-blue-600 hover:underline text-xs font-medium">Kho</button>
                        {canManage && (
                          <>
                            <button onClick={() => handleEdit(item)} className="text-blue-600 hover:underline text-xs font-medium">Sửa</button>
                            {item.dangKinhDoanh && (
                              <button onClick={() => handleDeactivate(item.id)} className="text-red-500 hover:underline text-xs font-medium">Khóa</button>
                            )}
                          </>
                        )}
                      </div>
                    </td>
                  </tr>
                ))}
              </tbody>
            </table>
          </div>
          <div className="px-6 py-4 border-t">
            <Pagination pageNumber={pageNumber} totalPages={totalPages} onPageChange={setPageNumber} />
          </div>
        </div>
      </div>
    </div>
  );
}
