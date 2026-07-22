import { useEffect, useState } from "react";
import { useNavigate, useParams } from "react-router-dom";
import {
  Pencil,
  Trash2,
  Receipt,
  ExternalLink,
  Users,
  FileText,
} from "lucide-react";
import contractApi from "../../api/contractApi";
import useAuthStore from "../auth/authStore";
import PageHeader from "../../components/common/PageHeader";
import Card, { Field } from "../../components/common/Card";
import Badge from "../../components/common/Badge";
import Button from "../../components/common/Button";
import EditContractModal from "./EditContractModal";
import { ROLES, CONTRACT_STATUS } from "../../utils/constants";
import { formatDate, formatDateTime } from "../../utils/formatters";

const STATUS_TONE = {
  DangThucHien: "success",
  TamDung: "warning",
  ThanhLy: "neutral",
  HetHan: "danger",
};

function formatMoney(n) {
  return n == null ? "Không có" : Number(n).toLocaleString("vi-VN") + " đ";
}

export default function ContractDetailPage() {
  const { id } = useParams();
  const navigate = useNavigate();
  const { user } = useAuthStore();

  const canManage = [ROLES.Sale, ROLES.Manager].includes(user?.role);
  const canDelete = user?.role === ROLES.Manager;

  const [contract, setContract] = useState(null);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState("");

  const [showEditModal, setShowEditModal] = useState(false);

  const [lichThanhToans, setLichThanhToans] = useState([]);
  const [loadingLich, setLoadingLich] = useState(false);

  const load = async () => {
    setLoading(true);
    setError("");
    try {
      const res = await contractApi.getById(id);
      setContract(res.data ?? null);
    } catch (err) {
      setError(err?.message || "Không thể tải thông tin hợp đồng");
    } finally {
      setLoading(false);
    }
  };

  const loadLichThanhToan = async () => {
    setLoadingLich(true);
    try {
      const res = await contractApi.getLichThanhToan(id);
      setLichThanhToans(res.data ?? []);
    } catch {
      // im lặng bỏ qua — không phải hợp đồng nào cũng có lịch trả góp
    } finally {
      setLoadingLich(false);
    }
  };

  useEffect(() => {
    load();
  }, [id]);

  useEffect(() => {
    if (contract?.hinhThucThanhToan === "TraGop") {
      loadLichThanhToan();
    }
  }, [contract?.hinhThucThanhToan, id]);

  const handleDelete = async () => {
    if (!window.confirm("Xóa hợp đồng này? Hành động không thể phục hồi."))
      return;
    try {
      await contractApi.delete(id);
      navigate("/contracts");
    } catch (err) {
      setError(err?.message || "Không thể xóa hợp đồng");
    }
  };

  if (loading) {
    return (
      <div className="text-sm text-ink-400 py-10 text-center">Đang tải...</div>
    );
  }

  if (error || !contract) {
    return (
      <div className="space-y-4">
        <PageHeader
          breadcrumb="CRM / Kinh doanh"
          title="Hợp đồng"
          onBack={() => navigate("/contracts")}
        />
        <div className="text-sm text-danger-600 bg-danger-50 rounded-lg p-4">
          {error || "Không tìm thấy hợp đồng."}
        </div>
      </div>
    );
  }

  const isFinal =
    contract.trangThai === "ThanhLy" || contract.trangThai === "HetHan";

  return (
    <div className="space-y-5">
      {showEditModal && (
        <EditContractModal
          contract={contract}
          onClose={() => setShowEditModal(false)}
          onSaved={() => {
            setShowEditModal(false);
            load();
          }}
        />
      )}

      <PageHeader
        breadcrumb="Hợp đồng"
        title={contract.maHopDong}
        onBack={() => navigate("/contracts")}
        badge={
          <Badge
            label={CONTRACT_STATUS[contract.trangThai] ?? contract.trangThai}
            tone={STATUS_TONE[contract.trangThai]}
          />
        }
        actions={
          <>
            {canManage && !isFinal && (
              <Button
                variant="secondary"
                icon={Pencil}
                onClick={() => setShowEditModal(true)}
              >
                Sửa
              </Button>
            )}
            {canDelete && (
              <Button variant="danger" icon={Trash2} onClick={handleDelete}>
                Xóa
              </Button>
            )}
          </>
        }
      />

      {error && (
        <div className="text-sm text-danger-600 bg-danger-50 rounded-lg p-3">
          {error}
        </div>
      )}

      <div className="grid grid-cols-1 lg:grid-cols-3 gap-4">
        <div className="lg:col-span-2 space-y-4">
          <Card title="Thông tin hợp đồng">
            <div className="grid grid-cols-2 gap-5">
              <Field label="Khách hàng" value={contract.tenKhachHang} />
              <Field
                label="Giá trị hợp đồng"
                value={formatMoney(contract.giaTri)}
              />
              <Field label="Ngày ký" value={formatDate(contract.ngayKy)} />
              <Field
                label="Thời hạn"
                value={
                  contract.thoiHan ? `${contract.thoiHan} tháng` : "Không có"
                }
              />
              <Field
                label="Ngày kết thúc"
                value={formatDate(contract.ngayKetThuc)}
              />
              <Field
                label="Hình thức thanh toán"
                value={
                  contract.hinhThucThanhToan === "TraGop"
                    ? "Trả góp nhiều đợt"
                    : "Thanh toán 1 lần"
                }
              />
              <Field
                label="Ngày tạo"
                value={formatDateTime(contract.createdAt)}
              />
              <Field
                label="Cập nhật gần nhất"
                value={formatDateTime(contract.updatedAt)}
              />
            </div>
          </Card>

          {contract.hinhThucThanhToan === "TraGop" && (
            <Card title="Lịch trả góp">
              {loadingLich ? (
                <p className="text-sm text-ink-400">Đang tải lịch trả góp...</p>
              ) : lichThanhToans.length === 0 ? (
                <p className="text-sm text-ink-400">
                  Chưa có dữ liệu lịch trả góp.
                </p>
              ) : (
                <table className="w-full text-sm">
                  <thead>
                    <tr className="text-left text-ink-400 text-xs uppercase">
                      <th className="pb-2">Đợt</th>
                      <th className="pb-2">Số tiền</th>
                      <th className="pb-2">Hạn thanh toán</th>
                      <th className="pb-2">Trạng thái</th>
                    </tr>
                  </thead>
                  <tbody>
                    {lichThanhToans.map((l) => (
                      <tr key={l.id} className="border-t border-ink-100">
                        <td className="py-2">Đợt {l.soDot}</td>
                        <td className="py-2">{formatMoney(l.soTien)}</td>
                        <td className="py-2">{formatDate(l.hanThanhToan)}</td>
                        <td className="py-2">
                          <Badge
                            label={l.trangThai}
                            tone={
                              l.trangThai === "DaThanhToan"
                                ? "success"
                                : l.trangThai === "QuaHan"
                                  ? "danger"
                                  : "neutral"
                            }
                          />
                        </td>
                      </tr>
                    ))}
                  </tbody>
                </table>
              )}
            </Card>
          )}

          {contract.maBaoGia && (
            <Card title="Báo giá đính kèm">
              <button
                onClick={() => navigate(`/quotes?search=${contract.maBaoGia}`)}
                className="w-full flex items-center justify-between p-3.5 bg-surface-alt rounded-lg hover:bg-ink-100 transition-colors text-left"
              >
                <div className="flex items-center gap-3">
                  <div className="w-9 h-9 rounded-lg bg-accent-50 flex items-center justify-center text-accent-700">
                    <Receipt size={17} />
                  </div>
                  <div>
                    <p className="text-sm font-medium text-ink-900 font-mono">
                      {contract.maBaoGia}
                    </p>
                    <p className="text-xs text-ink-400">
                      Tổng tiền {formatMoney(contract.giaTri)}
                    </p>
                  </div>
                </div>
                <ExternalLink size={15} className="text-ink-400" />
              </button>
            </Card>
          )}
        </div>

        <div className="space-y-4">
          {isFinal && (
            <Card>
              <p className="text-xs text-ink-400">
                Hợp đồng đã{" "}
                {contract.trangThai === "HetHan" ? "Hết hạn" : "Thanh lý"},
                không thể thay đổi thông tin.
              </p>
            </Card>
          )}
          <Card title="Khách hàng liên quan">
            <div className="flex items-center gap-3 mb-3.5">
              <div className="w-10 h-10 rounded-full bg-info-50 flex items-center justify-center text-sm font-semibold text-info-700 shrink-0">
                {(contract.tenKhachHang || "?").slice(0, 2).toUpperCase()}
              </div>
              <div>
                <p className="text-sm font-medium text-ink-900">
                  {contract.tenKhachHang}
                </p>
                <p className="text-xs text-ink-400">Khách hàng</p>
              </div>
            </div>
            <Button
              variant="secondary"
              size="sm"
              icon={Users}
              className="w-full"
              onClick={() => navigate(`/customers/${contract.khachHangId}`)}
            >
              Xem hồ sơ khách hàng
            </Button>
          </Card>

          <Card title="Mã hợp đồng">
            <div className="flex items-center gap-2.5 text-ink-700">
              <FileText size={16} className="text-ink-400" />
              <span className="font-mono text-sm">{contract.maHopDong}</span>
            </div>
          </Card>
        </div>
      </div>
    </div>
  );
}
