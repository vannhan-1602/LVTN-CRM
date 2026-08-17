import { useState } from "react";
import contractApi from "../../api/contractApi";
import Modal from "../../components/common/Modal";
import Button from "../../components/common/Button";
import MoneyInput from "../../components/common/MoneyInput";
import { getApiErrorMessage } from "../../utils/formatters";
import { Plus, X } from "lucide-react";

// Modal gia hạn hợp đồng — tạo 1 hợp đồng mới (LoaiHopDong=GiaHan) kế thừa KhachHangId/
// ThoiHan/HinhThucThanhToan từ hợp đồng cũ, đồng thời chuyển hợp đồng cũ sang Thanh lý.
// Nếu hợp đồng cũ là TraGop, backend BẮT BUỘC phải có lịch trả góp mới (không tự kế thừa
// lịch cũ vì kỳ hạn mới có thể đổi số đợt/số tiền) — nên form này luôn hỏi lại từ đầu.
export default function RenewContractModal({ contract, onClose, onSaved }) {
  const isTraGop = contract.hinhThucThanhToan === "TraGop";

  const [ngayKy, setNgayKy] = useState(new Date().toISOString().slice(0, 10));
  const [lichThanhToans, setLichThanhToans] = useState([
    {
      soDot: 1,
      soTien: "",
      hanThanhToan: new Date().toISOString().slice(0, 10),
    },
  ]);
  const [submitting, setSubmitting] = useState(false);
  const [error, setError] = useState("");

  const addDot = () =>
    setLichThanhToans((prev) => [
      ...prev,
      {
        soDot: prev.length + 1,
        soTien: "",
        hanThanhToan: new Date().toISOString().slice(0, 10),
      },
    ]);

  const removeDot = (idx) =>
    setLichThanhToans((prev) =>
      prev
        .filter((_, i) => i !== idx)
        .map((item, i) => ({ ...item, soDot: i + 1 })),
    );

  const updateDot = (idx, field, value) =>
    setLichThanhToans((prev) =>
      prev.map((item, i) => (i === idx ? { ...item, [field]: value } : item)),
    );

  const handleSubmit = async (e) => {
    e.preventDefault();
    if (isTraGop && lichThanhToans.length === 0) {
      setError(
        "Hợp đồng trả góp phải có ít nhất 1 đợt thanh toán cho kỳ gia hạn",
      );
      return;
    }
    setSubmitting(true);
    setError("");
    try {
      await contractApi.renew(contract.id, {
        ngayKy: ngayKy || null,
        lichThanhToans: isTraGop
          ? lichThanhToans.map((l) => ({
              soDot: l.soDot,
              soTien: Number(l.soTien) || 0,
              hanThanhToan: l.hanThanhToan,
            }))
          : [],
      });
      onSaved();
    } catch (err) {
      setError(getApiErrorMessage(err, "Không thể gia hạn hợp đồng"));
    } finally {
      setSubmitting(false);
    }
  };

  return (
    <Modal
      isOpen
      onClose={onClose}
      title={`Gia hạn hợp đồng ${contract.maHopDong}`}
      size="md"
    >
      <form
        onSubmit={handleSubmit}
        className="space-y-4 max-h-[80vh] overflow-y-auto px-1"
      >
        <p className="text-xs text-ink-400">
          Hệ thống sẽ tạo 1 hợp đồng mới (kế thừa khách hàng, thời hạn
          {contract.thoiHan ? ` ${contract.thoiHan} tháng` : ""}, hình thức
          thanh toán {isTraGop ? "trả góp" : "1 lần"}) và chuyển hợp đồng hiện
          tại sang trạng thái Thanh lý.
        </p>

        <div>
          <label className="block text-sm font-medium text-ink-700 mb-1">
            Ngày ký hợp đồng mới
          </label>
          <input
            type="date"
            value={ngayKy}
            onChange={(e) => setNgayKy(e.target.value)}
            className="w-full border border-ink-200 rounded-lg px-3 py-2 text-sm"
          />
        </div>

        {isTraGop && (
          <div className="space-y-3 border border-ink-200 rounded-lg p-3 bg-surface-alt">
            <div className="flex items-center justify-between">
              <span className="text-xs font-semibold text-ink-700 uppercase">
                Lịch trả góp cho kỳ gia hạn
              </span>
              <button
                type="button"
                onClick={addDot}
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
                <MoneyInput
                  value={dot.soTien}
                  onChange={(n) => updateDot(idx, "soTien", n)}
                  placeholder="Số tiền"
                  className="flex-1 border border-ink-200 rounded-lg px-2 py-1.5 text-xs"
                />
                <input
                  type="date"
                  value={dot.hanThanhToan}
                  onChange={(e) =>
                    updateDot(idx, "hanThanhToan", e.target.value)
                  }
                  className="w-32 border border-ink-200 rounded-lg px-2 py-1.5 text-xs"
                />
                {lichThanhToans.length > 1 && (
                  <button
                    type="button"
                    onClick={() => removeDot(idx)}
                    className="text-danger-500 hover:text-danger-700 p-1"
                  >
                    <X size={15} />
                  </button>
                )}
              </div>
            ))}
          </div>
        )}

        {error && (
          <div className="text-sm text-danger-600 bg-danger-50 rounded-lg p-2.5">
            {error}
          </div>
        )}

        <div className="flex gap-2 pt-2">
          <Button type="submit" disabled={submitting} className="flex-1">
            {submitting ? "Đang gia hạn..." : "Gia hạn hợp đồng"}
          </Button>
          <Button type="button" variant="secondary" onClick={onClose}>
            Hủy
          </Button>
        </div>
      </form>
    </Modal>
  );
}
