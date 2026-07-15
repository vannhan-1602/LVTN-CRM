import { useState } from "react";
import phieuThuChiApi from "../../api/phieuThuChiApi";
import Modal from "../../components/common/Modal";
import Button from "../../components/common/Button";
import { formatCurrency } from "../../utils/formatters";

export default function CreatePhieuThuModal({
  hoaDonId,
  maHoaDon,
  soTienConLai,
  onClose,
  onSaved,
}) {
  const [soTien, setSoTien] = useState(soTienConLai ?? "");
  const [submitting, setSubmitting] = useState(false);
  const [error, setError] = useState("");

  const handleSubmit = async (e) => {
    e.preventDefault();
    const amount = Number(soTien);

    if (!amount || amount <= 0) {
      setError("Số tiền thu phải lớn hơn 0");
      return;
    }
    if (soTienConLai != null && amount > soTienConLai) {
      setError(
        `Số tiền thu không được vượt quá số còn lại (${formatCurrency(soTienConLai)})`,
      );
      return;
    }

    setSubmitting(true);
    setError("");
    try {
      await phieuThuChiApi.create({
        hoaDonId: hoaDonId,
        khachHangId: null,
        loaiPhieu: "Thu",
        soTien: amount,
      });
      onSaved();
    } catch (err) {
      setError(err?.message || "Không thể tạo phiếu thu");
    } finally {
      setSubmitting(false);
    }
  };

  return (
    <Modal isOpen onClose={onClose} title="Tạo phiếu thu" size="sm">
      <form onSubmit={handleSubmit} className="space-y-4">
        <div className="text-sm bg-surface-alt rounded-lg p-3 space-y-1">
          <div className="flex justify-between">
            <span className="text-ink-400">Hóa đơn</span>
            <span className="font-medium text-ink-900">{maHoaDon}</span>
          </div>
          {soTienConLai != null && (
            <div className="flex justify-between">
              <span className="text-ink-400">Còn lại</span>
              <span className="font-medium text-danger-600">
                {formatCurrency(soTienConLai)}
              </span>
            </div>
          )}
        </div>

        <div>
          <label className="block text-sm font-medium text-ink-700 mb-1.5">
            Số tiền thu (VNĐ) <span className="text-danger-500">*</span>
          </label>
          <input
            type="number"
            min="0"
            step="1"
            value={soTien}
            onChange={(e) => setSoTien(e.target.value)}
            className="w-full border border-ink-200 rounded-lg px-3 py-2 text-sm focus:outline-none focus:ring-2 focus:ring-accent-400/40 focus:border-accent-400"
            placeholder="VD: 5000000"
          />
        </div>

        {error && (
          <div className="text-sm text-danger-600 bg-danger-50 rounded-lg p-2.5">
            {error}
          </div>
        )}

        <div className="flex gap-2 pt-1">
          <Button type="submit" disabled={submitting} className="flex-1">
            {submitting ? "Đang tạo..." : "Tạo phiếu thu"}
          </Button>
          <Button type="button" variant="secondary" onClick={onClose}>
            Hủy
          </Button>
        </div>
      </form>
    </Modal>
  );
}
