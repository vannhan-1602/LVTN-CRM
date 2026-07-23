import { useState, useEffect } from "react";
import { useForm } from "react-hook-form";
import Modal from "../../components/common/Modal";
import Button from "../../components/common/Button";
import invoiceApi from "../../api/invoiceApi";
import contractApi from "../../api/contractApi";
import customerApi from "../../api/customerApi";
import { formatCurrency, formatDate } from "../../utils/formatters";

export default function CreateInvoiceModal({ onClose, onSaved }) {
  const {
    register,
    handleSubmit,
    watch,
    setValue,
    formState: { errors, isSubmitting },
  } = useForm({
    defaultValues: {
      hopDongId: "",
      khachHangId: "",
      lichThanhToanId: "",
      tongTien: "",
    },
  });
  const [apiError, setApiError] = useState("");
  const [contracts, setContracts] = useState([]);
  const [customers, setCustomers] = useState([]);

  // Chi tiết hợp đồng đang chọn (giaTri, hinhThucThanhToan...) + các đợt trả góp còn có thể xuất
  const [contractDetail, setContractDetail] = useState(null);
  const [lichThanhToans, setLichThanhToans] = useState([]);
  const [loadingContractInfo, setLoadingContractInfo] = useState(false);

  const hopDongId = watch("hopDongId");
  const lichThanhToanId = watch("lichThanhToanId");
  const isTraGop = contractDetail?.hinhThucThanhToan === "TraGop";
  const isMotLan =
    !!hopDongId && contractDetail?.hinhThucThanhToan === "ThanhToanMotLan";

  useEffect(() => {
    // Load hợp đồng đang thực hiện để chọn
    contractApi
      .getAll({ pageSize: 100, trangThai: "DangThucHien" })
      .then((res) => setContracts(res.data?.items ?? []))
      .catch(() => {});
    // Load khách hàng
    customerApi
      .getAll({ pageSize: 100 })
      .then((res) => setCustomers(res.data?.items ?? []))
      .catch(() => {});
  }, []);

  // Khi chọn hợp đồng → lấy chi tiết hợp đồng + (tuỳ hình thức) số tiền còn lại hoặc danh sách đợt
  useEffect(() => {
    setValue("khachHangId", ""); // KhachHangId lấy từ HĐ ở backend
    setValue("lichThanhToanId", "");
    setContractDetail(null);
    setLichThanhToans([]);

    if (!hopDongId) {
      setValue("tongTien", "");
      return;
    }

    setLoadingContractInfo(true);
    contractApi
      .getById(hopDongId)
      .then(async (res) => {
        const detail = res.data ?? null;
        setContractDetail(detail);

        if (detail?.hinhThucThanhToan === "TraGop") {
          setValue("tongTien", "");
          try {
            const lichRes = await contractApi.getLichThanhToan(hopDongId);
            const dots = (lichRes.data ?? []).filter(
              (d) => d.trangThai !== "DaThanhToan" && !d.daCoHoaDon,
            );
            setLichThanhToans(dots);
          } catch {
            setLichThanhToans([]);
          }
        } else {
          try {
            const tongRes = await invoiceApi.getTongDaXuat(hopDongId);
            const daXuat = Number(tongRes.data ?? 0);
            const conLai = Math.max((detail?.giaTri ?? 0) - daXuat, 0);
            setValue("tongTien", conLai);
          } catch {
            setValue("tongTien", "");
          }
        }
      })
      .catch(() => setContractDetail(null))
      .finally(() => setLoadingContractInfo(false));
  }, [hopDongId]);

  // Khi chọn 1 đợt trả góp → tự điền đúng số tiền của đợt đó
  const handleChonDot = (id) => {
    setValue("lichThanhToanId", id);
    const dot = lichThanhToans.find((d) => String(d.id) === String(id));
    setValue("tongTien", dot ? dot.soTien : "");
  };

  const onSubmit = async (data) => {
    setApiError("");
    try {
      await invoiceApi.create({
        hopDongId: data.hopDongId ? Number(data.hopDongId) : null,
        khachHangId: data.hopDongId
          ? null
          : data.khachHangId
            ? Number(data.khachHangId)
            : null,
        lichThanhToanId:
          data.hopDongId && data.lichThanhToanId
            ? Number(data.lichThanhToanId)
            : null,
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
        {apiError && (
          <div className="text-sm text-danger-600 bg-danger-50 rounded-lg p-3">
            {apiError}
          </div>
        )}

        <div>
          <label className="block text-sm font-medium text-ink-700 mb-1">
            Hợp đồng{" "}
            <span className="text-ink-400 font-normal">
              (tuỳ chọn — bỏ trống nếu bán lẻ)
            </span>
          </label>
          <select
            {...register("hopDongId")}
            className="w-full border border-ink-200 rounded-lg px-3 py-2 text-sm focus:outline-none focus:ring-2 focus:ring-accent-400/40 focus:border-accent-400"
          >
            <option value="">— Không có hợp đồng —</option>
            {contracts.map((c) => (
              <option key={c.id} value={c.id}>
                {c.maHopDong} — {c.tenKhachHang}
              </option>
            ))}
          </select>
          <p className="text-xs text-ink-400 mt-1">
            Chỉ hiện hợp đồng đang thực hiện. Khi chọn hợp đồng, khách hàng được
            tự động lấy từ hợp đồng.
          </p>
        </div>

        {!hopDongId && (
          <div>
            <label className="block text-sm font-medium text-ink-700 mb-1">
              Khách hàng <span className="text-danger-500">*</span>
            </label>
            <select
              {...register("khachHangId", {
                required: !hopDongId && "Vui lòng chọn khách hàng",
              })}
              className="w-full border border-ink-200 rounded-lg px-3 py-2 text-sm focus:outline-none focus:ring-2 focus:ring-accent-400/40 focus:border-accent-400"
            >
              <option value="">— Chọn khách hàng —</option>
              {customers.map((c) => (
                <option key={c.id} value={c.id}>
                  {c.tenKhachHang}
                </option>
              ))}
            </select>
            {errors.khachHangId && (
              <p className="text-xs text-danger-600 mt-1">
                {errors.khachHangId.message}
              </p>
            )}
          </div>
        )}

        {loadingContractInfo && (
          <p className="text-xs text-ink-400">Đang tải thông tin hợp đồng...</p>
        )}

        {isTraGop && !loadingContractInfo && (
          <div>
            <label className="block text-sm font-medium text-ink-700 mb-1">
              Đợt thanh toán <span className="text-danger-500">*</span>
            </label>
            <select
              value={lichThanhToanId}
              onChange={(e) => handleChonDot(e.target.value)}
              className="w-full border border-ink-200 rounded-lg px-3 py-2 text-sm focus:outline-none focus:ring-2 focus:ring-accent-400/40 focus:border-accent-400"
            >
              <option value="">— Chọn đợt —</option>
              {lichThanhToans.map((d) => (
                <option key={d.id} value={d.id}>
                  Đợt {d.soDot} — {formatCurrency(d.soTien)} (hạn{" "}
                  {formatDate(d.hanThanhToan)})
                </option>
              ))}
            </select>
            {lichThanhToans.length === 0 && (
              <p className="text-xs text-ink-400 mt-1">
                Hợp đồng này không còn đợt nào chưa xuất hóa đơn.
              </p>
            )}
            <input
              type="hidden"
              {...register("lichThanhToanId", {
                required: isTraGop && "Vui lòng chọn đợt thanh toán",
              })}
            />
            {errors.lichThanhToanId && (
              <p className="text-xs text-danger-600 mt-1">
                {errors.lichThanhToanId.message}
              </p>
            )}
          </div>
        )}

        <div>
          <label className="block text-sm font-medium text-ink-700 mb-1">
            Tổng tiền (VNĐ) <span className="text-danger-500">*</span>
          </label>
          <input
            type="number"
            min="0"
            step="1"
            readOnly={isMotLan || isTraGop}
            {...register("tongTien", {
              required: "Vui lòng nhập tổng tiền",
              min: { value: 1, message: "Tổng tiền phải lớn hơn 0" },
            })}
            className={`w-full border border-ink-200 rounded-lg px-3 py-2 text-sm focus:outline-none focus:ring-2 focus:ring-accent-400/40 focus:border-accent-400 ${
              isMotLan || isTraGop ? "bg-surface-alt text-ink-500" : ""
            }`}
            placeholder="VD: 5000000"
          />
          {isMotLan && (
            <p className="text-xs text-ink-400 mt-1">
              Tự tính từ giá trị hợp đồng (
              {formatCurrency(contractDetail?.giaTri)}) trừ số đã xuất hóa đơn —
              không sửa tay được.
            </p>
          )}
          {isTraGop && (
            <p className="text-xs text-ink-400 mt-1">
              Tự điền theo đợt đã chọn ở trên — không sửa tay được.
            </p>
          )}
          {errors.tongTien && (
            <p className="text-xs text-danger-600 mt-1">
              {errors.tongTien.message}
            </p>
          )}
        </div>

        <div className="flex justify-end gap-2 pt-2">
          <Button variant="secondary" onClick={onClose} type="button">
            Huỷ
          </Button>
          <Button
            type="submit"
            disabled={isSubmitting || (isTraGop && lichThanhToans.length === 0)}
          >
            {isSubmitting ? "Đang tạo..." : "Tạo hóa đơn"}
          </Button>
        </div>
      </form>
    </Modal>
  );
}
