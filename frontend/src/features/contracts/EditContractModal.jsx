import { useState } from "react";
import contractApi from "../../api/contractApi";
import Modal from "../../components/common/Modal";
import Button from "../../components/common/Button";
import { CONTRACT_STATUS_OPTIONS } from "../../utils/constants";

// Modal sửa hợp đồng — hiện đè lên trang chi tiết, không chuyển trang.
// Hiện tại chỉ hỗ trợ đổi trạng thái (API backend chỉ có endpoint PUT /Contract/{id}/status).
export default function EditContractModal({ contract, onClose, onSaved }) {
  const [trangThai, setTrangThai] = useState(contract.trangThai);
  const [submitting, setSubmitting] = useState(false);
  const [error, setError] = useState("");

  const handleSubmit = async (e) => {
    e.preventDefault();
    setSubmitting(true);
    setError("");
    try {
      await contractApi.updateStatus(contract.id, trangThai);
      onSaved();
    } catch (err) {
      setError(err?.message || "Không thể cập nhật trạng thái");
    } finally {
      setSubmitting(false);
    }
  };

  return (
    <Modal isOpen onClose={onClose} title={`Sửa hợp đồng ${contract.maHopDong}`} size="sm">
      <form onSubmit={handleSubmit} className="space-y-4">
        <div>
          <label className="block text-sm font-medium text-ink-700 mb-1.5">Trạng thái</label>
          <select
            value={trangThai}
            onChange={(e) => setTrangThai(e.target.value)}
            className="w-full border border-ink-200 rounded-lg px-3 py-2 text-sm focus:outline-none focus:ring-2 focus:ring-accent-400/40 focus:border-accent-400"
          >
            {CONTRACT_STATUS_OPTIONS.map((o) => (
              <option key={o.value} value={o.value}>
                {o.label}
              </option>
            ))}
          </select>
        </div>

        {error && <div className="text-sm text-danger-600 bg-danger-50 rounded-lg p-2.5">{error}</div>}

        <div className="flex gap-2 pt-1">
          <Button type="submit" disabled={submitting || trangThai === contract.trangThai} className="flex-1">
            {submitting ? "Đang lưu..." : "Lưu thay đổi"}
          </Button>
          <Button type="button" variant="secondary" onClick={onClose}>
            Hủy
          </Button>
        </div>
      </form>
    </Modal>
  );
}
