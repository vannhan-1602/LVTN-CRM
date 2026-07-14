import { useEffect, useState } from "react";
import { useParams } from "react-router-dom";
import { CheckCircle2, XCircle, FileText } from "lucide-react";
import quotePublicApi from "../../api/quotePublicApi";

function formatMoney(n) {
  return Number(n || 0).toLocaleString("vi-VN") + " đ";
}

const STATUS_LABEL = {
  Nhap: "Nháp",
  DaGui: "Đã gửi",
  TuChoi: "Đã từ chối",
  ChapNhan: "Đã chấp nhận",
};

export default function PublicQuotePage() {
  const { token } = useParams();
  const [quote, setQuote] = useState(null);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState("");
  const [busy, setBusy] = useState(false);
  const [showRejectForm, setShowRejectForm] = useState(false);
  const [rejectReason, setRejectReason] = useState("");
  const [doneMessage, setDoneMessage] = useState("");

  const load = async () => {
    setLoading(true);
    setError("");
    try {
      const res = await quotePublicApi.getByToken(token);
      setQuote(res.data ?? null);
    } catch (err) {
      setError(err?.message || "Không tìm thấy báo giá hoặc liên kết đã hết hạn.");
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => {
    load();
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [token]);

  const handleAccept = async () => {
    setBusy(true);
    setError("");
    try {
      const res = await quotePublicApi.accept(token);
      setDoneMessage(res.message || "Bạn đã chấp nhận báo giá. Cảm ơn quý khách!");
      await load();
    } catch (err) {
      setError(err?.message || "Không thể chấp nhận báo giá.");
    } finally {
      setBusy(false);
    }
  };

  const handleReject = async () => {
    setBusy(true);
    setError("");
    try {
      const res = await quotePublicApi.reject(token, rejectReason.trim() || null);
      setDoneMessage(res.message || "Bạn đã từ chối báo giá.");
      setShowRejectForm(false);
      await load();
    } catch (err) {
      setError(err?.message || "Không thể từ chối báo giá.");
    } finally {
      setBusy(false);
    }
  };

  return (
    <div className="min-h-screen bg-surface-alt flex items-start justify-center py-10 px-4">
      <div className="w-full max-w-2xl space-y-4">
        <div className="text-center">
          <div className="inline-flex items-center gap-2 text-ink-900 font-semibold text-lg">
            <FileText size={20} /> CRM System
          </div>
        </div>

        <div className="bg-surface rounded-card border border-ink-100 p-6">
          {loading ? (
            <p className="text-center text-ink-400 py-10">Đang tải báo giá...</p>
          ) : error && !quote ? (
            <p className="text-center text-danger-600 py-10">{error}</p>
          ) : quote ? (
            <div className="space-y-5">
              <div className="flex items-center justify-between flex-wrap gap-2">
                <div>
                  <p className="text-xs text-ink-400">Báo giá</p>
                  <h1 className="text-xl font-bold text-ink-900">{quote.maBaoGia}</h1>
                </div>
                <span className="text-xs font-medium px-3 py-1.5 rounded-full bg-ink-100 text-ink-600">
                  {STATUS_LABEL[quote.trangThai] ?? quote.trangThai}
                </span>
              </div>

              <div className="text-sm text-ink-500">
                Kính gửi <span className="font-medium text-ink-900">{quote.tenKhachHang}</span>,
                dưới đây là chi tiết báo giá của quý khách.
              </div>

              <div className="border border-ink-100 rounded-lg overflow-hidden">
                <table className="w-full text-sm">
                  <thead>
                    <tr className="bg-surface-alt">
                      <th className="px-4 py-2.5 text-left text-xs font-medium text-ink-400 uppercase">Sản phẩm</th>
                      <th className="px-4 py-2.5 text-right text-xs font-medium text-ink-400 uppercase">SL</th>
                      <th className="px-4 py-2.5 text-right text-xs font-medium text-ink-400 uppercase">Đơn giá</th>
                      <th className="px-4 py-2.5 text-right text-xs font-medium text-ink-400 uppercase">Thành tiền</th>
                    </tr>
                  </thead>
                  <tbody className="divide-y divide-ink-100">
                    {(quote.chiTiet ?? []).map((item) => (
                      <tr key={item.id}>
                        <td className="px-4 py-2.5">
                          <div className="font-medium text-ink-900">{item.tenSP}</div>
                          {item.maSP && <div className="text-xs text-ink-400">{item.maSP}</div>}
                        </td>
                        <td className="px-4 py-2.5 text-right">{item.soLuong}</td>
                        <td className="px-4 py-2.5 text-right">{formatMoney(item.donGia)}</td>
                        <td className="px-4 py-2.5 text-right font-medium text-ink-900">
                          {formatMoney(item.thanhTien)}
                        </td>
                      </tr>
                    ))}
                  </tbody>
                </table>
                <div className="px-4 py-3 bg-surface-alt flex justify-end">
                  <span className="text-sm text-ink-500 mr-2">Tổng cộng:</span>
                  <span className="font-bold text-ink-900">{formatMoney(quote.tongTien)}</span>
                </div>
              </div>

              {doneMessage && (
                <div className="text-sm text-success-700 bg-success-50 rounded-lg p-3">{doneMessage}</div>
              )}
              {error && <div className="text-sm text-danger-600 bg-danger-50 rounded-lg p-3">{error}</div>}

              {quote.trangThai === "DaGui" && !doneMessage && (
                <div className="space-y-3 pt-2">
                  {!showRejectForm ? (
                    <div className="flex gap-3">
                      <button
                        onClick={handleAccept}
                        disabled={busy}
                        className="flex-1 inline-flex items-center justify-center gap-2 bg-success-600 hover:bg-success-700 text-white font-medium py-2.5 rounded-lg text-sm disabled:opacity-60"
                      >
                        <CheckCircle2 size={16} /> Chấp nhận báo giá
                      </button>
                      <button
                        onClick={() => setShowRejectForm(true)}
                        disabled={busy}
                        className="flex-1 inline-flex items-center justify-center gap-2 border border-danger-200 text-danger-600 hover:bg-danger-50 font-medium py-2.5 rounded-lg text-sm disabled:opacity-60"
                      >
                        <XCircle size={16} /> Từ chối
                      </button>
                    </div>
                  ) : (
                    <div className="space-y-2">
                      <textarea
                        rows={2}
                        value={rejectReason}
                        onChange={(e) => setRejectReason(e.target.value)}
                        placeholder="Lý do từ chối (không bắt buộc)..."
                        className="w-full border border-ink-200 rounded-lg px-3 py-2 text-sm resize-none focus:outline-none focus:ring-2 focus:ring-accent-400/40"
                      />
                      <div className="flex gap-2">
                        <button
                          onClick={handleReject}
                          disabled={busy}
                          className="flex-1 bg-danger-600 hover:bg-danger-700 text-white font-medium py-2 rounded-lg text-sm disabled:opacity-60"
                        >
                          {busy ? "Đang gửi..." : "Xác nhận từ chối"}
                        </button>
                        <button
                          onClick={() => setShowRejectForm(false)}
                          className="px-4 py-2 border border-ink-200 rounded-lg text-sm"
                        >
                          Hủy
                        </button>
                      </div>
                    </div>
                  )}
                </div>
              )}
            </div>
          ) : null}
        </div>
      </div>
    </div>
  );
}
