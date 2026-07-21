import { useEffect, useState } from "react";
import { useNavigate } from "react-router-dom";
import { Wallet } from "lucide-react";
import phieuThuChiApi from "../../api/phieuThuChiApi";
import Card from "../../components/common/Card";
import EmptyState from "../../components/common/EmptyState";
import { formatCurrency, formatDate } from "../../utils/formatters";

// Chi phí phát sinh cho khách hàng (ăn uống, đàm phán...) — KHÔNG tính vào
// tiến độ thanh toán hóa đơn hay hợp đồng. Chỉ Manager/Accountant được xem
// (kiểm soát ở CustomerDetailPage trước khi render component này).
export default function CustomerExpenseSection({ khachHangId }) {
  const navigate = useNavigate();
  const [items, setItems] = useState([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState("");

  useEffect(() => {
    let cancelled = false;
    (async () => {
      setLoading(true);
      setError("");
      try {
        const res = await phieuThuChiApi.getAll({
          khachHangId,
          loaiPhieu: "Chi",
          pageSize: 50,
        });
        if (!cancelled) setItems(res.data?.items ?? []);
      } catch (err) {
        if (!cancelled) setError(err?.message || "Không tải được chi phí");
      } finally {
        if (!cancelled) setLoading(false);
      }
    })();
    return () => {
      cancelled = true;
    };
  }, [khachHangId]);

  const tongChi = items.reduce((s, i) => s + (i.soTien ?? 0), 0);

  return (
    <Card title="Chi phí đã chi cho khách hàng">
      <p className="text-xs text-ink-400 -mt-2 mb-3">
        Chi phí nội bộ (ăn uống, đàm phán...) — không ảnh hưởng công nợ/hợp đồng.
      </p>

      {loading ? (
        <p className="text-sm text-ink-400 text-center py-4">Đang tải...</p>
      ) : error ? (
        <div className="text-sm text-danger-600 bg-danger-50 rounded-lg p-3">{error}</div>
      ) : items.length === 0 ? (
        <EmptyState icon={Wallet} title="Chưa phát sinh chi phí nào" />
      ) : (
        <>
          <div className="flex items-center justify-between mb-3 pb-3 border-b border-ink-100">
            <span className="text-xs text-ink-400">Tổng đã chi</span>
            <span className="text-lg font-semibold text-danger-600">
              {formatCurrency(tongChi)}
            </span>
          </div>
          <div className="space-y-2">
            {items.map((p) => (
              <div
                key={p.id}
                onClick={() => p.hoaDonId && navigate(`/invoices/${p.hoaDonId}`)}
                className={`flex items-center justify-between border border-ink-100 rounded-lg px-3 py-2.5 ${p.hoaDonId ? "cursor-pointer hover:bg-surface-alt" : ""}`}
              >
                <div>
                  <p className="font-mono text-info-600 text-xs font-semibold">{p.maPhieu}</p>
                  <p className="text-xs text-ink-400 mt-0.5">
                    {p.tenNguoiLap || "—"} · {formatDate(p.ngayTao)}
                  </p>
                </div>
                <p className="font-medium text-danger-600">{formatCurrency(p.soTien)}</p>
              </div>
            ))}
          </div>
        </>
      )}
    </Card>
  );
}
