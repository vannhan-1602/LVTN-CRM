import { useEffect, useState } from "react";
import {
  Phone,
  Users,
  Mail,
  MessageCircle,
  Plus,
  X,
  History,
} from "lucide-react";
import activityApi from "../../api/activityApi";
import Card from "../../components/common/Card";
import Badge from "../../components/common/Badge";
import Button from "../../components/common/Button";
import EmptyState from "../../components/common/EmptyState";
import { formatDateTime } from "../../utils/formatters";

const LOAI_HOAT_DONG_OPTIONS = [
  { value: "Call", label: "Cuộc gọi", icon: Phone, tone: "info" },
  { value: "Meeting", label: "Cuộc họp", icon: Users, tone: "success" },
  { value: "Email", label: "Email", icon: Mail, tone: "warning" },
  { value: "Zalo", label: "Zalo", icon: MessageCircle, tone: "neutral" },
];

const optionOf = (loai) =>
  LOAI_HOAT_DONG_OPTIONS.find((o) => o.value === loai) ??
  LOAI_HOAT_DONG_OPTIONS[0];

// yyyy-MM-ddTHH:mm cho input datetime-local (giờ local, không lệch UTC)
const toDatetimeLocal = (d) => {
  const pad = (n) => String(n).padStart(2, "0");
  return `${d.getFullYear()}-${pad(d.getMonth() + 1)}-${pad(d.getDate())}T${pad(d.getHours())}:${pad(d.getMinutes())}`;
};

const emptyForm = {
  loaiHoatDong: "Call",
  noiDung: "",
  thoiGianThucHien: toDatetimeLocal(new Date()),
};

/**
 * Nhật ký hoạt động chăm sóc (KH_HoatDong) — nhúng vào CustomerDetailPage hoặc LeadDetailPage.
 * Truyền đúng 1 trong 2 prop: khachHangId hoặc leadId.
 */
export default function ActivitySection({ khachHangId, leadId, canEdit }) {
  const [activities, setActivities] = useState([]);
  const [loading, setLoading] = useState(true);
  const [showForm, setShowForm] = useState(false);
  const [form, setForm] = useState(emptyForm);
  const [editingId, setEditingId] = useState(null);
  const [submitting, setSubmitting] = useState(false);
  const [error, setError] = useState("");

  const ownerId = khachHangId ?? leadId;

  const load = async () => {
    setLoading(true);
    try {
      const res = khachHangId
        ? await activityApi.getByCustomer(khachHangId)
        : await activityApi.getByLead(leadId);
      const list = res.data ?? [];
      list.sort(
        (a, b) => new Date(b.thoiGianThucHien) - new Date(a.thoiGianThucHien),
      );
      setActivities(list);
    } catch {
      setError("Không thể tải nhật ký hoạt động");
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => {
    if (ownerId) load();
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [ownerId]);

  const resetForm = () => {
    setForm(emptyForm);
    setEditingId(null);
    setShowForm(false);
    setError("");
  };

  const handleEdit = (act) => {
    setEditingId(act.id);
    setForm({
      loaiHoatDong: act.loaiHoatDong ?? "Call",
      noiDung: act.noiDung ?? "",
      thoiGianThucHien: act.thoiGianThucHien
        ? toDatetimeLocal(new Date(act.thoiGianThucHien))
        : toDatetimeLocal(new Date()),
    });
    setShowForm(true);
  };

  const handleSubmit = async (e) => {
    e.preventDefault();
    setSubmitting(true);
    setError("");
    try {
      const payload = {
        loaiHoatDong: form.loaiHoatDong,
        noiDung: form.noiDung || null,
        thoiGianThucHien: new Date(form.thoiGianThucHien).toISOString(),
      };
      if (editingId) {
        await activityApi.update(editingId, payload);
      } else {
        await activityApi.create({
          ...payload,
          khachHangId: khachHangId ?? null,
          leadId: leadId ?? null,
        });
      }
      await load();
      resetForm();
    } catch (err) {
      setError(err?.message || "Không thể lưu hoạt động");
    } finally {
      setSubmitting(false);
    }
  };

  const handleDelete = async (id) => {
    if (!window.confirm("Xóa hoạt động này?")) return;
    try {
      await activityApi.delete(id);
      await load();
    } catch (err) {
      setError(err?.message || "Không thể xóa");
    }
  };

  return (
    <Card
      title={`Nhật ký hoạt động (${activities.length})`}
      action={
        canEdit &&
        !showForm && (
          <Button
            size="sm"
            variant="secondary"
            icon={Plus}
            onClick={() => setShowForm(true)}
          >
            Ghi nhận hoạt động
          </Button>
        )
      }
    >
      {showForm && (
        <form
          onSubmit={handleSubmit}
          className="border border-ink-100 rounded-lg p-4 bg-surface-alt space-y-3 mb-4"
        >
          <div className="flex items-center justify-between">
            <h4 className="text-sm font-medium text-ink-900">
              {editingId ? "Sửa hoạt động" : "Ghi nhận hoạt động mới"}
            </h4>
            <button
              type="button"
              onClick={resetForm}
              className="text-ink-400 hover:text-ink-700"
            >
              <X size={16} />
            </button>
          </div>

          <div className="grid grid-cols-2 gap-3">
            <div>
              <label className="block text-xs font-medium text-ink-500 mb-1">
                Loại hoạt động
              </label>
              <select
                value={form.loaiHoatDong}
                onChange={(e) =>
                  setForm((f) => ({ ...f, loaiHoatDong: e.target.value }))
                }
                className="w-full border border-ink-200 rounded-lg px-3 py-2 text-sm"
              >
                {LOAI_HOAT_DONG_OPTIONS.map((o) => (
                  <option key={o.value} value={o.value}>
                    {o.label}
                  </option>
                ))}
              </select>
            </div>
            <div>
              <label className="block text-xs font-medium text-ink-500 mb-1">
                Thời gian thực hiện
              </label>
              <input
                type="datetime-local"
                value={form.thoiGianThucHien}
                onChange={(e) =>
                  setForm((f) => ({ ...f, thoiGianThucHien: e.target.value }))
                }
                className="w-full border border-ink-200 rounded-lg px-3 py-2 text-sm"
              />
            </div>
          </div>

          <div>
            <label className="block text-xs font-medium text-ink-500 mb-1">
              Nội dung
            </label>
            <textarea
              rows={3}
              value={form.noiDung}
              onChange={(e) =>
                setForm((f) => ({ ...f, noiDung: e.target.value }))
              }
              placeholder="Tóm tắt nội dung trao đổi..."
              className="w-full border border-ink-200 rounded-lg px-3 py-2 text-sm resize-none"
            />
          </div>

          {error && (
            <div className="text-sm text-danger-600 bg-danger-50 rounded-lg p-2">
              {error}
            </div>
          )}

          <div className="flex gap-2">
            <Button type="submit" size="sm" disabled={submitting}>
              {submitting ? "Đang lưu..." : editingId ? "Cập nhật" : "Ghi nhận"}
            </Button>
            <Button
              type="button"
              size="sm"
              variant="secondary"
              onClick={resetForm}
            >
              Hủy
            </Button>
          </div>
        </form>
      )}

      {!showForm && error && (
        <div className="text-sm text-danger-600 bg-danger-50 rounded-lg p-2 mb-3">
          {error}
        </div>
      )}

      {loading ? (
        <p className="text-sm text-ink-400 text-center py-4">Đang tải...</p>
      ) : activities.length === 0 ? (
        <EmptyState
          icon={History}
          title="Chưa có hoạt động nào được ghi nhận"
        />
      ) : (
        <div className="space-y-2">
          {activities.map((act) => {
            const opt = optionOf(act.loaiHoatDong);
            const Icon = opt.icon;
            return (
              <div
                key={act.id}
                className="border border-ink-100 rounded-lg p-3.5 flex items-start gap-3"
              >
                <div className="w-8 h-8 rounded-full bg-ink-50 flex items-center justify-center text-ink-500 shrink-0 mt-0.5">
                  <Icon size={15} />
                </div>
                <div className="flex-1 min-w-0">
                  <div className="flex items-center gap-2 flex-wrap mb-1">
                    <Badge label={opt.label} tone={opt.tone} />
                    <span className="text-xs text-ink-400">
                      {formatDateTime(act.thoiGianThucHien)}
                    </span>
                  </div>
                  {act.noiDung && (
                    <p className="text-sm text-ink-900 whitespace-pre-wrap">
                      {act.noiDung}
                    </p>
                  )}
                  {act.tenNhanVien && (
                    <p className="text-xs text-ink-400 mt-1">
                      Người thực hiện: {act.tenNhanVien}
                    </p>
                  )}
                </div>
                {canEdit && (
                  <div className="flex gap-3 shrink-0">
                    <button
                      onClick={() => handleEdit(act)}
                      className="text-xs font-medium text-info-600 hover:underline"
                    >
                      Sửa
                    </button>
                    <button
                      onClick={() => handleDelete(act.id)}
                      className="text-xs font-medium text-danger-600 hover:underline"
                    >
                      Xóa
                    </button>
                  </div>
                )}
              </div>
            );
          })}
        </div>
      )}
    </Card>
  );
}
