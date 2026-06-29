import { useEffect, useState } from "react";
import { useNavigate, useParams } from "react-router-dom";
import { Pencil, Lock, Plus, X } from "lucide-react";
import productApi from "../../api/productApi";
import useAuthStore from "../auth/authStore";
import PageHeader from "../../components/common/PageHeader";
import Card, { Field } from "../../components/common/Card";
import Badge from "../../components/common/Badge";
import Button from "../../components/common/Button";
import EmptyState from "../../components/common/EmptyState";
import ProductFormModal from "./ProductFormModal";
import { ROLES, STOCK_TRANSACTION_TYPE_OPTIONS, STOCK_TRANSACTION_TYPE_LABEL } from "../../utils/constants";
import { formatDateTime } from "../../utils/formatters";

const emptyStockForm = { loaiGiaoDich: "NhapMua", soLuong: "", maChungTu: "", ghiChu: "" };

// ── Khối quản lý kho — gộp ngay trong trang chi tiết sản phẩm ───────────────
function StockSection({ productId, canManage, onStockChanged }) {
  const [history, setHistory] = useState([]);
  const [tonHienTai, setTonHienTai] = useState(null);
  const [loading, setLoading] = useState(true);
  const [showForm, setShowForm] = useState(false);
  const [form, setForm] = useState(emptyStockForm);
  const [submitting, setSubmitting] = useState(false);
  const [error, setError] = useState("");

  const load = async () => {
    setLoading(true);
    try {
      const res = await productApi.getById(productId);
      setHistory(res.data?.lichSuKho ?? []);
      setTonHienTai(res.data?.soLuongTon ?? null);
    } catch { setError("Không thể tải lịch sử kho"); }
    finally { setLoading(false); }
  };

  useEffect(() => { load(); }, [productId]);

  const handleSubmit = async (e) => {
    e.preventDefault();
    if (!form.soLuong || Number(form.soLuong) <= 0) { setError("Số lượng phải lớn hơn 0"); return; }
    setSubmitting(true); setError("");
    try {
      await productApi.adjustStock(productId, {
        loaiGiaoDich: form.loaiGiaoDich,
        soLuong: Number(form.soLuong),
        maChungTu: form.maChungTu || null,
        ghiChu: form.ghiChu || null,
      });
      setForm(emptyStockForm);
      setShowForm(false);
      await load();
      onStockChanged?.();
    } catch (err) { setError(err?.message || "Cập nhật tồn kho thất bại"); }
    finally { setSubmitting(false); }
  };

  return (
    <Card
      title={`Lịch sử kho ${tonHienTai != null ? `— Tồn hiện tại: ${tonHienTai}` : ""}`}
      action={canManage && !showForm && (
        <Button size="sm" variant="secondary" icon={Plus} onClick={() => setShowForm(true)}>
          Phiếu nhập/xuất
        </Button>
      )}
    >
      {showForm && (
        <form onSubmit={handleSubmit} className="border border-ink-100 rounded-lg p-4 bg-surface-alt space-y-3 mb-4">
          <div className="flex items-center justify-between">
            <h4 className="text-sm font-medium text-ink-900">Tạo phiếu nhập/xuất kho</h4>
            <button type="button" onClick={() => { setShowForm(false); setError(""); }} className="text-ink-400 hover:text-ink-700">
              <X size={16} />
            </button>
          </div>
          <div className="grid grid-cols-2 gap-3">
            <div>
              <label className="block text-xs font-medium text-ink-500 mb-1">Loại giao dịch</label>
              <select value={form.loaiGiaoDich} onChange={(e) => setForm((f) => ({ ...f, loaiGiaoDich: e.target.value }))}
                className="w-full border border-ink-200 rounded-lg px-3 py-2 text-sm">
                {STOCK_TRANSACTION_TYPE_OPTIONS.map((o) => <option key={o.value} value={o.value}>{o.label}</option>)}
              </select>
            </div>
            <div>
              <label className="block text-xs font-medium text-ink-500 mb-1">Số lượng</label>
              <input type="number" min="1" value={form.soLuong} onChange={(e) => setForm((f) => ({ ...f, soLuong: e.target.value }))}
                className="w-full border border-ink-200 rounded-lg px-3 py-2 text-sm" />
            </div>
            <div>
              <label className="block text-xs font-medium text-ink-500 mb-1">Mã chứng từ</label>
              <input value={form.maChungTu} onChange={(e) => setForm((f) => ({ ...f, maChungTu: e.target.value }))}
                placeholder="VD: PNK001" className="w-full border border-ink-200 rounded-lg px-3 py-2 text-sm" />
            </div>
            <div>
              <label className="block text-xs font-medium text-ink-500 mb-1">Ghi chú</label>
              <input value={form.ghiChu} onChange={(e) => setForm((f) => ({ ...f, ghiChu: e.target.value }))}
                className="w-full border border-ink-200 rounded-lg px-3 py-2 text-sm" />
            </div>
          </div>
          {error && <div className="text-sm text-danger-600 bg-danger-50 rounded-lg p-2">{error}</div>}
          <Button type="submit" size="sm" disabled={submitting}>
            {submitting ? "Đang lưu..." : "Ghi nhận giao dịch"}
          </Button>
        </form>
      )}

      {loading ? (
        <p className="text-sm text-ink-400 text-center py-4">Đang tải...</p>
      ) : history.length === 0 ? (
        <EmptyState title="Chưa có giao dịch kho nào" />
      ) : (
        <div className="space-y-2 max-h-80 overflow-y-auto">
          {history.map((h) => (
            <div key={h.id} className="bg-surface-alt rounded-lg p-3 text-sm flex items-center justify-between gap-3">
              <div>
                <span className="font-medium text-ink-900">{STOCK_TRANSACTION_TYPE_LABEL[h.loaiGiaoDich] ?? h.loaiGiaoDich}</span>
                <span className={`ml-2 font-mono text-xs ${h.soLuongThayDoi > 0 ? "text-success-600" : "text-danger-600"}`}>
                  {h.soLuongThayDoi > 0 ? "+" : ""}{h.soLuongThayDoi}
                </span>
                {h.maChungTu && <span className="text-xs text-ink-400 ml-2">({h.maChungTu})</span>}
                {h.ghiChu && <p className="text-xs text-ink-400 mt-1">{h.ghiChu}</p>}
              </div>
              <div className="text-right text-xs text-ink-400 shrink-0">
                <div>Tồn sau: <strong className="text-ink-700">{h.tonCuoi}</strong></div>
                <div>{formatDateTime(h.ngayGiaoDich)}</div>
              </div>
            </div>
          ))}
        </div>
      )}
    </Card>
  );
}

export default function ProductDetailPage() {
  const { id } = useParams();
  const navigate = useNavigate();
  const { user } = useAuthStore();
  const canManage = user?.role === ROLES.Manager;

  const [product, setProduct] = useState(null);
  const [types, setTypes] = useState([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState("");
  const [showEditModal, setShowEditModal] = useState(false);

  const load = async () => {
    setLoading(true); setError("");
    try {
      const res = await productApi.getById(id);
      setProduct(res.data ?? null);
    } catch (err) { setError(err?.message || "Không thể tải thông tin sản phẩm"); }
    finally { setLoading(false); }
  };

  useEffect(() => { load(); }, [id]);
  useEffect(() => {
    productApi.getTypes().then((res) => setTypes(res.data ?? [])).catch(() => {});
  }, []);

  const handleDeactivate = async () => {
    if (!window.confirm("Khóa kinh doanh sản phẩm này? Sản phẩm sẽ không xuất hiện khi lập báo giá mới.")) return;
    try { await productApi.delete(id); await load(); }
    catch (err) { setError(err?.message || "Không thể khóa sản phẩm"); }
  };

  if (loading) return <div className="text-sm text-ink-400 py-10 text-center">Đang tải...</div>;

  if (error || !product) {
    return (
      <div className="space-y-4">
        <PageHeader breadcrumb="CRM / Danh mục" title="Sản phẩm" onBack={() => navigate("/products")} />
        <div className="text-sm text-danger-600 bg-danger-50 rounded-lg p-4">{error || "Không tìm thấy sản phẩm."}</div>
      </div>
    );
  }

  return (
    <div className="space-y-5">
      {showEditModal && (
        <ProductFormModal product={product} types={types} onClose={() => setShowEditModal(false)}
          onSaved={() => { setShowEditModal(false); load(); }} />
      )}

      <PageHeader
        breadcrumb="Sản phẩm"
        title={product.tenSP}
        onBack={() => navigate("/products")}
        badge={<Badge label={product.dangKinhDoanh ? "Đang kinh doanh" : "Ngừng kinh doanh"} tone={product.dangKinhDoanh ? "success" : "neutral"} />}
        actions={
          canManage && (
            <>
              <Button variant="secondary" icon={Pencil} onClick={() => setShowEditModal(true)}>Sửa</Button>
              {product.dangKinhDoanh && (
                <Button variant="danger" icon={Lock} onClick={handleDeactivate}>Khóa kinh doanh</Button>
              )}
            </>
          )
        }
      />

      {error && <div className="text-sm text-danger-600 bg-danger-50 rounded-lg p-3">{error}</div>}

      <div className="grid grid-cols-1 lg:grid-cols-3 gap-4">
        <div className="lg:col-span-2 space-y-4">
          <Card title="Thông tin sản phẩm">
            <div className="grid grid-cols-2 gap-5">
              <Field label="Mã sản phẩm" value={<span className="font-mono">{product.maSP}</span>} />
              <Field label="Loại" value={product.tenLoai} />
              <Field label="Đơn vị" value={product.donVi} />
              <Field label="Giá bán" value={`${Number(product.giaBan).toLocaleString("vi-VN")} đ${product.donVi ? `/${product.donVi}` : ""}`} />
              <Field label="Ngày tạo" value={formatDateTime(product.createdAt)} />
              <Field label="Cập nhật gần nhất" value={formatDateTime(product.updatedAt)} />
            </div>
          </Card>

          <StockSection productId={product.id} canManage={canManage} onStockChanged={load} />
        </div>

        <div className="space-y-4">
          <Card title="Tồn kho hiện tại">
            <p className={`text-3xl font-semibold ${product.soLuongTon <= 0 ? "text-danger-600" : "text-ink-900"}`}>
              {product.soLuongTon}
            </p>
            <p className="text-xs text-ink-400 mt-1">{product.donVi || "đơn vị"}</p>
          </Card>
        </div>
      </div>
    </div>
  );
}
