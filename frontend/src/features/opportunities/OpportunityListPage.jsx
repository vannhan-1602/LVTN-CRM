import { useCallback, useEffect, useState } from "react";
import { useNavigate, useSearchParams } from "react-router-dom";
import {
  LayoutGrid,
  List as ListIcon,
  Plus,
  TrendingUp,
  CheckCircle2,
  Wallet,
  Percent,
} from "lucide-react";
import opportunityApi from "../../api/opportunityApi";
import customerApi from "../../api/customerApi";
import leadApi from "../../api/leadApi";
import authApi from "../../api/authApi";
import useAuthStore from "../auth/authStore";
import Pagination from "../../components/common/Pagination";
import PageHeader from "../../components/common/PageHeader";
import StatCard from "../../components/common/StatCard";
import EmptyState from "../../components/common/EmptyState";
import Badge from "../../components/common/Badge";
import Button from "../../components/common/Button";
import OpportunityFormModal from "./OpportunityFormModal";
import ChangeStageModal from "./ChangeStageModal";
import {
  ROLES,
  GIAI_DOAN_LIST,
  GIAI_DOAN_LABEL,
  GIAI_DOAN_COLOR,
  GIAI_DOAN_HEADER_COLOR,
  NEXT_STAGE,
} from "../../utils/constants";

function formatMoney(n) {
  if (!n && n !== 0) return "—";
  return Number(n).toLocaleString("vi-VN") + " đ";
}

function KanbanCard({ item, onEdit, onChangeStage, onNavigate }) {
  const canMoveNext = NEXT_STAGE[item.giaiDoan];
  const isFinal = item.giaiDoan === "ThanhCong" || item.giaiDoan === "ThatBai";

  return (
    <div
      className="bg-surface rounded-lg border border-ink-100 p-3 space-y-2 hover:shadow-sm transition-shadow cursor-pointer"
      onClick={() => onNavigate(item.id)}
    >
      <div className="flex items-start justify-between gap-2">
        <p className="text-sm font-medium text-ink-900 leading-snug">
          {item.tenThuongVu}
        </p>
        <button
          onClick={(e) => {
            e.stopPropagation();
            onEdit(item);
          }}
          className="text-ink-400 hover:text-accent-600 shrink-0 mt-0.5 text-xs"
        >
          ✎
        </button>
      </div>

      {(item.tenKhachHang || item.tenLead) && (
        <p className="text-xs text-ink-400 truncate">
          {item.tenKhachHang || item.tenLead}
        </p>
      )}

      <div className="flex items-center gap-2 text-xs">
        <span className="bg-ink-100 text-ink-600 px-1.5 py-0.5 rounded font-medium">
          {item.tyLeThanhCong}%
        </span>
        {item.doanhThuKyVong && (
          <span className="text-success-700 font-medium">
            {formatMoney(item.doanhThuKyVong)}
          </span>
        )}
      </div>

      {item.ngayDuKien && (
        <p className="text-xs text-ink-400">Dự kiến: {item.ngayDuKien}</p>
      )}

      {!isFinal && (
        <div
          className="flex gap-1.5 pt-1.5 border-t border-ink-100"
          onClick={(e) => e.stopPropagation()}
        >
          <button
            onClick={() => onChangeStage(item, "ThatBai")}
            className="flex-1 text-xs border border-danger-100 text-danger-600 rounded py-1 hover:bg-danger-50"
          >
            Thất bại
          </button>
          {canMoveNext && (
            <button
              onClick={() => onChangeStage(item, canMoveNext)}
              className="flex-1 text-xs border border-info-100 text-info-600 rounded py-1 hover:bg-info-50"
            >
              → {GIAI_DOAN_LABEL[canMoveNext]}
            </button>
          )}
        </div>
      )}
    </div>
  );
}

function KanbanBoard({ items, onEdit, onChangeStage, onNavigate }) {
  const grouped = GIAI_DOAN_LIST.reduce((acc, stage) => {
    acc[stage] = items.filter((i) => i.giaiDoan === stage);
    return acc;
  }, {});

  return (
    <div className="flex gap-3 overflow-x-auto pb-2">
      {GIAI_DOAN_LIST.map((stage) => (
        <div key={stage} className="shrink-0 w-60">
          <div
            className={`${GIAI_DOAN_HEADER_COLOR[stage]} rounded-t-lg px-3 py-2 flex items-center justify-between`}
          >
            <span className="text-white text-sm font-medium">
              {GIAI_DOAN_LABEL[stage]}
            </span>
            <span className="bg-white/25 text-white text-xs rounded-full px-2 py-0.5">
              {grouped[stage].length}
            </span>
          </div>
          <div className="bg-surface-alt rounded-b-lg p-2 space-y-2 min-h-32">
            {grouped[stage].length === 0 ? (
              <p className="text-xs text-ink-400 text-center py-4">Trống</p>
            ) : (
              grouped[stage].map((item) => (
                <KanbanCard
                  key={item.id}
                  item={item}
                  onEdit={onEdit}
                  onChangeStage={onChangeStage}
                  onNavigate={onNavigate}
                />
              ))
            )}
          </div>
        </div>
      ))}
    </div>
  );
}

function ListTable({
  items,
  onEdit,
  onChangeStage,
  onDelete,
  onNavigate,
  canDelete,
}) {
  return (
    <div className="overflow-x-auto">
      <table className="w-full text-sm">
        <thead>
          <tr className="bg-surface-alt">
            <th className="px-5 py-3 text-left text-xs font-medium text-ink-400 uppercase tracking-wide">
              Thương vụ
            </th>
            <th className="px-5 py-3 text-left text-xs font-medium text-ink-400 uppercase tracking-wide">
              Khách hàng / Lead
            </th>
            <th className="px-5 py-3 text-left text-xs font-medium text-ink-400 uppercase tracking-wide">
              Giai đoạn
            </th>
            <th className="px-5 py-3 text-right text-xs font-medium text-ink-400 uppercase tracking-wide">
              Tỷ lệ
            </th>
            <th className="px-5 py-3 text-right text-xs font-medium text-ink-400 uppercase tracking-wide">
              DT kỳ vọng
            </th>
            <th className="px-5 py-3 text-left text-xs font-medium text-ink-400 uppercase tracking-wide">
              NV phụ trách
            </th>
            <th className="px-3 py-3"></th>
          </tr>
        </thead>
        <tbody className="divide-y divide-ink-100">
          {items.map((item) => {
            const isFinal =
              item.giaiDoan === "ThanhCong" || item.giaiDoan === "ThatBai";
            const canMoveNext = NEXT_STAGE[item.giaiDoan];
            return (
              <tr
                key={item.id}
                className="hover:bg-surface-alt cursor-pointer transition-colors"
                onClick={() => onNavigate(item.id)}
              >
                <td className="px-5 py-3.5">
                  <p className="font-medium text-ink-900">{item.tenThuongVu}</p>
                  {item.ghiChu && (
                    <p className="text-xs text-ink-400 truncate max-w-xs">
                      {item.ghiChu}
                    </p>
                  )}
                </td>
                <td className="px-5 py-3.5 text-ink-700">
                  {item.tenKhachHang || item.tenLead || "—"}
                </td>
                <td className="px-5 py-3.5">
                  <Badge
                    label={GIAI_DOAN_LABEL[item.giaiDoan]}
                    colorClass={GIAI_DOAN_COLOR[item.giaiDoan]}
                  />
                </td>
                <td className="px-5 py-3.5 text-right font-medium text-ink-900">
                  {item.tyLeThanhCong}%
                </td>
                <td className="px-5 py-3.5 text-right text-success-700 font-medium">
                  {formatMoney(item.doanhThuKyVong)}
                </td>
                <td className="px-5 py-3.5 text-xs text-ink-400">
                  {item.tenNhanVien ?? "—"}
                </td>
                <td
                  className="px-3 py-3.5"
                  onClick={(e) => e.stopPropagation()}
                >
                  <div className="flex items-center gap-1.5 justify-end">
                    <button
                      onClick={() => onEdit(item)}
                      className="text-xs px-2 py-1 border border-ink-200 rounded hover:bg-ink-100 text-ink-600"
                    >
                      Sửa
                    </button>
                    {!isFinal && canMoveNext && (
                      <button
                        onClick={() => onChangeStage(item, canMoveNext)}
                        className="text-xs px-2 py-1 border border-info-100 rounded hover:bg-info-50 text-info-600"
                      >
                        → {GIAI_DOAN_LABEL[canMoveNext]}
                      </button>
                    )}
                    {!isFinal && (
                      <button
                        onClick={() => onChangeStage(item, "ThatBai")}
                        className="text-xs px-2 py-1 border border-danger-100 rounded hover:bg-danger-50 text-danger-600"
                      >
                        Thất bại
                      </button>
                    )}
                    {canDelete && (
                      <button
                        onClick={() => onDelete(item)}
                        className="text-xs px-2 py-1 border border-danger-100 rounded hover:bg-danger-50 text-danger-600"
                      >
                        Xóa
                      </button>
                    )}
                  </div>
                </td>
              </tr>
            );
          })}
        </tbody>
      </table>
    </div>
  );
}

export default function OpportunityListPage() {
  const { user } = useAuthStore();
  const navigate = useNavigate();
  const [searchParams, setSearchParams] = useSearchParams();
  const khachHangIdFilter = searchParams.get("khachHangId");
  const leadIdFilter = searchParams.get("leadId");
  const canDelete = user?.role === ROLES.Manager;

  const [items, setItems] = useState([]);
  const [summary, setSummary] = useState(null);
  const [customers, setCustomers] = useState([]);
  const [leads, setLeads] = useState([]);
  const [nhanVienList, setNhanVienList] = useState([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState("");

  const [page, setPage] = useState(1);
  const [totalCount, setTotalCount] = useState(0);
  const PAGE_SIZE = 20;
  const [search, setSearch] = useState("");
  const [filterStage, setFilterStage] = useState("");
  const [viewMode, setViewMode] = useState("kanban");

  const [createModal, setCreateModal] = useState(false);
  const [editItem, setEditItem] = useState(null);
  const [stageChange, setStageChange] = useState(null);

  const fetchData = useCallback(async () => {
    setLoading(true);
    setError("");
    try {
      const params = { pageNumber: page, pageSize: PAGE_SIZE };
      if (search.trim()) params.search = search.trim();
      if (filterStage) params.giaiDoan = filterStage;
      if (khachHangIdFilter) params.khachHangId = khachHangIdFilter;
      if (leadIdFilter) params.leadId = leadIdFilter;
      const [res, sum] = await Promise.all([
        opportunityApi.getAll(params),
        opportunityApi.getSummary(),
      ]);
      setItems(res.data?.items ?? []);
      setTotalCount(res.data?.totalCount ?? 0);
      setSummary(sum.data);
    } catch (e) {
      setError(e?.message || "Không tải được dữ liệu");
    } finally {
      setLoading(false);
    }
  }, [page, search, filterStage, khachHangIdFilter, leadIdFilter]);

  useEffect(() => {
    fetchData();
  }, [fetchData]);

  useEffect(() => {
    customerApi
      .getAll({ pageNumber: 1, pageSize: 200 })
      .then((r) => setCustomers(r.data?.items ?? []))
      .catch(() => {});
    leadApi
      .getAll({ pageNumber: 1, pageSize: 200 })
      .then((r) => setLeads(r.data?.items ?? []))
      .catch(() => {});
    authApi
      .getStaffList()
      .then((r) => setNhanVienList(r.data ?? []))
      .catch(() => {});
  }, []);

  const handleDelete = async (item) => {
    if (!window.confirm(`Xóa cơ hội "${item.tenThuongVu}"?`)) return;
    try {
      await opportunityApi.delete(item.id);
      fetchData();
    } catch (e) {
      setError(e?.message || "Không thể xóa");
    }
  };

  const onSaved = () => {
    setCreateModal(false);
    setEditItem(null);
    setStageChange(null);
    fetchData();
  };
  const totalPages = Math.ceil(totalCount / PAGE_SIZE);

  return (
    <div className="space-y-5">
      {createModal && (
        <OpportunityFormModal
          customers={customers}
          leads={leads}
          nhanVienList={nhanVienList}
          canAssign={user?.role === ROLES.Manager}
          onClose={() => setCreateModal(false)}
          onSaved={onSaved}
        />
      )}
      {editItem && (
        <OpportunityFormModal
          item={editItem}
          customers={customers}
          leads={leads}
          nhanVienList={nhanVienList}
          canAssign={user?.role === ROLES.Manager}
          onClose={() => setEditItem(null)}
          onSaved={onSaved}
        />
      )}
      {stageChange && (
        <ChangeStageModal
          item={stageChange.item}
          targetStage={stageChange.targetStage}
          onClose={() => setStageChange(null)}
          onSaved={onSaved}
        />
      )}

      <PageHeader
        breadcrumb="CRM / Kinh doanh"
        title="Cơ hội bán hàng"
        actions={
          <Button icon={Plus} onClick={() => setCreateModal(true)}>
            Tạo cơ hội
          </Button>
        }
      />

      {(khachHangIdFilter || leadIdFilter) && (
        <div className="bg-info-50 border border-info-100 text-info-700 text-sm rounded-lg px-4 py-2.5 flex items-center justify-between">
          <span>
            Đang lọc theo{" "}
            {khachHangIdFilter ? "khách hàng đã chọn" : "lead đã chọn"}.
          </span>
          <button
            onClick={() => {
              setSearchParams({});
              setPage(1);
            }}
            className="text-info-700 font-medium hover:underline"
          >
            Bỏ lọc
          </button>
        </div>
      )}

      {summary && (
        <div className="grid grid-cols-2 sm:grid-cols-4 gap-3">
          <StatCard
            label="Đang xử lý"
            value={summary.totalActive ?? 0}
            tone="info"
            icon={TrendingUp}
          />
          <StatCard
            label="Thành công"
            value={summary.thanhCong ?? 0}
            tone="success"
            icon={CheckCircle2}
          />
          <StatCard
            label="DT kỳ vọng"
            value={formatMoney(summary.totalDoanhThuKyVong)}
            tone="accent"
            icon={Wallet}
          />
          <StatCard
            label="Tỷ lệ TB"
            value={`${Math.round(summary.tyLeThanhCongTrungBinh ?? 0)}%`}
            icon={Percent}
          />
        </div>
      )}

      <div className="bg-surface rounded-card border border-ink-100 p-4 flex flex-wrap gap-3 items-center">
        <input
          value={search}
          onChange={(e) => {
            setSearch(e.target.value);
            setPage(1);
          }}
          placeholder="Tìm theo tên thương vụ..."
          className="border border-ink-200 rounded-lg px-3 py-2 text-sm w-64 focus:outline-none focus:ring-2 focus:ring-accent-400/40 focus:border-accent-400"
        />
        <select
          value={filterStage}
          onChange={(e) => {
            setFilterStage(e.target.value);
            setPage(1);
          }}
          className="border border-ink-200 rounded-lg px-3 py-2 text-sm focus:outline-none focus:ring-2 focus:ring-accent-400/40 focus:border-accent-400"
        >
          <option value="">Tất cả giai đoạn</option>
          {GIAI_DOAN_LIST.map((s) => (
            <option key={s} value={s}>
              {GIAI_DOAN_LABEL[s]}
            </option>
          ))}
        </select>

        <div className="ml-auto flex gap-1 border border-ink-200 rounded-lg overflow-hidden">
          <button
            onClick={() => setViewMode("kanban")}
            className={`px-3 py-2 text-xs font-medium inline-flex items-center gap-1.5 ${viewMode === "kanban" ? "bg-brand-700 text-white" : "text-ink-600 hover:bg-ink-100"}`}
          >
            <LayoutGrid size={14} /> Kanban
          </button>
          <button
            onClick={() => setViewMode("list")}
            className={`px-3 py-2 text-xs font-medium inline-flex items-center gap-1.5 ${viewMode === "list" ? "bg-brand-700 text-white" : "text-ink-600 hover:bg-ink-100"}`}
          >
            <ListIcon size={14} /> Danh sách
          </button>
        </div>
      </div>

      {error && (
        <div className="text-sm text-danger-600 bg-danger-50 rounded-lg p-3">
          {error}
        </div>
      )}

      <div className="bg-surface rounded-card border border-ink-100 p-4">
        {loading ? (
          <div className="text-center py-12 text-ink-400 text-sm">
            Đang tải...
          </div>
        ) : items.length === 0 ? (
          <EmptyState
            icon={TrendingUp}
            title="Chưa có cơ hội bán hàng nào"
            action={
              <Button size="sm" onClick={() => setCreateModal(true)}>
                Tạo cơ hội đầu tiên
              </Button>
            }
          />
        ) : viewMode === "kanban" ? (
          <KanbanBoard
            items={items}
            onEdit={setEditItem}
            onChangeStage={(item, stage) =>
              setStageChange({ item, targetStage: stage })
            }
            onNavigate={(id) => navigate(`/opportunities/${id}`)}
          />
        ) : (
          <ListTable
            items={items}
            onEdit={setEditItem}
            onDelete={handleDelete}
            canDelete={canDelete}
            onChangeStage={(item, stage) =>
              setStageChange({ item, targetStage: stage })
            }
            onNavigate={(id) => navigate(`/opportunities/${id}`)}
          />
        )}

        {viewMode === "list" && totalPages > 1 && (
          <div className="mt-4 pt-4 border-t border-ink-100">
            <Pagination
              pageNumber={page}
              totalPages={totalPages}
              onPageChange={setPage}
            />
          </div>
        )}
      </div>
    </div>
  );
}
