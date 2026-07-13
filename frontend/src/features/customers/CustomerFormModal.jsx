import { useEffect, useState } from "react";
import customerApi from "../../api/customerApi";
import authApi from "../../api/authApi";
import Modal from "../../components/common/Modal";
import Button from "../../components/common/Button";
import useDanhMucStore from "../../stores/danhMucStore";

const emptyForm = {
  tenKhachHang: "",
  email: "",
  soDienThoai: "",
  loaiKhachHangId: "",
  tinhTrangId: "",
  maSoThue: "",
  ngaySinh: "",
  ngayThanhLap: "",
  nhanVienPhuTrachId: "",
};

const toInt = (v) => (v === "" || v == null ? null : Number(v));

export default function CustomerFormModal({ customer, onClose, onSaved }) {
  const isEdit = Boolean(customer);
  const { loaiKhachHang, tinhTrang, load: loadDanhMuc } = useDanhMucStore();

  useEffect(() => {
    loadDanhMuc();
  }, [loadDanhMuc]);

  const [form, setForm] = useState(
    isEdit
      ? {
          tenKhachHang: customer.tenKhachHang ?? "",
          email: customer.email ?? "",
          soDienThoai: customer.soDienThoai ?? "",
          loaiKhachHangId: customer.loaiKhachHangId ?? "",
          tinhTrangId: customer.tinhTrangId ?? "",
          maSoThue: customer.maSoThue ?? "",
          ngaySinh: customer.ngaySinh?.slice(0, 10) ?? "",
          ngayThanhLap: customer.ngayThanhLap?.slice(0, 10) ?? "",
          nhanVienPhuTrachId: customer.nhanVienPhuTrachId ?? "",
        }
      : emptyForm,
  );

  const [nhanVienList, setNhanVienList] = useState([]);
  const [submitting, setSubmitting] = useState(false);
  const [error, setError] = useState("");

  // Phát hiện loại KH để hiển thị đúng field (B2B → ngày thành lập, B2C → ngày sinh)
  const selectedLoai = loaiKhachHang.find(
    (l) => l.id === Number(form.loaiKhachHangId),
  );
  const isB2B =
    selectedLoai?.tenLoai?.toLowerCase().includes("b2b") ||
    selectedLoai?.tenLoai?.toLowerCase().includes("doanh nghiệp");
  const isB2C =
    selectedLoai?.tenLoai?.toLowerCase().includes("b2c") ||
    selectedLoai?.tenLoai?.toLowerCase().includes("cá nhân");

  useEffect(() => {
    authApi
      .getStaffList()
      .then((res) => setNhanVienList(res.data ?? []))
      .catch(() => {});
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
      ngaySinh: form.ngaySinh || null,
      ngayThanhLap: form.ngayThanhLap || null,
      nhanVienPhuTrachId: toInt(form.nhanVienPhuTrachId),
    };
    try {
      if (isEdit) await customerApi.update(customer.id, payload);
      else await customerApi.create(payload);
      onSaved();
    } catch (err) {
      setError(err?.message || "Không thể lưu khách hàng");
    } finally {
      setSubmitting(false);
    }
  };

  const cls =
    "w-full border border-ink-200 rounded-lg px-3 py-2 text-sm focus:outline-none focus:ring-2 focus:ring-accent-400/40 focus:border-accent-400";

  return (
    <Modal
      isOpen
      onClose={onClose}
      title={isEdit ? "Cập nhật khách hàng" : "Thêm khách hàng mới"}
      size="md"
    >
      <form onSubmit={handleSubmit} className="space-y-4">
        {/* Tên */}
        <div>
          <label className="block text-sm font-medium text-ink-700 mb-1.5">
            Tên khách hàng <span className="text-danger-500">*</span>
          </label>
          <input
            name="tenKhachHang"
            value={form.tenKhachHang}
            onChange={handleChange}
            placeholder="Nguyễn Văn A / Công ty XYZ"
            className={cls}
          />
        </div>

        {/* Email + SĐT */}
        <div className="grid grid-cols-2 gap-3">
          <div>
            <label className="block text-sm font-medium text-ink-700 mb-1.5">
              Email
            </label>
            <input
              name="email"
              type="email"
              value={form.email}
              onChange={handleChange}
              placeholder="example@mail.com"
              className={cls}
            />
          </div>
          <div>
            <label className="block text-sm font-medium text-ink-700 mb-1.5">
              Số điện thoại
            </label>
            <input
              name="soDienThoai"
              value={form.soDienThoai}
              onChange={handleChange}
              placeholder="0901234567"
              className={cls}
            />
          </div>
        </div>

        {/* Loại KH + Tình trạng — load từ DB */}
        <div className="grid grid-cols-2 gap-3">
          <div>
            <label className="block text-sm font-medium text-ink-700 mb-1.5">
              Loại khách hàng
            </label>
            <select
              name="loaiKhachHangId"
              value={form.loaiKhachHangId}
              onChange={handleChange}
              className={cls}
            >
              <option value="">-- Chọn --</option>
              {loaiKhachHang
                .filter((l) => l.isActive)
                .map((l) => (
                  <option key={l.id} value={l.id}>
                    {l.tenLoai}
                  </option>
                ))}
            </select>
          </div>
          <div>
            <label className="block text-sm font-medium text-ink-700 mb-1.5">
              Tình trạng
            </label>
            <select
              name="tinhTrangId"
              value={form.tinhTrangId}
              onChange={handleChange}
              className={cls}
            >
              <option value="">-- Chọn --</option>
              {tinhTrang
                .filter((t) => t.isActive)
                .map((t) => (
                  <option key={t.id} value={t.id}>
                    {t.tenTinhTrang}
                  </option>
                ))}
            </select>
          </div>
        </div>

        {/* MST */}
        <div>
          <label className="block text-sm font-medium text-ink-700 mb-1.5">
            Mã số thuế
          </label>
          <input
            name="maSoThue"
            value={form.maSoThue}
            onChange={handleChange}
            placeholder="0123456789"
            className={cls}
          />
        </div>

        {/* Ngày sinh / Ngày thành lập — hiển thị theo loại KH */}
        {(isB2C || (!isB2B && form.ngaySinh !== undefined)) && (
          <div>
            <label className="block text-sm font-medium text-ink-700 mb-1.5">
              Ngày sinh
              <span className="ml-1 text-xs text-ink-400 font-normal">
                (dùng để gửi email chúc mừng sinh nhật)
              </span>
            </label>
            <input
              type="date"
              name="ngaySinh"
              value={form.ngaySinh}
              onChange={handleChange}
              className={cls}
            />
          </div>
        )}
        {isB2B && (
          <div>
            <label className="block text-sm font-medium text-ink-700 mb-1.5">
              Ngày thành lập
              <span className="ml-1 text-xs text-ink-400 font-normal">
                (dùng để gửi email kỷ niệm)
              </span>
            </label>
            <input
              type="date"
              name="ngayThanhLap"
              value={form.ngayThanhLap}
              onChange={handleChange}
              className={cls}
            />
          </div>
        )}
        {/* Nếu chưa chọn loại KH, hiện cả 2 */}
        {!isB2B && !isB2C && !selectedLoai && (
          <div className="grid grid-cols-2 gap-3">
            <div>
              <label className="block text-sm font-medium text-ink-700 mb-1.5">
                Ngày sinh (B2C)
              </label>
              <input
                type="date"
                name="ngaySinh"
                value={form.ngaySinh}
                onChange={handleChange}
                className={cls}
              />
            </div>
            <div>
              <label className="block text-sm font-medium text-ink-700 mb-1.5">
                Ngày thành lập (B2B)
              </label>
              <input
                type="date"
                name="ngayThanhLap"
                value={form.ngayThanhLap}
                onChange={handleChange}
                className={cls}
              />
            </div>
          </div>
        )}

        {/* NV phụ trách */}
        <div>
          <label className="block text-sm font-medium text-ink-700 mb-1.5">
            Nhân viên phụ trách
            {isEdit && (
              <span className="ml-2 text-xs text-ink-400 font-normal">
                (không thể thay đổi)
              </span>
            )}
          </label>
          {isEdit ? (
            <input
              disabled
              value={
                nhanVienList.find(
                  (nv) => String(nv.id) === String(form.nhanVienPhuTrachId),
                )?.hoTen ??
                (form.nhanVienPhuTrachId
                  ? `NV #${form.nhanVienPhuTrachId}`
                  : "—")
              }
              className="w-full border border-ink-100 bg-surface-alt rounded-lg px-3 py-2 text-sm text-ink-500 cursor-not-allowed"
            />
          ) : nhanVienList.length > 0 ? (
            <select
              name="nhanVienPhuTrachId"
              value={form.nhanVienPhuTrachId}
              onChange={handleChange}
              className={cls}
            >
              <option value="">-- Chọn nhân viên --</option>
              {nhanVienList.map((nv) => (
                <option key={nv.id} value={nv.id}>
                  {nv.hoTen ?? `NV #${nv.id}`}
                  {nv.role ? ` (${nv.role})` : ""}
                </option>
              ))}
            </select>
          ) : (
            <input
              name="nhanVienPhuTrachId"
              type="number"
              min="1"
              value={form.nhanVienPhuTrachId}
              onChange={handleChange}
              placeholder="ID nhân viên"
              className={cls}
            />
          )}
        </div>

        {error && (
          <div className="text-sm text-danger-600 bg-danger-50 rounded-lg p-2.5">
            {error}
          </div>
        )}

        <div className="flex gap-2 pt-1">
          <Button type="submit" disabled={submitting} className="flex-1">
            {submitting ? "Đang lưu..." : isEdit ? "Cập nhật" : "Thêm mới"}
          </Button>
          <Button type="button" variant="secondary" onClick={onClose}>
            Hủy
          </Button>
        </div>
      </form>
    </Modal>
  );
}
