import { useEffect, useMemo, useState } from "react";
import { useNavigate, useParams } from "react-router-dom";
import { Trash2, Lock, Send, UserCog } from "lucide-react";
import ticketApi from "../../api/ticketApi";
import authApi from "../../api/authApi";
import useAuthStore from "../auth/authStore";
import PageHeader from "../../components/common/PageHeader";
import Card, { Field } from "../../components/common/Card";
import Badge from "../../components/common/Badge";
import Button from "../../components/common/Button";
import EmptyState from "../../components/common/EmptyState";
import {
  ROLES,
  TICKET_STATUS,
  TICKET_STATUS_OPTIONS,
  TICKET_STATUS_COLOR,
  TICKET_PRIORITY,
  TICKET_PRIORITY_OPTIONS,
  TICKET_PRIORITY_COLOR,
  TICKET_PHAN_HOI_LOAI_OPTIONS,
  TICKET_PHAN_HOI_LABEL,
} from "../../utils/constants";
import { formatDateTime } from "../../utils/formatters";

export default function TicketDetailPage() {
  const { id } = useParams();
  const navigate = useNavigate();
  const { user } = useAuthStore();
  const canDelete = user?.role === ROLES.Manager;

  const [ticket, setTicket] = useState(null);
  const [nhanVienList, setNhanVienList] = useState([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState("");

  const [editForm, setEditForm] = useState(null);
  const [savingEdit, setSavingEdit] = useState(false);
  const [assignNV, setAssignNV] = useState("");
  const [savingAssign, setSavingAssign] = useState(false);

  const [phanHoiForm, setPhanHoiForm] = useState({
    loaiPhanHoi: "NoiBoXuLy",
    noiDung: "",
  });
  const [submittingPhanHoi, setSubmittingPhanHoi] = useState(false);

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
      const res = await ticketApi.getById(id);
      setTicket(res.data);
      setEditForm({
        trangThai: res.data.trangThai,
        mucDoUuTien: res.data.mucDoUuTien,
        ngayHenXuLy: res.data.ngayHenXuLy
          ? res.data.ngayHenXuLy.slice(0, 16)
          : "",
      });
      setAssignNV(
        res.data.nhanVienXuLyId ? String(res.data.nhanVienXuLyId) : "",
      );
    } catch (err) {
      setError(err?.message || "Không thể tải ticket");
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => {
    load();
  }, [id]);
  useEffect(() => {
    authApi
      .getStaffList()
      .then((res) => setNhanVienList(res.data ?? []))
      .catch(() => {});
  }, []);

  const isClosed = ticket?.trangThai === "Dong";

  const handleSaveEdit = async (e) => {
    e.preventDefault();
    setSavingEdit(true);
    setError("");
    try {
      await ticketApi.update(id, {
        tieuDe: ticket.tieuDe,
        moTa: ticket.moTa,
        fileDinhKem: ticket.fileDinhKem,
        loaiTicketId: ticket.loaiTicketId,
        hopDongId: ticket.hopDongId,
        sanPhamId: ticket.sanPhamId,
        mucDoUuTien: editForm.mucDoUuTien,
        nguonTiepNhan: ticket.nguonTiepNhan,
        trangThai: editForm.trangThai,
        ngayHenXuLy: editForm.ngayHenXuLy || null,
      });
      await load();
    } catch (err) {
      setError(err?.message || "Cập nhật ticket thất bại");
    } finally {
      setSavingEdit(false);
    }
  };

  const handleAssign = async (e) => {
    e.preventDefault();
    setSavingAssign(true);
    setError("");
    try {
      await ticketApi.assign(id, {
        nhanVienXuLyId: assignNV ? Number(assignNV) : null,
      });
      await load();
    } catch (err) {
      setError(err?.message || "Gán nhân viên thất bại");
    } finally {
      setSavingAssign(false);
    }
  };

  const handleAddPhanHoi = async (e) => {
    e.preventDefault();
    if (!phanHoiForm.noiDung.trim()) return;
    setSubmittingPhanHoi(true);
    setError("");
    try {
      await ticketApi.addPhanHoi(id, phanHoiForm);
      setPhanHoiForm({ loaiPhanHoi: "NoiBoXuLy", noiDung: "" });
      await load();
    } catch (err) {
      setError(err?.message || "Thêm phản hồi thất bại");
    } finally {
      setSubmittingPhanHoi(false);
    }
  };

  const handleClose = async () => {
    const ly = window.prompt("Lý do đóng ticket (có thể để trống):");
    if (ly === null) return;
    try {
      await ticketApi.close(id, { lyDoDong: ly });
      await load();
    } catch (err) {
      setError(err?.message || "Không thể đóng ticket");
    }
  };

  const handleDelete = async () => {
    if (!window.confirm("Xóa ticket này?")) return;
    try {
      await ticketApi.delete(id);
      navigate("/tickets");
    } catch (err) {
      setError(err?.message || "Không thể xóa");
    }
  };

  if (loading)
    return (
      <div className="text-sm text-ink-400 py-10 text-center">Đang tải...</div>
    );

  if (error && !ticket) {
    return (
      <div className="space-y-4">
        <PageHeader
          breadcrumb="CRM / Hỗ trợ"
          title="Ticket"
          onBack={() => navigate("/tickets")}
        />
        <div className="text-sm text-danger-600 bg-danger-50 rounded-lg p-4">
          {error}
        </div>
      </div>
    );
  }
  if (!ticket) return null;

  return (
    <div className="space-y-5">
      <PageHeader
        breadcrumb="Ticket hỗ trợ"
        title={ticket.maTicket}
        onBack={() => navigate("/tickets")}
        badge={
          <div className="flex gap-1.5">
            <Badge
              label={TICKET_STATUS[ticket.trangThai] ?? ticket.trangThai}
              colorClass={TICKET_STATUS_COLOR[ticket.trangThai]}
            />
            <Badge
              label={TICKET_PRIORITY[ticket.mucDoUuTien] ?? ticket.mucDoUuTien}
              colorClass={TICKET_PRIORITY_COLOR[ticket.mucDoUuTien]}
            />
          </div>
        }
        actions={
          <>
            {!isClosed && (
              <Button variant="secondary" icon={Lock} onClick={handleClose}>
                Đóng ticket
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
          <Card title="Thông tin ticket">
            <div className="grid grid-cols-2 gap-5 mb-4">
              <Field label="Tiêu đề" value={ticket.tieuDe} />
              <Field label="Nguồn tiếp nhận" value={ticket.nguonTiepNhan} />
              {ticket.ngayDong && (
                <Field
                  label="Ngày đóng"
                  value={formatDateTime(ticket.ngayDong)}
                />
              )}
              {ticket.lyDoDong && (
                <Field label="Lý do đóng" value={ticket.lyDoDong} />
              )}
            </div>
            {ticket.moTa && (
              <div className="pt-3 border-t border-ink-100">
                <p className="text-xs text-ink-400 mb-1">Mô tả</p>
                <p className="text-sm text-ink-900">{ticket.moTa}</p>
              </div>
            )}
          </Card>

          <Card title={`Lịch sử phản hồi (${ticket.phanHois?.length ?? 0})`}>
            {!ticket.phanHois || ticket.phanHois.length === 0 ? (
              <EmptyState title="Chưa có phản hồi nào" />
            ) : (
              <div className="space-y-2.5 max-h-80 overflow-y-auto">
                {ticket.phanHois.map((ph) => (
                  <div
                    key={ph.id}
                    className="bg-surface-alt rounded-lg p-3 text-sm"
                  >
                    <div className="flex items-center justify-between mb-1 gap-2">
                      <span className="font-medium text-ink-900">
                        {nhanVienMap.get(String(ph.nguoiPhanHoiId)) ??
                          `NV #${ph.nguoiPhanHoiId}`}
                        <span className="text-xs text-ink-400 font-normal ml-1.5">
                          —{" "}
                          {TICKET_PHAN_HOI_LABEL[ph.loaiPhanHoi] ??
                            ph.loaiPhanHoi}
                        </span>
                      </span>
                      <span className="text-xs text-ink-400 shrink-0">
                        {formatDateTime(ph.createdAt)}
                      </span>
                    </div>
                    <p className="text-ink-700">{ph.noiDung}</p>
                    {(ph.trangThaiTruoc || ph.trangThaiSau) && (
                      <p className="text-xs text-ink-400 mt-1">
                        {TICKET_STATUS[ph.trangThaiTruoc] ?? ph.trangThaiTruoc}{" "}
                        → {TICKET_STATUS[ph.trangThaiSau] ?? ph.trangThaiSau}
                      </p>
                    )}
                  </div>
                ))}
              </div>
            )}

            {!isClosed && (
              <form
                onSubmit={handleAddPhanHoi}
                className="border-t border-ink-100 pt-4 mt-4 space-y-3"
              >
                <h4 className="text-sm font-medium text-ink-900">
                  Thêm phản hồi
                </h4>
                <select
                  value={phanHoiForm.loaiPhanHoi}
                  onChange={(e) =>
                    setPhanHoiForm((f) => ({
                      ...f,
                      loaiPhanHoi: e.target.value,
                    }))
                  }
                  className="w-full border border-ink-200 rounded-lg px-3 py-2 text-sm focus:outline-none focus:ring-2 focus:ring-accent-400/40 focus:border-accent-400"
                >
                  {TICKET_PHAN_HOI_LOAI_OPTIONS.map((o) => (
                    <option key={o.value} value={o.value}>
                      {o.label}
                    </option>
                  ))}
                </select>
                <textarea
                  value={phanHoiForm.noiDung}
                  onChange={(e) =>
                    setPhanHoiForm((f) => ({ ...f, noiDung: e.target.value }))
                  }
                  placeholder="Nội dung phản hồi..."
                  rows={3}
                  className="w-full border border-ink-200 rounded-lg px-3 py-2 text-sm resize-none focus:outline-none focus:ring-2 focus:ring-accent-400/40 focus:border-accent-400"
                />
                <Button
                  type="submit"
                  size="sm"
                  icon={Send}
                  disabled={submittingPhanHoi || !phanHoiForm.noiDung.trim()}
                >
                  {submittingPhanHoi ? "Đang gửi..." : "Gửi phản hồi"}
                </Button>
              </form>
            )}
          </Card>
        </div>

        <div className="space-y-4">
          <Card title="Cập nhật xử lý">
            <form onSubmit={handleSaveEdit} className="space-y-3">
              <div>
                <label className="block text-xs font-medium text-ink-500 mb-1">
                  Trạng thái
                </label>
                <select
                  value={editForm.trangThai}
                  onChange={(e) =>
                    setEditForm((f) => ({ ...f, trangThai: e.target.value }))
                  }
                  disabled={isClosed}
                  className="w-full border border-ink-200 rounded-lg px-3 py-2 text-sm disabled:bg-ink-100 disabled:text-ink-400"
                >
                  {TICKET_STATUS_OPTIONS.map((o) => (
                    <option key={o.value} value={o.value}>
                      {o.label}
                    </option>
                  ))}
                </select>
              </div>
              <div>
                <label className="block text-xs font-medium text-ink-500 mb-1">
                  Mức ưu tiên
                </label>
                <select
                  value={editForm.mucDoUuTien}
                  onChange={(e) =>
                    setEditForm((f) => ({ ...f, mucDoUuTien: e.target.value }))
                  }
                  disabled={isClosed}
                  className="w-full border border-ink-200 rounded-lg px-3 py-2 text-sm disabled:bg-ink-100 disabled:text-ink-400"
                >
                  {TICKET_PRIORITY_OPTIONS.map((o) => (
                    <option key={o.value} value={o.value}>
                      {o.label}
                    </option>
                  ))}
                </select>
              </div>
              <div>
                <label className="block text-xs font-medium text-ink-500 mb-1">
                  Ngày hẹn xử lý
                </label>
                <input
                  type="datetime-local"
                  value={editForm.ngayHenXuLy}
                  onChange={(e) =>
                    setEditForm((f) => ({ ...f, ngayHenXuLy: e.target.value }))
                  }
                  disabled={isClosed}
                  className="w-full border border-ink-200 rounded-lg px-3 py-2 text-sm disabled:bg-ink-100 disabled:text-ink-400"
                />
              </div>
              {!isClosed && (
                <Button
                  type="submit"
                  size="sm"
                  className="w-full"
                  disabled={savingEdit}
                >
                  {savingEdit ? "Đang lưu..." : "Lưu thay đổi"}
                </Button>
              )}
              {isClosed && (
                <p className="text-xs text-ink-400">
                  Ticket đã đóng, không thể chỉnh sửa thêm.
                </p>
              )}
            </form>
          </Card>

          {!isClosed && (
            <Card title="Nhân viên xử lý">
              <form onSubmit={handleAssign} className="space-y-3">
                <select
                  value={assignNV}
                  onChange={(e) => setAssignNV(e.target.value)}
                  className="w-full border border-ink-200 rounded-lg px-3 py-2 text-sm focus:outline-none focus:ring-2 focus:ring-accent-400/40 focus:border-accent-400"
                >
                  <option value="">-- Chưa gán --</option>
                  {nhanVienList.map((nv) => (
                    <option key={nv.id} value={nv.id}>
                      {nv.hoTen ?? `NV #${nv.id}`}
                      {nv.role ? ` (${nv.role})` : ""}
                    </option>
                  ))}
                </select>
                <Button
                  type="submit"
                  size="sm"
                  variant="secondary"
                  icon={UserCog}
                  className="w-full"
                  disabled={savingAssign}
                >
                  {savingAssign ? "Đang lưu..." : "Cập nhật gán việc"}
                </Button>
              </form>
            </Card>
          )}
        </div>
      </div>
    </div>
  );
}
