import { useState } from "react";
import contractApi from "../../api/contractApi";
import Modal from "../../components/common/Modal";
import Button from "../../components/common/Button";

// Modal tạo hợp đồng gia hạn từ hợp đồng hiện tại.
// Gọi POST /Contract/{id}/renew — backend tự copy điều khoản (khách hàng, thời hạn,
// hình thức thanh toán), liên kết hợp đồng mới về hợp đồng cũ, và chuyển hợp đồng
// cũ sang trạng thái Thanh lý.
export default function RenewContractModal({ contract, onClose, onRenewed }) {
  const [ngayKy, setNgayKy] = useState(new Date().toISOString().slice(0, 10));
  const [submitting, setSubmitting] = useState(false);
  const [error, setError] = useState("");

  const handleSubmit = async (e) => {
    e.preventDefault();
    setSubmitting(true);
    setError("");
    try {
      const res = await contractApi.renew(contract.id, ngayKy || null);
      onRenewed(res.data);
    } catch (err) {
      setError(
        err?.response?.data?.message ||
          err?.message ||
          "Không thể tạo hợp đồng gia hạn",
      );
    } finally {
      setSubmitting(false);
    }
  };

  return (
    <Modal
      isOpen
      onClose={onClose}
      title={`Gia hạn hợp đồng ${contract.maHopDong}`}
      size="sm"
    >
      <form onSubmit={handleSubmit} className="space-y-4">
        <p className="text-sm text-ink-500">
          Hệ thống sẽ tạo một hợp đồng mới giữ nguyên khách hàng, thời hạn và
          hình thức thanh toán của hợp đồng này. Hợp đồng {contract.maHopDong}{" "}
          sẽ tự động chuyển sang trạng thái <strong>Thanh lý</strong>.
        </p>

        <div>
          <label className="block text-sm font-medium text-ink-700 mb-1.5">
            Ngày ký hợp đồng gia hạn
          </label>
          <input
            type="date"
            value={ngayKy}
            onChange={(e) => setNgayKy(e.target.value)}
            className="w-full border border-ink-200 rounded-lg px-3 py-2 text-sm focus:outline-none focus:ring-2 focus:ring-accent-400/40 focus:border-accent-400"
          />
        </div>

        {error && (
          <div className="text-sm text-danger-600 bg-danger-50 rounded-lg p-2.5">
            {error}
          </div>
        )}

        <div className="flex gap-2 pt-1">
          <Button type="submit" disabled={submitting} className="flex-1">
            {submitting ? "Đang tạo..." : "Tạo hợp đồng gia hạn"}
          </Button>
          <Button type="button" variant="secondary" onClick={onClose}>
            Hủy
          </Button>
        </div>
      </form>
    </Modal>
  );
}
