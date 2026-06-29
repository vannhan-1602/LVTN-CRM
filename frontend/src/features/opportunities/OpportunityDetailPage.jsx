import { useEffect, useState } from "react";
import { useNavigate, useParams } from "react-router-dom";
import { Pencil, Trash2, ArrowRight, XCircle } from "lucide-react";
import opportunityApi from "../../api/opportunityApi";
import customerApi from "../../api/customerApi";
import leadApi from "../../api/leadApi";
import useAuthStore from "../auth/authStore";
import PageHeader from "../../components/common/PageHeader";
import Card, { Field } from "../../components/common/Card";
import Badge from "../../components/common/Badge";
import Button from "../../components/common/Button";
import OpportunityFormModal from "./OpportunityFormModal";
import ChangeStageModal from "./ChangeStageModal";
import { ROLES, GIAI_DOAN_LABEL, GIAI_DOAN_COLOR, NEXT_STAGE } from "../../utils/constants";
import { formatDateTime } from "../../utils/formatters";

function formatMoney(n) {
  if (!n && n !== 0) return "—";
  return Number(n).toLocaleString("vi-VN") + " đ";
}

export default function OpportunityDetailPage() {
  const { id } = useParams();
  const navigate = useNavigate();
  const { user } = useAuthStore();
  const canDelete = user?.role === ROLES.Manager;

  const [item, setItem] = useState(null);
  const [customers, setCustomers] = useState([]);
  const [leads, setLeads] = useState([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState("");
  const [showEditModal, setShowEditModal] = useState(false);
  const [stageChange, setStageChange] = useState(null);

  const load = async () => {
    setLoading(true); setError("");
    try {
      const res = await opportunityApi.getById(id);
      setItem(res.data ?? null);
    } catch (err) { setError(err?.message || "Không thể tải thông tin cơ hội"); }
    finally { setLoading(false); }
  };

  useEffect(() => { load(); }, [id]);
  useEffect(() => {
    customerApi.getAll({ pageNumber: 1, pageSize: 200 }).then((r) => setCustomers(r.data?.items ?? [])).catch(() => {});
    leadApi.getAll({ pageNumber: 1, pageSize: 200 }).then((r) => setLeads(r.data?.items ?? [])).catch(() => {});
  }, []);

  const handleDelete = async () => {
    if (!window.confirm(`Xóa cơ hội "${item.tenThuongVu}"?`)) return;
    try { await opportunityApi.delete(id); navigate("/opportunities"); }
    catch (err) { setError(err?.message || "Không thể xóa"); }
  };

  if (loading) return <div className="text-sm text-ink-400 py-10 text-center">Đang tải...</div>;

  if (error || !item) {
    return (
      <div className="space-y-4">
        <PageHeader breadcrumb="CRM / Kinh doanh" title="Cơ hội bán hàng" onBack={() => navigate("/opportunities")} />
        <div className="text-sm text-danger-600 bg-danger-50 rounded-lg p-4">{error || "Không tìm thấy cơ hội."}</div>
      </div>
    );
  }

  const isFinal = item.giaiDoan === "ThanhCong" || item.giaiDoan === "ThatBai";
  const canMoveNext = NEXT_STAGE[item.giaiDoan];

  return (
    <div className="space-y-5">
      {showEditModal && (
        <OpportunityFormModal item={item} customers={customers} leads={leads}
          onClose={() => setShowEditModal(false)} onSaved={() => { setShowEditModal(false); load(); }} />
      )}
      {stageChange && (
        <ChangeStageModal item={item} targetStage={stageChange} onClose={() => setStageChange(null)}
          onSaved={() => { setStageChange(null); load(); }} />
      )}

      <PageHeader
        breadcrumb="Cơ hội bán hàng"
        title={item.tenThuongVu}
        onBack={() => navigate("/opportunities")}
        badge={<Badge label={GIAI_DOAN_LABEL[item.giaiDoan]} colorClass={GIAI_DOAN_COLOR[item.giaiDoan]} />}
        actions={
          <>
            <Button variant="secondary" icon={Pencil} onClick={() => setShowEditModal(true)}>Sửa</Button>
            {canDelete && <Button variant="danger" icon={Trash2} onClick={handleDelete}>Xóa</Button>}
          </>
        }
      />

      {error && <div className="text-sm text-danger-600 bg-danger-50 rounded-lg p-3">{error}</div>}

      <div className="grid grid-cols-1 lg:grid-cols-3 gap-4">
        <div className="lg:col-span-2 space-y-4">
          <Card title="Thông tin thương vụ">
            <div className="grid grid-cols-2 gap-5">
              <Field label="Khách hàng / Lead" value={item.tenKhachHang || item.tenLead} />
              <Field label="Nhân viên phụ trách" value={item.tenNhanVien} />
              <Field label="Tỷ lệ thành công" value={`${item.tyLeThanhCong}%`} />
              <Field label="Doanh thu kỳ vọng" value={formatMoney(item.doanhThuKyVong)} />
              <Field label="Ngày dự kiến chốt" value={item.ngayDuKien ?? "—"} />
              <Field label="Cập nhật gần nhất" value={formatDateTime(item.updatedAt)} />
            </div>
            {item.ghiChu && (
              <div className="mt-4 pt-4 border-t border-ink-100">
                <p className="text-xs text-ink-400 mb-1">Ghi chú</p>
                <p className="text-sm text-ink-900">{item.ghiChu}</p>
              </div>
            )}
          </Card>
        </div>

        <div className="space-y-4">
          {!isFinal && (
            <Card title="Chuyển giai đoạn">
              <div className="space-y-2">
                {canMoveNext && (
                  <Button size="sm" icon={ArrowRight} className="w-full" onClick={() => setStageChange(canMoveNext)}>
                    Chuyển sang {GIAI_DOAN_LABEL[canMoveNext]}
                  </Button>
                )}
                <Button size="sm" variant="danger" icon={XCircle} className="w-full" onClick={() => setStageChange("ThatBai")}>
                  Đánh dấu thất bại
                </Button>
              </div>
            </Card>
          )}

          {isFinal && (
            <Card>
              <p className="text-xs text-ink-400">
                Thương vụ đã kết thúc ở giai đoạn <strong className="text-ink-700">{GIAI_DOAN_LABEL[item.giaiDoan]}</strong>.
              </p>
            </Card>
          )}
        </div>
      </div>
    </div>
  );
}
