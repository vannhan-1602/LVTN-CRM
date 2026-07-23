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

function formatMoney(n) {
  if (!n && n !== 0) return "—";
  return Number(n).toLocaleString("vi-VN") + " đ";
}

export default function ManagerDashboard() {
  const navigate = useNavigate();
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState("");
  const [data, setData] = useState(null);

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
          chiSummary,
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
          analyticsApi.getChiSummary().catch(() => null),
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
          tongChiThangNay: chiSummary?.data?.tongChiThangNay ?? 0,
          topKhachHangChi: chiSummary?.data?.topKhachHangPhatSinhChi ?? [],
        });
      } catch (err) {
        if (!cancelled)
          setError(err?.message || "Không thể tải dữ liệu dashboard");
      } finally {
        if (!cancelled) setLoading(false);
      }
    })();

    return () => {
      cancelled = true;
    };
  }, []);

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
          value={formatMoney(data.tongChiThangNay)}
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

          {data.topKhachHangChi.length > 0 && (
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
              <div className="grid grid-cols-1 sm:grid-cols-2 gap-2">
                {data.topKhachHangChi.map((kh) => (
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
            </Card>
          )}
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
