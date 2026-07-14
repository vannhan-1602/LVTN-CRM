import { useEffect, useState } from "react";
import { useNavigate } from "react-router-dom";
import { Wallet, Plus, ArrowDownCircle, ArrowUpCircle } from "lucide-react";
import phieuThuChiApi from "../../api/phieuThuChiApi";
import useAuthStore from "../auth/authStore";
import Pagination from "../../components/common/Pagination";
import PageHeader from "../../components/common/PageHeader";
import StatCard from "../../components/common/StatCard";
import EmptyState from "../../components/common/EmptyState";
import Badge from "../../components/common/Badge";
import Button from "../../components/common/Button";
import CreatePhieuChiModal from "./CreatePhieuChiModal";
import { ROLES } from "../../utils/constants";
import { formatCurrency, formatDate } from "../../utils/formatters";

const LOAI_LABEL = { Thu: "Phiếu thu", Chi: "Phiếu chi" };
const LOAI_COLOR = {
  Thu: "bg-success-50 text-success-700",
  Chi: "bg-danger-50 text-danger-600",
};

export default function PhieuThuChiListPage() {
  const { user } = useAuthStore();
  const navigate = useNavigate();
  const canCreateChi = [ROLES.Accountant, ROLES.Manager].includes(user?.role);

  const [items, setItems] = useState([]);
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState("");
  const [success, setSuccess] = useState("");
  const [showCreateChi, setShowCreateChi] = useState(false);

  const [filterLoai, setFilterLoai] = useState("");
  const [pageNumber, setPageNumber] = useState(1);
  const [totalPages, setTotalPages] = useState(1);
  const [totalCount, setTotalCount] = useState(0);
  const pageSize = 10;

  const load = async () => {
    setLoading(true);
    setError("");
    try {
      const res = await phieuThuChiApi.getAll({
        pageNumber,
        pageSize,
        loaiPhieu: filterLoai || undefined,
      });
      const data = res.data ?? {};
      setItems(data.items ?? []);
      setTotalPages(data.totalPages ?? 1);
      setTotalCount(data.totalCount ?? 0);
    } catch (err) {
      setError(err?.message || "Tải danh sách thất bại");
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => {
    load();
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [pageNumber, filterLoai]);

  const tongThu = items
    .filter((i) => i.loaiPhieu === "Thu")
    .reduce((s, i) => s + (i.soTien ?? 0), 0);
  const tongChi = items
    .filter((i) => i.loaiPhieu === "Chi")
    .reduce((s, i) => s + (i.soTien ?? 0), 0);

  return (
    <div className="space-y-5">
      {showCreateChi && (
        <CreatePhieuChiModal
          onClose={() => setShowCreateChi(false)}
          onSaved={() => {
            setShowCreateChi(false);
            setSuccess("Tạo phiếu chi thành công");
            load();
          }}
        />
      )}

      <PageHeader
        breadcrumb="CRM / Kế toán"
        title="Phiếu thu / Phiếu chi"
        actions={
          canCreateChi && (
            <Button icon={Plus} onClick={() => setShowCreateChi(true)}>
              Tạo phiếu chi
            </Button>
          )
        }
      />

      <div className="grid grid-cols-1 sm:grid-cols-3 gap-3">
        <StatCard label="Tổng số phiếu" value={totalCount} icon={Wallet} />
        <StatCard
          label="Tổng thu (trang này)"
          value={formatCurrency(tongThu)}
          tone="success"
          icon={ArrowDownCircle}
        />
        <StatCard
          label="Tổng chi (trang này)"
          value={formatCurrency(tongChi)}
          tone="warning"
          icon={ArrowUpCircle}
        />
      </div>

      {error && <div className="text-sm text-danger-600 bg-danger-50 rounded-lg p-3">{error}</div>}
      {success && <div className="text-sm text-success-700 bg-success-50 rounded-lg p-3">{success}</div>}

      <div className="bg-surface rounded-card border border-ink-100 overflow-hidden">
        <div className="px-5 py-3.5 border-b border-ink-100 flex items-center gap-3 flex-wrap">
          <select
            value={filterLoai}
            onChange={(e) => {
              setFilterLoai(e.target.value);
              setPageNumber(1);
            }}
            className="border border-ink-200 rounded-lg px-3 py-2 text-sm focus:outline-none focus:ring-2 focus:ring-accent-400/40 focus:border-accent-400"
          >
            <option value="">Tất cả loại phiếu</option>
            {Object.entries(LOAI_LABEL).map(([v, l]) => (
              <option key={v} value={v}>
                {l}
              </option>
            ))}
          </select>
        </div>

        <div className="overflow-x-auto">
          <table className="w-full text-sm">
            <thead>
              <tr className="bg-surface-alt">
                {["Mã phiếu", "Loại", "Khách hàng", "Hóa đơn", "Số tiền", "Người lập", "Ngày tạo"].map(
                  (h) => (
                    <th
                      key={h}
                      className="px-5 py-3 text-left text-xs font-medium text-ink-400 uppercase tracking-wide"
                    >
                      {h}
                    </th>
                  ),
                )}
              </tr>
            </thead>
            <tbody className="divide-y divide-ink-100">
              {items.length === 0 ? (
                <tr>
                  <td colSpan={7}>
                    <EmptyState
                      icon={Wallet}
                      title={loading ? "Đang tải..." : "Chưa có phiếu thu/chi nào"}
                      description={
                        !loading
                          ? "Phiếu thu được tạo từ hóa đơn; phiếu chi tạo trực tiếp ở đây."
                          : undefined
                      }
                    />
                  </td>
                </tr>
              ) : (
                items.map((item) => (
                  <tr
                    key={item.id}
                    className="hover:bg-surface-alt transition-colors"
                    onClick={() => item.hoaDonId && navigate(`/invoices/${item.hoaDonId}`)}
                  >
                    <td className="px-5 py-3.5 font-mono text-info-600 text-xs font-semibold">
                      {item.maPhieu}
                    </td>
                    <td className="px-5 py-3.5">
                      <Badge
                        label={LOAI_LABEL[item.loaiPhieu] ?? item.loaiPhieu}
                        colorClass={LOAI_COLOR[item.loaiPhieu] ?? "bg-ink-100 text-ink-500"}
                      />
                    </td>
                    <td className="px-5 py-3.5 font-medium text-ink-900">
                      {item.tenKhachHang || "—"}
                    </td>
                    <td className="px-5 py-3.5 text-ink-500 text-xs">{item.maHoaDon || "—"}</td>
                    <td
                      className={`px-5 py-3.5 font-medium ${
                        item.loaiPhieu === "Chi" ? "text-danger-600" : "text-success-600"
                      }`}
                    >
                      {formatCurrency(item.soTien)}
                    </td>
                    <td className="px-5 py-3.5 text-ink-500 text-xs">{item.tenNguoiLap || "—"}</td>
                    <td className="px-5 py-3.5 text-ink-500 text-xs">{formatDate(item.ngayTao)}</td>
                  </tr>
                ))
              )}
            </tbody>
          </table>
        </div>

        <div className="px-5 py-3.5 border-t border-ink-100 flex items-center justify-between">
          <p className="text-xs text-ink-400">{totalCount} phiếu</p>
          <Pagination pageNumber={pageNumber} totalPages={totalPages} onPageChange={setPageNumber} />
        </div>
      </div>
    </div>
  );
}
