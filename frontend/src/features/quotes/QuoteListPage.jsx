import { useEffect, useMemo, useState } from "react";
import { useNavigate } from "react-router-dom";
import { Receipt, Plus, Search, Eye, Pencil, Trash2, Wallet, CheckCircle2 } from "lucide-react";
import quoteApi from "../../api/quoteApi";
import customerApi from "../../api/customerApi";
import productApi from "../../api/productApi";
import useAuthStore from "../auth/authStore";
import Pagination from "../../components/common/Pagination";
import PageHeader from "../../components/common/PageHeader";
import StatCard from "../../components/common/StatCard";
import EmptyState from "../../components/common/EmptyState";
import RowMenu from "../../components/common/RowMenu";
import Badge from "../../components/common/Badge";
import Button from "../../components/common/Button";
import QuoteFormModal from "./QuoteFormModal";
import { ROLES, QUOTE_STATUS, QUOTE_STATUS_OPTIONS, QUOTE_STATUS_COLOR } from "../../utils/constants";
import { formatDateTime } from "../../utils/formatters";

function formatMoney(n) {
  return Number(n || 0).toLocaleString("vi-VN") + " đ";
}

export default function QuoteListPage() {
  const { user } = useAuthStore();
  const navigate = useNavigate();
  const canDelete = user?.role === ROLES.Manager;
  const canEdit = [ROLES.Sale, ROLES.Manager].includes(user?.role);

  const [items, setItems] = useState([]);
  const [customers, setCustomers] = useState([]);
  const [products, setProducts] = useState([]);
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState("");
  const [success, setSuccess] = useState("");

  const [formModal, setFormModal] = useState(null);

  const [search, setSearch] = useState("");
  const [filterStatus, setFilterStatus] = useState("");
  const [pageNumber, setPageNumber] = useState(1);
  const [totalPages, setTotalPages] = useState(1);
  const [totalCount, setTotalCount] = useState(0);
  const pageSize = 10;

  const stats = useMemo(
    () => ({
      total: totalCount,
      chapNhan: items.filter((i) => i.trangThai === "ChapNhan").length,
      tongTien: items.reduce((sum, i) => sum + (i.tongTien || 0), 0),
    }),
    [items, totalCount],
  );

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
    } catch { /* không có quyền hoặc lỗi tải dữ liệu tham chiếu, bỏ qua */ }
  };

  useEffect(() => { loadQuotes(); }, [pageNumber, filterStatus]);
  useEffect(() => { loadLookups(); }, []);

  const handleDelete = async (id) => {
    if (!window.confirm("Xóa báo giá nháp này?")) return;
    try { await quoteApi.delete(id); setSuccess("Xóa báo giá thành công"); await loadQuotes(); }
    catch (err) { setError(err?.message || "Không thể xóa"); }
  };

  return (
    <div className="space-y-5">
      {formModal && (
        <QuoteFormModal quote={formModal.quote} customers={customers} products={products}
          onClose={() => setFormModal(null)}
          onSaved={() => { setFormModal(null); setSuccess("Lưu báo giá thành công"); loadQuotes(); }} />
      )}

      <PageHeader
        breadcrumb="CRM / Kinh doanh"
        title="Quản lý báo giá"
        actions={canEdit && <Button icon={Plus} onClick={() => setFormModal({})}>Lập báo giá</Button>}
      />

      <div className="grid grid-cols-1 sm:grid-cols-3 gap-3">
        <StatCard label="Tổng báo giá (trang này)" value={stats.total} icon={Receipt} />
        <StatCard label="Đã chấp nhận" value={stats.chapNhan} tone="success" icon={CheckCircle2} />
        <StatCard label="Tổng giá trị (trang này)" value={formatMoney(stats.tongTien)} tone="accent" icon={Wallet} />
      </div>

      {error && <div className="text-sm text-danger-600 bg-danger-50 rounded-lg p-3">{error}</div>}
      {success && <div className="text-sm text-success-700 bg-success-50 rounded-lg p-3">{success}</div>}

      <div className="bg-surface rounded-card border border-ink-100 overflow-hidden">
        <div className="px-5 py-3.5 border-b border-ink-100 flex items-center justify-between gap-3 flex-wrap">
          <div className="relative">
            <Search size={15} className="absolute left-3 top-1/2 -translate-y-1/2 text-ink-400" />
            <input
              type="search" placeholder="Tìm theo mã báo giá..."
              value={search} onChange={(e) => setSearch(e.target.value)}
              onKeyDown={(e) => { if (e.key === "Enter") { setPageNumber(1); loadQuotes(); } }}
              className="border border-ink-200 rounded-lg pl-9 pr-3 py-2 text-sm w-60 focus:outline-none focus:ring-2 focus:ring-accent-400/40 focus:border-accent-400"
            />
          </div>
          <select value={filterStatus} onChange={(e) => { setFilterStatus(e.target.value); setPageNumber(1); }}
            className="border border-ink-200 rounded-lg px-3 py-2 text-sm focus:outline-none focus:ring-2 focus:ring-accent-400/40 focus:border-accent-400">
            <option value="">Tất cả trạng thái</option>
            {QUOTE_STATUS_OPTIONS.map((o) => <option key={o.value} value={o.value}>{o.label}</option>)}
          </select>
        </div>

        <div className="overflow-x-auto">
          <table className="w-full text-sm">
            <thead>
              <tr className="bg-surface-alt">
                <th className="px-5 py-3 text-left text-xs font-medium text-ink-400 uppercase tracking-wide">Mã báo giá</th>
                <th className="px-5 py-3 text-left text-xs font-medium text-ink-400 uppercase tracking-wide">Khách hàng</th>
                <th className="px-5 py-3 text-left text-xs font-medium text-ink-400 uppercase tracking-wide">Tổng tiền</th>
                <th className="px-5 py-3 text-left text-xs font-medium text-ink-400 uppercase tracking-wide">Trạng thái</th>
                <th className="px-5 py-3 text-left text-xs font-medium text-ink-400 uppercase tracking-wide">Ngày tạo</th>
                <th className="w-12"></th>
              </tr>
            </thead>
            <tbody className="divide-y divide-ink-100">
              {items.length === 0 ? (
                <tr>
                  <td colSpan={6}>
                    <EmptyState icon={Receipt} title={loading ? "Đang tải..." : "Chưa có báo giá nào"}
                      description={!loading && canEdit ? "Lập báo giá mới cho khách hàng." : undefined} />
                  </td>
                </tr>
              ) : items.map((item) => (
                <tr key={item.id} className="hover:bg-surface-alt cursor-pointer transition-colors"
                  onClick={() => navigate(`/quotes/${item.id}`)}>
                  <td className="px-5 py-3.5 font-mono text-info-600 text-xs font-semibold">{item.maBaoGia}</td>
                  <td className="px-5 py-3.5 font-medium text-ink-900">{item.tenKhachHang}</td>
                  <td className="px-5 py-3.5 text-ink-700">{formatMoney(item.tongTien)}</td>
                  <td className="px-5 py-3.5">
                    <Badge label={QUOTE_STATUS[item.trangThai] ?? item.trangThai} colorClass={QUOTE_STATUS_COLOR[item.trangThai]} />
                  </td>
                  <td className="px-5 py-3.5 text-xs text-ink-400">{formatDateTime(item.createdAt)}</td>
                  <td className="px-3 py-3.5 text-center" onClick={(e) => e.stopPropagation()}>
                    <RowMenu
                      items={[
                        { label: "Xem chi tiết", icon: Eye, onClick: () => navigate(`/quotes/${item.id}`) },
                        ...(canEdit && item.trangThai === "Nhap"
                          ? [{ label: "Sửa", icon: Pencil, onClick: () => setFormModal({ quote: item }) }]
                          : []),
                        ...(canDelete && item.trangThai === "Nhap"
                          ? [{ label: "Xóa", icon: Trash2, danger: true, onClick: () => handleDelete(item.id) }]
                          : []),
                      ]}
                    />
                  </td>
                </tr>
              ))}
            </tbody>
          </table>
        </div>

        <div className="px-5 py-3.5 border-t border-ink-100 flex items-center justify-between">
          <p className="text-xs text-ink-400">{totalCount} báo giá</p>
          <Pagination pageNumber={pageNumber} totalPages={totalPages} onPageChange={setPageNumber} />
        </div>
      </div>
    </div>
  );
}
