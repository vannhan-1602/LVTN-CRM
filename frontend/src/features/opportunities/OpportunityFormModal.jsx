import { useState } from "react";
import opportunityApi from "../../api/opportunityApi";
import Modal from "../../components/common/Modal";
import Button from "../../components/common/Button";
import EmployeeSelect from "../../components/common/EmployeeSelect";

import { getApiErrorMessage } from "../../utils/formatters";
// Modal Tạo/Sửa cơ hội bán hàng. Có prop `item` => chế độ Sửa.
// `nhanVienList`: danh sách nhân viên (kèm role) cho dropdown phụ trách.
// `canAssign`: chỉ Manager mới được chọn/đổi người phụ trách; Sale luôn tự gán cho mình ở backend.
export default function OpportunityFormModal({
  item,
  customers,
  leads,
  nhanVienList = [],
  canAssign = false,
  presetKhachHangId,
  onClose,
  onSaved,
}) {
  const isEdit = Boolean(item);
  const [form, setForm] = useState({
    tenThuongVu: item?.tenThuongVu ?? "",
    khachHangId: item?.khachHangId ?? presetKhachHangId ?? "",
    leadId: item?.leadId ?? "",
    tyLeThanhCong: item?.tyLeThanhCong ?? 50,
    doanhThuKyVong: item?.doanhThuKyVong ?? "",
    ghiChu: item?.ghiChu ?? "",
    ngayDuKien: item?.ngayDuKien ?? "",
    nhanVienPhuTrachId: item?.nhanVienPhuTrachId ?? "",
  });
  const [submitting, setSubmitting] = useState(false);
  const [error, setError] = useState("");

  // set("khachHangId", x) và set("leadId", x) dùng hàm riêng bên dưới để đảm bảo loại trừ lẫn
  // nhau ngay trên UI — tránh người dùng chọn được cả 2 rồi mới bị backend từ chối sau khi bấm Lưu.
  const set = (k, v) => setForm((f) => ({ ...f, [k]: v }));
  const setKhachHang = (v) =>
    setForm((f) => ({ ...f, khachHangId: v, leadId: v ? "" : f.leadId }));
  const setLead = (v) =>
    setForm((f) => ({ ...f, leadId: v, khachHangId: v ? "" : f.khachHangId }));

  const handleSubmit = async (e) => {
    e.preventDefault();
    if (!form.tenThuongVu.trim()) {
      setError("Tên thương vụ không được trống");
      return;
    }
    if (!form.khachHangId && !form.leadId) {
      setError("Phải chọn đúng một trong hai: Khách hàng hoặc Lead");
      return;
    }
    if (form.khachHangId && form.leadId) {
      setError("Chỉ được chọn Khách hàng hoặc Lead, không được chọn cả hai");
      return;
    }

    setSubmitting(true);
    setError("");
    const payload = {
      tenThuongVu: form.tenThuongVu.trim(),
      khachHangId: form.khachHangId ? Number(form.khachHangId) : null,
      leadId: form.leadId ? Number(form.leadId) : null,
      tyLeThanhCong: Number(form.tyLeThanhCong) || 0,
      doanhThuKyVong:
        form.doanhThuKyVong !== "" ? Number(form.doanhThuKyVong) : null,
      ghiChu: form.ghiChu || null,
      ngayDuKien: form.ngayDuKien || null,
      nhanVienPhuTrachId: form.nhanVienPhuTrachId
        ? Number(form.nhanVienPhuTrachId)
        : null,
    };
    try {
      if (isEdit) await opportunityApi.update(item.id, payload);
      else await opportunityApi.create(payload);
      onSaved();
    } catch (err) {
      setError(getApiErrorMessage(err, "Không thể lưu"));
    } finally {
      setSubmitting(false);
    }
  };

  return (
    <Modal
      isOpen
      onClose={onClose}
      title={
        isEdit ? `Sửa cơ hội: ${item.tenThuongVu}` : "Tạo cơ hội bán hàng mới"
      }
      size="md"
    >
      <form onSubmit={handleSubmit} className="space-y-4">
        <div>
          <label className="block text-sm font-medium text-ink-700 mb-1.5">
            Tên thương vụ <span className="text-danger-500">*</span>
          </label>
          <input
            value={form.tenThuongVu}
            onChange={(e) => set("tenThuongVu", e.target.value)}
            placeholder="VD: Triển khai ERP cho công ty ABC"
            className="w-full border border-ink-200 rounded-lg px-3 py-2 text-sm focus:outline-none focus:ring-2 focus:ring-accent-400/40 focus:border-accent-400"
          />
        </div>

        <div>
          <label className="block text-sm font-medium text-ink-700 mb-1.5">
            Khách hàng
          </label>
          <select
            value={form.khachHangId}
            onChange={(e) => setKhachHang(e.target.value)}
            disabled={Boolean(form.leadId)}
            className="w-full border border-ink-200 rounded-lg px-3 py-2 text-sm focus:outline-none focus:ring-2 focus:ring-accent-400/40 focus:border-accent-400 disabled:bg-ink-50 disabled:text-ink-400"
          >
            <option value="">-- Chọn khách hàng --</option>
            {customers.map((c) => (
              <option key={c.id} value={c.id}>
                {c.tenKhachHang} ({c.maKhachHang})
              </option>
            ))}
          </select>
          {form.leadId && (
            <p className="text-xs text-ink-500 mt-1">
              Đã chọn Lead — xóa Lead bên dưới nếu muốn chọn Khách hàng thay vào đó.
            </p>
          )}
        </div>

        <div>
          <label className="block text-sm font-medium text-ink-700 mb-1.5">
            Lead (nếu chưa là KH chính thức)
          </label>
          <select
            value={form.leadId}
            onChange={(e) => setLead(e.target.value)}
            disabled={Boolean(form.khachHangId)}
            className="w-full border border-ink-200 rounded-lg px-3 py-2 text-sm focus:outline-none focus:ring-2 focus:ring-accent-400/40 focus:border-accent-400 disabled:bg-ink-50 disabled:text-ink-400"
          >
            <option value="">-- Chọn lead --</option>
            {leads.map((l) => (
              <option key={l.id} value={l.id}>
                {l.tenLead}
                {l.tenCongTy ? ` — ${l.tenCongTy}` : ""}
              </option>
            ))}
          </select>
          {form.khachHangId && (
            <p className="text-xs text-ink-500 mt-1">
              Đã chọn Khách hàng — xóa Khách hàng bên trên nếu muốn chọn Lead thay vào đó.
            </p>
          )}
        </div>

        <div>
          <label className="block text-sm font-medium text-ink-700 mb-1.5">
            Nhân viên phụ trách
          </label>
          {canAssign ? (
            <EmployeeSelect
              value={form.nhanVienPhuTrachId}
              onChange={(v) => set("nhanVienPhuTrachId", v)}
              options={nhanVienList}
            />
          ) : (
            <div className="w-full border border-ink-200 rounded-lg px-3 py-2 text-sm bg-surface-alt text-ink-400">
              Tự động gán cho bạn
            </div>
          )}
        </div>

        <div className="grid grid-cols-2 gap-3">
          <div>
            <label className="block text-sm font-medium text-ink-700 mb-1.5">
              Tỷ lệ thành công (%)
            </label>
            <input
              type="number"
              min="0"
              max="100"
              value={form.tyLeThanhCong}
              onChange={(e) => set("tyLeThanhCong", e.target.value)}
              className="w-full border border-ink-200 rounded-lg px-3 py-2 text-sm focus:outline-none focus:ring-2 focus:ring-accent-400/40 focus:border-accent-400"
            />
          </div>
          <div>
            <label className="block text-sm font-medium text-ink-700 mb-1.5">
              Doanh thu kỳ vọng (đ)
            </label>
            <input
              type="number"
              min="0"
              value={form.doanhThuKyVong}
              onChange={(e) => set("doanhThuKyVong", e.target.value)}
              placeholder="0"
              className="w-full border border-ink-200 rounded-lg px-3 py-2 text-sm focus:outline-none focus:ring-2 focus:ring-accent-400/40 focus:border-accent-400"
            />
          </div>
        </div>

        <div>
          <label className="block text-sm font-medium text-ink-700 mb-1.5">
            Ngày dự kiến chốt
          </label>
          <input
            type="date"
            value={form.ngayDuKien}
            onChange={(e) => set("ngayDuKien", e.target.value)}
            className="w-full border border-ink-200 rounded-lg px-3 py-2 text-sm focus:outline-none focus:ring-2 focus:ring-accent-400/40 focus:border-accent-400"
          />
        </div>

        <div>
          <label className="block text-sm font-medium text-ink-700 mb-1.5">
            Ghi chú
          </label>
          <textarea
            rows={3}
            value={form.ghiChu}
            onChange={(e) => set("ghiChu", e.target.value)}
            placeholder="Ghi chú thêm về thương vụ..."
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
            {submitting ? "Đang lưu..." : isEdit ? "Cập nhật" : "Tạo cơ hội"}
          </Button>
          <Button type="button" variant="secondary" onClick={onClose}>
            Hủy
          </Button>
        </div>
      </form>
    </Modal>
  );
}