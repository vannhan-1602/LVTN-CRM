import { useEffect, useState } from "react";
import {
  GraduationCap,
  PackageCheck,
  ClipboardCheck,
  ListChecks,
  Plus,
  X,
  Paperclip,
} from "lucide-react";
import contractApi from "../../api/contractApi";
import authApi from "../../api/authApi";
import Card from "../../components/common/Card";
import Badge from "../../components/common/Badge";
import Button from "../../components/common/Button";
import EmptyState from "../../components/common/EmptyState";
import EmployeeSelect from "../../components/common/EmployeeSelect";
import {
  MOC_LOAI_OPTIONS,
  MOC_TRANG_THAI_OPTIONS,
  MOC_TRANG_THAI_LABEL,
  MOC_TRANG_THAI_COLOR,
} from "../../utils/constants";
import { formatDateTime } from "../../utils/formatters";

const LOAI_MOC_META = {
  DaoTao: { label: "Đào tạo", icon: GraduationCap, tone: "info" },
  BanGiao: { label: "Bàn giao", icon: PackageCheck, tone: "warning" },
  NghiemThu: { label: "Nghiệm thu", icon: ClipboardCheck, tone: "success" },
};

const metaOf = (loaiMoc) =>
  LOAI_MOC_META[loaiMoc] ?? {
    label: loaiMoc,
    icon: ListChecks,
    tone: "neutral",
  };

// yyyy-MM-ddTHH:mm cho input datetime-local (giờ local, không lệch UTC)
const toDatetimeLocal = (d) => {
  const pad = (n) => String(n).padStart(2, "0");
  return `${d.getFullYear()}-${pad(d.getMonth() + 1)}-${pad(d.getDate())}T${pad(d.getHours())}:${pad(d.getMinutes())}`;
};

const emptyForm = {
  loaiMoc: "DaoTao",
  noiDung: "",
  ngayThucHien: toDatetimeLocal(new Date()),
  nhanVienThucHienId: "",
  trangThai: "ChuaThucHien",
  nguoiXacNhanKhach: "",
  fileBienBan: "",
};

/**
 * Mốc triển khai hợp đồng (HD_MocTrienKhai) — Đào tạo / Bàn giao / Nghiệm thu.
 * Nhúng vào ContractDetailPage. Sale/Manager được ghi (SalesTeam policy ở
 * ContractController); Accountant/Admin chỉ xem qua CustomerReadAccess.
 *
 * Props:
 *  - hopDongId: id hợp đồng
 *  - canEdit: true nếu role hiện tại là Sale/Manager
 *  - isFinal: hợp đồng đã Thanh lý/Hết hạn — khóa thêm/sửa/xóa mốc
 */
export default function MilestoneSection({ hopDongId, canEdit, isFinal }) {
  const [milestones, setMilestones] = useState([]);
  const [nhanVienList, setNhanVienList] = useState([]);
  const [loading, setLoading] = useState(true);
  const [showForm, setShowForm] = useState(false);
  const [form, setForm] = useState(emptyForm);
  const [editingId, setEditingId] = useState(null);
  const [submitting, setSubmitting] = useState(false);
  const [error, setError] = useState("");

  const load = async () => {
    setLoading(true);
    try {
      const res = await contractApi.getMocTrienKhai(hopDongId);
      const list = res.data ?? [];
      list.sort((a, b) => new Date(b.createdAt) - new Date(a.createdAt));
      setMilestones(list);
    } catch {
      setError("Không thể tải danh sách mốc triển khai");
    } finally {
      setLoading(false);
    }
  };

  const loadNhanVien = async () => {
    try {
      const res = await authApi.getStaffList();
      setNhanVienList(res.data ?? []);
    } catch {
      /* không có quyền xem danh sách nhân viên, bỏ qua */
    }
  };

  useEffect(() => {
    if (hopDongId) load();
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [hopDongId]);

  useEffect(() => {
    if (canEdit) loadNhanVien();
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [canEdit]);

  const resetForm = () => {
    setForm(emptyForm);
    setEditingId(null);
    setShowForm(false);
    setError("");
  };

  const handleEdit = (moc) => {
    setEditingId(moc.id);
    setForm({
      loaiMoc: moc.loaiMoc,
      noiDung: moc.noiDung ?? "",
      ngayThucHien: moc.ngayThucHien
        ? toDatetimeLocal(new Date(moc.ngayThucHien))
        : toDatetimeLocal(new Date()),
      nhanVienThucHienId: moc.nhanVienThucHienId ?? "",
      trangThai: moc.trangThai ?? "ChuaThucHien",
      nguoiXacNhanKhach: moc.nguoiXacNhanKhach ?? "",
      fileBienBan: moc.fileBienBan ?? "",
    });
    setShowForm(true);
  };

  const handleSubmit = async (e) => {
    e.preventDefault();
    setSubmitting(true);
    setError("");
    try {
      const ngayThucHienIso = form.ngayThucHien
        ? new Date(form.ngayThucHien).toISOString()
        : null;
      const nhanVienId = form.nhanVienThucHienId
        ? Number(form.nhanVienThucHienId)
        : null;

      if (editingId) {
        // PUT /Contract/moc-trien-khai/{id} — cập nhật đầy đủ, kể cả trạng thái
        // và bằng chứng nghiệm thu (người xác nhận khách + link biên bản).
        await contractApi.updateMocTrienKhai(editingId, {
          noiDung: form.noiDung.trim() || null,
          ngayThucHien: ngayThucHienIso,
          nhanVienThucHienId: nhanVienId,
          nguoiXacNhanKhach: form.nguoiXacNhanKhach.trim() || null,
          fileBienBan: form.fileBienBan.trim() || null,
          trangThai: form.trangThai,
        });
      } else {
        // POST /Contract/{id}/moc-trien-khai — tạo mới, chưa có trạng thái xác nhận.
        await contractApi.createMocTrienKhai(hopDongId, {
          loaiMoc: form.loaiMoc,
          noiDung: form.noiDung.trim() || null,
          ngayThucHien: ngayThucHienIso,
          nhanVienThucHienId: nhanVienId,
        });
      }
      await load();
      resetForm();
    } catch (err) {
      setError(err?.message || "Không thể lưu mốc triển khai");
    } finally {
      setSubmitting(false);
    }
  };

  const handleDelete = async (id) => {
    if (
      !window.confirm("Xóa mốc triển khai này? Hành động không thể phục hồi.")
    )
      return;
    try {
      await contractApi.deleteMocTrienKhai(id);
      await load();
    } catch (err) {
      setError(err?.message || "Không thể xóa mốc triển khai");
    }
  };

  const canAdd = canEdit && !isFinal;

  return (
    <Card
      title={`Mốc triển khai (${milestones.length})`}
      action={
        canAdd &&
        !showForm && (
          <Button
            size="sm"
            variant="secondary"
            icon={Plus}
            onClick={() => setShowForm(true)}
          >
            Thêm mốc
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
              {editingId
                ? "Cập nhật mốc triển khai"
                : "Thêm mốc triển khai mới"}
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
                Loại mốc
              </label>
              <select
                value={form.loaiMoc}
                onChange={(e) =>
                  setForm((f) => ({ ...f, loaiMoc: e.target.value }))
                }
                disabled={Boolean(editingId)}
                className="w-full border border-ink-200 rounded-lg px-3 py-2 text-sm disabled:bg-ink-50 disabled:text-ink-400"
              >
                {MOC_LOAI_OPTIONS.map((o) => (
                  <option key={o.value} value={o.value}>
                    {o.label}
                  </option>
                ))}
              </select>
            </div>
            <div>
              <label className="block text-xs font-medium text-ink-500 mb-1">
                Ngày thực hiện
              </label>
              <input
                type="datetime-local"
                value={form.ngayThucHien}
                onChange={(e) =>
                  setForm((f) => ({ ...f, ngayThucHien: e.target.value }))
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
              rows={2}
              value={form.noiDung}
              onChange={(e) =>
                setForm((f) => ({ ...f, noiDung: e.target.value }))
              }
              placeholder="Mô tả công việc đã/sẽ thực hiện..."
              className="w-full border border-ink-200 rounded-lg px-3 py-2 text-sm resize-none"
            />
          </div>

          <div>
            <label className="block text-xs font-medium text-ink-500 mb-1">
              Nhân viên thực hiện
            </label>
            {nhanVienList.length > 0 ? (
              <EmployeeSelect
                value={form.nhanVienThucHienId}
                onChange={(v) =>
                  setForm((f) => ({ ...f, nhanVienThucHienId: v }))
                }
                options={nhanVienList}
              />
            ) : (
              <input
                type="number"
                min="1"
                value={form.nhanVienThucHienId}
                onChange={(e) =>
                  setForm((f) => ({ ...f, nhanVienThucHienId: e.target.value }))
                }
                placeholder="ID nhân viên"
                className="w-full border border-ink-200 rounded-lg px-3 py-2 text-sm"
              />
            )}
          </div>

          {/* Chỉ hiện khi Sửa: xác nhận nghiệm thu (backend Update mới nhận các field này) */}
          {editingId && (
            <>
              <div className="grid grid-cols-2 gap-3">
                <div>
                  <label className="block text-xs font-medium text-ink-500 mb-1">
                    Trạng thái
                  </label>
                  <select
                    value={form.trangThai}
                    onChange={(e) =>
                      setForm((f) => ({ ...f, trangThai: e.target.value }))
                    }
                    className="w-full border border-ink-200 rounded-lg px-3 py-2 text-sm"
                  >
                    {MOC_TRANG_THAI_OPTIONS.map((o) => (
                      <option key={o.value} value={o.value}>
                        {o.label}
                      </option>
                    ))}
                  </select>
                </div>
                <div>
                  <label className="block text-xs font-medium text-ink-500 mb-1">
                    Người xác nhận (khách)
                  </label>
                  <input
                    value={form.nguoiXacNhanKhach}
                    onChange={(e) =>
                      setForm((f) => ({
                        ...f,
                        nguoiXacNhanKhach: e.target.value,
                      }))
                    }
                    placeholder="Tên người phía khách hàng ký xác nhận"
                    className="w-full border border-ink-200 rounded-lg px-3 py-2 text-sm"
                  />
                </div>
              </div>
              <div>
                <label className="block text-xs font-medium text-ink-500 mb-1">
                  Link biên bản
                </label>
                <input
                  value={form.fileBienBan}
                  onChange={(e) =>
                    setForm((f) => ({ ...f, fileBienBan: e.target.value }))
                  }
                  placeholder="Đường dẫn/URL file biên bản đã ký"
                  className="w-full border border-ink-200 rounded-lg px-3 py-2 text-sm"
                />
              </div>
            </>
          )}

          {error && (
            <div className="text-sm text-danger-600 bg-danger-50 rounded-lg p-2">
              {error}
            </div>
          )}

          <div className="flex gap-2">
            <Button type="submit" size="sm" disabled={submitting}>
              {submitting ? "Đang lưu..." : editingId ? "Cập nhật" : "Thêm mốc"}
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
      ) : milestones.length === 0 ? (
        <EmptyState
          icon={ListChecks}
          title="Chưa có mốc triển khai nào"
          description="Thêm mốc Đào tạo, Bàn giao hoặc Nghiệm thu để theo dõi tiến độ triển khai hợp đồng."
        />
      ) : (
        <div className="space-y-2">
          {milestones.map((moc) => {
            const meta = metaOf(moc.loaiMoc);
            const Icon = meta.icon;
            return (
              <div
                key={moc.id}
                className="border border-ink-100 rounded-lg p-3.5 flex items-start gap-3"
              >
                <div className="w-8 h-8 rounded-full bg-ink-50 flex items-center justify-center text-ink-500 shrink-0 mt-0.5">
                  <Icon size={15} />
                </div>
                <div className="flex-1 min-w-0">
                  <div className="flex items-center gap-2 flex-wrap mb-1">
                    <Badge label={meta.label} tone={meta.tone} />
                    <Badge
                      label={
                        MOC_TRANG_THAI_LABEL[moc.trangThai] ?? moc.trangThai
                      }
                      colorClass={MOC_TRANG_THAI_COLOR[moc.trangThai]}
                    />
                    {moc.ngayThucHien && (
                      <span className="text-xs text-ink-400">
                        {formatDateTime(moc.ngayThucHien)}
                      </span>
                    )}
                  </div>
                  {moc.noiDung && (
                    <p className="text-sm text-ink-900 whitespace-pre-wrap">
                      {moc.noiDung}
                    </p>
                  )}
                  <div className="flex flex-wrap gap-x-4 mt-1">
                    {moc.tenNhanVienThucHien && (
                      <p className="text-xs text-ink-400">
                        Người thực hiện: {moc.tenNhanVienThucHien}
                      </p>
                    )}
                    {moc.nguoiXacNhanKhach && (
                      <p className="text-xs text-ink-400">
                        Khách xác nhận: {moc.nguoiXacNhanKhach}
                      </p>
                    )}
                  </div>
                  {moc.fileBienBan && (
                    <a
                      href={moc.fileBienBan}
                      target="_blank"
                      rel="noreferrer"
                      className="text-xs font-medium text-info-600 hover:underline inline-flex items-center gap-1 mt-1.5"
                    >
                      <Paperclip size={12} /> Xem biên bản
                    </a>
                  )}
                </div>
                {canEdit && !isFinal && (
                  <div className="flex gap-3 shrink-0">
                    <button
                      onClick={() => handleEdit(moc)}
                      className="text-xs font-medium text-info-600 hover:underline"
                    >
                      Sửa
                    </button>
                    <button
                      onClick={() => handleDelete(moc.id)}
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
