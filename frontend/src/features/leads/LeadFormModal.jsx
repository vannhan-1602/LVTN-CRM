import { useState } from "react";
import leadApi from "../../api/leadApi";
import Modal from "../../components/common/Modal";
import Button from "../../components/common/Button";
import EmployeeSelect from "../../components/common/EmployeeSelect";
import { LEAD_TINH_TRANG_OPTIONS } from "../../utils/constants";

const emptyForm = {
  tenLead: "",
  tenCongTy: "",
  soDienThoai: "",
  email: "",
  tinhTrang: "",
  nhanVienPhuTrachId: "",
};

const toInt = (v) => (v === "" || v == null ? null : Number(v));

// Modal Thêm/Sửa Lead. Có prop `lead` => chế độ Sửa.
export default function LeadFormModal({ lead, nhanVienList = [], onClose, onSaved }) {
  const isEdit = Boolean(lead);
  const [form, setForm] = useState(
    isEdit
      ? {
          tenLead: lead.tenLead ?? "",
          tenCongTy: lead.tenCongTy ?? "",
          soDienThoai: lead.soDienThoai ?? "",
          email: lead.email ?? "",
          tinhTrang: lead.tinhTrang ?? "",
          nhanVienPhuTrachId: lead.nhanVienPhuTrachId ?? "",
        }
      : emptyForm,
  );
  const [submitting, setSubmitting] = useState(false);
  const [error, setError] = useState("");

  const handleChange = (e) => setForm((f) => ({ ...f, [e.target.name]: e.target.value }));

  const handleSubmit = async (e) => {
    e.preventDefault();
    if (!form.tenLead.trim()) { setError("Tên lead không được để trống"); return; }
    setSubmitting(true); setError("");
    const payload = {
      tenLead: form.tenLead.trim(),
      tenCongTy: form.tenCongTy.trim() || null,
      soDienThoai: form.soDienThoai.trim() || null,
      email: form.email.trim() || null,
      tinhTrang: form.tinhTrang || null,
      nhanVienPhuTrachId: toInt(form.nhanVienPhuTrachId),
    };
    try {
      if (isEdit) await leadApi.update(lead.id, payload);
      else await leadApi.create(payload);
      onSaved();
    } catch (err) { setError(err?.message || "Không thể lưu lead"); }
    finally { setSubmitting(false); }
  };

  return (
    <Modal isOpen onClose={onClose} title={isEdit ? "Cập nhật Lead" : "Thêm Lead mới"} size="md">
      <form onSubmit={handleSubmit} className="space-y-4">
        <div>
          <label className="block text-sm font-medium text-ink-700 mb-1.5">
            Tên Lead <span className="text-danger-500">*</span>
          </label>
          <input name="tenLead" value={form.tenLead} onChange={handleChange} placeholder="Nguyễn Văn A"
            className="w-full border border-ink-200 rounded-lg px-3 py-2 text-sm focus:outline-none focus:ring-2 focus:ring-accent-400/40 focus:border-accent-400" />
        </div>

        <div>
          <label className="block text-sm font-medium text-ink-700 mb-1.5">Công ty</label>
          <input name="tenCongTy" value={form.tenCongTy} onChange={handleChange} placeholder="Công ty ABC"
            className="w-full border border-ink-200 rounded-lg px-3 py-2 text-sm focus:outline-none focus:ring-2 focus:ring-accent-400/40 focus:border-accent-400" />
        </div>

        <div className="grid grid-cols-2 gap-3">
          <div>
            <label className="block text-sm font-medium text-ink-700 mb-1.5">Email</label>
            <input name="email" type="email" value={form.email} onChange={handleChange} placeholder="email@example.com"
              className="w-full border border-ink-200 rounded-lg px-3 py-2 text-sm focus:outline-none focus:ring-2 focus:ring-accent-400/40 focus:border-accent-400" />
          </div>
          <div>
            <label className="block text-sm font-medium text-ink-700 mb-1.5">Số điện thoại</label>
            <input name="soDienThoai" value={form.soDienThoai} onChange={handleChange} placeholder="0901234567"
              className="w-full border border-ink-200 rounded-lg px-3 py-2 text-sm focus:outline-none focus:ring-2 focus:ring-accent-400/40 focus:border-accent-400" />
          </div>
        </div>

        <div>
          <label className="block text-sm font-medium text-ink-700 mb-1.5">Tình trạng</label>
          <select name="tinhTrang" value={form.tinhTrang} onChange={handleChange}
            className="w-full border border-ink-200 rounded-lg px-3 py-2 text-sm focus:outline-none focus:ring-2 focus:ring-accent-400/40 focus:border-accent-400">
            <option value="">-- Chọn --</option>
            {LEAD_TINH_TRANG_OPTIONS.map((o) => <option key={o.value} value={o.value}>{o.label}</option>)}
          </select>
        </div>

        <div>
          <label className="block text-sm font-medium text-ink-700 mb-1.5">Nhân viên phụ trách</label>
          {nhanVienList.length > 0 ? (
            <EmployeeSelect
              value={form.nhanVienPhuTrachId}
              onChange={(v) => setForm((f) => ({ ...f, nhanVienPhuTrachId: v }))}
              options={nhanVienList}
            />
          ) : (
            <input name="nhanVienPhuTrachId" type="number" min="1" value={form.nhanVienPhuTrachId} onChange={handleChange}
              placeholder="ID nhân viên"
              className="w-full border border-ink-200 rounded-lg px-3 py-2 text-sm focus:outline-none focus:ring-2 focus:ring-accent-400/40 focus:border-accent-400" />
          )}
        </div>

        {error && <div className="text-sm text-danger-600 bg-danger-50 rounded-lg p-2.5">{error}</div>}

        <div className="flex gap-2 pt-1">
          <Button type="submit" disabled={submitting} className="flex-1">
            {submitting ? "Đang lưu..." : isEdit ? "Cập nhật" : "Thêm mới"}
          </Button>
          <Button type="button" variant="secondary" onClick={onClose}>Hủy</Button>
        </div>
      </form>
    </Modal>
  );
}
