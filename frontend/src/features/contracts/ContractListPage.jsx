import { useEffect, useMemo, useState } from "react";
import contractApi from "../../api/contractApi";
import quoteApi from "../../api/quoteApi";
import useAuthStore from "../auth/authStore";
import Pagination from "../../components/common/Pagination";
import { ROLES, CONTRACT_STATUS, CONTRACT_STATUS_OPTIONS, CONTRACT_STATUS_COLOR } from "../../utils/constants";
import { formatDate, formatDateTime } from "../../utils/formatters";

function Badge({ label, colorClass }) {
  return <span className={`px-2 py-0.5 rounded-full text-xs font-semibold ${colorClass}`}>{label}</span>;
}

function formatMoney(n) {
  return n == null ? "—" : Number(n).toLocaleString("vi-VN") + " đ";
}

// ── Modal: tạo hợp đồng từ báo giá đã chấp nhận ─────────────────────────────
function CreateContractModal({ onClose, onSaved }) {
  const [acceptedQuotes, setAcceptedQuotes] = useState([]);
  const [baoGiaId, setBaoGiaId] = useState("");
  const [ngayKy, setNgayKy] = useState(new Date().toISOString().slice(0, 10));
  const [thoiHan, setThoiHan] = useState("12");
  const [loading, setLoading] = useState(true);
  const [submitting, setSubmitting] = useState(false);
  const [error, setError] = useState("");

  useEffect(() => {
    (async () => {
      try {
        const res = await quoteApi.getAll({ pageSize: 100, trangThai: "ChapNhan" });
        setAcceptedQuotes(res.data?.items ?? []);
      } catch { setError("Không thể tải danh sách báo giá đã chấp nhận"); }
      finally { setLoading(false); }
    })();
  }, []);

  const handleSubmit = async (e) => {
    e.preventDefault();
    if (!baoGiaId) { setError("Vui lòng chọn báo giá"); return; }
    setSubmitting(true); setError("");
    try {
      await contractApi.createFromQuote({
        baoGiaId: Number(baoGiaId),
        ngayKy: ngayKy || null,
        thoiHan: thoiHan ? Number(thoiHan) : null,
      });
      onSaved();
    } catch (err) {
      setError(err?.message || "Không thể tạo hợp đồng");
    } finally { setSubmitting(false); }
  };

  return (
    <div className="fixed inset-0 z-50 flex items-center justify-center bg-black/40 p-4">
      <form onSubmit={handleSubmit} className="bg-white rounded-xl shadow-xl w-full max-w-md p-6 space-y-4">
        <h3 className="font-bold text-lg text-gray-800">Tạo hợp đồng từ báo giá</h3>

        {loading ? (
          <p className="text-sm text-gray-400">Đang tải danh sách báo giá...</p>
        ) : acceptedQuotes.length === 0 ? (
          <p className="text-sm text-gray-500 bg-gray-50 rounded-lg p-3">
            Chưa có báo giá nào ở trạng thái "Đã chấp nhận". Hãy chuyển trạng thái báo giá
            sang Đã chấp nhận ở trang Báo giá trước khi tạo hợp đồng.
          </p>
        ) : (
          <>
            <div>
              <label className="block text-sm font-medium mb-1">Báo giá đã chấp nhận <span className="text-red-500">*</span></label>
              <select value={baoGiaId} onChange={(e) => setBaoGiaId(e.target.value)}
                className="w-full border rounded-lg px-3 py-2 text-sm">
                <option value="">-- Chọn báo giá --</option>
                {acceptedQuotes.map(q => (
                  <option key={q.id} value={q.id}>{q.maBaoGia} — {q.tenKhachHang} — {formatMoney(q.tongTien)}</option>
                ))}
              </select>
            </div>
            <div className="grid grid-cols-2 gap-3">
              <div>
                <label className="block text-sm font-medium mb-1">Ngày ký</label>
                <input type="date" value={ngayKy} onChange={(e) => setNgayKy(e.target.value)}
                  className="w-full border rounded-lg px-3 py-2 text-sm" />
              </div>
              <div>
                <label className="block text-sm font-medium mb-1">Thời hạn (tháng)</label>
                <input type="number" min="1" value={thoiHan} onChange={(e) => setThoiHan(e.target.value)}
                  className="w-full border rounded-lg px-3 py-2 text-sm" />
              </div>
            </div>
          </>
        )}

        {error && <div className="text-sm text-red-600 bg-red-50 rounded p-2">{error}</div>}

        <div className="flex gap-2">
          <button type="submit" disabled={submitting || acceptedQuotes.length === 0}
            className="flex-1 bg-blue-600 text-white rounded-lg py-2 text-sm font-medium hover:bg-blue-700 disabled:opacity-50">
            {submitting ? "Đang tạo..." : "Tạo hợp đồng"}
          </button>
          <button type="button" onClick={onClose} className="px-4 border rounded-lg py-2 text-sm hover:bg-gray-50">Hủy</button>
        </div>
      </form>
    </div>
  );
}

// ── Modal: chi tiết hợp đồng + đổi trạng thái ───────────────────────────────
function ContractDetailModal({ contract, onClose, canManage, onChanged }) {
  const [trangThai, setTrangThai] = useState(contract.trangThai);
  const [submitting, setSubmitting] = useState(false);
  const [error, setError] = useState("");

  const isFinal = contract.trangThai === "ThanhLy";

  const handleUpdateStatus = async () => {
    setSubmitting(true); setError("");
    try {
      await contractApi.updateStatus(contract.id, trangThai);
      onChanged();
      onClose();
    } catch (err) {
      setError(err?.message || "Không thể cập nhật trạng thái");
    } finally { setSubmitting(false); }
  };

  return (
    <div className="fixed inset-0 z-50 flex items-center justify-center bg-black/40 p-4">
      <div className="bg-white rounded-xl shadow-xl w-full max-w-md p-6 space-y-4">
        <div className="flex items-center justify-between">
          <h3 className="font-bold text-lg text-gray-800">{contract.maHopDong}</h3>
          <button onClick={onClose} className="text-gray-400 hover:text-gray-700 text-xl font-bold">×</button>
        </div>

        <div className="space-y-2 text-sm">
          <div><span className="text-gray-500">Khách hàng:</span> <span className="font-medium">{contract.tenKhachHang}</span></div>
          {contract.maBaoGia && (
            <div><span className="text-gray-500">Từ báo giá:</span> <span className="font-mono">{contract.maBaoGia}</span></div>
          )}
          <div><span className="text-gray-500">Giá trị:</span> <span className="font-semibold">{formatMoney(contract.giaTri)}</span></div>
          <div><span className="text-gray-500">Ngày ký:</span> {formatDate(contract.ngayKy)}</div>
          <div><span className="text-gray-500">Thời hạn:</span> {contract.thoiHan ? `${contract.thoiHan} tháng` : "—"}</div>
        </div>

        {canManage && !isFinal ? (
          <div className="border-t pt-4 space-y-3">
            <label className="block text-sm font-medium">Cập nhật trạng thái</label>
            <select value={trangThai} onChange={(e) => setTrangThai(e.target.value)}
              className="w-full border rounded-lg px-3 py-2 text-sm">
              {CONTRACT_STATUS_OPTIONS.map(o => <option key={o.value} value={o.value}>{o.label}</option>)}
            </select>
            {error && <div className="text-sm text-red-600 bg-red-50 rounded p-2">{error}</div>}
            <button onClick={handleUpdateStatus} disabled={submitting || trangThai === contract.trangThai}
              className="w-full bg-blue-600 text-white rounded-lg py-2 text-sm font-medium hover:bg-blue-700 disabled:opacity-50">
              {submitting ? "Đang lưu..." : "Lưu trạng thái"}
            </button>
          </div>
        ) : (
          <div className="border-t pt-4">
            <Badge label={CONTRACT_STATUS[contract.trangThai] ?? contract.trangThai} colorClass={CONTRACT_STATUS_COLOR[contract.trangThai]} />
            {isFinal && <p className="text-xs text-gray-400 mt-2">Hợp đồng đã thanh lý, không thể thay đổi thêm.</p>}
          </div>
        )}
      </div>
    </div>
  );
}

export default function ContractListPage() {
  const { user } = useAuthStore();
  const canManage = [ROLES.Sale, ROLES.Manager].includes(user?.role);
  const canDelete = user?.role === ROLES.Manager;
  const isReadOnly = user?.role === ROLES.Accountant;

  const [items, setItems] = useState([]);
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState("");
  const [success, setSuccess] = useState("");

  const [showCreateModal, setShowCreateModal] = useState(false);
  const [detailContract, setDetailContract] = useState(null);

  const [search, setSearch] = useState("");
  const [filterStatus, setFilterStatus] = useState("");
  const [pageNumber, setPageNumber] = useState(1);
  const [totalPages, setTotalPages] = useState(1);
  const [totalCount, setTotalCount] = useState(0);
  const pageSize = 10;

  const loadContracts = async () => {
    setLoading(true); setError("");
    try {
      const res = await contractApi.getAll({
        pageNumber, pageSize, search: search.trim() || undefined,
        trangThai: filterStatus || undefined,
      });
      setItems(res.data?.items ?? []);
      setTotalPages(res.data?.totalPages ?? 1);
      setTotalCount(res.data?.totalCount ?? 0);
    } catch (err) { setError(err?.message || "Tải danh sách thất bại"); }
    finally { setLoading(false); }
  };

  useEffect(() => { loadContracts(); }, [pageNumber, filterStatus]);

  const handleDelete = async (id) => {
    if (!window.confirm("Xóa hợp đồng này? Hành động không thể hoàn tác.")) return;
    try { await contractApi.delete(id); setSuccess("Xóa hợp đồng thành công"); await loadContracts(); }
    catch (err) { setError(err?.message || "Không thể xóa"); }
  };

  return (
    <div className="space-y-6">
      {showCreateModal && (
        <CreateContractModal onClose={() => setShowCreateModal(false)}
          onSaved={() => { setShowCreateModal(false); setSuccess("Tạo hợp đồng thành công"); loadContracts(); }} />
      )}
      {detailContract && (
        <ContractDetailModal contract={detailContract} canManage={canManage}
          onClose={() => setDetailContract(null)} onChanged={loadContracts} />
      )}

      <div className="flex flex-col sm:flex-row sm:items-center sm:justify-between gap-4">
        <div>
          <p className="text-xs text-gray-500 uppercase tracking-wide">CRM / Kinh doanh</p>
          <h1 className="text-2xl font-bold text-gray-800">Quản lý Hợp đồng</h1>
        </div>
        <div className="flex flex-wrap gap-2">
          <input type="search" placeholder="Tìm theo mã hợp đồng..."
            value={search} onChange={(e) => setSearch(e.target.value)}
            onKeyDown={(e) => { if (e.key === "Enter") { setPageNumber(1); loadContracts(); } }}
            className="border rounded-lg px-3 py-2 text-sm w-56" />
          <select value={filterStatus} onChange={(e) => { setFilterStatus(e.target.value); setPageNumber(1); }}
            className="border rounded-lg px-3 py-2 text-sm">
            <option value="">Tất cả trạng thái</option>
            {CONTRACT_STATUS_OPTIONS.map(o => <option key={o.value} value={o.value}>{o.label}</option>)}
          </select>
          {canManage && (
            <button onClick={() => setShowCreateModal(true)}
              className="bg-blue-600 text-white px-4 py-2 rounded-lg text-sm font-medium hover:bg-blue-700">
              + Tạo từ báo giá
            </button>
          )}
        </div>
      </div>

      {isReadOnly && (
        <div className="text-xs text-gray-500 bg-blue-50 border border-blue-100 rounded-lg p-3">
          Tài khoản Kế toán chỉ có quyền xem danh sách hợp đồng (không thể tạo/sửa/xóa).
        </div>
      )}

      {error && <div className="text-sm text-red-600 bg-red-50 rounded-lg p-3">{error}</div>}
      {success && <div className="text-sm text-green-700 bg-green-50 rounded-lg p-3">{success}</div>}

      <div className="bg-white rounded-xl border shadow-sm overflow-hidden">
        <div className="px-6 py-4 border-b flex items-center justify-between">
          <div>
            <h2 className="font-semibold text-gray-800">Danh sách hợp đồng</h2>
            <p className="text-xs text-gray-400">Trang {pageNumber}/{totalPages} — {totalCount} hợp đồng</p>
          </div>
          {loading && <span className="text-xs text-gray-400 animate-pulse">Đang tải...</span>}
        </div>
        <div className="overflow-x-auto">
          <table className="w-full text-sm">
            <thead className="bg-gray-50 text-xs text-gray-500 uppercase">
              <tr>
                <th className="px-4 py-3 text-left">Mã hợp đồng</th>
                <th className="px-4 py-3 text-left">Khách hàng</th>
                <th className="px-4 py-3 text-left">Giá trị</th>
                <th className="px-4 py-3 text-left">Ngày ký</th>
                <th className="px-4 py-3 text-left">Trạng thái</th>
                {!isReadOnly && <th className="px-4 py-3 text-left">Hành động</th>}
              </tr>
            </thead>
            <tbody className="divide-y divide-gray-100">
              {items.length === 0 ? (
                <tr><td colSpan={isReadOnly ? 5 : 6} className="text-center py-10 text-gray-400">{loading ? "Đang tải..." : "Không có dữ liệu"}</td></tr>
              ) : items.map((item) => (
                <tr key={item.id} className="hover:bg-gray-50 cursor-pointer" onClick={() => setDetailContract(item)}>
                  <td className="px-4 py-3 font-mono text-blue-600 text-xs font-semibold">{item.maHopDong}</td>
                  <td className="px-4 py-3 font-medium text-gray-900">{item.tenKhachHang}</td>
                  <td className="px-4 py-3 text-gray-700">{formatMoney(item.giaTri)}</td>
                  <td className="px-4 py-3 text-xs text-gray-400">{formatDate(item.ngayKy)}</td>
                  <td className="px-4 py-3"><Badge label={CONTRACT_STATUS[item.trangThai] ?? item.trangThai} colorClass={CONTRACT_STATUS_COLOR[item.trangThai]} /></td>
                  {!isReadOnly && (
                    <td className="px-4 py-3" onClick={(e) => e.stopPropagation()}>
                      <div className="flex gap-2">
                        <button onClick={() => setDetailContract(item)} className="text-blue-600 hover:underline text-xs font-medium">Xem</button>
                        {canDelete && <button onClick={() => handleDelete(item.id)} className="text-red-500 hover:underline text-xs font-medium">Xóa</button>}
                      </div>
                    </td>
                  )}
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
