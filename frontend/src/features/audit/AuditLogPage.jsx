import { useEffect, useState } from "react";
import { ScrollText, Search, Eye, X } from "lucide-react";
import auditLogApi from "../../api/auditLogApi";
import Pagination from "../../components/common/Pagination";
import PageHeader from "../../components/common/PageHeader";
import EmptyState from "../../components/common/EmptyState";
import Badge from "../../components/common/Badge";
import Modal from "../../components/common/Modal";
import { formatDateTime } from "../../utils/formatters";

const TABLE_LABEL = {
  KH_KhachHang: "Khách hàng",
  KH_Lead: "Lead",
  BH_CoHoiBanHang: "Cơ hội bán hàng",
  BH_SanPham: "Sản phẩm",
  HD_BaoGia: "Báo giá",
  HD_HopDong: "Hợp đồng",
  KT_HoaDon: "Hóa đơn",
  KT_PhieuThuChi: "Phiếu thu/chi",
  TK_Ticket: "Ticket",
  TK_Ticket_PhanHoi: "Phản hồi Ticket",
  Kho_TheKho: "Thẻ kho",
  HT_User: "Tài khoản người dùng",
};

const ACTION_BADGE = {
  INSERT: { label: "Tạo mới", tone: "success" },
  UPDATE: { label: "Cập nhật", tone: "warning" },
  DELETE: { label: "Xóa", tone: "danger" },
  RESTORE: { label: "Khôi phục", tone: "info" },
};

function JsonPreview({ label, json }) {
  if (!json) return null;
  let pretty = json;
  try {
    pretty = JSON.stringify(JSON.parse(json), null, 2);
  } catch {
    /* giữ nguyên chuỗi gốc nếu không parse được */
  }
  return (
    <div>
      <p className="text-xs font-medium text-ink-500 mb-1">{label}</p>
      <pre className="bg-surface-alt rounded-lg p-3 text-xs text-ink-700 overflow-x-auto whitespace-pre-wrap break-all max-h-80">
        {pretty}
      </pre>
    </div>
  );
}

export default function AuditLogPage() {
  const [items, setItems] = useState([]);
  const [tableNames, setTableNames] = useState([]);
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState("");
  const [detail, setDetail] = useState(null);

  const [filterTable, setFilterTable] = useState("");
  const [filterAction, setFilterAction] = useState("");
  const [filterRecordId, setFilterRecordId] = useState("");
  const [filterTuNgay, setFilterTuNgay] = useState("");
  const [filterDenNgay, setFilterDenNgay] = useState("");
  const [pageNumber, setPageNumber] = useState(1);
  const [totalPages, setTotalPages] = useState(1);
  const [totalCount, setTotalCount] = useState(0);
  const pageSize = 20;

  const loadTableNames = async () => {
    try {
      const res = await auditLogApi.getTableNames();
      setTableNames(res.data ?? []);
    } catch {
      /* không chặn trang nếu lỗi tải danh sách bảng */
    }
  };

  const loadLogs = async () => {
    setLoading(true);
    setError("");
    try {
      const res = await auditLogApi.getAll({
        pageNumber,
        pageSize,
        tableName: filterTable || undefined,
        action: filterAction || undefined,
        recordId: filterRecordId || undefined,
        tuNgay: filterTuNgay ? `${filterTuNgay}T00:00:00` : undefined,
        denNgay: filterDenNgay ? `${filterDenNgay}T23:59:59` : undefined,
      });
      setItems(res.data?.items ?? []);
      setTotalPages(res.data?.totalPages ?? 1);
      setTotalCount(res.data?.totalCount ?? 0);
    } catch (err) {
      setError(err?.message || "Tải nhật ký thất bại");
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => {
    loadTableNames();
  }, []);

  useEffect(() => {
    loadLogs();
  }, [pageNumber, filterTable, filterAction, filterRecordId, filterTuNgay, filterDenNgay]);

  return (
    <div className="space-y-5">
      {detail && (
        <Modal isOpen onClose={() => setDetail(null)} title="Chi tiết nhật ký" size="lg">
          <div className="space-y-4">
            <div className="grid grid-cols-2 gap-3 text-sm">
              <div>
                <p className="text-xs text-ink-400">Bảng</p>
                <p className="font-medium text-ink-900">
                  {TABLE_LABEL[detail.tableName] ?? detail.tableName}
                </p>
              </div>
              <div>
                <p className="text-xs text-ink-400">ID bản ghi</p>
                <p className="font-medium text-ink-900">{detail.recordId}</p>
              </div>
              <div>
                <p className="text-xs text-ink-400">Người thực hiện</p>
                <p className="font-medium text-ink-900">
                  {detail.tenNguoiThucHien || detail.usernameNguoiThucHien || "—"}
                </p>
              </div>
              <div>
                <p className="text-xs text-ink-400">Thời điểm</p>
                <p className="font-medium text-ink-900">{formatDateTime(detail.changedAt)}</p>
              </div>
            </div>
            <JsonPreview label="Dữ liệu trước (OldData)" json={detail.oldData} />
            <JsonPreview label="Dữ liệu sau (NewData)" json={detail.newData} />
          </div>
        </Modal>
      )}

      <PageHeader
        breadcrumb="CRM / Quản trị hệ thống"
        title="Nhật ký hệ thống (Audit Log)"
      />

      {error && <div className="text-sm text-danger-600 bg-danger-50 rounded-lg p-3">{error}</div>}

      <div className="bg-surface rounded-card border border-ink-100 overflow-hidden">
        <div className="px-5 py-3.5 border-b border-ink-100 flex items-center gap-3 flex-wrap">
          <div className="relative">
            <Search size={15} className="absolute left-3 top-1/2 -translate-y-1/2 text-ink-400" />
            <input
              type="number"
              placeholder="Tìm theo ID bản ghi..."
              value={filterRecordId}
              onChange={(e) => { setFilterRecordId(e.target.value); setPageNumber(1); }}
              className="border border-ink-200 rounded-lg pl-9 pr-3 py-2 text-sm w-48 focus:outline-none focus:ring-2 focus:ring-accent-400/40 focus:border-accent-400"
            />
          </div>
          <select
            value={filterTable}
            onChange={(e) => { setFilterTable(e.target.value); setPageNumber(1); }}
            className="border border-ink-200 rounded-lg px-3 py-2 text-sm focus:outline-none focus:ring-2 focus:ring-accent-400/40 focus:border-accent-400"
          >
            <option value="">Tất cả bảng dữ liệu</option>
            {tableNames.map((t) => (
              <option key={t} value={t}>{TABLE_LABEL[t] ?? t}</option>
            ))}
          </select>
          <select
            value={filterAction}
            onChange={(e) => { setFilterAction(e.target.value); setPageNumber(1); }}
            className="border border-ink-200 rounded-lg px-3 py-2 text-sm focus:outline-none focus:ring-2 focus:ring-accent-400/40 focus:border-accent-400"
          >
            <option value="">Tất cả hành động</option>
            <option value="INSERT">Tạo mới</option>
            <option value="UPDATE">Cập nhật</option>
            <option value="DELETE">Xóa</option>
            <option value="RESTORE">Khôi phục</option>
          </select>
          <input
            type="date"
            value={filterTuNgay}
            onChange={(e) => { setFilterTuNgay(e.target.value); setPageNumber(1); }}
            className="border border-ink-200 rounded-lg px-3 py-2 text-sm focus:outline-none focus:ring-2 focus:ring-accent-400/40 focus:border-accent-400"
          />
          <span className="text-ink-400 text-sm">—</span>
          <input
            type="date"
            value={filterDenNgay}
            onChange={(e) => { setFilterDenNgay(e.target.value); setPageNumber(1); }}
            className="border border-ink-200 rounded-lg px-3 py-2 text-sm focus:outline-none focus:ring-2 focus:ring-accent-400/40 focus:border-accent-400"
          />
          {(filterTable || filterAction || filterRecordId || filterTuNgay || filterDenNgay) && (
            <button
              onClick={() => {
                setFilterTable(""); setFilterAction(""); setFilterRecordId("");
                setFilterTuNgay(""); setFilterDenNgay(""); setPageNumber(1);
              }}
              className="inline-flex items-center gap-1 text-xs text-ink-400 hover:text-danger-600"
            >
              <X size={13} /> Xóa lọc
            </button>
          )}
        </div>

        <div className="overflow-x-auto">
          <table className="w-full text-sm">
            <thead>
              <tr className="bg-surface-alt">
                <th className="px-5 py-3 text-left text-xs font-medium text-ink-400 uppercase tracking-wide">Thời điểm</th>
                <th className="px-5 py-3 text-left text-xs font-medium text-ink-400 uppercase tracking-wide">Bảng</th>
                <th className="px-5 py-3 text-left text-xs font-medium text-ink-400 uppercase tracking-wide">ID bản ghi</th>
                <th className="px-5 py-3 text-left text-xs font-medium text-ink-400 uppercase tracking-wide">Hành động</th>
                <th className="px-5 py-3 text-left text-xs font-medium text-ink-400 uppercase tracking-wide">Người thực hiện</th>
                <th className="w-12"></th>
              </tr>
            </thead>
            <tbody className="divide-y divide-ink-100">
              {items.length === 0 ? (
                <tr>
                  <td colSpan={6}>
                    <EmptyState
                      icon={ScrollText}
                      title={loading ? "Đang tải..." : "Chưa có nhật ký nào phù hợp"}
                    />
                  </td>
                </tr>
              ) : items.map((item) => {
                const badge = ACTION_BADGE[item.action] ?? { label: item.action, tone: "neutral" };
                return (
                  <tr key={item.id} className="hover:bg-surface-alt transition-colors">
                    <td className="px-5 py-3.5 text-ink-700 whitespace-nowrap">
                      {formatDateTime(item.changedAt)}
                    </td>
                    <td className="px-5 py-3.5 text-ink-700">
                      {TABLE_LABEL[item.tableName] ?? item.tableName}
                    </td>
                    <td className="px-5 py-3.5 font-mono text-xs text-ink-500">#{item.recordId}</td>
                    <td className="px-5 py-3.5">
                      <Badge label={badge.label} tone={badge.tone} />
                    </td>
                    <td className="px-5 py-3.5 text-ink-700">
                      {item.tenNguoiThucHien || item.usernameNguoiThucHien || (
                        <span className="text-ink-400">Hệ thống</span>
                      )}
                    </td>
                    <td className="px-3 py-3.5 text-center">
                      <button
                        onClick={() => setDetail(item)}
                        title="Xem chi tiết"
                        className="p-1.5 rounded-lg text-ink-400 hover:text-accent-600 hover:bg-accent-50"
                      >
                        <Eye size={15} />
                      </button>
                    </td>
                  </tr>
                );
              })}
            </tbody>
          </table>
        </div>

        <div className="px-5 py-3.5 border-t border-ink-100 flex items-center justify-between">
          <p className="text-xs text-ink-400">{totalCount} bản ghi nhật ký</p>
          <Pagination pageNumber={pageNumber} totalPages={totalPages} onPageChange={setPageNumber} />
        </div>
      </div>
    </div>
  );
}
