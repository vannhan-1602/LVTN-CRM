import { useEffect, useMemo, useState } from "react";
import { useQuery } from "@tanstack/react-query";
import { useNavigate, useSearchParams } from "react-router-dom";
import {
  Headset,
  Plus,
  Search,
  Eye,
  Trash2,
  AlertCircle,
  CheckCircle2,
} from "lucide-react";
import ticketApi from "../../api/ticketApi";
import customerApi from "../../api/customerApi";
import authApi from "../../api/authApi";
import useAuthStore from "../auth/authStore";
import useDanhMucStore from "../../stores/danhMucStore";
import Pagination from "../../components/common/Pagination";
import PageHeader from "../../components/common/PageHeader";
import StatCard from "../../components/common/StatCard";
import EmptyState from "../../components/common/EmptyState";
import RowMenu from "../../components/common/RowMenu";
import Badge from "../../components/common/Badge";
import Button from "../../components/common/Button";
import CreateTicketModal from "./CreateTicketModal";
import {
  ROLES,
  TICKET_STATUS,
  TICKET_STATUS_OPTIONS,
  TICKET_STATUS_COLOR,
  TICKET_PRIORITY,
  TICKET_PRIORITY_OPTIONS,
  TICKET_PRIORITY_COLOR,
} from "../../utils/constants";
import { formatDateTime } from "../../utils/formatters";

export default function TicketListPage() {
  const { user } = useAuthStore();
  const loadDanhMuc = useDanhMucStore((s) => s.load);
  const navigate = useNavigate();
  const [searchParams, setSearchParams] = useSearchParams();
  const khachHangIdFilter = searchParams.get("khachHangId");
  const canDelete = user?.role === ROLES.Manager;

  // Gọi trực tiếp ở đây (idempotent nhờ cờ loaded/loading trong store) — tránh trường hợp
  // component này mount trước khi danh mục (loaiTicket) load xong, vd: F5 lại đúng trang
  // /tickets, khiến dropdown "Loại ticket" trong CreateTicketModal bị trống.
  useEffect(() => {
    loadDanhMuc();
  }, [loadDanhMuc]);

  const [nhanVienList, setNhanVienList] = useState([]);
  const [customers, setCustomers] = useState([]);
  const [error, setError] = useState("");
  const [success, setSuccess] = useState("");
  const [showCreateModal, setShowCreateModal] = useState(false);

  const [search, setSearch] = useState("");
  const [filterStatus, setFilterStatus] = useState("");
  const [filterPriority, setFilterPriority] = useState("");
  const [pageNumber, setPageNumber] = useState(1);
  const pageSize = 10;

  const {
    data,
    isLoading: loading,
    refetch: loadTickets,
  } = useQuery({
    queryKey: [
      "tickets",
      pageNumber,
      filterStatus,
      filterPriority,
      khachHangIdFilter,
    ],
    queryFn: async () => {
      const res = await ticketApi.getAll({
        pageNumber,
        pageSize,
        search: search.trim() || undefined,
        trangThai: filterStatus || undefined,
        mucDoUuTien: filterPriority || undefined,
        khachHangId: khachHangIdFilter || undefined,
      });
      return res.data ?? {};
    },
    refetchInterval: 1000, // tự tải lại mỗi 1 giây (bắt ticket mới từ voucher)
    refetchOnWindowFocus: true,
  });

  const items = data?.items ?? [];
  const totalPages = data?.totalPages ?? 1;
  const totalCount = data?.totalCount ?? 0;

  const nhanVienMap = useMemo(
    () =>
      new Map(
        nhanVienList.map((nv) => [String(nv.id), nv.hoTen ?? `NV #${nv.id}`]),
      ),
    [nhanVienList],
  );

  const stats = useMemo(
    () => ({
      total: totalCount,
      moi: items.filter((i) => i.trangThai === "Moi").length,
      khanCap: items.filter(
        (i) => i.mucDoUuTien === "KhanCap" && i.trangThai !== "Dong",
      ).length,
    }),
    [items, totalCount],
  );

  useEffect(() => {
    authApi
      .getStaffList()
      .then((res) => setNhanVienList(res.data ?? []))
      .catch(() => {});
    customerApi
      .getAll({ pageSize: 100 })
      .then((res) => setCustomers(res.data?.items ?? []))
      .catch(() => {});
  }, []);

  const handleDelete = async (id) => {
    if (!window.confirm("Xóa ticket này?")) return;
    try {
      await ticketApi.delete(id);
      setSuccess("Xóa ticket thành công");
      await loadTickets();
    } catch (err) {
      setError(err?.message || "Không thể xóa");
    }
  };

  return (
    <div className="space-y-5">
      {showCreateModal && (
        <CreateTicketModal
          customers={customers}
          nhanVienList={nhanVienList}
          onClose={() => setShowCreateModal(false)}
          onSaved={() => {
            setShowCreateModal(false);
            setSuccess("Tạo ticket thành công");
            loadTickets();
          }}
        />
      )}

      <PageHeader
        breadcrumb="CRM / Hỗ trợ"
        title="Quản lý Ticket hỗ trợ"
        actions={
          <Button icon={Plus} onClick={() => setShowCreateModal(true)}>
            Tạo ticket
          </Button>
        }
      />

      {khachHangIdFilter && (
        <div className="bg-info-50 border border-info-100 text-info-700 text-sm rounded-lg px-4 py-2.5 flex items-center justify-between">
          <span>Đang lọc theo khách hàng đã chọn.</span>
          <button
            onClick={() => {
              setSearchParams({});
              setPageNumber(1);
            }}
            className="text-info-700 font-medium hover:underline"
          >
            Bỏ lọc
          </button>
        </div>
      )}

      <div className="grid grid-cols-1 sm:grid-cols-3 gap-3">
        <StatCard
          label="Tổng ticket (trang này)"
          value={stats.total}
          icon={Headset}
        />
        <StatCard
          label="Ticket mới"
          value={stats.moi}
          tone="info"
          icon={CheckCircle2}
        />
        <StatCard
          label="Khẩn cấp (chưa đóng)"
          value={stats.khanCap}
          tone="warning"
          icon={AlertCircle}
        />
      </div>

      {error && (
        <div className="text-sm text-danger-600 bg-danger-50 rounded-lg p-3">
          {error}
        </div>
      )}
      {success && (
        <div className="text-sm text-success-700 bg-success-50 rounded-lg p-3">
          {success}
        </div>
      )}

      <div className="bg-surface rounded-card border border-ink-100 overflow-hidden">
        <div className="px-5 py-3.5 border-b border-ink-100 flex items-center justify-between gap-3 flex-wrap">
          <div className="relative">
            <Search
              size={15}
              className="absolute left-3 top-1/2 -translate-y-1/2 text-ink-400"
            />
            <input
              type="search"
              placeholder="Tìm theo mã, tiêu đề..."
              value={search}
              onChange={(e) => setSearch(e.target.value)}
              onKeyDown={(e) => {
                if (e.key === "Enter") {
                  setPageNumber(1);
                  loadTickets();
                }
              }}
              className="border border-ink-200 rounded-lg pl-9 pr-3 py-2 text-sm w-56 focus:outline-none focus:ring-2 focus:ring-accent-400/40 focus:border-accent-400"
            />
          </div>
          <div className="flex gap-2">
            <select
              value={filterStatus}
              onChange={(e) => {
                setFilterStatus(e.target.value);
                setPageNumber(1);
              }}
              className="border border-ink-200 rounded-lg px-3 py-2 text-sm focus:outline-none focus:ring-2 focus:ring-accent-400/40 focus:border-accent-400"
            >
              <option value="">Tất cả trạng thái</option>
              {TICKET_STATUS_OPTIONS.map((o) => (
                <option key={o.value} value={o.value}>
                  {o.label}
                </option>
              ))}
            </select>
            <select
              value={filterPriority}
              onChange={(e) => {
                setFilterPriority(e.target.value);
                setPageNumber(1);
              }}
              className="border border-ink-200 rounded-lg px-3 py-2 text-sm focus:outline-none focus:ring-2 focus:ring-accent-400/40 focus:border-accent-400"
            >
              <option value="">Tất cả mức ưu tiên</option>
              {TICKET_PRIORITY_OPTIONS.map((o) => (
                <option key={o.value} value={o.value}>
                  {o.label}
                </option>
              ))}
            </select>
          </div>
        </div>

        <div className="overflow-x-auto">
          <table className="w-full text-sm">
            <thead>
              <tr className="bg-surface-alt">
                <th className="px-5 py-3 text-left text-xs font-medium text-ink-400 uppercase tracking-wide">
                  Mã ticket
                </th>
                <th className="px-5 py-3 text-left text-xs font-medium text-ink-400 uppercase tracking-wide">
                  Tiêu đề
                </th>
                <th className="px-5 py-3 text-left text-xs font-medium text-ink-400 uppercase tracking-wide">
                  Trạng thái
                </th>
                <th className="px-5 py-3 text-left text-xs font-medium text-ink-400 uppercase tracking-wide">
                  Ưu tiên
                </th>
                <th className="px-5 py-3 text-left text-xs font-medium text-ink-400 uppercase tracking-wide">
                  NV xử lý
                </th>
                <th className="px-5 py-3 text-left text-xs font-medium text-ink-400 uppercase tracking-wide">
                  Ngày tạo
                </th>
                <th className="w-12"></th>
              </tr>
            </thead>
            <tbody className="divide-y divide-ink-100">
              {items.length === 0 ? (
                <tr>
                  <td colSpan={7}>
                    <EmptyState
                      icon={Headset}
                      title={loading ? "Đang tải..." : "Chưa có ticket nào"}
                      description={
                        !loading
                          ? "Tạo ticket mới khi khách hàng cần hỗ trợ."
                          : undefined
                      }
                    />
                  </td>
                </tr>
              ) : (
                items.map((item) => (
                  <tr
                    key={item.id}
                    className="hover:bg-surface-alt cursor-pointer transition-colors"
                    onClick={() => navigate(`/tickets/${item.id}`)}
                  >
                    <td className="px-5 py-3.5 font-mono text-info-600 text-xs font-semibold">
                      {item.maTicket}
                    </td>
                    <td className="px-5 py-3.5 max-w-xs">
                      <div className="font-medium text-ink-900 truncate">
                        {item.tieuDe}
                      </div>
                      <div className="text-xs text-ink-400">
                        {item.nguonTiepNhan}
                      </div>
                    </td>
                    <td className="px-5 py-3.5">
                      <Badge
                        label={TICKET_STATUS[item.trangThai] ?? item.trangThai}
                        colorClass={TICKET_STATUS_COLOR[item.trangThai]}
                      />
                    </td>
                    <td className="px-5 py-3.5">
                      <Badge
                        label={
                          TICKET_PRIORITY[item.mucDoUuTien] ?? item.mucDoUuTien
                        }
                        colorClass={TICKET_PRIORITY_COLOR[item.mucDoUuTien]}
                      />
                    </td>
                    <td className="px-5 py-3.5 text-ink-700">
                      {item.nhanVienXuLyId ? (
                        (nhanVienMap.get(String(item.nhanVienXuLyId)) ??
                        `NV #${item.nhanVienXuLyId}`)
                      ) : (
                        <span className="text-ink-400">Chưa gán</span>
                      )}
                    </td>
                    <td className="px-5 py-3.5 text-xs text-ink-400">
                      {formatDateTime(item.createdAt)}
                    </td>
                    <td
                      className="px-3 py-3.5 text-center"
                      onClick={(e) => e.stopPropagation()}
                    >
                      <RowMenu
                        items={[
                          {
                            label: "Xem / Xử lý",
                            icon: Eye,
                            onClick: () => navigate(`/tickets/${item.id}`),
                          },
                          ...(canDelete
                            ? [
                                {
                                  label: "Xóa",
                                  icon: Trash2,
                                  danger: true,
                                  onClick: () => handleDelete(item.id),
                                },
                              ]
                            : []),
                        ]}
                      />
                    </td>
                  </tr>
                ))
              )}
            </tbody>
          </table>
        </div>

        <div className="px-5 py-3.5 border-t border-ink-100 flex items-center justify-between">
          <p className="text-xs text-ink-400">{totalCount} ticket</p>
          <Pagination
            pageNumber={pageNumber}
            totalPages={totalPages}
            onPageChange={setPageNumber}
          />
        </div>
      </div>
    </div>
  );
}
