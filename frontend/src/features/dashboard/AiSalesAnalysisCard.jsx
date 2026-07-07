import { useState } from "react";
import {
  Sparkles, Loader2, RefreshCw, TrendingUp, Target, Receipt,
  Headset, Package, Info, AlertTriangle,
} from "lucide-react";
import {
  ResponsiveContainer, BarChart, Bar, XAxis, YAxis, CartesianGrid, Tooltip,
} from "recharts";
import analyticsApi from "../../api/analyticsApi";
import Card from "../../components/common/Card";
import Button from "../../components/common/Button";

function formatMoney(n) {
  if (!n && n !== 0) return "—";
  return Number(n).toLocaleString("vi-VN") + " đ";
}

function formatThang(nam, thang) {
  return `${String(thang).padStart(2, "0")}/${nam}`;
}

const NHOM_ICON = {
  DoanhThu: TrendingUp,
  CoHoi: Target,
  CongNo: Receipt,
  Ticket: Headset,
  SanPham: Package,
  Khac: Info,
};

const UU_TIEN_STYLE = {
  Cao: { badge: "bg-danger-50 text-danger-600", bar: "bg-danger-500", label: "Ưu tiên cao" },
  TrungBinh: { badge: "bg-warning-50 text-warning-700", bar: "bg-warning-500", label: "Ưu tiên vừa" },
  Thap: { badge: "bg-success-50 text-success-700", bar: "bg-success-500", label: "Ưu tiên thấp" },
};

function DeXuatCard({ item }) {
  const Icon = NHOM_ICON[item.nhomVanDe] || Sparkles;
  const style = UU_TIEN_STYLE[item.mucDoUuTien] || UU_TIEN_STYLE.TrungBinh;
  return (
    <div className="flex gap-3 border border-ink-100 rounded-lg p-3">
      <div className={`shrink-0 w-1 rounded-full ${style.bar}`} />
      <div className="flex-1 min-w-0">
        <div className="flex items-center justify-between gap-2 mb-1">
          <div className="flex items-center gap-2 min-w-0">
            <Icon size={15} className="text-ink-400 shrink-0" />
            <p className="text-sm font-semibold text-ink-900 truncate">{item.tieuDe}</p>
          </div>
          <span className={`shrink-0 text-[11px] font-medium px-2 py-0.5 rounded-full ${style.badge}`}>
            {style.label}
          </span>
        </div>
        <p className="text-sm text-ink-500 leading-relaxed">{item.moTa}</p>
      </div>
    </div>
  );
}

// Tách riêng component: gọi AI tốn token/tiền + có độ trễ (vài giây), nên KHÔNG tự động
// gọi khi vào Dashboard — Manager bấm nút mới gọi, tránh gọi AI vô ích mỗi lần F5.
export default function AiSalesAnalysisCard() {
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState("");
  const [result, setResult] = useState(null);

  const handleAnalyze = async () => {
    setLoading(true);
    setError("");
    try {
      const res = await analyticsApi.getAiSalesAnalysis(6);
      setResult(res.data);
    } catch (err) {
      setError(
        err?.response?.data?.message || "Không thể tạo phân tích AI. Vui lòng thử lại."
      );
    } finally {
      setLoading(false);
    }
  };

  const doanhThuChart = (result?.duLieu.doanhThuTheoThang || []).map((m) => ({
    thang: formatThang(m.nam, m.thang),
    doanhThu: m.doanhThu,
  }));
  const sanPhamChart = (result?.duLieu.top5SanPhamBanChay || []).map((sp) => ({
    ten: sp.tenSanPham,
    soLuong: sp.soLuongBan,
  }));

  return (
    <Card
      title="AI phân tích dữ liệu bán hàng"
      action={
        <Button
          size="sm"
          variant={result ? "secondary" : "primary"}
          icon={loading ? Loader2 : result ? RefreshCw : Sparkles}
          onClick={handleAnalyze}
          disabled={loading}
        >
          {loading ? "Đang phân tích..." : result ? "Phân tích lại" : "Phân tích ngay"}
        </Button>
      }
    >
      {!result && !loading && !error && (
        <p className="text-sm text-ink-400">
          Bấm "Phân tích ngay" để xem biểu đồ doanh thu/sản phẩm bán chạy 6 tháng gần nhất,
          kèm đề xuất hành động từ AI cho từng nhóm vấn đề (doanh thu, cơ hội, công nợ, ticket).
        </p>
      )}

      {loading && (
        <div className="flex items-center gap-2 text-sm text-ink-400 py-4">
          <Loader2 size={16} className="animate-spin" />
          Đang gửi dữ liệu cho AI, có thể mất vài giây...
        </div>
      )}

      {error && (
        <div className="flex items-center gap-2 text-sm text-danger-600 bg-danger-50 rounded-lg px-3 py-2.5">
          <AlertTriangle size={16} className="shrink-0" />
          {error}
        </div>
      )}

      {result && !loading && (
        <div className="space-y-5">
          {/* Số liệu nhanh */}
          <div className="grid grid-cols-2 sm:grid-cols-4 gap-3">
            <div className="bg-surface-alt rounded-lg px-3 py-2">
              <p className="text-xs text-ink-400">Tỉ lệ thắng cơ hội</p>
              <p className="text-sm font-semibold text-ink-900">{result.duLieu.tyLeThangCoHoi}%</p>
            </div>
            <div className="bg-surface-alt rounded-lg px-3 py-2">
              <p className="text-xs text-ink-400">Ticket khẩn cấp</p>
              <p className="text-sm font-semibold text-ink-900">{result.duLieu.soTicketKhanCap}</p>
            </div>
            <div className="bg-surface-alt rounded-lg px-3 py-2">
              <p className="text-xs text-ink-400">Công nợ chưa thu</p>
              <p className="text-sm font-semibold text-ink-900">{formatMoney(result.duLieu.tongCongNoChuaThu)}</p>
            </div>
            <div className="bg-surface-alt rounded-lg px-3 py-2">
              <p className="text-xs text-ink-400">Số cơ hội</p>
              <p className="text-sm font-semibold text-ink-900">{result.duLieu.tongSoCoHoi}</p>
            </div>
          </div>

          {/* Biểu đồ */}
          <div className="grid grid-cols-1 lg:grid-cols-2 gap-4">
            <div>
              <p className="text-xs font-medium text-ink-500 mb-2">Doanh thu theo tháng</p>
              <div className="h-48">
                <ResponsiveContainer width="100%" height="100%">
                  <BarChart data={doanhThuChart}>
                    <CartesianGrid strokeDasharray="3 3" vertical={false} stroke="#eee" />
                    <XAxis dataKey="thang" tick={{ fontSize: 11 }} />
                    <YAxis
                      tick={{ fontSize: 11 }}
                      tickFormatter={(v) => (v >= 1_000_000 ? `${(v / 1_000_000).toFixed(0)}tr` : v)}
                    />
                    <Tooltip formatter={(v) => formatMoney(v)} />
                    <Bar dataKey="doanhThu" fill="#6366f1" radius={[4, 4, 0, 0]} />
                  </BarChart>
                </ResponsiveContainer>
              </div>
            </div>
            <div>
              <p className="text-xs font-medium text-ink-500 mb-2">Top sản phẩm bán chạy</p>
              <div className="h-48">
                <ResponsiveContainer width="100%" height="100%">
                  <BarChart data={sanPhamChart} layout="vertical" margin={{ left: 8 }}>
                    <CartesianGrid strokeDasharray="3 3" horizontal={false} stroke="#eee" />
                    <XAxis type="number" tick={{ fontSize: 11 }} />
                    <YAxis dataKey="ten" type="category" width={110} tick={{ fontSize: 11 }} />
                    <Tooltip />
                    <Bar dataKey="soLuong" fill="#10b981" radius={[0, 4, 4, 0]} />
                  </BarChart>
                </ResponsiveContainer>
              </div>
            </div>
          </div>

          {/* Nhận định + đề xuất kiểu agency */}
          <div>
            <p className="text-sm text-ink-700 mb-3">{result.nhanDinhTongQuan}</p>
            <div className="space-y-2">
              {result.deXuat.map((item, idx) => (
                <DeXuatCard key={idx} item={item} />
              ))}
            </div>
          </div>

          <p className="text-xs text-ink-300">
            Tạo lúc {new Date(result.generatedAt).toLocaleString("vi-VN")}
          </p>
        </div>
      )}
    </Card>
  );
}
