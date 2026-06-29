import { useEffect, useState } from "react";
import customerApi from "../../api/customerApi";
import authApi from "../../api/authApi";
import Modal from "../../components/common/Modal";
import Button from "../../components/common/Button";
import { LOAI_KHACH_HANG_OPTIONS, TINH_TRANG_KHACH_HANG_OPTIONS } from "../../utils/constants";

const emptyForm = {
  tenKhachHang: "",
  email: "",
  soDienThoai: "",
  loaiKhachHangId: "",
  tinhTrangId: "",
  maSoThue: "",
  nhanVienPhuTrachId: "",
};

const toInt = (v) => (v === "" || v == null ? null : Number(v));

// Modal Thêm/Sửa khách hàng. Nếu có prop `customer` => chế độ Sửa.
export default function CustomerFormModal({ customer, onClose, onSaved }) {
  const isEdit = Boolean(customer);
  const [form, setForm] = useState(
    isEdit
      ? {
          tenKhachHang: customer.tenKhachHang ?? "",
          email: customer.email ?? "",
          soDienThoai: customer.soDienThoai ?? "",
          loaiKhachHangId: customer.loaiKhachHangId ?? "",
          tinhTrangId: customer.tinhTrangId ?? "",
          maSoThue: customer.maSoThue ?? "",
          nhanVienPhuTrachId: customer.nhanVienPhuTrachId ?? "",
        }
      : emptyForm,
  );
  const [nhanVienList, setNhanVienList] = useState([]);
  const [submitting, setSubmitting] = useState(false);
  const [error, setError] = useState("");

  useEffect(() => {
    (async () => {
      try {
        const res = await authApi.getStaffList();
        setNhanVienList(res.data ?? []);
      } catch {
        /* không có quyền xem danh sách nhân viên, bỏ qua */
      }
    })();
  }, []);

  const handleChange = (e) => {
    const { name, value } = e.target;
    setForm((prev) => ({ ...prev, [name]: value }));
  };

  const handleSubmit = async (e) => {
    e.preventDefault();
    if (!form.tenKhachHang.trim()) {
      setError("Tên khách hàng không được để trống");
      return;
    }
    setSubmitting(true);
    setError("");
    const payload = {
      tenKhachHang: form.tenKhachHang.trim(),
      email: form.email.trim() || null,
      soDienThoai: form.soDienThoai.trim() || null,
      loaiKhachHangId: toInt(form.loaiKhachHangId),
      tinhTrangId: toInt(form.tinhTrangId),
      maSoThue: form.maSoThue.trim() || null,
      nhanVienPhuTrachId: toInt(form.nhanVienPhuTrachId),
    };
    try {
      if (isEdit) await customerApi.update(customer.id, payload);
      else await customerApi.create(payload);
      onSaved();
    } catch (err) {
      setError(err?.message || "Không thể lưu khách hàng");
    } finally { setSubmitting(false); }
  };

  return (
    <Modal isOpen onClose={onClose} title={isEdit ? "Cập nhật khách hàng" : "Thêm khách hàng mới"} size="md">
      <form onSubmit={handleSubmit} className="space-y-4">
        <div>
          <label className="block text-sm font-medium text-ink-700 mb-1.5">
            Tên khách hàng <span className="text-danger-500">*</span>
          </label>
          <input name="tenKhachHang" value={form.tenKhachHang} onChange={handleChange}
            placeholder="Nguyễn Văn A"
            className="w-full border border-ink-200 rounded-lg px-3 py-2 text-sm focus:outline-none focus:ring-2 focus:ring-accent-400/40 focus:border-accent-400" />
        </div>

        <div className="grid grid-cols-2 gap-3">
          <div>
            <label className="block text-sm font-medium text-ink-700 mb-1.5">Email</label>
            <input name="email" type="email" value={form.email} onChange={handleChange}
              placeholder="example@mail.com"
              className="w-full border border-ink-200 rounded-lg px-3 py-2 text-sm focus:outline-none focus:ring-2 focus:ring-accent-400/40 focus:border-accent-400" />
          </div>
          <div>
            <label className="block text-sm font-medium text-ink-700 mb-1.5">Số điện thoại</label>
            <input name="soDienThoai" value={form.soDienThoai} onChange={handleChange}
              placeholder="0901234567"
              className="w-full border border-ink-200 rounded-lg px-3 py-2 text-sm focus:outline-none focus:ring-2 focus:ring-accent-400/40 focus:border-accent-400" />
          </div>
        </div>

        <div className="grid grid-cols-2 gap-3">
          <div>
            <label className="block text-sm font-medium text-ink-700 mb-1.5">Loại khách hàng</label>
            <select name="loaiKhachHangId" value={form.loaiKhachHangId} onChange={handleChange}
              className="w-full border border-ink-200 rounded-lg px-3 py-2 text-sm focus:outline-none focus:ring-2 focus:ring-accent-400/40 focus:border-accent-400">
              <option value="">-- Chọn --</option>
              {LOAI_KHACH_HANG_OPTIONS.map(o => <option key={o.value} value={o.value}>{o.label}</option>)}
            </select>
          </div>
          <div>
            <label className="block text-sm font-medium text-ink-700 mb-1.5">Tình trạng</label>
            <select name="tinhTrangId" value={form.tinhTrangId} onChange={handleChange}
              className="w-full border border-ink-200 rounded-lg px-3 py-2 text-sm focus:outline-none focus:ring-2 focus:ring-accent-400/40 focus:border-accent-400">
              <option value="">-- Chọn --</option>
              {TINH_TRANG_KHACH_HANG_OPTIONS.map(o => <option key={o.value} value={o.value}>{o.label}</option>)}
            </select>
          </div>
        </div>

        <div>
          <label className="block text-sm font-medium text-ink-700 mb-1.5">Mã số thuế</label>
          <input name="maSoThue" value={form.maSoThue} onChange={handleChange}
            placeholder="0123456789"
            className="w-full border border-ink-200 rounded-lg px-3 py-2 text-sm focus:outline-none focus:ring-2 focus:ring-accent-400/40 focus:border-accent-400" />
        </div>

        <div>
          <label className="block text-sm font-medium text-ink-700 mb-1.5">Nhân viên phụ trách</label>
          {nhanVienList.length > 0 ? (
            <select name="nhanVienPhuTrachId" value={form.nhanVienPhuTrachId} onChange={handleChange}
              className="w-full border border-ink-200 rounded-lg px-3 py-2 text-sm focus:outline-none focus:ring-2 focus:ring-accent-400/40 focus:border-accent-400">
              <option value="">-- Chọn nhân viên --</option>
              {nhanVienList.map((nv) => <option key={nv.id} value={nv.id}>{nv.hoTen ?? `NV #${nv.id}`}</option>)}
            </select>
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
