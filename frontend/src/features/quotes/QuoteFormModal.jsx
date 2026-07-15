import { useEffect, useMemo, useState } from "react";
import { Plus, X } from "lucide-react";
import quoteApi from "../../api/quoteApi";
import customerApi from "../../api/customerApi";
import Modal from "../../components/common/Modal";
import Button from "../../components/common/Button";
import { formatDate } from "../../utils/formatters";

const LY_DO_VOUCHER_LABEL = {
  ThangHang: "Thăng hạng",
  SinhNhat: "Sinh nhật",
  NgayThanhLap: "Ngày thành lập",
  NgayLe: "Ngày lễ",
  CuoiNam: "Cuối năm",
  ThuCong: "Thủ công",
};

function formatMoney(n) {
  return Number(n || 0).toLocaleString("vi-VN") + " đ";
}

// Modal Tạo/Sửa báo giá nhiều dòng sản phẩm. Có prop `quote` => chế độ Sửa (chỉ sửa được khi Nháp).
export default function QuoteFormModal({
  quote,
  customers,
  products,
  onClose,
  onSaved,
}) {
  const isEdit = Boolean(quote);
  const [khachHangId, setKhachHangId] = useState(quote?.khachHangId ?? "");
  const [maVoucher, setMaVoucher] = useState("");
  const [customerVouchers, setCustomerVouchers] = useState([]);
  const [loadingVouchers, setLoadingVouchers] = useState(false);
  const [lines, setLines] = useState(
    isEdit && quote?.chiTiet?.length
      ? quote.chiTiet.map((c) => ({
          sanPhamId: String(c.sanPhamId),
          soLuong: c.soLuong,
          donGia: c.donGia,
        }))
      : [{ sanPhamId: "", soLuong: 1, donGia: "" }],
  );
  const [submitting, setSubmitting] = useState(false);
  const [error, setError] = useState("");

  const addLine = () =>
    setLines((l) => [...l, { sanPhamId: "", soLuong: 1, donGia: "" }]);
  const removeLine = (idx) => setLines((l) => l.filter((_, i) => i !== idx));
  const updateLine = (idx, field, value) =>
    setLines((l) =>
      l.map((row, i) => {
        if (i !== idx) return row;
        const next = { ...row, [field]: value };
        if (field === "sanPhamId") {
          const p = products.find((p) => String(p.id) === value);
          if (p && !row.donGia) next.donGia = p.giaBan;
        }
        return next;
      }),
    );

  const tongTien = useMemo(
    () =>
      lines.reduce(
        (sum, l) => sum + (Number(l.soLuong) || 0) * (Number(l.donGia) || 0),
        0,
      ),
    [lines],
  );

  // Khi chọn khách hàng (chỉ áp dụng lúc tạo mới), tự tải danh sách voucher còn
  // hiệu lực của khách để hiển thị cho nhân viên chọn nhanh thay vì phải gõ tay.
  useEffect(() => {
    if (isEdit || !khachHangId) {
      setCustomerVouchers([]);
      return;
    }
    setMaVoucher("");
    setLoadingVouchers(true);
    customerApi
      .getLoyaltyInfo(khachHangId)
      .then((res) => {
        const vouchers = (res.data?.vouchers ?? []).filter(
          (v) => v.trangThai === "ConHieuLuc",
        );
        setCustomerVouchers(vouchers);
      })
      .catch(() => setCustomerVouchers([]))
      .finally(() => setLoadingVouchers(false));
  }, [khachHangId, isEdit]);

  const handleSubmit = async (e) => {
    e.preventDefault();
    if (!isEdit && !khachHangId) {
      setError("Vui lòng chọn khách hàng");
      return;
    }
    const validLines = lines.filter(
      (l) => l.sanPhamId && Number(l.soLuong) > 0,
    );
    if (validLines.length === 0) {
      setError("Báo giá phải có ít nhất 1 sản phẩm hợp lệ");
      return;
    }

    setSubmitting(true);
    setError("");
    const chiTiet = validLines.map((l) => ({
      sanPhamId: Number(l.sanPhamId),
      soLuong: Number(l.soLuong),
      donGia: l.donGia === "" ? null : Number(l.donGia),
    }));

    try {
      if (isEdit) await quoteApi.update(quote.id, { chiTiet });
      else
        await quoteApi.create({
          khachHangId: Number(khachHangId),
          chiTiet,
          maVoucher: maVoucher.trim() || null,
        });
      onSaved();
    } catch (err) {
      setError(err?.message || "Không thể lưu báo giá");
    } finally {
      setSubmitting(false);
    }
  };

  return (
    <Modal
      isOpen
      onClose={onClose}
      title={isEdit ? `Sửa báo giá ${quote.maBaoGia}` : "Lập báo giá mới"}
      size="lg"
    >
      <form onSubmit={handleSubmit} className="space-y-4">
        {!isEdit && (
          <div>
            <label className="block text-sm font-medium text-ink-700 mb-1.5">
              Khách hàng <span className="text-danger-500">*</span>
            </label>
            <select
              value={khachHangId}
              onChange={(e) => setKhachHangId(e.target.value)}
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
        )}

        <div className="space-y-2">
          <div className="flex items-center justify-between">
            <label className="block text-sm font-medium text-ink-700">
              Danh sách sản phẩm
            </label>
            <button
              type="button"
              onClick={addLine}
              className="text-xs text-accent-600 font-medium hover:underline inline-flex items-center gap-1"
            >
              <Plus size={13} /> Thêm dòng
            </button>
          </div>
          {lines.map((line, idx) => (
            <div key={idx} className="grid grid-cols-12 gap-2 items-center">
              <select
                value={line.sanPhamId}
                onChange={(e) => updateLine(idx, "sanPhamId", e.target.value)}
                className="col-span-5 border border-ink-200 rounded-lg px-2 py-2 text-sm focus:outline-none focus:ring-2 focus:ring-accent-400/40 focus:border-accent-400"
              >
                <option value="">-- Sản phẩm --</option>
                {products.map((p) => (
                  <option key={p.id} value={p.id}>
                    {p.tenSP} ({p.maSP})
                  </option>
                ))}
              </select>
              <input
                type="number"
                min="1"
                value={line.soLuong}
                placeholder="SL"
                onChange={(e) => updateLine(idx, "soLuong", e.target.value)}
                className="col-span-2 border border-ink-200 rounded-lg px-2 py-2 text-sm focus:outline-none focus:ring-2 focus:ring-accent-400/40 focus:border-accent-400"
              />
              <input
                type="number"
                min="0"
                value={line.donGia}
                placeholder="Đơn giá"
                onChange={(e) => updateLine(idx, "donGia", e.target.value)}
                className="col-span-4 border border-ink-200 rounded-lg px-2 py-2 text-sm focus:outline-none focus:ring-2 focus:ring-accent-400/40 focus:border-accent-400"
              />
              <button
                type="button"
                onClick={() => removeLine(idx)}
                className="col-span-1 text-danger-500 hover:text-danger-700 flex justify-center"
              >
                <X size={16} />
              </button>
            </div>
          ))}
        </div>

        {!isEdit && (
          <div className="space-y-2">
            <label className="block text-sm font-medium text-ink-700 mb-1.5">
              Voucher{" "}
              <span className="text-ink-400 font-normal">(nếu khách có)</span>
            </label>

            <select
              value={
                customerVouchers.some((v) => v.maVoucher === maVoucher)
                  ? maVoucher
                  : ""
              }
              onChange={(e) => setMaVoucher(e.target.value)}
              disabled={!khachHangId || loadingVouchers}
              className="w-full border border-ink-200 rounded-lg px-3 py-2 text-sm focus:outline-none focus:ring-2 focus:ring-accent-400/40 focus:border-accent-400 disabled:bg-surface-alt disabled:text-ink-400"
            >
              <option value="">
                {!khachHangId
                  ? "-- Chọn khách hàng trước --"
                  : loadingVouchers
                    ? "Đang tải voucher..."
                    : customerVouchers.length === 0
                      ? "-- Khách chưa có voucher còn hiệu lực --"
                      : "-- Chọn voucher của khách --"}
              </option>
              {customerVouchers.map((v) => (
                <option key={v.id} value={v.maVoucher}>
                  {v.maVoucher} — Giảm {v.giaTriGiam}
                  {v.loaiGiamGia === "PhanTram" ? "%" : "đ"}
                  {v.giaTriGiamToiDa
                    ? ` (tối đa ${v.giaTriGiamToiDa.toLocaleString("vi-VN")}đ)`
                    : ""}
                  {" · "}
                  {LY_DO_VOUCHER_LABEL[v.lyDoPhatHanh] ?? v.lyDoPhatHanh}
                  {" · HSD "}
                  {formatDate(v.ngayHetHan)}
                </option>
              ))}
            </select>

            <p className="text-xs text-ink-400">
              hoặc nhập mã trực tiếp nếu khách đọc mã qua điện thoại/tin nhắn:
            </p>
            <input
              type="text"
              value={maVoucher}
              onChange={(e) => setMaVoucher(e.target.value)}
              placeholder="VD: VC-20260715-131B8A"
              className="w-full border border-ink-200 rounded-lg px-3 py-2 text-sm focus:outline-none focus:ring-2 focus:ring-accent-400/40 focus:border-accent-400"
            />
          </div>
        )}

        <div className="text-right text-sm border-t border-ink-100 pt-3">
          Tổng tiền:{" "}
          <span className="font-semibold text-lg text-accent-600">
            {formatMoney(tongTien)}
          </span>
          {!isEdit && maVoucher.trim() && (
            <p className="text-xs text-ink-400 mt-0.5">
              Giá trên chưa trừ voucher — hệ thống sẽ tự trừ khi lưu.
            </p>
          )}
        </div>

        {error && (
          <div className="text-sm text-danger-600 bg-danger-50 rounded-lg p-2.5">
            {error}
          </div>
        )}

        <div className="flex gap-2 pt-1">
          <Button type="submit" disabled={submitting} className="flex-1">
            {submitting
              ? "Đang lưu..."
              : isEdit
                ? "Cập nhật báo giá"
                : "Tạo báo giá"}
          </Button>
          <Button type="button" variant="secondary" onClick={onClose}>
            Hủy
          </Button>
        </div>
      </form>
    </Modal>
  );
}
