import { useEffect, useMemo, useState } from "react";
import { useNavigate, useParams } from "react-router-dom";
import {
  Pencil,
  Trash2,
  ArrowRightLeft,
  Phone,
  Mail,
  Building2,
  PlayCircle,
  StopCircle,
} from "lucide-react";
import leadApi from "../../api/leadApi";
import authApi from "../../api/authApi";
import useAuthStore from "../auth/authStore";
import PageHeader from "../../components/common/PageHeader";
import Card, { Field } from "../../components/common/Card";
import Badge from "../../components/common/Badge";
import Button from "../../components/common/Button";
import LeadFormModal from "./LeadFormModal";
import ActivitySection from "../activities/ActivitySection";
import {
  ROLES,
  LEAD_TINH_TRANG_LABEL,
  LEAD_TINH_TRANG_COLOR,
} from "../../utils/constants";
import { formatDateTime } from "../../utils/formatters";

export default function LeadDetailPage() {
  const { id } = useParams();
  const navigate = useNavigate();
  const { user } = useAuthStore();
  const canEdit = [ROLES.Sale, ROLES.Manager].includes(user?.role);
  const canDelete = user?.role === ROLES.Manager;

  const [lead, setLead] = useState(null);
  const [nhanVienList, setNhanVienList] = useState([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState("");
  const [actionMsg, setActionMsg] = useState("");
  const [showEditModal, setShowEditModal] = useState(false);

  const nhanVienMap = useMemo(
    () =>
      new Map(
        nhanVienList.map((nv) => [String(nv.id), nv.hoTen ?? `NV #${nv.id}`]),
      ),
    [nhanVienList],
  );

  const load = async () => {
    setLoading(true);
    setError("");
    try {
      const res = await leadApi.getById(id);
      setLead(res.data ?? null);
    } catch (err) {
      setError(err?.message || "Không thể tải thông tin lead");
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => {
    load();
  }, [id]);
  useEffect(() => {
    (async () => {
      try {
        const res = await authApi.getStaffList();
        setNhanVienList(res.data ?? []);
      } catch {
        /* không có quyền, bỏ qua */
      }
    })();
  }, []);

  const handleDelete = async () => {
    if (!window.confirm("Xóa lead này? Hành động không thể hoàn tác.")) return;
    try {
      await leadApi.delete(id);
      navigate("/leads");
    } catch (err) {
      setError(err?.message || "Không thể xóa lead");
    }
  };

  const handleBatDauChamSoc = async () => {
    try {
      await leadApi.update(id, { ...lead, tinhTrang: "DangChamSoc" });
      setActionMsg("Đã chuyển sang Đang chăm sóc");
      await load();
    } catch (err) {
      setError(err?.message || "Không thể cập nhật trạng thái");
    }
  };

  const handleNgungChamSoc = async () => {
    if (!window.confirm("Ngừng chăm sóc lead này?")) return;
    try {
      await leadApi.update(id, { ...lead, tinhTrang: "ThatBai" });
      setActionMsg("Đã ngừng chăm sóc lead");
      await load();
    } catch (err) {
      setError(err?.message || "Không thể cập nhật trạng thái");
    }
  };

  const handleConvert = async () => {
    if (
      !window.confirm(
        `Chuyển đổi "${lead.tenLead}" thành khách hàng chính thức?`,
      )
    )
      return;
    try {
      await leadApi.convert(id, {
        tenKhachHang: lead.tenLead,
        email: lead.email || null,
        soDienThoai: lead.soDienThoai || null,
        nhanVienPhuTrachId: lead.nhanVienPhuTrachId || null,
      });
      navigate("/customers");
    } catch (err) {
      setError(err?.message || "Chuyển đổi thất bại");
    }
  };

  if (loading)
    return (
      <div className="text-sm text-ink-400 py-10 text-center">Đang tải...</div>
    );

  if (error || !lead) {
    return (
      <div className="space-y-4">
        <PageHeader
          breadcrumb="CRM / Kinh doanh"
          title="Lead"
          onBack={() => navigate("/leads")}
        />
        <div className="text-sm text-danger-600 bg-danger-50 rounded-lg p-4">
          {error || "Không tìm thấy lead."}
        </div>
      </div>
    );
  }

  const isMoi = lead.tinhTrang === "Moi";
  const isDangChamSoc = lead.tinhTrang === "DangChamSoc";
  const isConverted = lead.tinhTrang === "DaChuyenDoi";
  const isEnded = lead.tinhTrang === "ThatBai";

  return (
    <div className="space-y-5">
      {showEditModal && (
        <LeadFormModal
          lead={lead}
          nhanVienList={nhanVienList}
          canAssign={user?.role === ROLES.Manager}
          onClose={() => setShowEditModal(false)}
          onSaved={() => {
            setShowEditModal(false);
            load();
          }}
        />
      )}

      <PageHeader
        breadcrumb="Lead"
        title={lead.tenLead}
        onBack={() => navigate("/leads")}
        badge={
          lead.tinhTrang && (
            <Badge
              label={LEAD_TINH_TRANG_LABEL[lead.tinhTrang] ?? lead.tinhTrang}
              colorClass={LEAD_TINH_TRANG_COLOR[lead.tinhTrang]}
            />
          )
        }
        actions={
          <>
            {/* Chỉ được sửa khi chưa chuyển đổi */}
            {canEdit && !isConverted && (
              <Button
                variant="secondary"
                icon={Pencil}
                onClick={() => setShowEditModal(true)}
              >
                Sửa
              </Button>
            )}
            {/* Mới → Đang chăm sóc */}
            {canEdit && isMoi && (
              <Button icon={PlayCircle} onClick={handleBatDauChamSoc}>
                Bắt đầu chăm sóc
              </Button>
            )}
            {/* Đang chăm sóc → Ngừng hoặc Chuyển KH */}
            {canEdit && isDangChamSoc && (
              <>
                <Button
                  variant="secondary"
                  icon={StopCircle}
                  onClick={handleNgungChamSoc}
                >
                  Ngừng chăm sóc
                </Button>
                <Button icon={ArrowRightLeft} onClick={handleConvert}>
                  Chuyển thành KH
                </Button>
              </>
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
      {actionMsg && (
        <div className="text-sm text-success-700 bg-success-50 rounded-lg p-3">
          {actionMsg}
        </div>
      )}

      <div className="grid grid-cols-1 lg:grid-cols-3 gap-4">
        <div className="lg:col-span-2 space-y-4">
          <Card title="Thông tin Lead">
            <div className="grid grid-cols-2 gap-5">
              <Field label="Công ty" value={lead.tenCongTy} />
              <Field
                label="Nhân viên phụ trách"
                value={
                  lead.nhanVienPhuTrachId
                    ? (nhanVienMap.get(String(lead.nhanVienPhuTrachId)) ??
                      `NV #${lead.nhanVienPhuTrachId}`)
                    : "—"
                }
              />
              <Field label="Ngày tạo" value={formatDateTime(lead.createdAt)} />
              <Field
                label="Cập nhật gần nhất"
                value={formatDateTime(lead.updatedAt)}
              />
            </div>
          </Card>

          <ActivitySection leadId={lead.id} canEdit={canEdit} />
        </div>

        <div className="space-y-4">
          <Card title="Thông tin liên hệ">
            <div className="space-y-3">
              <div className="flex items-center gap-2.5 text-sm">
                <Mail size={15} className="text-ink-400 shrink-0" />
                <span className="text-ink-900">{lead.email || "—"}</span>
              </div>
              <div className="flex items-center gap-2.5 text-sm">
                <Phone size={15} className="text-ink-400 shrink-0" />
                <span className="text-ink-900">{lead.soDienThoai || "—"}</span>
              </div>
              <div className="flex items-center gap-2.5 text-sm">
                <Building2 size={15} className="text-ink-400 shrink-0" />
                <span className="text-ink-900">{lead.tenCongTy || "—"}</span>
              </div>
            </div>
          </Card>

          {isConverted && (
            <Card>
              <p className="text-xs text-success-700 bg-success-50 rounded-lg p-2.5">
                Lead này đã được chuyển đổi thành khách hàng chính thức.
              </p>
            </Card>
          )}
          {isEnded && (
            <Card>
              <p className="text-xs text-ink-500 bg-ink-50 rounded-lg p-2.5">
                Lead này đã ngừng chăm sóc.
              </p>
            </Card>
          )}
        </div>
      </div>
    </div>
  );
}
