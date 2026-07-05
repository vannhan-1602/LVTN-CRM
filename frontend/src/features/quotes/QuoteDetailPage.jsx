import { useEffect, useState } from "react";
import { useNavigate, useParams } from "react-router-dom";
import { Pencil, Trash2, Send, CheckCircle2, XCircle } from "lucide-react";
import quoteApi from "../../api/quoteApi";
import customerApi from "../../api/customerApi";
import productApi from "../../api/productApi";
import useAuthStore from "../auth/authStore";
import PageHeader from "../../components/common/PageHeader";
import Card, { Field } from "../../components/common/Card";
import Badge from "../../components/common/Badge";
import Button from "../../components/common/Button";
import QuoteFormModal from "./QuoteFormModal";
import { ROLES, QUOTE_STATUS, QUOTE_STATUS_COLOR } from "../../utils/constants";
import { formatDateTime } from "../../utils/formatters";

function formatMoney(n) {
  return Number(n || 0).toLocaleString("vi-VN") + " đ";
}

export default function QuoteDetailPage() {
  const { id } = useParams();
  const navigate = useNavigate();
  const { user } = useAuthStore();
  const canEdit = [ROLES.Sale, ROLES.Manager].includes(user?.role);
  const canDelete = user?.role === ROLES.Manager;

  const [quote, setQuote] = useState(null);
  const [customers, setCustomers] = useState([]);
  const [products, setProducts] = useState([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState("");
  const [busy, setBusy] = useState(false);
  const [showEditModal, setShowEditModal] = useState(false);
  const [showRejectForm, setShowRejectForm] = useState(false);
  const [rejectReason, setRejectReason] = useState("");

  const load = async () => {
    setLoading(true); setError("");
    try {
      const res = await quoteApi.getById(id);
      setQuote(res.data ?? null);
    } catch (err) { setError(err?.message || "Không thể tải báo giá"); }
    finally { setLoading(false); }
  };

  useEffect(() => { load(); }, [id]);
  useEffect(() => {
    Promise.all([customerApi.getAll({ pageSize: 100 }), productApi.getAll({ pageSize: 200, dangKinhDoanh: true })])
      .then(([cRes, pRes]) => { setCustomers(cRes.data?.items ?? []); setProducts(pRes.data?.items ?? []); })
      .catch(() => {});
  }, []);

  const doAction = async (fn) => {
    setBusy(true); setError("");
    try { await fn(); await load(); }
    catch (err) { setError(err?.message || "Thao tác thất bại"); }
    finally { setBusy(false); }
  };

  const handleDelete = async () => {
    if (!window.confirm("Xóa báo giá nháp này?")) return;
    try { await quoteApi.delete(id); navigate("/quotes"); }
    catch (err) { setError(err?.message || "Không thể xóa"); }
  };

  const handleConfirmReject = async () => {
    await doAction(() => quoteApi.reject(quote.id, rejectReason.trim() || null));
    setShowRejectForm(false);
    setRejectReason("");
  };

  if (loading) return <div className="text-sm text-ink-400 py-10 text-center">Đang tải...</div>;

  if (error && !quote) {
    return (
      <div className="space-y-4">
        <PageHeader breadcrumb="CRM / Kinh doanh" title="Báo giá" onBack={() => navigate("/quotes")} />
        <div className="text-sm text-danger-600 bg-danger-50 rounded-lg p-4">{error}</div>
      </div>
    );
  }
  if (!quote) return null;

  const isDraft = quote.trangThai === "Nhap";

  return (
    <div className="space-y-5">
      {showEditModal && (
        <QuoteFormModal quote={quote} customers={customers} products={products}
          onClose={() => setShowEditModal(false)} onSaved={() => { setShowEditModal(false); load(); }} />
      )}

      <PageHeader
        breadcrumb="Báo giá"
        title={quote.maBaoGia}
        onBack={() => navigate("/quotes")}
        badge={<Badge label={QUOTE_STATUS[quote.trangThai] ?? quote.trangThai} colorClass={QUOTE_STATUS_COLOR[quote.trangThai]} />}
        actions={
          <>
            {canEdit && isDraft && <Button variant="secondary" icon={Pencil} onClick={() => setShowEditModal(true)}>Sửa</Button>}
            {canDelete && isDraft && <Button variant="danger" icon={Trash2} onClick={handleDelete}>Xóa</Button>}
          </>
        }
      />

      {error && <div className="text-sm text-danger-600 bg-danger-50 rounded-lg p-3">{error}</div>}

      <div className="grid grid-cols-1 lg:grid-cols-3 gap-4">
        <div className="lg:col-span-2 space-y-4">
          <Card title="Danh sách sản phẩm">
            <table className="w-full text-sm">
              <thead>
                <tr className="text-xs text-ink-400 uppercase border-b border-ink-100">
                  <th className="text-left py-2">Sản phẩm</th>
                  <th className="text-right py-2">SL</th>
                  <th className="text-right py-2">Đơn giá</th>
                  <th className="text-right py-2">Thành tiền</th>
                </tr>
              </thead>
              <tbody className="divide-y divide-ink-100">
                {quote.chiTiet?.map((c) => (
                  <tr key={c.id}>
                    <td className="py-2.5 text-ink-900">{c.tenSP} <span className="text-ink-400 text-xs">({c.maSP})</span></td>
                    <td className="py-2.5 text-right text-ink-700">{c.soLuong}</td>
                    <td className="py-2.5 text-right text-ink-700">{formatMoney(c.donGia)}</td>
                    <td className="py-2.5 text-right font-medium text-ink-900">{formatMoney(c.thanhTien)}</td>
                  </tr>
                ))}
              </tbody>
            </table>
            <div className="text-right font-semibold text-lg border-t border-ink-100 pt-3 mt-1 text-ink-900">
              Tổng: <span className="text-accent-600">{formatMoney(quote.tongTien)}</span>
            </div>
          </Card>

          <Card title="Thông tin báo giá">
            <div className="grid grid-cols-2 gap-5">
              <Field label="Khách hàng" value={quote.tenKhachHang} />
              <Field label="Ngày tạo" value={formatDateTime(quote.createdAt)} />
              <Field label="Cập nhật gần nhất" value={formatDateTime(quote.updatedAt)} />
            </div>
          </Card>
        </div>

        <div className="space-y-4">
          <Card title="Hành động">
            <div className="space-y-2">
              {quote.trangThai === "Nhap" && canEdit && (
                <Button size="sm" icon={Send} className="w-full" disabled={busy}
                  onClick={() => doAction(() => quoteApi.send(quote.id))}>
                  Gửi báo giá
                </Button>
              )}
              {quote.trangThai === "DaGui" && canEdit && (
                <>
                  <Button size="sm" icon={CheckCircle2} className="w-full" disabled={busy}
                    onClick={() => doAction(() => quoteApi.accept(quote.id))}>
                    Khách chấp nhận
                  </Button>
                  {!showRejectForm ? (
                    <Button size="sm" variant="danger" icon={XCircle} className="w-full" disabled={busy}
                      onClick={() => setShowRejectForm(true)}>
                      Khách từ chối
                    </Button>
                  ) : (
                    <div className="space-y-2 border border-ink-100 rounded-lg p-2.5">
                      <label className="block text-xs font-medium text-ink-700">
                        Lý do từ chối (không bắt buộc)
                      </label>
                      <textarea
                        value={rejectReason}
                        onChange={(e) => setRejectReason(e.target.value)}
                        maxLength={500}
                        rows={3}
                        placeholder="VD: Giá chưa phù hợp, khách chọn nhà cung cấp khác..."
                        className="w-full border border-ink-200 rounded-lg px-2.5 py-1.5 text-sm focus:outline-none focus:ring-2 focus:ring-accent-400/40 focus:border-accent-400"
                      />
                      <div className="flex gap-2">
                        <Button size="sm" variant="danger" className="flex-1" disabled={busy}
                          onClick={handleConfirmReject}>
                          Xác nhận từ chối
                        </Button>
                        <Button size="sm" variant="secondary" disabled={busy}
                          onClick={() => { setShowRejectForm(false); setRejectReason(""); }}>
                          Hủy
                        </Button>
                      </div>
                    </div>
                  )}
                </>
              )}
              {quote.trangThai === "ChapNhan" && (
                <p className="text-xs text-success-700 bg-success-50 rounded-lg p-2.5">
                  Đã chấp nhận — có thể tạo hợp đồng từ trang Hợp đồng.
                </p>
              )}
              {quote.trangThai === "TuChoi" && (
                <div className="text-xs text-ink-500 bg-ink-50 rounded-lg p-2.5 space-y-1">
                  <p className="text-ink-700 font-medium">Khách hàng đã từ chối báo giá này.</p>
                  {quote.lyDoTuChoi && (
                    <p><span className="text-ink-400">Lý do: </span>{quote.lyDoTuChoi}</p>
                  )}
                </div>
              )}
            </div>
          </Card>
        </div>
      </div>
    </div>
  );
}
