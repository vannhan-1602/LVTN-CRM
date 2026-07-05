import { useEffect, useState } from "react";
import ticketApi from "../../api/ticketApi";
import Modal from "../../components/common/Modal";
import Button from "../../components/common/Button";
import {
  TICKET_PRIORITY_OPTIONS,
  TICKET_SOURCE_OPTIONS,
} from "../../utils/constants";
import useDanhMucStore from "../../stores/danhMucStore";

const emptyForm = {
  tieuDe: "",
  moTa: "",
  khachHangId: "",
  loaiTicketId: "",
  mucDoUuTien: "TrungBinh",
  nguonTiepNhan: "Phone",
  nhanVienXuLyId: "",
  ngayHenXuLy: "",
};

export default function CreateTicketModal({
  customers,
  nhanVienList,
  onClose,
  onSaved,
}) {
  const { loaiTicket, load: loadDanhMuc } = useDanhMucStore();

  useEffect(() => {
    loadDanhMuc();
  }, [loadDanhMuc]);
  const [form, setForm] = useState(emptyForm);
  const [submitting, setSubmitting] = useState(false);
  const [error, setError] = useState("");

  const set = (k, v) => setForm((f) => ({ ...f, [k]: v }));

  const handleSubmit = async (e) => {
    e.preventDefault();
    if (!form.tieuDe.trim() || !form.khachHangId) {
      setError("Tiêu đề và khách hàng là bắt buộc");
      return;
    }
    setSubmitting(true);
    setError("");
    try {
      await ticketApi.create({
        tieuDe: form.tieuDe.trim(),
        moTa: form.moTa.trim() || null,
        khachHangId: Number(form.khachHangId),
        loaiTicketId: form.loaiTicketId ? Number(form.loaiTicketId) : null,
        mucDoUuTien: form.mucDoUuTien,
        nguonTiepNhan: form.nguonTiepNhan,
        nhanVienXuLyId: form.nhanVienXuLyId
          ? Number(form.nhanVienXuLyId)
          : null,
        ngayHenXuLy: form.ngayHenXuLy || null,
      });
      onSaved();
    } catch (err) {
      setError(err?.message || "Không thể tạo ticket");
    } finally {
      setSubmitting(false);
    }
  };

  return (
    <Modal isOpen onClose={onClose} title="Tạo ticket hỗ trợ mới" size="md">
      <form onSubmit={handleSubmit} className="space-y-4">
        <div>
          <label className="block text-sm font-medium text-ink-700 mb-1.5">
            Tiêu đề <span className="text-danger-500">*</span>
          </label>
          <input
            value={form.tieuDe}
            onChange={(e) => set("tieuDe", e.target.value)}
            placeholder="VD: Không đăng nhập được hệ thống"
            className="w-full border border-ink-200 rounded-lg px-3 py-2 text-sm focus:outline-none focus:ring-2 focus:ring-accent-400/40 focus:border-accent-400"
          />
        </div>

        <div>
          <label className="block text-sm font-medium text-ink-700 mb-1.5">
            Khách hàng <span className="text-danger-500">*</span>
          </label>
          <select
            value={form.khachHangId}
            onChange={(e) => set("khachHangId", e.target.value)}
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

        {/* Loại ticket — load từ DB, Admin có thể thêm/sửa */}
        <div>
          <label className="block text-sm font-medium text-ink-700 mb-1.5">
            Loại ticket
          </label>
          <select
            value={form.loaiTicketId}
            onChange={(e) => set("loaiTicketId", e.target.value)}
            className="w-full border border-ink-200 rounded-lg px-3 py-2 text-sm focus:outline-none focus:ring-2 focus:ring-accent-400/40 focus:border-accent-400"
          >
            <option value="">-- Chọn loại --</option>
            {loaiTicket
              .filter((l) => l.isActive)
              .map((l) => (
                <option key={l.id} value={l.id}>
                  {l.tenLoai}
                </option>
              ))}
          </select>
        </div>

        <div className="grid grid-cols-2 gap-3">
          <div>
            <label className="block text-sm font-medium text-ink-700 mb-1.5">
              Mức ưu tiên
            </label>
            <select
              value={form.mucDoUuTien}
              onChange={(e) => set("mucDoUuTien", e.target.value)}
              className="w-full border border-ink-200 rounded-lg px-3 py-2 text-sm focus:outline-none focus:ring-2 focus:ring-accent-400/40 focus:border-accent-400"
            >
              {TICKET_PRIORITY_OPTIONS.map((o) => (
                <option key={o.value} value={o.value}>
                  {o.label}
                </option>
              ))}
            </select>
          </div>
          <div>
            <label className="block text-sm font-medium text-ink-700 mb-1.5">
              Nguồn tiếp nhận
            </label>
            <select
              value={form.nguonTiepNhan}
              onChange={(e) => set("nguonTiepNhan", e.target.value)}
              className="w-full border border-ink-200 rounded-lg px-3 py-2 text-sm focus:outline-none focus:ring-2 focus:ring-accent-400/40 focus:border-accent-400"
            >
              {TICKET_SOURCE_OPTIONS.map((o) => (
                <option key={o.value} value={o.value}>
                  {o.label}
                </option>
              ))}
            </select>
          </div>
        </div>

        <div>
          <label className="block text-sm font-medium text-ink-700 mb-1.5">
            Nhân viên xử lý
          </label>
          <select
            value={form.nhanVienXuLyId}
            onChange={(e) => set("nhanVienXuLyId", e.target.value)}
            className="w-full border border-ink-200 rounded-lg px-3 py-2 text-sm focus:outline-none focus:ring-2 focus:ring-accent-400/40 focus:border-accent-400"
          >
            <option value="">-- Chưa gán --</option>
            {nhanVienList.map((nv) => (
              <option key={nv.id} value={nv.id}>
                {nv.hoTen ?? `NV #${nv.id}`}
              </option>
            ))}
          </select>
        </div>

        <div>
          <label className="block text-sm font-medium text-ink-700 mb-1.5">
            Ngày hẹn xử lý
          </label>
          <input
            type="datetime-local"
            value={form.ngayHenXuLy}
            onChange={(e) => set("ngayHenXuLy", e.target.value)}
            className="w-full border border-ink-200 rounded-lg px-3 py-2 text-sm focus:outline-none focus:ring-2 focus:ring-accent-400/40 focus:border-accent-400"
          />
        </div>

        <div>
          <label className="block text-sm font-medium text-ink-700 mb-1.5">
            Mô tả chi tiết
          </label>
          <textarea
            rows={3}
            value={form.moTa}
            onChange={(e) => set("moTa", e.target.value)}
            placeholder="Mô tả chi tiết vấn đề..."
            className="w-full border border-ink-200 rounded-lg px-3 py-2 text-sm resize-none focus:outline-none focus:ring-2 focus:ring-accent-400/40 focus:border-accent-400"
          />
        </div>

        {error && (
          <div className="text-sm text-danger-600 bg-danger-50 rounded-lg p-2.5">
            {error}
          </div>
        )}

        <div className="flex gap-2 pt-1">
          <Button type="submit" disabled={submitting} className="flex-1">
            {submitting ? "Đang tạo..." : "Tạo ticket"}
          </Button>
          <Button type="button" variant="secondary" onClick={onClose}>
            Hủy
          </Button>
        </div>
      </form>
    </Modal>
  );
}
