import { useEffect, useMemo, useState } from "react";
import { useNavigate } from "react-router-dom";
import { Package, Plus, Search, Eye, Pencil, Lock, Boxes } from "lucide-react";
import productApi from "../../api/productApi";
import useAuthStore from "../auth/authStore";
import Pagination from "../../components/common/Pagination";
import PageHeader from "../../components/common/PageHeader";
import StatCard from "../../components/common/StatCard";
import EmptyState from "../../components/common/EmptyState";
import RowMenu from "../../components/common/RowMenu";
import Badge from "../../components/common/Badge";
import Button from "../../components/common/Button";
import ProductFormModal from "./ProductFormModal";
import { ROLES } from "../../utils/constants";

export default function ProductListPage() {
  const { user } = useAuthStore();
  const navigate = useNavigate();
  const canManage = user?.role === ROLES.Manager;

  const [items, setItems] = useState([]);
  const [types, setTypes] = useState([]);
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState("");
  const [success, setSuccess] = useState("");

  const [showCreateModal, setShowCreateModal] = useState(false);
  const [editingProduct, setEditingProduct] = useState(null);

  const [search, setSearch] = useState("");
  const [filterType, setFilterType] = useState("");
  const [filterActive, setFilterActive] = useState("");
  const [pageNumber, setPageNumber] = useState(1);
  const [totalPages, setTotalPages] = useState(1);
  const [totalCount, setTotalCount] = useState(0);
  const pageSize = 10;

  const stats = useMemo(
    () => ({
      total: totalCount,
      active: items.filter((i) => i.dangKinhDoanh).length,
      outOfStock: items.filter((i) => i.soLuongTon <= 0).length,
    }),
    [items, totalCount],
  );

  const loadProducts = async () => {
    setLoading(true); setError("");
    try {
      const res = await productApi.getAll({
        pageNumber, pageSize, search: search.trim() || undefined,
        loaiSanPhamId: filterType || undefined,
        dangKinhDoanh: filterActive === "" ? undefined : filterActive === "true",
      });
      setItems(res.data?.items ?? []);
      setTotalPages(res.data?.totalPages ?? 1);
      setTotalCount(res.data?.totalCount ?? 0);
    } catch (err) { setError(err?.message || "Tải danh sách thất bại"); }
    finally { setLoading(false); }
  };

  const loadTypes = async () => {
    try { const res = await productApi.getTypes(); setTypes(res.data ?? []); }
    catch { /* không có quyền hoặc lỗi tải loại sản phẩm, bỏ qua */ }
  };

  useEffect(() => { loadProducts(); }, [pageNumber, filterType, filterActive]);
  useEffect(() => { loadTypes(); }, []);

  const handleDeactivate = async (id) => {
    if (!window.confirm("Khóa kinh doanh sản phẩm này? Sản phẩm sẽ không xuất hiện khi lập báo giá mới.")) return;
    try { await productApi.delete(id); setSuccess("Đã khóa kinh doanh sản phẩm"); await loadProducts(); }
    catch (err) { setError(err?.message || "Không thể khóa sản phẩm"); }
  };

  return (
    <div className="space-y-5">
      {showCreateModal && (
        <ProductFormModal types={types} onClose={() => setShowCreateModal(false)}
          onSaved={() => { setShowCreateModal(false); setSuccess("Thêm sản phẩm thành công"); loadProducts(); }} />
      )}
      {editingProduct && (
        <ProductFormModal product={editingProduct} types={types} onClose={() => setEditingProduct(null)}
          onSaved={() => { setEditingProduct(null); setSuccess("Cập nhật sản phẩm thành công"); loadProducts(); }} />
      )}

      <PageHeader
        breadcrumb="CRM / Danh mục"
        title="Sản phẩm / Dịch vụ"
        actions={canManage && <Button icon={Plus} onClick={() => setShowCreateModal(true)}>Thêm sản phẩm</Button>}
      />

      <div className="grid grid-cols-1 sm:grid-cols-3 gap-3">
        <StatCard label="Tổng sản phẩm (trang này)" value={stats.total} icon={Package} />
        <StatCard label="Đang kinh doanh" value={stats.active} tone="success" icon={Boxes} />
        <StatCard label="Hết hàng" value={stats.outOfStock} tone="warning" icon={Boxes} />
      </div>

      {error && <div className="text-sm text-danger-600 bg-danger-50 rounded-lg p-3">{error}</div>}
      {success && <div className="text-sm text-success-700 bg-success-50 rounded-lg p-3">{success}</div>}

      <div className="bg-surface rounded-card border border-ink-100 overflow-hidden">
        <div className="px-5 py-3.5 border-b border-ink-100 flex items-center justify-between gap-3 flex-wrap">
          <div className="relative">
            <Search size={15} className="absolute left-3 top-1/2 -translate-y-1/2 text-ink-400" />
            <input
              type="search" placeholder="Tìm theo mã, tên..."
              value={search} onChange={(e) => setSearch(e.target.value)}
              onKeyDown={(e) => { if (e.key === "Enter") { setPageNumber(1); loadProducts(); } }}
              className="border border-ink-200 rounded-lg pl-9 pr-3 py-2 text-sm w-56 focus:outline-none focus:ring-2 focus:ring-accent-400/40 focus:border-accent-400"
            />
          </div>
          <div className="flex gap-2">
            <select value={filterType} onChange={(e) => { setFilterType(e.target.value); setPageNumber(1); }}
              className="border border-ink-200 rounded-lg px-3 py-2 text-sm focus:outline-none focus:ring-2 focus:ring-accent-400/40 focus:border-accent-400">
              <option value="">Tất cả loại</option>
              {types.map((t) => <option key={t.id} value={t.id}>{t.tenLoai}</option>)}
            </select>
            <select value={filterActive} onChange={(e) => { setFilterActive(e.target.value); setPageNumber(1); }}
              className="border border-ink-200 rounded-lg px-3 py-2 text-sm focus:outline-none focus:ring-2 focus:ring-accent-400/40 focus:border-accent-400">
              <option value="">Tất cả trạng thái</option>
              <option value="true">Đang kinh doanh</option>
              <option value="false">Ngừng kinh doanh</option>
            </select>
          </div>
        </div>

        <div className="overflow-x-auto">
          <table className="w-full text-sm">
            <thead>
              <tr className="bg-surface-alt">
                <th className="px-5 py-3 text-left text-xs font-medium text-ink-400 uppercase tracking-wide">Mã SP</th>
                <th className="px-5 py-3 text-left text-xs font-medium text-ink-400 uppercase tracking-wide">Tên sản phẩm</th>
                <th className="px-5 py-3 text-left text-xs font-medium text-ink-400 uppercase tracking-wide">Loại</th>
                <th className="px-5 py-3 text-left text-xs font-medium text-ink-400 uppercase tracking-wide">Giá bán</th>
                <th className="px-5 py-3 text-left text-xs font-medium text-ink-400 uppercase tracking-wide">Tồn kho</th>
                <th className="px-5 py-3 text-left text-xs font-medium text-ink-400 uppercase tracking-wide">Trạng thái</th>
                <th className="w-12"></th>
              </tr>
            </thead>
            <tbody className="divide-y divide-ink-100">
              {items.length === 0 ? (
                <tr>
                  <td colSpan={7}>
                    <EmptyState icon={Package} title={loading ? "Đang tải..." : "Chưa có sản phẩm nào"}
                      description={!loading && canManage ? "Thêm sản phẩm mới để bắt đầu lập báo giá." : undefined} />
                  </td>
                </tr>
              ) : items.map((item) => (
                <tr key={item.id} className="hover:bg-surface-alt cursor-pointer transition-colors"
                  onClick={() => navigate(`/products/${item.id}`)}>
                  <td className="px-5 py-3.5 font-mono text-info-600 text-xs font-semibold">{item.maSP}</td>
                  <td className="px-5 py-3.5 font-medium text-ink-900">{item.tenSP}</td>
                  <td className="px-5 py-3.5 text-ink-700">{item.tenLoai || "—"}</td>
                  <td className="px-5 py-3.5 text-ink-700">
                    {Number(item.giaBan).toLocaleString("vi-VN")} đ{item.donVi ? `/${item.donVi}` : ""}
                  </td>
                  <td className="px-5 py-3.5">
                    <span className={item.soLuongTon <= 0 ? "text-danger-600 font-semibold" : "text-ink-700"}>{item.soLuongTon}</span>
                  </td>
                  <td className="px-5 py-3.5">
                    <Badge label={item.dangKinhDoanh ? "Đang kinh doanh" : "Ngừng kinh doanh"}
                      tone={item.dangKinhDoanh ? "success" : "neutral"} />
                  </td>
                  <td className="px-3 py-3.5 text-center" onClick={(e) => e.stopPropagation()}>
                    <RowMenu
                      items={[
                        { label: "Xem chi tiết & kho", icon: Eye, onClick: () => navigate(`/products/${item.id}`) },
                        ...(canManage ? [{ label: "Sửa", icon: Pencil, onClick: () => setEditingProduct(item) }] : []),
                        ...(canManage && item.dangKinhDoanh
                          ? [{ label: "Khóa kinh doanh", icon: Lock, danger: true, onClick: () => handleDeactivate(item.id) }]
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
          <p className="text-xs text-ink-400">{totalCount} sản phẩm</p>
          <Pagination pageNumber={pageNumber} totalPages={totalPages} onPageChange={setPageNumber} />
        </div>
      </div>
    </div>
  );
}
