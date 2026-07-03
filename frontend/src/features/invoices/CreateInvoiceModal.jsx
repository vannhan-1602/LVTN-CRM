import { useState, useEffect } from "react";
import { useForm } from "react-hook-form";
import Modal from "../../components/common/Modal";
import Button from "../../components/common/Button";
import invoiceApi from "../../api/invoiceApi";
import contractApi from "../../api/contractApi";
import customerApi from "../../api/customerApi";

export default function CreateInvoiceModal({ onClose, onSaved }) {
  const { register, handleSubmit, watch, setValue, formState: { errors, isSubmitting } } = useForm({
    defaultValues: { hopDongId: "", khachHangId: "", tongTien: "" }
  });
  const [apiError, setApiError] = useState("");
  const [contracts, setContracts] = useState([]);
  const [customers, setCustomers] = useState([]);
  const hopDongId = watch("hopDongId");

  useEffect(() => {
    // Load hợp đồng đang thực hiện để chọn
    contractApi.getAll({ pageSize: 100, trangThai: "DangThucHien" })
      .then(res => setContracts(res.data?.items ?? []))
      .catch(() => {});
    // Load khách hàng
    customerApi.getAll({ pageSize: 100 })
      .then(res => setCustomers(res.data?.items ?? []))
      .catch(() => {});
  }, []);

  // Khi chọn hợp đồng → tự điền khách hàng
  useEffect(() => {
    if (hopDongId) {
      const contract = contracts.find(c => String(c.id) === String(hopDongId));
      if (contract) setValue("khachHangId", ""); // KhachHangId lấy từ HĐ ở backend
    }
  }, [hopDongId, contracts]);

  const onSubmit = async (data) => {
    setApiError("");
    try {
      await invoiceApi.create({
        hopDongId: data.hopDongId ? Number(data.hopDongId) : null,
        khachHangId: data.hopDongId ? null : (data.khachHangId ? Number(data.khachHangId) : null),
        tongTien: Number(data.tongTien),
      });
      onSaved();
    } catch (err) {
      setApiError(err?.message || "Tạo hóa đơn thất bại");
    }
  };

  return (
    <Modal isOpen title="Tạo hóa đơn mới" onClose={onClose} size="md">
      <form onSubmit={handleSubmit(onSubmit)} className="space-y-4">
        {apiError && <div className="text-sm text-danger-600 bg-danger-50 rounded-lg p-3">{apiError}</div>}

        <div>
          <label className="block text-sm font-medium text-ink-700 mb-1">
            Hợp đồng <span className="text-ink-400 font-normal">(tuỳ chọn — bỏ trống nếu bán lẻ)</span>
          </label>
          <select {...register("hopDongId")}
            className="w-full border border-ink-200 rounded-lg px-3 py-2 text-sm focus:outline-none focus:ring-2 focus:ring-accent-400/40 focus:border-accent-400">
            <option value="">— Không có hợp đồng —</option>
            {contracts.map(c => (
              <option key={c.id} value={c.id}>{c.maHopDong} — {c.tenKhachHang}</option>
            ))}
          </select>
          <p className="text-xs text-ink-400 mt-1">
            Chỉ hiện hợp đồng đang thực hiện. Khi chọn hợp đồng, khách hàng được tự động lấy từ hợp đồng.
          </p>
        </div>

        {!hopDongId && (
          <div>
            <label className="block text-sm font-medium text-ink-700 mb-1">
              Khách hàng <span className="text-danger-500">*</span>
            </label>
            <select {...register("khachHangId", { required: !hopDongId && "Vui lòng chọn khách hàng" })}
              className="w-full border border-ink-200 rounded-lg px-3 py-2 text-sm focus:outline-none focus:ring-2 focus:ring-accent-400/40 focus:border-accent-400">
              <option value="">— Chọn khách hàng —</option>
              {customers.map(c => (
                <option key={c.id} value={c.id}>{c.tenKhachHang}</option>
              ))}
            </select>
            {errors.khachHangId && <p className="text-xs text-danger-600 mt-1">{errors.khachHangId.message}</p>}
          </div>
        )}

        <div>
          <label className="block text-sm font-medium text-ink-700 mb-1">
            Tổng tiền (VNĐ) <span className="text-danger-500">*</span>
          </label>
          <input type="number" min="1" step="1000"
            {...register("tongTien", {
              required: "Vui lòng nhập tổng tiền",
              min: { value: 1, message: "Tổng tiền phải lớn hơn 0" }
            })}
            className="w-full border border-ink-200 rounded-lg px-3 py-2 text-sm focus:outline-none focus:ring-2 focus:ring-accent-400/40 focus:border-accent-400"
            placeholder="VD: 5000000"
          />
          {errors.tongTien && <p className="text-xs text-danger-600 mt-1">{errors.tongTien.message}</p>}
        </div>

        <div className="flex justify-end gap-2 pt-2">
          <Button variant="secondary" onClick={onClose} type="button">Huỷ</Button>
          <Button type="submit" disabled={isSubmitting}>
            {isSubmitting ? "Đang tạo..." : "Tạo hóa đơn"}
          </Button>
        </div>
      </form>
    </Modal>
  );
}
