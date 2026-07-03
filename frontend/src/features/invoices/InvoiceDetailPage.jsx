import { useEffect, useState } from "react";
import { useParams, useNavigate } from "react-router-dom";
import { ArrowLeft, Plus, Receipt, FileText } from "lucide-react";
import invoiceApi from "../../api/invoiceApi";
import phieuThuChiApi from "../../api/phieuThuChiApi";
import useAuthStore from "../auth/authStore";
import Button from "../../components/common/Button";
import Badge from "../../components/common/Badge";
import { ROLES } from "../../utils/constants";
import { formatCurrency, formatDate, formatDateTime } from "../../utils/formatters";
import CreatePhieuThuModal from "../phieuthuchi/CreatePhieuThuModal";

const STATUS_LABEL = {
  ChuaThanhToan: "Chưa thanh toán",
  ThanhToan1Phan: "Thanh toán 1 phần",
  HoanTat: "Hoàn tất",
};
const STATUS_COLOR = {
  ChuaThanhToan: "bg-danger-50 text-danger-600",
  ThanhToan1Phan: "bg-warning-50 text-warning-700",
  HoanTat: "bg-success-50 text-success-700",
};

export default function InvoiceDetailPage() {
  const { id } = useParams();
  const navigate = useNavigate();
  const { user } = useAuthStore();
  const canAddPhieuThu = [ROLES.Accountant, ROLES.Manager].includes(user?.role);

  const [invoice, setInvoice] = useState(null);
  const [phieuList, setPhieuList] = useState([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState("");
  const [showPhieuThuModal, setShowPhieuThuModal] = useState(false);

  const load = async () => {
    try {
      const [invRes, phieuRes] = await Promise.all([
        invoiceApi.getById(id),
        phieuThuChiApi.getAll({ hoaDonId: id, pageSize: 50 }),
      ]);
      setInvoice(invRes.data);
      setPhieuList(phieuRes.data?.items ?? []);
    } catch {
      setError("Không tải được hóa đơn.");
    } finally { setLoading(false); }
  };

  useEffect(() => { load(); }, [id]);

  if (loading) return <div className="text-center py-20 text-ink-400">Đang tải...</div>;
  if (error) return <div className="text-danger-600 text-center py-20">{error}</div>;
  if (!invoice) return null;

  const pctDaThu = invoice.tongTien > 0
    ? Math.min(100, Math.round((invoice.soTienDaThu / invoice.tongTien) * 100))
    : 0;

  return (
    <div className="space-y-5 max-w-4xl">
      {showPhieuThuModal && (
        <CreatePhieuThuModal
          hoaDonId={invoice.id}
          maHoaDon={invoice.maHoaDon}
          soTienConLai={invoice.soTienConLai}
          onClose={() => setShowPhieuThuModal(false)}
          onSaved={() => { setShowPhieuThuModal(false); load(); }}
        />
      )}

      {/* Header */}
      <div className="flex items-center gap-3">
        <button onClick={() => navigate("/invoices")}
          className="text-ink-400 hover:text-ink-700 hover:bg-ink-100 rounded-lg p-1.5 transition-colors">
          <ArrowLeft size={18} />
        </button>
        <div className="flex-1">
          <p className="text-xs text-ink-400">Kế toán / Hóa đơn</p>
          <h1 className="text-xl font-bold text-ink-900">{invoice.maHoaDon}</h1>
        </div>
        <Badge label={STATUS_LABEL[invoice.trangThaiThanhToan] ?? invoice.trangThaiThanhToan}
          colorClass={STATUS_COLOR[invoice.trangThaiThanhToan] ?? "bg-ink-100 text-ink-500"} />
      </div>

      {/* Thông tin hóa đơn */}
      <div className="bg-surface rounded-card border border-ink-100 p-5 grid grid-cols-2 gap-5">
        <div>
          <p className="text-xs text-ink-400 mb-0.5">Khách hàng</p>
          <p className="font-semibold text-ink-900">{invoice.tenKhachHang || "—"}</p>
        </div>
        <div>
          <p className="text-xs text-ink-400 mb-0.5">Hợp đồng liên kết</p>
          <p className="text-ink-700">{invoice.maHopDong || "Không có"}</p>
        </div>
        <div>
          <p className="text-xs text-ink-400 mb-0.5">Ngày tạo</p>
          <p className="text-ink-700">{formatDate(invoice.createdAt)}</p>
        </div>
        <div>
          <p className="text-xs text-ink-400 mb-0.5">Cập nhật lần cuối</p>
          <p className="text-ink-700">{formatDateTime(invoice.updatedAt)}</p>
        </div>
      </div>

      {/* Tiến độ thanh toán */}
      <div className="bg-surface rounded-card border border-ink-100 p-5 space-y-3">
        <h3 className="font-semibold text-ink-900">Tiến độ thanh toán</h3>
        <div className="flex gap-6">
          <div>
            <p className="text-xs text-ink-400">Tổng tiền</p>
            <p className="text-lg font-bold text-ink-900">{formatCurrency(invoice.tongTien)}</p>
          </div>
          <div>
            <p className="text-xs text-ink-400">Đã thu</p>
            <p className="text-lg font-bold text-success-600">{formatCurrency(invoice.soTienDaThu)}</p>
          </div>
          <div>
            <p className="text-xs text-ink-400">Còn lại</p>
            <p className="text-lg font-bold text-danger-600">{formatCurrency(invoice.soTienConLai)}</p>
          </div>
        </div>
        <div className="w-full bg-ink-100 rounded-full h-2">
          <div className="bg-success-500 h-2 rounded-full transition-all"
            style={{ width: `${pctDaThu}%` }} />
        </div>
        <p className="text-xs text-ink-400">{pctDaThu}% đã thanh toán</p>
      </div>

      {/* Danh sách phiếu thu */}
      <div className="bg-surface rounded-card border border-ink-100 overflow-hidden">
        <div className="px-5 py-3.5 border-b border-ink-100 flex items-center justify-between">
          <h3 className="font-semibold text-ink-900">Phiếu thu / chi</h3>
          {canAddPhieuThu && invoice.trangThaiThanhToan !== "HoanTat" && (
            <Button size="sm" icon={Plus} onClick={() => setShowPhieuThuModal(true)}>
              Tạo phiếu thu
            </Button>
          )}
        </div>
        {phieuList.length === 0 ? (
          <div className="py-10 text-center text-ink-400 text-sm">Chưa có phiếu thu nào</div>
        ) : (
          <table className="w-full text-sm">
            <thead>
              <tr className="bg-surface-alt">
                {["Mã phiếu", "Loại", "Số tiền", "Người lập", "Ngày tạo"].map(h => (
                  <th key={h} className="px-5 py-3 text-left text-xs font-medium text-ink-400 uppercase tracking-wide">{h}</th>
                ))}
              </tr>
            </thead>
            <tbody className="divide-y divide-ink-100">
              {phieuList.map(p => (
                <tr key={p.id} className="hover:bg-surface-alt transition-colors">
                  <td className="px-5 py-3 font-mono text-info-600 text-xs font-semibold">{p.maPhieu}</td>
                  <td className="px-5 py-3">
                    <Badge
                      label={p.loaiPhieu === "Thu" ? "Thu tiền" : "Chi tiền"}
                      colorClass={p.loaiPhieu === "Thu" ? "bg-success-50 text-success-700" : "bg-danger-50 text-danger-600"}
                    />
                  </td>
                  <td className="px-5 py-3 font-medium text-ink-900">{formatCurrency(p.soTien)}</td>
                  <td className="px-5 py-3 text-ink-600">{p.tenNguoiLap || "—"}</td>
                  <td className="px-5 py-3 text-ink-500 text-xs">{formatDateTime(p.ngayTao)}</td>
                </tr>
              ))}
            </tbody>
          </table>
        )}
      </div>
    </div>
  );
}
