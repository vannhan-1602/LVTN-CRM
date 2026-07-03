import { useEffect, useState } from "react";
import { useNavigate } from "react-router-dom";
import { Wallet, Plus, Search, Eye, FileText } from "lucide-react";
import invoiceApi from "../../api/invoiceApi";
import useAuthStore from "../auth/authStore";
import Pagination from "../../components/common/Pagination";
import PageHeader from "../../components/common/PageHeader";
import StatCard from "../../components/common/StatCard";
import EmptyState from "../../components/common/EmptyState";
import Badge from "../../components/common/Badge";
import Button from "../../components/common/Button";
import RowMenu from "../../components/common/RowMenu";
import CreateInvoiceModal from "./CreateInvoiceModal";
import { ROLES } from "../../utils/constants";
import { formatCurrency, formatDate } from "../../utils/formatters";

const STATUS_LABEL = {
  ChuaThanhToan: "Chưa thanh toán",
  ThanhToan1Phan: "Thanh toán 1 phần",
  HoanTat: "Hoàn tất",
};
const STATUS_COLOR = {
  ChuaThanhToan: "bg-danger-50 text-danger-600",
  ThanhToan1Phan: "bg-warning-50 text-warning-700",
  HoanTat: "bg-success-50 text-success-700",
};

export default function InvoiceListPage() {
  const { user } = useAuthStore();
  const navigate = useNavigate();
  const canCreate = [ROLES.Accountant, ROLES.Manager].includes(user?.role);

  const [items, setItems] = useState([]);
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState("");
  const [success, setSuccess] = useState("");
  const [showCreate, setShowCreate] = useState(false);

  const [search, setSearch] = useState("");
  const [filterStatus, setFilterStatus] = useState("");
  const [pageNumber, setPageNumber] = useState(1);
  const [totalPages, setTotalPages] = useState(1);
  const [totalCount, setTotalCount] = useState(0);
  const pageSize = 10;

  const load = async () => {
    setLoading(true);
    setError("");
    try {
      const res = await invoiceApi.getAll({
        pageNumber, pageSize,
        search: search.trim() || undefined,
        trangThaiThanhToan: filterStatus || undefined,
      });
      setItems(res.data?.items ?? []);
      setTotalPages(res.data?.totalPages ?? 1);
      setTotalCount(res.data?.totalCount ?? 0);
    } catch (err) {
      setError(err?.message || "Tải danh sách thất bại");
    } finally { setLoading(false); }
  };

  useEffect(() => { load(); }, [pageNumber, filterStatus]);

  const chuaThanhToan = items.filter(i => i.trangThaiThanhToan === "ChuaThanhToan").length;
  const tongNo = items.reduce((s, i) => s + (i.soTienConLai ?? 0), 0);

  return (
    <div className="space-y-5">
      {showCreate && (
        <CreateInvoiceModal
          onClose={() => setShowCreate(false)}
          onSaved={() => { setShowCreate(false); setSuccess("Tạo hóa đơn thành công"); load(); }}
        />
      )}

      <PageHeader
        breadcrumb="CRM / Kế toán"
        title="Hóa đơn & Công nợ"
        actions={canCreate && (
          <Button icon={Plus} onClick={() => setShowCreate(true)}>Tạo hóa đơn</Button>
        )}
      />

      <div className="grid grid-cols-1 sm:grid-cols-3 gap-3">
        <StatCard label="Tổng hóa đơn" value={totalCount} icon={FileText} />
        <StatCard label="Chưa thanh toán" value={chuaThanhToan} tone="danger" icon={Wallet} />
        <StatCard label="Tổng công nợ" value={formatCurrency(tongNo)} tone="warning" icon={Wallet} />
      </div>

      {error && <div className="text-sm text-danger-600 bg-danger-50 rounded-lg p-3">{error}</div>}
      {success && <div className="text-sm text-success-700 bg-success-50 rounded-lg p-3">{success}</div>}

      <div className="bg-surface rounded-card border border-ink-100 overflow-hidden">
        <div className="px-5 py-3.5 border-b border-ink-100 flex items-center gap-3 flex-wrap">
          <div className="relative">
            <Search size={15} className="absolute left-3 top-1/2 -translate-y-1/2 text-ink-400" />
            <input
              type="search" placeholder="Tìm mã hóa đơn, tên khách..."
              value={search} onChange={e => setSearch(e.target.value)}
              onKeyDown={e => { if (e.key === "Enter") { setPageNumber(1); load(); } }}
              className="border border-ink-200 rounded-lg pl-9 pr-3 py-2 text-sm w-64 focus:outline-none focus:ring-2 focus:ring-accent-400/40 focus:border-accent-400"
            />
          </div>
          <select value={filterStatus} onChange={e => { setFilterStatus(e.target.value); setPageNumber(1); }}
            className="border border-ink-200 rounded-lg px-3 py-2 text-sm focus:outline-none focus:ring-2 focus:ring-accent-400/40 focus:border-accent-400">
            <option value="">Tất cả trạng thái</option>
            {Object.entries(STATUS_LABEL).map(([v, l]) => <option key={v} value={v}>{l}</option>)}
          </select>
        </div>

        <div className="overflow-x-auto">
          <table className="w-full text-sm">
            <thead>
              <tr className="bg-surface-alt">
                {["Mã HĐ", "Khách hàng", "Hợp đồng", "Tổng tiền", "Đã thu", "Còn lại", "Trạng thái", "Ngày tạo", ""].map(h => (
                  <th key={h} className="px-5 py-3 text-left text-xs font-medium text-ink-400 uppercase tracking-wide">{h}</th>
                ))}
              </tr>
            </thead>
            <tbody className="divide-y divide-ink-100">
              {items.length === 0 ? (
                <tr><td colSpan={9}>
                  <EmptyState icon={FileText}
                    title={loading ? "Đang tải..." : "Chưa có hóa đơn nào"}
                    description={!loading ? "Tạo hóa đơn đầu tiên để bắt đầu quản lý công nợ." : undefined}
                  />
                </td></tr>
              ) : items.map(item => (
                <tr key={item.id} className="hover:bg-surface-alt cursor-pointer transition-colors"
                  onClick={() => navigate(`/invoices/${item.id}`)}>
                  <td className="px-5 py-3.5 font-mono text-info-600 text-xs font-semibold">{item.maHoaDon}</td>
                  <td className="px-5 py-3.5">
                    <div className="font-medium text-ink-900">{item.tenKhachHang || "—"}</div>
                  </td>
                  <td className="px-5 py-3.5 text-ink-500 text-xs">{item.maHopDong || "—"}</td>
                  <td className="px-5 py-3.5 font-medium text-ink-900">{formatCurrency(item.tongTien)}</td>
                  <td className="px-5 py-3.5 text-success-600">{formatCurrency(item.soTienDaThu)}</td>
                  <td className="px-5 py-3.5 text-danger-600 font-medium">{formatCurrency(item.soTienConLai)}</td>
                  <td className="px-5 py-3.5">
                    <Badge
                      label={STATUS_LABEL[item.trangThaiThanhToan] ?? item.trangThaiThanhToan}
                      colorClass={STATUS_COLOR[item.trangThaiThanhToan] ?? "bg-ink-100 text-ink-500"}
                    />
                  </td>
                  <td className="px-5 py-3.5 text-ink-500 text-xs">{formatDate(item.createdAt)}</td>
                  <td className="px-3 py-3.5 text-center" onClick={e => e.stopPropagation()}>
                    <RowMenu items={[
                      { label: "Xem chi tiết", icon: Eye, onClick: () => navigate(`/invoices/${item.id}`) },
                    ]} />
                  </td>
                </tr>
              ))}
            </tbody>
          </table>
        </div>

        <div className="px-5 py-3.5 border-t border-ink-100 flex items-center justify-between">
          <p className="text-xs text-ink-400">{totalCount} hóa đơn</p>
          <Pagination pageNumber={pageNumber} totalPages={totalPages} onPageChange={setPageNumber} />
        </div>
      </div>
    </div>
  );
}
