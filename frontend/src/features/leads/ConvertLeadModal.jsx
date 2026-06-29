import { useState } from "react";
import leadApi from "../../api/leadApi";
import Modal from "../../components/common/Modal";
import Button from "../../components/common/Button";
import { LOAI_KHACH_HANG_OPTIONS, TINH_TRANG_KHACH_HANG_OPTIONS } from "../../utils/constants";

const toInt = (v) => (v === "" || v == null ? null : Number(v));

// Modal chuyển đổi Lead → Khách hàng chính thức.
export default function ConvertLeadModal({ lead, nhanVienList = [], onClose, onConverted }) {
  const [form, setForm] = useState({
    tenKhachHang: lead.tenLead,
    loaiKhachHangId: "",
    tinhTrangId: "",
    email: lead.email ?? "",
    soDienThoai: lead.soDienThoai ?? "",
    maSoThue: "",
    nhanVienPhuTrachId: lead.nhanVienPhuTrachId ?? "",
  });
  const [submitting, setSubmitting] = useState(false);
  const [error, setError] = useState("");

  const handleSubmit = async (e) => {
    e.preventDefault();
    setSubmitting(true); setError("");
    try {
      await leadApi.convert(lead.id, {
        tenKhachHang: form.tenKhachHang.trim(),
        loaiKhachHangId: toInt(form.loaiKhachHangId),
        tinhTrangId: toInt(form.tinhTrangId),
        email: form.email || null,
        soDienThoai: form.soDienThoai || null,
        maSoThue: form.maSoThue || null,
        nhanVienPhuTrachId: toInt(form.nhanVienPhuTrachId),
      });
      onConverted();
    } catch (err) { setError(err?.message || "Chuyển đổi thất bại"); }
    finally { setSubmitting(false); }
  };

  return (
    <Modal isOpen onClose={onClose} title="Chuyển đổi Lead → Khách hàng" size="sm">
      <form onSubmit={handleSubmit} className="space-y-4">
        <div>
          <label className="block text-sm font-medium text-ink-700 mb-1.5">
            Tên khách hàng <span className="text-danger-500">*</span>
          </label>
          <input value={form.tenKhachHang} onChange={(e) => setForm((f) => ({ ...f, tenKhachHang: e.target.value }))}
            required className="w-full border border-ink-200 rounded-lg px-3 py-2 text-sm focus:outline-none focus:ring-2 focus:ring-accent-400/40 focus:border-accent-400" />
        </div>

        <div className="grid grid-cols-2 gap-3">
          <div>
            <label className="block text-sm font-medium text-ink-700 mb-1.5">Loại KH</label>
            <select value={form.loaiKhachHangId} onChange={(e) => setForm((f) => ({ ...f, loaiKhachHangId: e.target.value }))}
              className="w-full border border-ink-200 rounded-lg px-3 py-2 text-sm focus:outline-none focus:ring-2 focus:ring-accent-400/40 focus:border-accent-400">
              <option value="">-- Chọn --</option>
              {LOAI_KHACH_HANG_OPTIONS.map((o) => <option key={o.value} value={o.value}>{o.label}</option>)}
            </select>
          </div>
          <div>
            <label className="block text-sm font-medium text-ink-700 mb-1.5">Tình trạng</label>
            <select value={form.tinhTrangId} onChange={(e) => setForm((f) => ({ ...f, tinhTrangId: e.target.value }))}
              className="w-full border border-ink-200 rounded-lg px-3 py-2 text-sm focus:outline-none focus:ring-2 focus:ring-accent-400/40 focus:border-accent-400">
              <option value="">-- Chọn --</option>
              {TINH_TRANG_KHACH_HANG_OPTIONS.map((o) => <option key={o.value} value={o.value}>{o.label}</option>)}
            </select>
          </div>
        </div>

        {nhanVienList.length > 0 && (
          <div>
            <label className="block text-sm font-medium text-ink-700 mb-1.5">Nhân viên phụ trách</label>
            <select value={form.nhanVienPhuTrachId} onChange={(e) => setForm((f) => ({ ...f, nhanVienPhuTrachId: e.target.value }))}
              className="w-full border border-ink-200 rounded-lg px-3 py-2 text-sm focus:outline-none focus:ring-2 focus:ring-accent-400/40 focus:border-accent-400">
              <option value="">-- Chọn --</option>
              {nhanVienList.map((nv) => <option key={nv.id} value={nv.id}>{nv.hoTen ?? `NV #${nv.id}`}</option>)}
            </select>
          </div>
        )}

        {error && <div className="text-sm text-danger-600 bg-danger-50 rounded-lg p-2.5">{error}</div>}

        <div className="flex gap-2 pt-1">
          <Button type="submit" disabled={submitting} className="flex-1">
            {submitting ? "Đang xử lý..." : "Xác nhận chuyển đổi"}
          </Button>
          <Button type="button" variant="secondary" onClick={onClose}>Hủy</Button>
        </div>
      </form>
    </Modal>
  );
}
