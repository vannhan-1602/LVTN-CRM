import { useEffect, useState } from "react";
import contractApi from "../../api/contractApi";
import quoteApi from "../../api/quoteApi";
import Modal from "../../components/common/Modal";
import Button from "../../components/common/Button";
import { Plus, X } from "lucide-react";

function formatMoney(n) {
  return n == null ? "" : Number(n).toLocaleString("vi-VN") + " đ";
}

export default function CreateContractModal({ onClose, onSaved }) {
  const [acceptedQuotes, setAcceptedQuotes] = useState([]);
  const [baoGiaId, setBaoGiaId] = useState("");
  const [ngayKy, setNgayKy] = useState(new Date().toISOString().slice(0, 10));
  const [thoiHan, setThoiHan] = useState("12");

  // Bổ sung state cho Phase 1: Hình thức thanh toán & Lịch trả góp
  const [hinhThucThanhToan, setHinhThucThanhToan] = useState("ThanhToanMotLan");
  const [lichThanhToans, setLichThanhToans] = useState([
    {
      soDot: 1,
      soTien: "",
      hanThanhToan: new Date().toISOString().slice(0, 10),
    },
  ]);

  const [loadingQuotes, setLoadingQuotes] = useState(true);
  const [submitting, setSubmitting] = useState(false);
  const [error, setError] = useState("");

  useEffect(() => {
    (async () => {
      try {
        const res = await quoteApi.getAll({
          pageSize: 100,
          trangThai: "ChapNhan",
        });
        setAcceptedQuotes(res.data?.items ?? []);
      } catch {
        setError("Không thể tải danh sách báo giá đã chấp nhận");
      } finally {
        setLoadingQuotes(false);
      }
    })();
  }, []);

  const addDotTraGop = () => {
    setLichThanhToans((prev) => [
      ...prev,
      {
        soDot: prev.length + 1,
        soTien: "",
        hanThanhToan: new Date().toISOString().slice(0, 10),
      },
    ]);
  };

  const removeDotTraGop = (idx) => {
    setLichThanhToans((prev) =>
      prev
        .filter((_, i) => i !== idx)
        .map((item, index) => ({ ...item, soDot: index + 1 })),
    );
  };

  const updateDotTraGop = (idx, field, value) => {
    setLichThanhToans((prev) =>
      prev.map((item, i) => (i === idx ? { ...item, [field]: value } : item)),
    );
  };

  const handleSubmit = async (e) => {
    e.preventDefault();
    if (!baoGiaId) {
      setError("Vui lòng chọn báo giá");
      return;
    }

    if (hinhThucThanhToan === "TraGop") {
      if (lichThanhToans.length === 0) {
        setError("Hợp đồng trả góp phải có ít nhất 1 đợt thanh toán");
        return;
      }
    }

    setSubmitting(true);
    setError("");

    try {
      await contractApi.createFromQuote({
        baoGiaId: Number(baoGiaId),
        ngayKy: ngayKy || null,
        thoiHan: thoiHan ? Number(thoiHan) : null,
        hinhThucThanhToan,
        lichThanhToans:
          hinhThucThanhToan === "TraGop"
            ? lichThanhToans.map((l) => ({
                soDot: l.soDot,
                soTien: Number(l.soTien) || 0,
                hanThanhToan: l.hanThanhToan,
              }))
            : [],
      });
      onSaved();
    } catch (err) {
      setError(err?.message || "Không thể tạo hợp đồng");
    } finally {
      setSubmitting(false);
    }
  };

  return (
    <Modal isOpen onClose={onClose} title="Tạo hợp đồng từ báo giá" size="md">
      <form
        onSubmit={handleSubmit}
        className="space-y-4 max-h-[80vh] overflow-y-auto px-1"
      >
        {loadingQuotes ? (
          <p className="text-sm text-ink-400">Đang tải danh sách báo giá...</p>
        ) : acceptedQuotes.length === 0 ? (
          <p className="text-sm text-ink-500 bg-ink-100 rounded-lg p-3">
            Chưa có báo giá nào đạt trạng thái "Đã chấp nhận". Hãy chuyển trạng
            thái báo giá sang "Đã chấp nhận" ở trang Báo giá trước khi tạo hợp
            đồng.
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
                className="w-full border border-ink-200 rounded-lg px-3 py-2 text-sm focus:outline-none focus:ring-2 focus:ring-accent-400/40"
              >
                <option value="">-- Chọn báo giá --</option>
                {acceptedQuotes.map((q) => (
                  <option key={q.id} value={q.id}>
                    {q.maBaoGia} - {q.tenKhachHang} - {formatMoney(q.tongTien)}
                  </option>
                ))}
              </select>
            </div>

            <div className="grid grid-cols-2 gap-3">
              <div>
                <label className="block text-sm font-medium text-ink-700 mb-1.5">
                  Ngày ký
                </label>
                <input
                  type="date"
                  value={ngayKy}
                  onChange={(e) => setNgayKy(e.target.value)}
                  className="w-full border border-ink-200 rounded-lg px-3 py-2 text-sm focus:outline-none focus:ring-2 focus:ring-accent-400/40"
                />
              </div>
              <div>
                <label className="block text-sm font-medium text-ink-700 mb-1.5">
                  Thời hạn (tháng)
                </label>
                <input
                  type="number"
                  min="1"
                  value={thoiHan}
                  onChange={(e) => setThoiHan(e.target.value)}
                  className="w-full border border-ink-200 rounded-lg px-3 py-2 text-sm focus:outline-none focus:ring-2 focus:ring-accent-400/40"
                />
              </div>
            </div>

            {/* Bổ sung lựa chọn hình thức thanh toán */}
            <div>
              <label className="block text-sm font-medium text-ink-700 mb-1.5">
                Hình thức thanh toán
              </label>
              <select
                value={hinhThucThanhToan}
                onChange={(e) => setHinhThucThanhToan(e.target.value)}
                className="w-full border border-ink-200 rounded-lg px-3 py-2 text-sm focus:outline-none focus:ring-2 focus:ring-accent-400/40"
              >
                <option value="ThanhToanMotLan">Thanh toán 1 lần</option>
                <option value="TraGop">Trả góp định kỳ</option>
              </select>
            </div>

            {/* Giao diện nhập lịch trả góp nếu chọn TraGop */}
            {hinhThucThanhToan === "TraGop" && (
              <div className="space-y-3 border border-ink-200 rounded-lg p-3 bg-surface-alt">
                <div className="flex items-center justify-between">
                  <span className="text-xs font-semibold text-ink-700 uppercase">
                    Kế hoạch các đợt thanh toán
                  </span>
                  <button
                    type="button"
                    onClick={addDotTraGop}
                    className="text-xs text-accent-600 font-medium hover:underline inline-flex items-center gap-1"
                  >
                    <Plus size={13} /> Thêm đợt
                  </button>
                </div>

                {lichThanhToans.map((dot, idx) => (
                  <div key={idx} className="flex gap-2 items-center">
                    <span className="text-xs font-medium text-ink-500 w-12">
                      Đợt {dot.soDot}
                    </span>
                    <input
                      type="number"
                      placeholder="Số tiền"
                      value={dot.soTien}
                      onChange={(e) =>
                        updateDotTraGop(idx, "soTien", e.target.value)
                      }
                      className="flex-1 border border-ink-200 rounded-lg px-2 py-1.5 text-xs"
                    />
                    <input
                      type="date"
                      value={dot.hanThanhToan}
                      onChange={(e) =>
                        updateDotTraGop(idx, "hanThanhToan", e.target.value)
                      }
                      className="w-32 border border-ink-200 rounded-lg px-2 py-1.5 text-xs"
                    />
                    {lichThanhToans.length > 1 && (
                      <button
                        type="button"
                        onClick={() => removeDotTraGop(idx)}
                        className="text-danger-500 hover:text-danger-700 p-1"
                      >
                        <X size={15} />
                      </button>
                    )}
                  </div>
                ))}
              </div>
            )}
          </>
        )}

        {error && (
          <div className="text-sm text-danger-600 bg-danger-50 rounded-lg p-2.5">
            {error}
          </div>
        )}

        <div className="flex gap-2 pt-2">
          <Button
            type="submit"
            disabled={submitting || acceptedQuotes.length === 0}
            className="flex-1"
          >
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
