import { useEffect, useState } from "react";
import { useNavigate } from "react-router-dom";
import {
  Users,
  FileText,
  Receipt,
  Headset,
  TrendingUp,
  CheckCircle2,
  AlertCircle,
  ArrowRight,
  Target,
  Wallet,
} from "lucide-react";
import opportunityApi from "../../api/opportunityApi";
import contractApi from "../../api/contractApi";
import quoteApi from "../../api/quoteApi";
import ticketApi from "../../api/ticketApi";
import customerApi from "../../api/customerApi";
import analyticsApi from "../../api/analyticsApi";
import Card from "../../components/common/Card";
import StatCard from "../../components/common/StatCard";
import Button from "../../components/common/Button";
import {
  GIAI_DOAN_LIST,
  GIAI_DOAN_LABEL,
  GIAI_DOAN_HEADER_COLOR,
} from "../../utils/constants";
import AiSalesAnalysisCard from "./AiSalesAnalysisCard";
import DashboardAlertsCard from "./DashboardAlertsCard";

import { getApiErrorMessage } from "../../utils/formatters";
import useRealtimeStore from "../../stores/realtimeStore";
function formatMoney(n) {
  if (!n && n !== 0) return "—";
  return Number(n).toLocaleString("vi-VN") + " đ";
}

export default function ManagerDashboard() {
  const navigate = useNavigate();
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState("");
  const [data, setData] = useState(null);

  // Khung "Top khách hàng phát sinh chi phí" có bộ lọc thời gian riêng, tách khỏi phần load
  // chính của dashboard để đổi bộ lọc không phải gọi lại toàn bộ API khác.
  const [chiFilter, setChiFilter] = useState({ tuNgay: "", denNgay: "" });
  const [chiSummary, setChiSummary] = useState(null);
  const [loadingChi, setLoadingChi] = useState(true);

  // Dashboard tổng hợp số liệu từ nhiều module — chỉ cần BẤT KỲ module nào trong số này đổi là
  // tải lại toàn bộ, không cần người dùng F5 (đúng mục tiêu ban đầu: dashboard tự cập nhật).
  // Đưa cả 5 giá trị riêng biệt vào dependency array bên dưới thay vì gộp thành 1 biến — gộp
  // bằng ?? sẽ chỉ nhận giá trị đầu tiên khác null, bỏ lỡ thay đổi ở các module còn lại.
  const customerTick = useRealtimeStore((s) => s.lastUpdated.customer);
  const contractTick = useRealtimeStore((s) => s.lastUpdated.contract);
  const quoteTick = useRealtimeStore((s) => s.lastUpdated.quote);
  const ticketTick = useRealtimeStore((s) => s.lastUpdated.ticket);
  const opportunityTick = useRealtimeStore((s) => s.lastUpdated.opportunity);

  useEffect(() => {
    let cancelled = false;
    (async () => {
      setLoadingChi(true);
      try {
        const res = await analyticsApi.getChiSummary({
          tuNgay: chiFilter.tuNgay || undefined,
          denNgay: chiFilter.denNgay || undefined,
        });
        if (!cancelled) setChiSummary(res.data ?? null);
      } catch {
        if (!cancelled) setChiSummary(null);
      } finally {
        if (!cancelled) setLoadingChi(false);
      }
    })();
    return () => {
      cancelled = true;
    };
  }, [chiFilter.tuNgay, chiFilter.denNgay]);

  useEffect(() => {
    let cancelled = false;

    (async () => {
      setLoading(true);
      setError("");
      try {
        const [
          oppSummary,
          customers,
          contractsTotal,
          contractsActive,
          quotesTotal,
          quotesPending,
          ticketsOpen,
          ticketsUrgent,
          trends,
        ] = await Promise.all([
          opportunityApi.getSummary(),
          customerApi.getAll({ pageNumber: 1, pageSize: 1 }),
          contractApi.getAll({ pageNumber: 1, pageSize: 1 }),
          contractApi.getAll({
            pageNumber: 1,
            pageSize: 1,
            trangThai: "DangThucHien",
          }),
          quoteApi.getAll({ pageNumber: 1, pageSize: 1 }),
          quoteApi.getAll({ pageNumber: 1, pageSize: 1, trangThai: "DaGui" }),
          ticketApi.getAll({ pageNumber: 1, pageSize: 1, trangThai: "Moi" }),
          ticketApi.getAll({
            pageNumber: 1,
            pageSize: 1,
            mucDoUuTien: "KhanCap",
          }),
          // Không để lỗi API trends làm hỏng cả dashboard — nếu lỗi thì coi như không có trend,
          // stat card vẫn hiện số liệu chính bình thường, chỉ thiếu mũi tên xu hướng.
          analyticsApi.getDashboardTrends().catch(() => null),
        ]);

        if (cancelled) return;
        const t = trends?.data;
        setData({
          opp: oppSummary.data,
          tongKhachHang: customers.data?.totalCount ?? 0,
          tongHopDong: contractsTotal.data?.totalCount ?? 0,
          hopDongDangThucHien: contractsActive.data?.totalCount ?? 0,
          tongBaoGia: quotesTotal.data?.totalCount ?? 0,
          baoGiaChoPhanHoi: quotesPending.data?.totalCount ?? 0,
          ticketMoi: ticketsOpen.data?.totalCount ?? 0,
          ticketKhanCap: ticketsUrgent.data?.totalCount ?? 0,
          trendKhachHang: t
            ? t.khachHangMoiThangNay - t.khachHangMoiThangTruoc
            : undefined,
          trendHopDong: t
            ? t.hopDongMoiThangNay - t.hopDongMoiThangTruoc
            : undefined,
          trendBaoGia: t
            ? t.baoGiaMoiThangNay - t.baoGiaMoiThangTruoc
            : undefined,
          trendTicket: t
            ? t.ticketMoiThangNay - t.ticketMoiThangTruoc
            : undefined,
        });
      } catch (err) {
        if (!cancelled)
          setError(getApiErrorMessage(err, "Không thể tải dữ liệu dashboard"));
      } finally {
        if (!cancelled) setLoading(false);
      }
    })();

    return () => {
      cancelled = true;
    };
  }, [customerTick, contractTick, quoteTick, ticketTick, opportunityTick]);

  if (loading)
    return (
      <div className="text-sm text-ink-400 py-10 text-center">
        Đang tải dashboard...
      </div>
    );
  if (error)
    return (
      <div className="text-sm text-danger-600 bg-danger-50 rounded-lg p-4">
        {error}
      </div>
    );
  if (!data) return null;

  const maxStageCount = Math.max(
    1,
    ...GIAI_DOAN_LIST.map((s) => data.opp.countByStage?.[s] ?? 0),
  );

  return (
    <div className="space-y-5">
      <div>
        <p className="text-xs text-ink-400 uppercase tracking-wide mb-0.5">
          CRM / Tổng quan
        </p>
        <h1 className="text-xl font-semibold text-ink-900">
          Dashboard quản lý
        </h1>
      </div>

      {/* Hàng số liệu chính */}
      <div className="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-5 gap-3">
        <StatCard
          label="Tổng khách hàng"
          value={data.tongKhachHang}
          icon={Users}
          trend={data.trendKhachHang}
        />
        <StatCard
          label="Hợp đồng đang thực hiện"
          value={`${data.hopDongDangThucHien} / ${data.tongHopDong}`}
          tone="success"
          icon={FileText}
          trend={data.trendHopDong}
        />
        <StatCard
          label="Báo giá chờ phản hồi"
          value={`${data.baoGiaChoPhanHoi} / ${data.tongBaoGia}`}
          tone="info"
          icon={Receipt}
          trend={data.trendBaoGia}
        />
        <StatCard
          label="Ticket khẩn cấp"
          value={data.ticketKhanCap}
          tone={data.ticketKhanCap > 0 ? "warning" : "default"}
          icon={AlertCircle}
          trend={data.trendTicket}
        />
        <StatCard
          label="Tổng chi tháng này"
          value={formatMoney(chiSummary?.tongChiThangNay)}
          tone="warning"
          icon={Wallet}
        />
      </div>

      <div className="grid grid-cols-1 lg:grid-cols-3 gap-4 items-start">
        {/* Pipeline cơ hội bán hàng */}
        <div className="lg:col-span-2 space-y-4">
          <Card
            title="Pipeline cơ hội bán hàng"
            action={
              <Button
                size="sm"
                variant="secondary"
                icon={ArrowRight}
                onClick={() => navigate("/opportunities")}
              >
                Xem chi tiết
              </Button>
            }
          >
            <div className="space-y-3">
              {GIAI_DOAN_LIST.map((stage) => {
                const count = data.opp.countByStage?.[stage] ?? 0;
                const pct = Math.round((count / maxStageCount) * 100);
                return (
                  <div key={stage} className="flex items-center gap-3">
                    <span className="text-xs text-ink-500 w-28 shrink-0">
                      {GIAI_DOAN_LABEL[stage]}
                    </span>
                    <div className="flex-1 bg-ink-100 rounded-full h-2.5 overflow-hidden">
                      <div
                        className={`h-full rounded-full ${GIAI_DOAN_HEADER_COLOR[stage]}`}
                        style={{ width: `${pct}%` }}
                      />
                    </div>
                    <span className="text-xs font-medium text-ink-900 w-6 text-right shrink-0">
                      {count}
                    </span>
                  </div>
                );
              })}
            </div>

            <div className="grid grid-cols-3 gap-3 mt-5 pt-4 border-t border-ink-100">
              <div>
                <p className="text-xs text-ink-400 mb-0.5">Đang xử lý</p>
                <p className="text-lg font-semibold text-info-600">
                  {data.opp.totalActive}
                </p>
              </div>
              <div>
                <p className="text-xs text-ink-400 mb-0.5">DT kỳ vọng</p>
                <p className="text-lg font-semibold text-accent-600">
                  {formatMoney(data.opp.totalDoanhThuKyVong)}
                </p>
              </div>
              <div>
                <p className="text-xs text-ink-400 mb-0.5">DT đã chốt</p>
                <p className="text-lg font-semibold text-success-700">
                  {formatMoney(data.opp.doanhThuThanhCong)}
                </p>
              </div>
            </div>
          </Card>

          <Card
            title="Top khách hàng phát sinh chi phí"
            action={
              <Button
                size="sm"
                variant="secondary"
                icon={ArrowRight}
                onClick={() => navigate("/phieu-thu-chi")}
              >
                Xem tất cả
              </Button>
            }
          >
            <div className="flex flex-wrap items-end gap-2 mb-4">
              <div>
                <label className="block text-xs text-ink-400 mb-1">
                  Từ ngày
                </label>
                <input
                  type="date"
                  value={chiFilter.tuNgay}
                  onChange={(e) =>
                    setChiFilter((f) => ({ ...f, tuNgay: e.target.value }))
                  }
                  className="border border-ink-200 rounded-lg px-2.5 py-1.5 text-sm focus:outline-none focus:ring-2 focus:ring-accent-400/40 focus:border-accent-400"
                />
              </div>
              <div>
                <label className="block text-xs text-ink-400 mb-1">
                  Đến ngày
                </label>
                <input
                  type="date"
                  value={chiFilter.denNgay}
                  onChange={(e) =>
                    setChiFilter((f) => ({ ...f, denNgay: e.target.value }))
                  }
                  className="border border-ink-200 rounded-lg px-2.5 py-1.5 text-sm focus:outline-none focus:ring-2 focus:ring-accent-400/40 focus:border-accent-400"
                />
              </div>
              {(chiFilter.tuNgay || chiFilter.denNgay) && (
                <button
                  onClick={() => setChiFilter({ tuNgay: "", denNgay: "" })}
                  className="text-xs font-medium text-ink-500 hover:underline pb-1.5"
                >
                  Bỏ lọc
                </button>
              )}
            </div>

            <div className="flex items-center justify-between bg-surface-alt rounded-lg px-3.5 py-2.5 mb-3">
              <span className="text-xs text-ink-500">
                Tổng cộng đã chi{" "}
                {chiFilter.tuNgay || chiFilter.denNgay
                  ? "trong khoảng đã lọc"
                  : "(toàn thời gian)"}
              </span>
              <span className="text-sm font-semibold text-danger-600">
                {loadingChi
                  ? "Đang tính..."
                  : formatMoney(chiSummary?.tongChiTheoBoLoc)}
              </span>
            </div>

            {loadingChi ? (
              <p className="text-sm text-ink-400 text-center py-3">
                Đang tải...
              </p>
            ) : (chiSummary?.topKhachHangPhatSinhChi?.length ?? 0) === 0 ? (
              <p className="text-sm text-ink-400 text-center py-3">
                Không có phiếu chi nào trong khoảng thời gian này.
              </p>
            ) : (
              <div className="grid grid-cols-1 sm:grid-cols-2 gap-2">
                {chiSummary.topKhachHangPhatSinhChi.map((kh) => (
                  <button
                    key={kh.khachHangId}
                    onClick={() => navigate(`/customers/${kh.khachHangId}`)}
                    className="w-full flex items-center justify-between bg-surface-alt border border-ink-100 rounded-lg px-3 py-2.5 text-left hover:bg-ink-100"
                  >
                    <div>
                      <p className="text-sm font-medium text-ink-900">
                        {kh.tenKhachHang}
                      </p>
                      <p className="text-xs text-ink-400">
                        {kh.soPhieu} phiếu chi
                      </p>
                    </div>
                    <span className="text-sm font-semibold text-danger-600">
                      {formatMoney(kh.tongChi)}
                    </span>
                  </button>
                ))}
              </div>
            )}
          </Card>
        </div>

        {/* Lối tắt + cảnh báo */}
        <div className="space-y-4">
          <DashboardAlertsCard />

          <Card title="Cần chú ý">
            <div className="space-y-2">
              {data.ticketKhanCap > 0 && (
                <button
                  onClick={() => navigate("/tickets")}
                  className="w-full flex items-center justify-between bg-warning-50 border border-warning-100 rounded-lg px-3 py-2.5 text-left hover:bg-warning-50/70"
                >
                  <span className="text-sm text-warning-700 font-medium">
                    {data.ticketKhanCap} ticket khẩn cấp
                  </span>
                  <ArrowRight size={15} className="text-warning-600" />
                </button>
              )}
              {data.ticketMoi > 0 && (
                <button
                  onClick={() => navigate("/tickets")}
                  className="w-full flex items-center justify-between bg-info-50 border border-info-100 rounded-lg px-3 py-2.5 text-left hover:bg-info-50/70"
                >
                  <span className="text-sm text-info-700 font-medium">
                    {data.ticketMoi} ticket chưa xử lý
                  </span>
                  <ArrowRight size={15} className="text-info-600" />
                </button>
              )}
              {data.baoGiaChoPhanHoi > 0 && (
                <button
                  onClick={() => navigate("/quotes")}
                  className="w-full flex items-center justify-between bg-surface-alt border border-ink-100 rounded-lg px-3 py-2.5 text-left hover:bg-ink-100"
                >
                  <span className="text-sm text-ink-700 font-medium">
                    {data.baoGiaChoPhanHoi} báo giá chờ khách phản hồi
                  </span>
                  <ArrowRight size={15} className="text-ink-500" />
                </button>
              )}
              {data.ticketKhanCap === 0 &&
                data.ticketMoi === 0 &&
                data.baoGiaChoPhanHoi === 0 && (
                  <div className="flex items-center gap-2 text-success-700 bg-success-50 rounded-lg px-3 py-2.5">
                    <CheckCircle2 size={16} />
                    <span className="text-sm font-medium">
                      Mọi thứ đang ổn, chưa có việc cần xử lý gấp.
                    </span>
                  </div>
                )}
            </div>
          </Card>

          <Card title="Truy cập nhanh">
            <div className="grid grid-cols-2 gap-2">
              <Button
                size="sm"
                variant="secondary"
                icon={Target}
                onClick={() => navigate("/leads")}
              >
                Lead
              </Button>
              <Button
                size="sm"
                variant="secondary"
                icon={Users}
                onClick={() => navigate("/customers")}
              >
                Khách hàng
              </Button>
              <Button
                size="sm"
                variant="secondary"
                icon={TrendingUp}
                onClick={() => navigate("/opportunities")}
              >
                Cơ hội
              </Button>
              <Button
                size="sm"
                variant="secondary"
                icon={Receipt}
                onClick={() => navigate("/quotes")}
              >
                Báo giá
              </Button>
              <Button
                size="sm"
                variant="secondary"
                icon={FileText}
                onClick={() => navigate("/contracts")}
              >
                Hợp đồng
              </Button>
              <Button
                size="sm"
                variant="secondary"
                icon={Headset}
                onClick={() => navigate("/tickets")}
              >
                Ticket
              </Button>
            </div>
          </Card>
        </div>
      </div>

      <AiSalesAnalysisCard />
    </div>
  );
}
