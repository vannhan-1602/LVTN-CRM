import { useEffect, useMemo, useState } from "react";
import quoteApi from "../../api/quoteApi";
import customerApi from "../../api/customerApi";
import productApi from "../../api/productApi";
import useAuthStore from "../auth/authStore";
import Pagination from "../../components/common/Pagination";
import { ROLES, QUOTE_STATUS, QUOTE_STATUS_OPTIONS, QUOTE_STATUS_COLOR } from "../../utils/constants";
import { formatDateTime } from "../../utils/formatters";

function Badge({ label, colorClass }) {
  return <span className={`px-2 py-0.5 rounded-full text-xs font-semibold ${colorClass}`}>{label}</span>;
}

function formatMoney(n) {
  return Number(n || 0).toLocaleString("vi-VN") + " đ";
}

// ── Modal: tạo / sửa báo giá (multi sản phẩm) ───────────────────────────────
function QuoteFormModal({ mode, quote, customers, products, onClose, onSaved }) {
  const isEdit = mode === "edit";
  const [khachHangId, setKhachHangId] = useState(quote?.khachHangId ?? "");
  const [lines, setLines] = useState(
    isEdit && quote?.chiTiet?.length
      ? quote.chiTiet.map(c => ({ sanPhamId: String(c.sanPhamId), soLuong: c.soLuong, donGia: c.donGia }))
      : [{ sanPhamId: "", soLuong: 1, donGia: "" }]
  );
  const [submitting, setSubmitting] = useState(false);
  const [error, setError] = useState("");

  const addLine = () => setLines(l => [...l, { sanPhamId: "", soLuong: 1, donGia: "" }]);
  const removeLine = (idx) => setLines(l => l.filter((_, i) => i !== idx));
  const updateLine = (idx, field, value) => setLines(l => l.map((row, i) => {
    if (i !== idx) return row;
    const next = { ...row, [field]: value };
    if (field === "sanPhamId") {
      const p = products.find(p => String(p.id) === value);
      if (p && !row.donGia) next.donGia = p.giaBan;
    }
    return next;
  }));

  const tongTien = useMemo(() =>
    lines.reduce((sum, l) => sum + (Number(l.soLuong) || 0) * (Number(l.donGia) || 0), 0), [lines]);

  const handleSubmit = async (e) => {
    e.preventDefault();
    if (!isEdit && !khachHangId) { setError("Vui lòng chọn khách hàng"); return; }
    const validLines = lines.filter(l => l.sanPhamId && Number(l.soLuong) > 0);
    if (validLines.length === 0) { setError("Báo giá phải có ít nhất 1 sản phẩm hợp lệ"); return; }

    setSubmitting(true); setError("");
    const chiTiet = validLines.map(l => ({
      sanPhamId: Number(l.sanPhamId), soLuong: Number(l.soLuong),
      donGia: l.donGia === "" ? null : Number(l.donGia),
    }));

    try {
      if (isEdit) {
        await quoteApi.update(quote.id, { chiTiet });
      } else {
        await quoteApi.create({ khachHangId: Number(khachHangId), chiTiet });
      }
      onSaved();
    } catch (err) {
      setError(err?.message || "Không thể lưu báo giá");
    } finally { setSubmitting(false); }
  };

  return (
    <div className="fixed inset-0 z-50 flex items-center justify-center bg-black/40 p-4">
      <form onSubmit={handleSubmit} className="bg-white rounded-xl shadow-xl w-full max-w-2xl max-h-[90vh] overflow-y-auto p-6 space-y-4">
        <h3 className="font-bold text-lg text-gray-800">{isEdit ? `Sửa báo giá ${quote.maBaoGia}` : "Lập báo giá mới"}</h3>

        {!isEdit && (
          <div>
            <label className="block text-sm font-medium mb-1">Khách hàng <span className="text-red-500">*</span></label>
            <select value={khachHangId} onChange={(e) => setKhachHangId(e.target.value)}
              className="w-full border rounded-lg px-3 py-2 text-sm">
              <option value="">-- Chọn khách hàng --</option>
              {customers.map(c => <option key={c.id} value={c.id}>{c.tenKhachHang} ({c.maKhachHang})</option>)}
            </select>
          </div>
        )}

        <div className="space-y-2">
          <div className="flex items-center justify-between">
            <label className="block text-sm font-medium">Danh sách sản phẩm</label>
            <button type="button" onClick={addLine} className="text-xs text-blue-600 font-medium hover:underline">+ Thêm dòng</button>
          </div>
          {lines.map((line, idx) => (
            <div key={idx} className="grid grid-cols-12 gap-2 items-center">
              <select value={line.sanPhamId} onChange={(e) => updateLine(idx, "sanPhamId", e.target.value)}
                className="col-span-5 border rounded-lg px-2 py-2 text-sm">
                <option value="">-- Sản phẩm --</option>
                {products.map(p => <option key={p.id} value={p.id}>{p.tenSP} ({p.maSP})</option>)}
              </select>
              <input type="number" min="1" value={line.soLuong} placeholder="SL"
                onChange={(e) => updateLine(idx, "soLuong", e.target.value)}
                className="col-span-2 border rounded-lg px-2 py-2 text-sm" />
              <input type="number" min="0" value={line.donGia} placeholder="Đơn giá"
                onChange={(e) => updateLine(idx, "donGia", e.target.value)}
                className="col-span-4 border rounded-lg px-2 py-2 text-sm" />
              <button type="button" onClick={() => removeLine(idx)}
                className="col-span-1 text-red-500 hover:text-red-700 text-lg leading-none">×</button>
            </div>
          ))}
        </div>

        <div className="text-right text-sm border-t pt-3">
          Tổng tiền: <span className="font-bold text-lg text-blue-600">{formatMoney(tongTien)}</span>
        </div>

        {error && <div className="text-sm text-red-600 bg-red-50 rounded p-2">{error}</div>}

        <div className="flex gap-2">
          <button type="submit" disabled={submitting}
            className="flex-1 bg-blue-600 text-white rounded-lg py-2 text-sm font-medium hover:bg-blue-700 disabled:opacity-50">
            {submitting ? "Đang lưu..." : isEdit ? "Cập nhật báo giá" : "Tạo báo giá"}
          </button>
          <button type="button" onClick={onClose} className="px-4 border rounded-lg py-2 text-sm hover:bg-gray-50">Hủy</button>
        </div>
      </form>
    </div>
  );
}

// ── Modal: xem chi tiết báo giá + hành động trạng thái ──────────────────────
function QuoteDetailModal({ quoteId, onClose, onChanged }) {
  const [quote, setQuote] = useState(null);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState("");
  const [busy, setBusy] = useState(false);

  const load = async () => {
    setLoading(true);
    try { const res = await quoteApi.getById(quoteId); setQuote(res.data); }
    catch { setError("Không thể tải báo giá"); }
    finally { setLoading(false); }
  };
  useEffect(() => { load(); }, [quoteId]);

  const doAction = async (fn) => {
    setBusy(true); setError("");
    try { await fn(); await load(); onChanged(); }
    catch (err) { setError(err?.message || "Thao tác thất bại"); }
    finally { setBusy(false); }
  };

  return (
    <div className="fixed inset-0 z-50 flex items-center justify-center bg-black/40 p-4">
      <div className="bg-white rounded-xl shadow-xl w-full max-w-xl max-h-[90vh] overflow-y-auto">
        <div className="flex items-center justify-between px-6 py-4 border-b sticky top-0 bg-white">
          <h3 className="font-bold text-lg text-gray-800">Chi tiết báo giá</h3>
          <button onClick={onClose} className="text-gray-400 hover:text-gray-700 text-xl font-bold">×</button>
        </div>
        {loading ? (
          <div className="p-8 text-center text-gray-400">Đang tải...</div>
        ) : quote ? (
          <div className="p-6 space-y-4">
            {error && <div className="text-sm text-red-600 bg-red-50 rounded p-2">{error}</div>}
            <div className="flex items-center justify-between">
              <div>
                <p className="font-mono font-semibold text-blue-600">{quote.maBaoGia}</p>
                <p className="text-sm text-gray-600">{quote.tenKhachHang}</p>
              </div>
              <Badge label={QUOTE_STATUS[quote.trangThai] ?? quote.trangThai} colorClass={QUOTE_STATUS_COLOR[quote.trangThai]} />
            </div>

            <table className="w-full text-sm border-t pt-2">
              <thead className="text-xs text-gray-500 uppercase">
                <tr><th className="text-left py-1">Sản phẩm</th><th className="text-right py-1">SL</th><th className="text-right py-1">Đơn giá</th><th className="text-right py-1">Thành tiền</th></tr>
              </thead>
              <tbody className="divide-y divide-gray-100">
                {quote.chiTiet?.map(c => (
                  <tr key={c.id}>
                    <td className="py-2">{c.tenSP} <span className="text-gray-400">({c.maSP})</span></td>
                    <td className="py-2 text-right">{c.soLuong}</td>
                    <td className="py-2 text-right">{formatMoney(c.donGia)}</td>
                    <td className="py-2 text-right font-medium">{formatMoney(c.thanhTien)}</td>
                  </tr>
                ))}
              </tbody>
            </table>
            <div className="text-right font-bold text-lg border-t pt-2">Tổng: {formatMoney(quote.tongTien)}</div>

            <div className="flex gap-2 flex-wrap border-t pt-4">
              {quote.trangThai === "Nhap" && (
                <button disabled={busy} onClick={() => doAction(() => quoteApi.send(quote.id))}
                  className="bg-blue-600 text-white px-4 py-2 rounded-lg text-sm font-medium hover:bg-blue-700 disabled:opacity-50">Gửi báo giá</button>
              )}
              {quote.trangThai === "DaGui" && (
                <>
                  <button disabled={busy} onClick={() => doAction(() => quoteApi.accept(quote.id))}
                    className="bg-green-600 text-white px-4 py-2 rounded-lg text-sm font-medium hover:bg-green-700 disabled:opacity-50">Khách chấp nhận</button>
                  <button disabled={busy} onClick={() => doAction(() => quoteApi.reject(quote.id, null))}
                    className="bg-red-500 text-white px-4 py-2 rounded-lg text-sm font-medium hover:bg-red-600 disabled:opacity-50">Khách từ chối</button>
                </>
              )}
              {quote.trangThai === "ChapNhan" && (
                <p className="text-sm text-green-700">✓ Đã chấp nhận — có thể tạo hợp đồng từ trang Hợp đồng.</p>
              )}
            </div>
          </div>
        ) : <div className="p-8 text-center text-red-400">Không tìm thấy báo giá</div>}
      </div>
    </div>
  );
}

export default function QuoteListPage() {
  const { user } = useAuthStore();
  const canDelete = user?.role === ROLES.Manager;

  const [items, setItems] = useState([]);
  const [customers, setCustomers] = useState([]);
  const [products, setProducts] = useState([]);
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState("");
  const [success, setSuccess] = useState("");

  const [formModal, setFormModal] = useState(null);
  const [detailId, setDetailId] = useState(null);

  const [search, setSearch] = useState("");
  const [filterStatus, setFilterStatus] = useState("");
  const [pageNumber, setPageNumber] = useState(1);
  const [totalPages, setTotalPages] = useState(1);
  const [totalCount, setTotalCount] = useState(0);
  const pageSize = 10;

  const loadQuotes = async () => {
    setLoading(true); setError("");
    try {
      const res = await quoteApi.getAll({
        pageNumber, pageSize, search: search.trim() || undefined,
        trangThai: filterStatus || undefined,
      });
      setItems(res.data?.items ?? []);
      setTotalPages(res.data?.totalPages ?? 1);
      setTotalCount(res.data?.totalCount ?? 0);
    } catch (err) { setError(err?.message || "Tải danh sách thất bại"); }
    finally { setLoading(false); }
  };

  const loadLookups = async () => {
    try {
      const [cRes, pRes] = await Promise.all([
        customerApi.getAll({ pageSize: 100 }),
        productApi.getAll({ pageSize: 200, dangKinhDoanh: true }),
      ]);
      setCustomers(cRes.data?.items ?? []);
      setProducts(pRes.data?.items ?? []);
    } catch {}
  };

  useEffect(() => { loadQuotes(); }, [pageNumber, filterStatus]);
  useEffect(() => { loadLookups(); }, []);

  const handleDelete = async (id) => {
    if (!window.confirm("Xóa báo giá nháp này?")) return;
    try { await quoteApi.delete(id); setSuccess("Xóa báo giá thành công"); await loadQuotes(); }
    catch (err) { setError(err?.message || "Không thể xóa"); }
  };

  return (
    <div className="space-y-6">
      {formModal && (
        <QuoteFormModal mode={formModal.mode} quote={formModal.quote} customers={customers} products={products}
          onClose={() => setFormModal(null)}
          onSaved={() => { setFormModal(null); setSuccess("Lưu báo giá thành công"); loadQuotes(); }} />
      )}
      {detailId && (
        <QuoteDetailModal quoteId={detailId} onClose={() => setDetailId(null)} onChanged={loadQuotes} />
      )}

      <div className="flex flex-col sm:flex-row sm:items-center sm:justify-between gap-4">
        <div>
          <p className="text-xs text-gray-500 uppercase tracking-wide">CRM / Kinh doanh</p>
          <h1 className="text-2xl font-bold text-gray-800">Quản lý Báo giá</h1>
        </div>
        <div className="flex flex-wrap gap-2">
          <input type="search" placeholder="Tìm theo mã báo giá..."
            value={search} onChange={(e) => setSearch(e.target.value)}
            onKeyDown={(e) => { if (e.key === "Enter") { setPageNumber(1); loadQuotes(); } }}
            className="border rounded-lg px-3 py-2 text-sm w-56" />
          <select value={filterStatus} onChange={(e) => { setFilterStatus(e.target.value); setPageNumber(1); }}
            className="border rounded-lg px-3 py-2 text-sm">
            <option value="">Tất cả trạng thái</option>
            {QUOTE_STATUS_OPTIONS.map(o => <option key={o.value} value={o.value}>{o.label}</option>)}
          </select>
          <button onClick={() => setFormModal({ mode: "create" })}
            className="bg-blue-600 text-white px-4 py-2 rounded-lg text-sm font-medium hover:bg-blue-700">+ Lập báo giá</button>
        </div>
      </div>

      {error && <div className="text-sm text-red-600 bg-red-50 rounded-lg p-3">{error}</div>}
      {success && <div className="text-sm text-green-700 bg-green-50 rounded-lg p-3">{success}</div>}

      <div className="bg-white rounded-xl border shadow-sm overflow-hidden">
        <div className="px-6 py-4 border-b flex items-center justify-between">
          <div>
            <h2 className="font-semibold text-gray-800">Danh sách báo giá</h2>
            <p className="text-xs text-gray-400">Trang {pageNumber}/{totalPages} — {totalCount} báo giá</p>
          </div>
          {loading && <span className="text-xs text-gray-400 animate-pulse">Đang tải...</span>}
        </div>
        <div className="overflow-x-auto">
          <table className="w-full text-sm">
            <thead className="bg-gray-50 text-xs text-gray-500 uppercase">
              <tr>
                <th className="px-4 py-3 text-left">Mã báo giá</th>
                <th className="px-4 py-3 text-left">Khách hàng</th>
                <th className="px-4 py-3 text-left">Tổng tiền</th>
                <th className="px-4 py-3 text-left">Trạng thái</th>
                <th className="px-4 py-3 text-left">Ngày tạo</th>
                <th className="px-4 py-3 text-left">Hành động</th>
              </tr>
            </thead>
            <tbody className="divide-y divide-gray-100">
              {items.length === 0 ? (
                <tr><td colSpan="6" className="text-center py-10 text-gray-400">{loading ? "Đang tải..." : "Không có dữ liệu"}</td></tr>
              ) : items.map((item) => (
                <tr key={item.id} className="hover:bg-gray-50">
                  <td className="px-4 py-3 font-mono text-blue-600 text-xs font-semibold">{item.maBaoGia}</td>
                  <td className="px-4 py-3 font-medium text-gray-900">{item.tenKhachHang}</td>
                  <td className="px-4 py-3 text-gray-700">{formatMoney(item.tongTien)}</td>
                  <td className="px-4 py-3"><Badge label={QUOTE_STATUS[item.trangThai] ?? item.trangThai} colorClass={QUOTE_STATUS_COLOR[item.trangThai]} /></td>
                  <td className="px-4 py-3 text-xs text-gray-400">{formatDateTime(item.createdAt)}</td>
                  <td className="px-4 py-3">
                    <div className="flex gap-2 flex-wrap">
                      <button onClick={() => setDetailId(item.id)} className="text-blue-600 hover:underline text-xs font-medium">Xem</button>
                      {item.trangThai === "Nhap" && (
                        <>
                          <button onClick={() => setFormModal({ mode: "edit", quote: item })} className="text-blue-600 hover:underline text-xs font-medium">Sửa</button>
                          {canDelete && <button onClick={() => handleDelete(item.id)} className="text-red-500 hover:underline text-xs font-medium">Xóa</button>}
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
  );
}
