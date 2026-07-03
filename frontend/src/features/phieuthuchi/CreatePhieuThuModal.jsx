import { useState } from "react";
import { useForm } from "react-hook-form";
import Modal from "../../components/common/Modal";
import Button from "../../components/common/Button";
import phieuThuChiApi from "../../api/phieuThuChiApi";
import { formatCurrency } from "../../utils/formatters";

export default function CreatePhieuThuModal({ hoaDonId, maHoaDon, soTienConLai, onClose, onSaved }) {
  const { register, handleSubmit, formState: { errors, isSubmitting } } = useForm({
    defaultValues: { soTien: "", loaiPhieu: "Thu" }
  });
  const [apiError, setApiError] = useState("");

  const onSubmit = async (data) => {
    setApiError("");
    try {
      await phieuThuChiApi.create({
        hoaDonId: hoaDonId ? Number(hoaDonId) : null,
        loaiPhieu: data.loaiPhieu,
        soTien: Number(data.soTien),
      });
      onSaved();
    } catch (err) {
      setApiError(err?.message || "Tạo phiếu thất bại");
    }
  };

  return (
    <Modal isOpen title={`Tạo phiếu thu — ${maHoaDon}`} onClose={onClose} size="sm">
      <form onSubmit={handleSubmit(onSubmit)} className="space-y-4">
        {apiError && <div className="text-sm text-danger-600 bg-danger-50 rounded-lg p-3">{apiError}</div>}

        <div className="bg-info-50 border border-info-100 rounded-lg p-3 text-sm text-info-700">
          Số tiền còn lại của hóa đơn: <strong>{formatCurrency(soTienConLai)}</strong>
        </div>

        <div>
          <label className="block text-sm font-medium text-ink-700 mb-1">Loại phiếu</label>
          <select {...register("loaiPhieu")}
            className="w-full border border-ink-200 rounded-lg px-3 py-2 text-sm focus:outline-none focus:ring-2 focus:ring-accent-400/40 focus:border-accent-400">
            <option value="Thu">Phiếu Thu</option>
            <option value="Chi">Phiếu Chi</option>
          </select>
        </div>

        <div>
          <label className="block text-sm font-medium text-ink-700 mb-1">
            Số tiền (VNĐ) <span className="text-danger-500">*</span>
          </label>
          <input type="number" min="1" step="1000"
            {...register("soTien", {
              required: "Vui lòng nhập số tiền",
              min: { value: 1, message: "Số tiền phải lớn hơn 0" },
              max: { value: soTienConLai, message: `Không được vượt quá ${formatCurrency(soTienConLai)}` }
            })}
            className="w-full border border-ink-200 rounded-lg px-3 py-2 text-sm focus:outline-none focus:ring-2 focus:ring-accent-400/40 focus:border-accent-400"
            placeholder={`Tối đa ${formatCurrency(soTienConLai)}`}
          />
          {errors.soTien && <p className="text-xs text-danger-600 mt-1">{errors.soTien.message}</p>}
        </div>

        <div className="flex justify-end gap-2 pt-2">
          <Button variant="secondary" onClick={onClose} type="button">Huỷ</Button>
          <Button type="submit" disabled={isSubmitting}>
            {isSubmitting ? "Đang tạo..." : "Xác nhận thu tiền"}
          </Button>
        </div>
      </form>
    </Modal>
  );
}
