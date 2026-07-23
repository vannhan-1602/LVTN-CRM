import { useState } from "react";
import { useQuery } from "@tanstack/react-query";
import { Crown, Gift, TrendingUp, History, Ticket } from "lucide-react";
import customerApi from "../../api/customerApi";
import Card from "../../components/common/Card";
import Badge from "../../components/common/Badge";
import EmptyState from "../../components/common/EmptyState";
import { formatDate, formatDateTime } from "../../utils/formatters";

const LOAI_GIAO_DICH_LABEL = {
  MuaHang: "Mua hàng",
  DieuChinh: "Điều chỉnh",
};

const LY_DO_HANG_LABEL = {
  TuDongDuDieuKien: "Tự động đủ điều kiện",
  DieuChinhThuCong: "Điều chỉnh thủ công",
};

const LY_DO_VOUCHER_LABEL = {
  ThangHang: "Thăng hạng",
  SinhNhat: "Sinh nhật",
  NgayThanhLap: "Ngày thành lập",
  NgayLe: "Ngày lễ",
  CuoiNam: "Cuối năm",
  ThuCong: "Thủ công",
};

const VOUCHER_STATUS = {
  ConHieuLuc: { label: "Còn hiệu lực", tone: "success" },
  DaSuDung: { label: "Đã sử dụng", tone: "neutral" },
  HetHan: { label: "Hết hạn", tone: "danger" },
};

const TABS = [
  { key: "diem", label: "Lịch sử điểm", icon: TrendingUp },
  { key: "hang", label: "Lịch sử hạng", icon: History },
  { key: "voucher", label: "Voucher", icon: Ticket },
];

export default function CustomerLoyaltySection({ khachHangId }) {
  const [activeTab, setActiveTab] = useState("diem");

  const {
    data: info,
    isLoading: loading,
    error: queryError,
  } = useQuery({
    queryKey: ["customerLoyalty", khachHangId],
    queryFn: async () => {
      const res = await customerApi.getLoyaltyInfo(khachHangId);
      return res.data;
    },
    refetchInterval: 10000, // tự tải lại mỗi 1 giây (voucher vừa đổi ở nơi khác)
    refetchOnWindowFocus: true,
  });

  const error = queryError?.message || "";
  if (loading) {
    return (
      <Card title="Khách hàng thân thiết">
        <p className="text-sm text-ink-400 py-2">Đang tải...</p>
      </Card>
    );
  }

  if (error) {
    return (
      <Card title="Khách hàng thân thiết">
        <p className="text-sm text-danger-600">{error}</p>
      </Card>
    );
  }

  if (!info) return null;

  // Khách chưa từng có giao dịch nào (mua hàng) trong 12 tháng gần nhất —
  // lúc này "còn 0 điểm nữa để lên hạng" gây hiểu lầm, nên đổi sang thông điệp
  // yêu cầu giao dịch đầu tiên thay vì hiển thị số/phần trăm.
  const chuaCoGiaoDich = (info.soLanThu12Thang ?? 0) === 0;

  const mauSoTienDo =
    (info.tongDiem12Thang ?? 0) + (info.soDiemCanThemDeLenHang ?? 0);
  const tienDoPhanTram =
    info.soDiemCanThemDeLenHang != null &&
    info.tongDiem12Thang != null &&
    mauSoTienDo > 0
      ? Math.min(100, Math.round((info.tongDiem12Thang / mauSoTienDo) * 100))
      : null;

  return (
    <Card title="Khách hàng thân thiết">
      <div className="space-y-4">
        {/* Hạng hiện tại + điểm tích lũy */}
        <div className="flex items-center justify-between">
          <div className="flex items-center gap-2">
            <div className="w-9 h-9 rounded-full bg-warning-50 flex items-center justify-center">
              <Crown size={17} className="text-warning-600" />
            </div>
            <div>
              <p className="text-sm font-semibold text-ink-900">
                {info.tenHangHienTai ?? "Chưa xếp hạng"}
              </p>
              {info.phanTramGiamVoucher > 0 && (
                <p className="text-xs text-ink-400">
                  Ưu đãi voucher {info.phanTramGiamVoucher}%
                </p>
              )}
            </div>
          </div>
          <div className="text-right">
            <p className="text-lg font-semibold text-ink-900">
              {info.tongDiem12Thang.toLocaleString("vi-VN")}
            </p>
            <p className="text-xs text-ink-400">
              điểm / 12 tháng · {info.soLanThu12Thang} lần mua
            </p>
          </div>
        </div>

        {info.moTaQuyenLoi && (
          <p className="text-xs text-ink-500 bg-surface-alt rounded-lg p-2.5">
            {info.moTaQuyenLoi}
          </p>
        )}

        {/* Tiến độ lên hạng kế tiếp */}
        {info.tenHangTiepTheo && (
          <div className="space-y-1.5">
            <div className="flex items-center justify-between text-xs text-ink-500">
              <span>
                {chuaCoGiaoDich
                  ? `Cần giao dịch đầu tiên để bắt đầu tích điểm lên hạng ${info.tenHangTiepTheo}`
                  : `Còn ${info.soDiemCanThemDeLenHang?.toLocaleString("vi-VN")} điểm nữa để lên hạng ${info.tenHangTiepTheo}`}
              </span>
              {!chuaCoGiaoDich && tienDoPhanTram != null && (
                <span>{tienDoPhanTram}%</span>
              )}
            </div>
            <div className="h-1.5 bg-ink-100 rounded-full overflow-hidden">
              <div
                className="h-full bg-accent-500 rounded-full"
                style={{ width: `${chuaCoGiaoDich ? 0 : (tienDoPhanTram ?? 0)}%` }}
              />
            </div>
          </div>
        )}
        {!info.tenHangTiepTheo && info.tenHangHienTai && (
          <p className="text-xs text-success-700 bg-success-50 rounded-lg p-2.5">
            Đã đạt hạng cao nhất.
          </p>
        )}

        {/* Tabs */}
        <div className="border-b border-ink-100 flex gap-1">
          {TABS.map((t) => (
            <button
              key={t.key}
              onClick={() => setActiveTab(t.key)}
              className={`flex items-center gap-1.5 px-3 py-2 text-xs font-medium border-b-2 transition-colors ${
                activeTab === t.key
                  ? "border-accent-500 text-accent-600"
                  : "border-transparent text-ink-400 hover:text-ink-700"
              }`}
            >
              <t.icon size={13} />
              {t.label}
            </button>
          ))}
        </div>

        {activeTab === "diem" &&
          (info.lichSuDiem.length === 0 ? (
            <EmptyState icon={TrendingUp} title="Chưa có lịch sử điểm" />
          ) : (
            <div className="space-y-2 max-h-64 overflow-y-auto">
              {info.lichSuDiem.map((d) => (
                <div
                  key={d.id}
                  className="flex items-center justify-between text-sm border-b border-ink-50 pb-2 last:border-0"
                >
                  <div>
                    <p className="text-ink-700">
                      {LOAI_GIAO_DICH_LABEL[d.loaiGiaoDich] ?? d.loaiGiaoDich}
                    </p>
                    <p className="text-xs text-ink-400">
                      {formatDate(d.ngayPhatSinh)}
                    </p>
                  </div>
                  <span
                    className={`font-semibold ${d.soDiem >= 0 ? "text-success-600" : "text-danger-600"}`}
                  >
                    {d.soDiem >= 0 ? "+" : ""}
                    {d.soDiem}
                  </span>
                </div>
              ))}
            </div>
          ))}

        {activeTab === "hang" &&
          (info.lichSuHang.length === 0 ? (
            <EmptyState icon={History} title="Chưa có lịch sử thay đổi hạng" />
          ) : (
            <div className="space-y-2 max-h-64 overflow-y-auto">
              {info.lichSuHang.map((h) => (
                <div
                  key={h.id}
                  className="text-sm border-b border-ink-50 pb-2 last:border-0"
                >
                  <p className="text-ink-700">
                    {h.tenHangCu
                      ? `${h.tenHangCu} → ${h.tenHangMoi}`
                      : `Xếp hạng: ${h.tenHangMoi}`}
                  </p>
                  <p className="text-xs text-ink-400">
                    {LY_DO_HANG_LABEL[h.lyDo] ?? h.lyDo} ·{" "}
                    {formatDateTime(h.createdAt)}
                  </p>
                </div>
              ))}
            </div>
          ))}

        {activeTab === "voucher" &&
          (info.vouchers.length === 0 ? (
            <EmptyState icon={Gift} title="Chưa có voucher nào" />
          ) : (
            <div className="space-y-2 max-h-64 overflow-y-auto">
              {info.vouchers.map((v) => {
                const status = VOUCHER_STATUS[v.trangThai] ?? {
                  label: v.trangThai,
                  tone: "neutral",
                };
                return (
                  <div
                    key={v.id}
                    className="text-sm border-b border-ink-50 pb-2 last:border-0"
                  >
                    <div className="flex items-center justify-between">
                      <span className="font-mono text-xs font-semibold text-info-600">
                        {v.maVoucher}
                      </span>
                      <Badge label={status.label} tone={status.tone} />
                    </div>
                    <p className="text-xs text-ink-500 mt-0.5">
                      Giảm {v.giaTriGiam}
                      {v.loaiGiamGia === "PhanTram" ? "%" : "đ"}
                      {v.giaTriGiamToiDa
                        ? ` (tối đa ${v.giaTriGiamToiDa.toLocaleString("vi-VN")}đ)`
                        : ""}
                      {" · "}
                      {LY_DO_VOUCHER_LABEL[v.lyDoPhatHanh] ?? v.lyDoPhatHanh}
                    </p>
                    <p className="text-xs text-ink-400">
                      HSD: {formatDate(v.ngayBatDau)} —{" "}
                      {formatDate(v.ngayHetHan)}
                    </p>
                  </div>
                );
              })}
            </div>
          ))}
      </div>
    </Card>
  );
}
