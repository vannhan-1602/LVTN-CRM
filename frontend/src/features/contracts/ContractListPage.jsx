import { useEffect, useState } from "react";
import { useNavigate, useSearchParams } from "react-router-dom";
import {
  FileText,
  Plus,
  Search,
  Eye,
  Pencil,
  Trash2,
  CheckCircle2,
  Wallet,
} from "lucide-react";
import contractApi from "../../api/contractApi";
import useAuthStore from "../auth/authStore";
import Pagination from "../../components/common/Pagination";
import PageHeader from "../../components/common/PageHeader";
import StatCard from "../../components/common/StatCard";
import EmptyState from "../../components/common/EmptyState";
import RowMenu from "../../components/common/RowMenu";
import Badge from "../../components/common/Badge";
import Button from "../../components/common/Button";
import CreateContractModal from "./CreateContractModal";
import {
  ROLES,
  CONTRACT_STATUS,
  CONTRACT_STATUS_OPTIONS,
} from "../../utils/constants";
import { formatDate } from "../../utils/formatters";

const STATUS_TONE = {
  DangThucHien: "success",
  TamDung: "warning",
  ThanhLy: "neutral",
};

function formatMoney(n) {
  return n == null ? "—" : Number(n).toLocaleString("vi-VN") + " đ";
}

export default function ContractListPage() {
  const { user } = useAuthStore();
  const navigate = useNavigate();
  const [searchParams, setSearchParams] = useSearchParams();
  const khachHangIdFilter = searchParams.get("khachHangId");
  const canManage = [ROLES.Sale, ROLES.Manager].includes(user?.role);
  const canDelete = user?.role === ROLES.Manager;
  const isReadOnly = user?.role === ROLES.Accountant;

  const [items, setItems] = useState([]);
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState("");
  const [success, setSuccess] = useState("");

  const [showCreateModal, setShowCreateModal] = useState(false);

  const [search, setSearch] = useState("");
  const [filterStatus, setFilterStatus] = useState("");
  const [pageNumber, setPageNumber] = useState(1);
  const [totalPages, setTotalPages] = useState(1);
  const [totalCount, setTotalCount] = useState(0);
  const pageSize = 10;

  const loadContracts = async () => {
    setLoading(true);
    setError("");
    try {
      const res = await contractApi.getAll({
        pageNumber,
        pageSize,
        search: search.trim() || undefined,
        trangThai: filterStatus || undefined,
        khachHangId: khachHangIdFilter || undefined,
      });
      setItems(res.data?.items ?? []);
      setTotalPages(res.data?.totalPages ?? 1);
      setTotalCount(res.data?.totalCount ?? 0);
    } catch (err) {
      setError(err?.message || "Tải danh sách thất bại");
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => {
    loadContracts();
  }, [pageNumber, filterStatus, khachHangIdFilter]);

  const handleDelete = async (id) => {
    if (!window.confirm("Xóa hợp đồng này? Hành động không thể hoàn tác."))
      return;
    try {
      await contractApi.delete(id);
      setSuccess("Xóa hợp đồng thành công");
      await loadContracts();
    } catch (err) {
      setError(err?.message || "Không thể xóa");
    }
  };

  const dangThucHienCount = items.filter(
    (i) => i.trangThai === "DangThucHien",
  ).length;
  const tongGiaTri = items.reduce((sum, i) => sum + (i.giaTri || 0), 0);

  return (
    <div className="space-y-5">
      {showCreateModal && (
        <CreateContractModal
          onClose={() => setShowCreateModal(false)}
          onSaved={() => {
            setShowCreateModal(false);
            setSuccess("Tạo hợp đồng thành công");
            loadContracts();
          }}
        />
      )}

      <PageHeader
        breadcrumb="CRM / Kinh doanh"
        title="Quản lý hợp đồng"
        actions={
          canManage && (
            <Button icon={Plus} onClick={() => setShowCreateModal(true)}>
              Tạo từ báo giá
            </Button>
          )
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
          label="Tổng hợp đồng (trang này)"
          value={totalCount}
          icon={FileText}
        />
        <StatCard
          label="Đang thực hiện"
          value={dangThucHienCount}
          tone="success"
          icon={CheckCircle2}
        />
        <StatCard
          label="Giá trị (trang này)"
          value={formatMoney(tongGiaTri)}
          tone="accent"
          icon={Wallet}
        />
      </div>

      {isReadOnly && (
        <div className="text-xs text-info-700 bg-info-50 border border-info-100 rounded-lg p-3">
          Tài khoản Kế toán chỉ có quyền xem danh sách hợp đồng (không thể
          tạo/sửa/xóa).
        </div>
      )}
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
              placeholder="Tìm theo mã hợp đồng..."
              value={search}
              onChange={(e) => setSearch(e.target.value)}
              onKeyDown={(e) => {
                if (e.key === "Enter") {
                  setPageNumber(1);
                  loadContracts();
                }
              }}
              className="border border-ink-200 rounded-lg pl-9 pr-3 py-2 text-sm w-60 focus:outline-none focus:ring-2 focus:ring-accent-400/40 focus:border-accent-400"
            />
          </div>
          <select
            value={filterStatus}
            onChange={(e) => {
              setFilterStatus(e.target.value);
              setPageNumber(1);
            }}
            className="border border-ink-200 rounded-lg px-3 py-2 text-sm focus:outline-none focus:ring-2 focus:ring-accent-400/40 focus:border-accent-400"
          >
            <option value="">Tất cả trạng thái</option>
            {CONTRACT_STATUS_OPTIONS.map((o) => (
              <option key={o.value} value={o.value}>
                {o.label}
              </option>
            ))}
          </select>
        </div>

        <div className="overflow-x-auto">
          <table className="w-full text-sm">
            <thead>
              <tr className="bg-surface-alt">
                <th className="px-5 py-3 text-left text-xs font-medium text-ink-400 uppercase tracking-wide">
                  Mã hợp đồng
                </th>
                <th className="px-5 py-3 text-left text-xs font-medium text-ink-400 uppercase tracking-wide">
                  Khách hàng
                </th>
                <th className="px-5 py-3 text-left text-xs font-medium text-ink-400 uppercase tracking-wide">
                  Giá trị
                </th>
                <th className="px-5 py-3 text-left text-xs font-medium text-ink-400 uppercase tracking-wide">
                  Ngày ký
                </th>
                <th className="px-5 py-3 text-left text-xs font-medium text-ink-400 uppercase tracking-wide">
                  Trạng thái
                </th>
                <th className="w-12"></th>
              </tr>
            </thead>
            <tbody className="divide-y divide-ink-100">
              {items.length === 0 ? (
                <tr>
                  <td colSpan={6}>
                    <EmptyState
                      icon={FileText}
                      title={loading ? "Đang tải..." : "Chưa có hợp đồng nào"}
                      description={
                        !loading
                          ? "Tạo hợp đồng từ một báo giá đã được khách hàng chấp nhận."
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
                    onClick={() => navigate(`/contracts/${item.id}`)}
                  >
                    <td className="px-5 py-3.5 font-mono text-info-600 text-xs font-semibold">
                      {item.maHopDong}
                    </td>
                    <td className="px-5 py-3.5 font-medium text-ink-900">
                      {item.tenKhachHang}
                    </td>
                    <td className="px-5 py-3.5 text-ink-700">
                      {formatMoney(item.giaTri)}
                    </td>
                    <td className="px-5 py-3.5 text-xs text-ink-400">
                      {formatDate(item.ngayKy)}
                    </td>
                    <td className="px-5 py-3.5">
                      <Badge
                        label={
                          CONTRACT_STATUS[item.trangThai] ?? item.trangThai
                        }
                        tone={STATUS_TONE[item.trangThai]}
                      />
                    </td>
                    <td
                      className="px-3 py-3.5 text-center"
                      onClick={(e) => e.stopPropagation()}
                    >
                      {!isReadOnly && (
                        <RowMenu
                          items={[
                            {
                              label: "Xem chi tiết",
                              icon: Eye,
                              onClick: () => navigate(`/contracts/${item.id}`),
                            },
                            ...(canManage
                              ? [
                                  {
                                    label: "Sửa",
                                    icon: Pencil,
                                    onClick: () =>
                                      navigate(`/contracts/${item.id}?edit=1`),
                                  },
                                ]
                              : []),
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
                      )}
                    </td>
                  </tr>
                ))
              )}
            </tbody>
          </table>
        </div>

        <div className="px-5 py-3.5 border-t border-ink-100 flex items-center justify-between">
          <p className="text-xs text-ink-400">{totalCount} hợp đồng</p>
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
