import { useEffect, useState } from "react";
import { useNavigate, useParams } from "react-router-dom";
import { Pencil, KeyRound, Lock, Unlock, Trash2 } from "lucide-react";
import userManagementApi from "../../api/userManagementApi";
import PageHeader from "../../components/common/PageHeader";
import Card, { Field } from "../../components/common/Card";
import Badge from "../../components/common/Badge";
import Button from "../../components/common/Button";
import UserFormModal from "./UserFormModal";
import ResetPasswordModal from "./ResetPasswordModal";
import { USER_ROLE_COLOR, USER_STATUS_LABEL, USER_STATUS_COLOR } from "../../utils/constants";
import { formatDateTime } from "../../utils/formatters";

export default function UserDetailPage() {
  const { id } = useParams();
  const navigate = useNavigate();

  const [user, setUser] = useState(null);
  const [lookups, setLookups] = useState({ roles: [], phongBans: [], chucVus: [] });
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState("");
  const [showEditModal, setShowEditModal] = useState(false);
  const [showResetModal, setShowResetModal] = useState(false);

  const load = async () => {
    setLoading(true); setError("");
    try {
      const [userRes, lookupsRes] = await Promise.all([userManagementApi.getById(id), userManagementApi.getLookups()]);
      setUser(userRes.data ?? null);
      setLookups(lookupsRes.data ?? { roles: [], phongBans: [], chucVus: [] });
    } catch (err) { setError(err?.message || "Không thể tải thông tin tài khoản"); }
    finally { setLoading(false); }
  };

  useEffect(() => { load(); }, [id]);

  const handleToggleStatus = async () => {
    const next = user.trangThai === "Active" ? "Locked" : "Active";
    const verb = next === "Locked" ? "khóa" : "mở khóa";
    if (!window.confirm(`Bạn có chắc muốn ${verb} tài khoản "${user.username}"?`)) return;
    try { await userManagementApi.toggleStatus(id, next); await load(); }
    catch (err) { setError(err?.message || "Không thể đổi trạng thái"); }
  };

  const handleDelete = async () => {
    if (!window.confirm(`Xóa vĩnh viễn tài khoản "${user.username}"? Hành động này không thể hoàn tác.`)) return;
    try { await userManagementApi.delete(id); navigate("/users"); }
    catch (err) { setError(err?.message || "Không thể xóa tài khoản"); }
  };

  if (loading) return <div className="text-sm text-ink-400 py-10 text-center">Đang tải...</div>;

  if (error || !user) {
    return (
      <div className="space-y-4">
        <PageHeader breadcrumb="CRM / Admin" title="Tài khoản" onBack={() => navigate("/users")} />
        <div className="text-sm text-danger-600 bg-danger-50 rounded-lg p-4">{error || "Không tìm thấy tài khoản."}</div>
      </div>
    );
  }

  return (
    <div className="space-y-5">
      {showEditModal && (
        <UserFormModal user={user} lookups={lookups} onClose={() => setShowEditModal(false)}
          onSaved={() => { setShowEditModal(false); load(); }} />
      )}
      {showResetModal && (
        <ResetPasswordModal user={user} onClose={() => setShowResetModal(false)} onDone={() => setShowResetModal(false)} />
      )}

      <PageHeader
        breadcrumb="Tài khoản người dùng"
        title={user.hoTen ?? user.username}
        onBack={() => navigate("/users")}
        badge={
          <div className="flex gap-1.5">
            <Badge label={user.roleName ?? "—"} colorClass={USER_ROLE_COLOR[user.roleName]} />
            <Badge label={USER_STATUS_LABEL[user.trangThai] ?? user.trangThai} colorClass={USER_STATUS_COLOR[user.trangThai]} />
          </div>
        }
        actions={
          <>
            <Button variant="secondary" icon={Pencil} onClick={() => setShowEditModal(true)}>Sửa</Button>
            <Button variant="secondary" icon={KeyRound} onClick={() => setShowResetModal(true)}>Đặt lại mật khẩu</Button>
            <Button variant="secondary" icon={user.trangThai === "Active" ? Lock : Unlock} onClick={handleToggleStatus}>
              {user.trangThai === "Active" ? "Khóa" : "Mở khóa"}
            </Button>
            <Button variant="danger" icon={Trash2} onClick={handleDelete}>Xóa</Button>
          </>
        }
      />

      {error && <div className="text-sm text-danger-600 bg-danger-50 rounded-lg p-3">{error}</div>}

      <div className="grid grid-cols-1 lg:grid-cols-3 gap-4">
        <div className="lg:col-span-2 space-y-4">
          <Card title="Thông tin tài khoản">
            <div className="grid grid-cols-2 gap-5">
              <Field label="Tên đăng nhập" value={<span className="font-mono">{user.username}</span>} />
              <Field label="Họ tên" value={user.hoTen} />
              <Field label="Email" value={user.email} />
              <Field label="Số điện thoại" value={user.soDienThoai} />
              <Field label="Phòng ban" value={user.tenPhongBan} />
              <Field label="Chức vụ" value={user.tenChucVu} />
              <Field label="Ngày tạo" value={formatDateTime(user.createdAt)} />
              <Field label="Cập nhật gần nhất" value={formatDateTime(user.updatedAt)} />
            </div>
          </Card>
        </div>
      </div>
    </div>
  );
}
