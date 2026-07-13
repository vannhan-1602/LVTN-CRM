import { useEffect, useState } from "react";
import phieuThuChiApi from "../../api/phieuThuChiApi";
import customerApi from "../../api/customerApi";
import Modal from "../../components/common/Modal";
import Button from "../../components/common/Button";

// Tạo phiếu chi — không bắt buộc gắn hóa đơn, chỉ cần chọn khách hàng.
// Dùng cho các khoản chi phát sinh với khách (VD: tiếp khách, quà tặng...)
// mà công ty vẫn cần ghi nhận vào công nợ/kế toán.
export default function CreatePhieuChiModal({ onClose, onSaved }) {
  const [customers, setCustomers] = useState([]);
  const [khachHangId, setKhachHangId] = useState("");
  const [soTien, setSoTien] = useState("");
  const [submitting, setSubmitting] = useState(false);
  const [error, setError] = useState("");

  useEffect(() => {
    customerApi
      .getAll({ pageNumber: 1, pageSize: 200 })
      .then((r) => setCustomers(r.data?.items ?? []))
      .catch(() => {});
  }, []);

  const handleSubmit = async (e) => {
    e.preventDefault();
    const amount = Number(soTien);

    if (!khachHangId) {
      setError("Vui lòng chọn khách hàng");
      return;
    }
    if (!amount || amount <= 0) {
      setError("Số tiền chi phải lớn hơn 0");
      return;
    }

    setSubmitting(true);
    setError("");
    try {
      await phieuThuChiApi.create({
        hoaDonId: null,
        khachHangId: Number(khachHangId),
        loaiPhieu: "Chi",
        soTien: amount,
      });
      onSaved();
    } catch (err) {
      setError(err?.message || "Không thể tạo phiếu chi");
    } finally {
      setSubmitting(false);
    }
  };

  return (
    <Modal isOpen onClose={onClose} title="Tạo phiếu chi" size="sm">
      <form onSubmit={handleSubmit} className="space-y-4">
        <div>
          <label className="block text-sm font-medium text-ink-700 mb-1.5">
            Khách hàng <span className="text-danger-500">*</span>
          </label>
          <select
            value={khachHangId}
            onChange={(e) => setKhachHangId(e.target.value)}
            className="w-full border border-ink-200 rounded-lg px-3 py-2 text-sm focus:outline-none focus:ring-2 focus:ring-accent-400/40 focus:border-accent-400"
          >
            <option value="">-- Chọn khách hàng --</option>
            {customers.map((c) => (
              <option key={c.id} value={c.id}>
                {c.tenKhachHang} ({c.maKhachHang})
              </option>
            ))}
          </select>
        </div>

        <div>
          <label className="block text-sm font-medium text-ink-700 mb-1.5">
            Số tiền chi (VNĐ) <span className="text-danger-500">*</span>
          </label>
          <input
            type="number"
            min="0"
            step="1"
            value={soTien}
            onChange={(e) => setSoTien(e.target.value)}
            className="w-full border border-ink-200 rounded-lg px-3 py-2 text-sm focus:outline-none focus:ring-2 focus:ring-accent-400/40 focus:border-accent-400"
            placeholder="VD: 2000000 (tiếp khách, quà tặng...)"
          />
        </div>

        {error && (
          <div className="text-sm text-danger-600 bg-danger-50 rounded-lg p-2.5">
            {error}
          </div>
        )}

        <div className="flex gap-2 pt-1">
          <Button type="submit" disabled={submitting} className="flex-1">
            {submitting ? "Đang tạo..." : "Tạo phiếu chi"}
          </Button>
          <Button type="button" variant="secondary" onClick={onClose}>
            Hủy
          </Button>
        </div>
      </form>
    </Modal>
  );
}
