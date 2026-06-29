import { useState } from "react";
import opportunityApi from "../../api/opportunityApi";
import Modal from "../../components/common/Modal";
import Button from "../../components/common/Button";
import Badge from "../../components/common/Badge";
import { GIAI_DOAN_LABEL, GIAI_DOAN_COLOR } from "../../utils/constants";

export default function ChangeStageModal({ item, targetStage, onClose, onSaved }) {
  const [ghiChu, setGhiChu] = useState("");
  const [submitting, setSubmitting] = useState(false);
  const [error, setError] = useState("");
  const isFailure = targetStage === "ThatBai";

  const handleSubmit = async (e) => {
    e.preventDefault();
    if (isFailure && !ghiChu.trim()) { setError("Vui lòng nhập lý do thất bại"); return; }
    setSubmitting(true); setError("");
    try {
      await opportunityApi.changeStage(item.id, targetStage, ghiChu || null);
      onSaved();
    } catch (err) { setError(err?.message || "Không thể cập nhật giai đoạn"); }
    finally { setSubmitting(false); }
  };

  return (
    <Modal isOpen onClose={onClose} title="Chuyển giai đoạn" size="sm">
      <form onSubmit={handleSubmit} className="space-y-4">
        <p className="text-sm text-ink-700">
          Thương vụ <strong className="text-ink-900">{item.tenThuongVu}</strong> sẽ chuyển sang{" "}
          <Badge label={GIAI_DOAN_LABEL[targetStage]} colorClass={GIAI_DOAN_COLOR[targetStage]} />
        </p>

        <div>
          <label className="block text-sm font-medium text-ink-700 mb-1.5">
            {isFailure ? "Lý do thất bại *" : "Ghi chú (không bắt buộc)"}
          </label>
          <textarea rows={3} value={ghiChu} onChange={(e) => setGhiChu(e.target.value)}
            placeholder={isFailure ? "Nêu rõ lý do..." : "Ghi chú bổ sung..."}
            className="w-full border border-ink-200 rounded-lg px-3 py-2 text-sm resize-none focus:outline-none focus:ring-2 focus:ring-accent-400/40 focus:border-accent-400" />
        </div>

        {error && <div className="text-sm text-danger-600 bg-danger-50 rounded-lg p-2.5">{error}</div>}

        <div className="flex gap-2 pt-1">
          <Button type="submit" disabled={submitting}
            variant={isFailure ? "dangerSolid" : "primary"} className="flex-1">
            {submitting ? "Đang lưu..." : "Xác nhận"}
          </Button>
          <Button type="button" variant="secondary" onClick={onClose}>Hủy</Button>
        </div>
      </form>
    </Modal>
  );
}
