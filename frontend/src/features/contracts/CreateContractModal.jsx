import { useEffect, useState } from "react";
import contractApi from "../../api/contractApi";
import quoteApi from "../../api/quoteApi";
import Modal from "../../components/common/Modal";
import Button from "../../components/common/Button";

function formatMoney(n) {
  return n == null ? "—" : Number(n).toLocaleString("vi-VN") + " đ";
}

// Modal tạo hợp đồng từ một báo giá đã được khách hàng chấp nhận (trạng thái ChapNhan).
export default function CreateContractModal({ onClose, onSaved }) {
  const [acceptedQuotes, setAcceptedQuotes] = useState([]);
  const [baoGiaId, setBaoGiaId] = useState("");
  const [ngayKy, setNgayKy] = useState(new Date().toISOString().slice(0, 10));
  const [thoiHan, setThoiHan] = useState("12");
  const [loadingQuotes, setLoadingQuotes] = useState(true);
  const [submitting, setSubmitting] = useState(false);
  const [error, setError] = useState("");

  useEffect(() => {
    (async () => {
      try {
        const res = await quoteApi.getAll({ pageSize: 100, trangThai: "ChapNhan" });
        setAcceptedQuotes(res.data?.items ?? []);
      } catch {
        setError("Không thể tải danh sách báo giá đã chấp nhận");
      } finally {
        setLoadingQuotes(false);
      }
    })();
  }, []);

  const handleSubmit = async (e) => {
    e.preventDefault();
    if (!baoGiaId) {
      setError("Vui lòng chọn báo giá");
      return;
    }
    setSubmitting(true);
    setError("");
    try {
      await contractApi.createFromQuote({
        baoGiaId: Number(baoGiaId),
        ngayKy: ngayKy || null,
        thoiHan: thoiHan ? Number(thoiHan) : null,
      });
      onSaved();
    } catch (err) {
      setError(err?.message || "Không thể tạo hợp đồng");
    } finally {
      setSubmitting(false);
    }
  };

  return (
    <Modal isOpen onClose={onClose} title="Tạo hợp đồng từ báo giá" size="sm">
      <form onSubmit={handleSubmit} className="space-y-4">
        {loadingQuotes ? (
          <p className="text-sm text-ink-400">Đang tải danh sách báo giá...</p>
        ) : acceptedQuotes.length === 0 ? (
          <p className="text-sm text-ink-500 bg-ink-100 rounded-lg p-3">
            Chưa có báo giá nào ở trạng thái "Đã chấp nhận". Hãy chuyển trạng thái báo giá
            sang Đã chấp nhận ở trang Báo giá trước khi tạo hợp đồng.
          </p>
        ) : (
          <>
            <div>
              <label className="block text-sm font-medium text-ink-700 mb-1.5">
                Báo giá đã chấp nhận <span className="text-danger-500">*</span>
              </label>
              <select
                value={baoGiaId}
                onChange={(e) => setBaoGiaId(e.target.value)}
                className="w-full border border-ink-200 rounded-lg px-3 py-2 text-sm focus:outline-none focus:ring-2 focus:ring-accent-400/40 focus:border-accent-400"
              >
                <option value="">-- Chọn báo giá --</option>
                {acceptedQuotes.map((q) => (
                  <option key={q.id} value={q.id}>
                    {q.maBaoGia} — {q.tenKhachHang} — {formatMoney(q.tongTien)}
                  </option>
                ))}
              </select>
            </div>

            <div className="grid grid-cols-2 gap-3">
              <div>
                <label className="block text-sm font-medium text-ink-700 mb-1.5">Ngày ký</label>
                <input
                  type="date"
                  value={ngayKy}
                  onChange={(e) => setNgayKy(e.target.value)}
                  className="w-full border border-ink-200 rounded-lg px-3 py-2 text-sm focus:outline-none focus:ring-2 focus:ring-accent-400/40 focus:border-accent-400"
                />
              </div>
              <div>
                <label className="block text-sm font-medium text-ink-700 mb-1.5">Thời hạn (tháng)</label>
                <input
                  type="number"
                  min="1"
                  value={thoiHan}
                  onChange={(e) => setThoiHan(e.target.value)}
                  className="w-full border border-ink-200 rounded-lg px-3 py-2 text-sm focus:outline-none focus:ring-2 focus:ring-accent-400/40 focus:border-accent-400"
                />
              </div>
            </div>
          </>
        )}

        {error && <div className="text-sm text-danger-600 bg-danger-50 rounded-lg p-2.5">{error}</div>}

        <div className="flex gap-2 pt-1">
          <Button type="submit" disabled={submitting || acceptedQuotes.length === 0} className="flex-1">
            {submitting ? "Đang tạo..." : "Tạo hợp đồng"}
          </Button>
          <Button type="button" variant="secondary" onClick={onClose}>
            Hủy
          </Button>
        </div>
      </form>
    </Modal>
  );
}
