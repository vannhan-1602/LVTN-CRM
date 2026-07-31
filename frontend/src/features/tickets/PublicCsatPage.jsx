import { useEffect, useState } from "react";
import { useParams } from "react-router-dom";
import { Star, LifeBuoy } from "lucide-react";
import ticketPublicApi from "../../api/ticketPublicApi";
import { getApiErrorMessage } from "../../utils/formatters";

export default function PublicCsatPage() {
  const { token } = useParams();
  const [csat, setCsat] = useState(null);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState("");
  const [busy, setBusy] = useState(false);
  const [diem, setDiem] = useState(0);
  const [hoverDiem, setHoverDiem] = useState(0);
  const [nhanXet, setNhanXet] = useState("");
  const [doneMessage, setDoneMessage] = useState("");

  const load = async () => {
    setLoading(true);
    setError("");
    try {
      const res = await ticketPublicApi.getCsatByToken(token);
      setCsat(res.data ?? null);
    } catch (err) {
      setError(getApiErrorMessage(err, "Không tìm thấy liên kết khảo sát hoặc đã hết hạn."));
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => {
    load();
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [token]);

  const handleSubmit = async () => {
    if (diem < 1) {
      setError("Vui lòng chọn số sao đánh giá.");
      return;
    }
    setBusy(true);
    setError("");
    try {
      const res = await ticketPublicApi.submitCsat(token, diem, nhanXet.trim() || null);
      setDoneMessage(res.message || "Cảm ơn quý khách đã đánh giá dịch vụ!");
      setCsat(res.data ?? null);
    } catch (err) {
      setError(getApiErrorMessage(err, "Không thể gửi đánh giá. Có thể liên kết đã được sử dụng."));
    } finally {
      setBusy(false);
    }
  };

  const daDanhGia = Boolean(csat?.daDanhGia);
  const activeDiem = hoverDiem || diem;

  return (
    <div className="min-h-screen bg-surface-alt flex items-start justify-center py-10 px-4">
      <div className="w-full max-w-lg space-y-4">
        <div className="text-center">
          <div className="inline-flex items-center gap-2 text-ink-900 font-semibold text-lg">
            <LifeBuoy size={20} /> CRM System
          </div>
        </div>

        <div className="bg-surface rounded-card border border-ink-100 p-6">
          {loading ? (
            <p className="text-center text-ink-400 py-10">Đang tải khảo sát...</p>
          ) : error && !csat ? (
            <p className="text-center text-danger-600 py-10">{error}</p>
          ) : csat ? (
            <div className="space-y-5">
              <div className="text-center">
                <p className="text-xs text-ink-400">Ticket {csat.maTicket}</p>
                <h1 className="text-lg font-bold text-ink-900">{csat.tieuDeTicket}</h1>
              </div>

              {daDanhGia || doneMessage ? (
                <div className="text-center space-y-3 py-4">
                  <div className="flex items-center justify-center gap-1">
                    {[1, 2, 3, 4, 5].map((n) => (
                      <Star
                        key={n}
                        size={28}
                        className={n <= (csat.diemDanhGia ?? diem) ? "fill-purple-500 text-purple-500" : "text-ink-200"}
                      />
                    ))}
                  </div>
                  <p className="text-sm text-success-700 bg-success-50 rounded-lg p-3">
                    {doneMessage || "Quý khách đã đánh giá dịch vụ này. Cảm ơn quý khách!"}
                  </p>
                  {csat.nhanXet && (
                    <p className="text-sm text-ink-700 bg-surface-alt rounded-lg p-3 text-left">
                      "{csat.nhanXet}"
                    </p>
                  )}
                </div>
              ) : (
                <div className="space-y-4">
                  <p className="text-sm text-ink-500 text-center">
                    Quý khách đánh giá thế nào về chất lượng xử lý ticket này?
                  </p>
                  <div className="flex items-center justify-center gap-1.5" onMouseLeave={() => setHoverDiem(0)}>
                    {[1, 2, 3, 4, 5].map((n) => (
                      <button
                        key={n}
                        type="button"
                        onClick={() => setDiem(n)}
                        onMouseEnter={() => setHoverDiem(n)}
                        className="p-1"
                      >
                        <Star
                          size={32}
                          className={n <= activeDiem ? "fill-purple-500 text-purple-500" : "text-ink-200"}
                        />
                      </button>
                    ))}
                  </div>
                  <textarea
                    rows={3}
                    value={nhanXet}
                    onChange={(e) => setNhanXet(e.target.value)}
                    placeholder="Nhận xét thêm (không bắt buộc)..."
                    className="w-full border border-ink-200 rounded-lg px-3 py-2 text-sm resize-none focus:outline-none focus:ring-2 focus:ring-accent-400/40"
                  />

                  {error && <div className="text-sm text-danger-600 bg-danger-50 rounded-lg p-3">{error}</div>}

                  <button
                    onClick={handleSubmit}
                    disabled={busy}
                    className="w-full bg-purple-600 hover:bg-purple-700 text-white font-medium py-2.5 rounded-lg text-sm disabled:opacity-60"
                  >
                    {busy ? "Đang gửi..." : "Gửi đánh giá"}
                  </button>
                </div>
              )}
            </div>
          ) : null}
        </div>
      </div>
    </div>
  );
}
