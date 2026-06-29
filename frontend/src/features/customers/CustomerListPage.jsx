import { useEffect, useMemo, useState } from "react";
import { useNavigate } from "react-router-dom";
import { Users, Plus, Search, Eye, Pencil, Trash2, Crown, Building2 } from "lucide-react";
import customerApi from "../../api/customerApi";
import useAuthStore from "../auth/authStore";
import Pagination from "../../components/common/Pagination";
import PageHeader from "../../components/common/PageHeader";
import StatCard from "../../components/common/StatCard";
import EmptyState from "../../components/common/EmptyState";
import RowMenu from "../../components/common/RowMenu";
import Badge from "../../components/common/Badge";
import Button from "../../components/common/Button";
import CustomerFormModal from "./CustomerFormModal";
import {
  ROLES,
  LOAI_KHACH_HANG_OPTIONS,
  TINH_TRANG_KHACH_HANG_OPTIONS,
  LOAI_BADGE_COLOR,
  TINH_TRANG_BADGE_COLOR,
} from "../../utils/constants";

export default function CustomerListPage() {
  const { user } = useAuthStore();
  const navigate = useNavigate();
  const canDelete = user?.role === ROLES.Manager;
  const canEdit = user?.role === ROLES.Manager || user?.role === ROLES.Sale;
  const isReadOnly = user?.role === ROLES.Accountant;

  const [items, setItems] = useState([]);
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState("");
  const [success, setSuccess] = useState("");

  const [showCreateModal, setShowCreateModal] = useState(false);
  const [editingCustomer, setEditingCustomer] = useState(null);

  const [search, setSearch] = useState("");
  const [filterLoai, setFilterLoai] = useState("");
  const [filterTinhTrang, setFilterTinhTrang] = useState("");
  const [pageNumber, setPageNumber] = useState(1);
  const [totalPages, setTotalPages] = useState(1);
  const [totalCount, setTotalCount] = useState(0);
  const pageSize = 10;

  const stats = useMemo(
    () => ({
      total: totalCount,
      vip: items.filter((i) => i.loaiKhachHangId === 1).length,
      b2b: items.filter((i) => i.loaiKhachHangId === 2).length,
    }),
    [items, totalCount],
  );

  const loadCustomers = async () => {
    setLoading(true);
    setError("");
    try {
      const res = await customerApi.getAll({
        pageNumber, pageSize, search: search.trim() || undefined,
        loaiKhachHangId: filterLoai || undefined,
        tinhTrangId: filterTinhTrang || undefined,
      });
      setItems(res.data?.items ?? []);
      setTotalPages(res.data?.totalPages ?? 1);
      setTotalCount(res.data?.totalCount ?? 0);
    } catch (err) {
      setError(err?.message || "Tải danh sách thất bại");
    } finally { setLoading(false); }
  };

  useEffect(() => { loadCustomers(); }, [pageNumber, filterLoai, filterTinhTrang]);

  const handleDelete = async (id) => {
    if (!window.confirm("Xóa khách hàng này? Hành động không thể hoàn tác.")) return;
    try {
      await customerApi.delete(id);
      setSuccess("Xóa khách hàng thành công");
      await loadCustomers();
    } catch (err) { setError(err?.message || "Không thể xóa khách hàng"); }
  };

  return (
    <div className="space-y-5">
      {showCreateModal && (
        <CustomerFormModal
          onClose={() => setShowCreateModal(false)}
          onSaved={() => { setShowCreateModal(false); setSuccess("Thêm khách hàng thành công"); loadCustomers(); }}
        />
      )}
      {editingCustomer && (
        <CustomerFormModal
          customer={editingCustomer}
          onClose={() => setEditingCustomer(null)}
          onSaved={() => { setEditingCustomer(null); setSuccess("Cập nhật khách hàng thành công"); loadCustomers(); }}
        />
      )}

      <PageHeader
        breadcrumb="CRM / Kinh doanh"
        title="Quản lý khách hàng"
        actions={
          canEdit && (
            <Button icon={Plus} onClick={() => setShowCreateModal(true)}>Thêm khách hàng</Button>
          )
        }
      />

      <div className="grid grid-cols-1 sm:grid-cols-3 gap-3">
        <StatCard label="Tổng khách hàng (trang này)" value={stats.total} icon={Users} />
        <StatCard label="Khách VIP" value={stats.vip} tone="warning" icon={Crown} />
        <StatCard label="Khách B2B" value={stats.b2b} tone="info" icon={Building2} />
      </div>

      {isReadOnly && (
        <div className="text-xs text-info-700 bg-info-50 border border-info-100 rounded-lg p-3">
          Tài khoản Kế toán chỉ có quyền xem danh sách khách hàng.
        </div>
      )}
      {error && <div className="text-sm text-danger-600 bg-danger-50 rounded-lg p-3">{error}</div>}
      {success && <div className="text-sm text-success-700 bg-success-50 rounded-lg p-3">{success}</div>}

      <div className="bg-surface rounded-card border border-ink-100 overflow-hidden">
        <div className="px-5 py-3.5 border-b border-ink-100 flex items-center justify-between gap-3 flex-wrap">
          <div className="relative">
            <Search size={15} className="absolute left-3 top-1/2 -translate-y-1/2 text-ink-400" />
            <input
              type="search" placeholder="Tìm theo mã, tên, email, SĐT..."
              value={search} onChange={(e) => setSearch(e.target.value)}
              onKeyDown={(e) => { if (e.key === "Enter") { setPageNumber(1); loadCustomers(); } }}
              className="border border-ink-200 rounded-lg pl-9 pr-3 py-2 text-sm w-64 focus:outline-none focus:ring-2 focus:ring-accent-400/40 focus:border-accent-400"
            />
          </div>
          <div className="flex gap-2">
            <select value={filterLoai} onChange={(e) => { setFilterLoai(e.target.value); setPageNumber(1); }}
              className="border border-ink-200 rounded-lg px-3 py-2 text-sm focus:outline-none focus:ring-2 focus:ring-accent-400/40 focus:border-accent-400">
              <option value="">Tất cả loại</option>
              {LOAI_KHACH_HANG_OPTIONS.map(o => <option key={o.value} value={o.value}>{o.label}</option>)}
            </select>
            <select value={filterTinhTrang} onChange={(e) => { setFilterTinhTrang(e.target.value); setPageNumber(1); }}
              className="border border-ink-200 rounded-lg px-3 py-2 text-sm focus:outline-none focus:ring-2 focus:ring-accent-400/40 focus:border-accent-400">
              <option value="">Tất cả tình trạng</option>
              {TINH_TRANG_KHACH_HANG_OPTIONS.map(o => <option key={o.value} value={o.value}>{o.label}</option>)}
            </select>
          </div>
        </div>

        <div className="overflow-x-auto">
          <table className="w-full text-sm">
            <thead>
              <tr className="bg-surface-alt">
                <th className="px-5 py-3 text-left text-xs font-medium text-ink-400 uppercase tracking-wide">Mã KH</th>
                <th className="px-5 py-3 text-left text-xs font-medium text-ink-400 uppercase tracking-wide">Tên khách hàng</th>
                <th className="px-5 py-3 text-left text-xs font-medium text-ink-400 uppercase tracking-wide">Liên hệ</th>
                <th className="px-5 py-3 text-left text-xs font-medium text-ink-400 uppercase tracking-wide">Loại</th>
                <th className="px-5 py-3 text-left text-xs font-medium text-ink-400 uppercase tracking-wide">Tình trạng</th>
                <th className="px-5 py-3 text-left text-xs font-medium text-ink-400 uppercase tracking-wide">NV phụ trách</th>
                <th className="w-12"></th>
              </tr>
            </thead>
            <tbody className="divide-y divide-ink-100">
              {items.length === 0 ? (
                <tr>
                  <td colSpan={7}>
                    <EmptyState
                      icon={Users}
                      title={loading ? "Đang tải..." : "Chưa có khách hàng nào"}
                      description={!loading ? "Thêm khách hàng mới để bắt đầu quản lý quan hệ." : undefined}
                    />
                  </td>
                </tr>
              ) : items.map((item) => (
                <tr key={item.id} className="hover:bg-surface-alt cursor-pointer transition-colors"
                  onClick={() => navigate(`/customers/${item.id}`)}>
                  <td className="px-5 py-3.5 font-mono text-info-600 text-xs font-semibold">{item.maKhachHang}</td>
                  <td className="px-5 py-3.5">
                    <div className="font-medium text-ink-900">{item.tenKhachHang}</div>
                    {item.maSoThue && <div className="text-xs text-ink-400">MST: {item.maSoThue}</div>}
                  </td>
                  <td className="px-5 py-3.5 text-ink-700">
                    <div>{item.email || "—"}</div>
                    {item.soDienThoai && <div className="text-xs text-ink-400">{item.soDienThoai}</div>}
                  </td>
                  <td className="px-5 py-3.5">
                    {item.loaiKhachHangId ? (
                      <Badge label={item.tenLoaiKhachHang ?? `Loại ${item.loaiKhachHangId}`}
                        colorClass={LOAI_BADGE_COLOR[item.loaiKhachHangId]} />
                    ) : "—"}
                  </td>
                  <td className="px-5 py-3.5">
                    {item.tinhTrangId ? (
                      <Badge label={item.tenTinhTrang ?? `Tình trạng ${item.tinhTrangId}`}
                        colorClass={TINH_TRANG_BADGE_COLOR[item.tinhTrangId]} />
                    ) : "—"}
                  </td>
                  <td className="px-5 py-3.5 text-ink-700">{item.tenNhanVienPhuTrach || "—"}</td>
                  <td className="px-3 py-3.5 text-center" onClick={(e) => e.stopPropagation()}>
                    {!isReadOnly && (
                      <RowMenu
                        items={[
                          { label: "Xem chi tiết", icon: Eye, onClick: () => navigate(`/customers/${item.id}`) },
                          ...(canEdit ? [{ label: "Sửa", icon: Pencil, onClick: () => setEditingCustomer(item) }] : []),
                          ...(canDelete ? [{ label: "Xóa", icon: Trash2, danger: true, onClick: () => handleDelete(item.id) }] : []),
                        ]}
                      />
                    )}
                  </td>
                </tr>
              ))}
            </tbody>
          </table>
        </div>

        <div className="px-5 py-3.5 border-t border-ink-100 flex items-center justify-between">
          <p className="text-xs text-ink-400">{totalCount} khách hàng</p>
          <Pagination pageNumber={pageNumber} totalPages={totalPages} onPageChange={setPageNumber} />
        </div>
      </div>
    </div>
  );
}
