import { useEffect, useState, useMemo } from "react";
import ticketApi from "../../api/ticketApi";
import customerApi from "../../api/customerApi";
import authApi from "../../api/authApi";
import useAuthStore from "../auth/authStore";
import Pagination from "../../components/common/Pagination";
import {
  ROLES,
  TICKET_STATUS,
  TICKET_STATUS_OPTIONS,
  TICKET_STATUS_COLOR,
  TICKET_PRIORITY,
  TICKET_PRIORITY_OPTIONS,
  TICKET_PRIORITY_COLOR,
  TICKET_SOURCE_OPTIONS,
  TICKET_PHAN_HOI_LOAI_OPTIONS,
} from "../../utils/constants";
import { formatDateTime } from "../../utils/formatters";

function Badge({ label, colorClass }) {
  return (
    <span
      className={`px-2 py-0.5 rounded-full text-xs font-semibold ${colorClass}`}
    >
      {label}
    </span>
  );
}

// ── Modal chi tiết: xem + SỬA trạng thái/ưu tiên + phản hồi ────────────────
function TicketDetailModal({ ticketId, onClose, nhanVienMap }) {
  const [ticket, setTicket] = useState(null);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState("");

  // Form chỉnh sửa nhanh (trạng thái + ưu tiên + ngày hẹn)
  const [editForm, setEditForm] = useState(null);
  const [savingEdit, setSavingEdit] = useState(false);

  // Form thêm phản hồi
  const [phanHoiForm, setPhanHoiForm] = useState({
    loaiPhanHoi: "NoiBoXuLy",
    noiDung: "",
  });
  const [submittingPhanHoi, setSubmittingPhanHoi] = useState(false);

  const loadTicket = async () => {
    setLoading(true);
    setError("");
    try {
      const res = await ticketApi.getById(ticketId);
      setTicket(res.data);
      setEditForm({
        trangThai: res.data.trangThai,
        mucDoUuTien: res.data.mucDoUuTien,
        ngayHenXuLy: res.data.ngayHenXuLy
          ? res.data.ngayHenXuLy.slice(0, 16)
          : "",
      });
    } catch {
      setError("Không thể tải ticket");
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => {
    loadTicket();
  }, [ticketId]);

  // ── Lưu thay đổi trạng thái / ưu tiên / ngày hẹn ──────────────────────────
  const handleSaveEdit = async (e) => {
    e.preventDefault();
    setSavingEdit(true);
    setError("");
    try {
      await ticketApi.update(ticketId, {
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
      await loadTicket();
    } catch (err) {
      setError(err?.message || "Cập nhật ticket thất bại");
    } finally {
      setSavingEdit(false);
    }
  };

  const handleAddPhanHoi = async (e) => {
    e.preventDefault();
    if (!phanHoiForm.noiDung.trim()) return;
    setSubmittingPhanHoi(true);
    setError("");
    try {
      await ticketApi.addPhanHoi(ticketId, phanHoiForm);
      setPhanHoiForm({ loaiPhanHoi: "NoiBoXuLy", noiDung: "" });
      await loadTicket(); // phản hồi có thể tự đổi trạng thái -> reload toàn bộ
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
      await ticketApi.close(ticketId, { lyDoDong: ly });
      await loadTicket();
    } catch (err) {
      setError(err?.message || "Không thể đóng ticket");
    }
  };

  const isClosed = ticket?.trangThai === "Dong";

  return (
    <div className="fixed inset-0 z-50 flex items-center justify-center bg-black/40 p-4">
      <div className="bg-white rounded-xl shadow-xl w-full max-w-2xl max-h-[90vh] overflow-y-auto">
        <div className="flex items-center justify-between px-6 py-4 border-b sticky top-0 bg-white z-10">
          <h3 className="font-bold text-lg text-gray-800">Chi tiết Ticket</h3>
          <button
            onClick={onClose}
            className="text-gray-400 hover:text-gray-700 text-xl font-bold"
          >
            ×
          </button>
        </div>

        {loading ? (
          <div className="p-8 text-center text-gray-400">Đang tải...</div>
        ) : ticket ? (
          <div className="p-6 space-y-5">
            {error && (
              <div className="text-sm text-red-600 bg-red-50 rounded-lg p-3">
                {error}
              </div>
            )}

            {/* Ticket info (read-only) */}
            <div className="grid grid-cols-2 gap-4 text-sm">
              <div>
                <span className="text-gray-500">Mã ticket:</span>{" "}
                <span className="font-mono font-semibold text-blue-600">
                  {ticket.maTicket}
                </span>
              </div>
              <div>
                <span className="text-gray-500">Nguồn:</span>{" "}
                {ticket.nguonTiepNhan}
              </div>
              <div className="col-span-2">
                <span className="text-gray-500">Tiêu đề:</span>{" "}
                <span className="font-medium">{ticket.tieuDe}</span>
              </div>
              {ticket.moTa && (
                <div className="col-span-2">
                  <span className="text-gray-500">Mô tả:</span>
                  <p className="mt-1 text-gray-700">{ticket.moTa}</p>
                </div>
              )}
              <div>
                <span className="text-gray-500">Nhân viên xử lý:</span>{" "}
                {ticket.nhanVienXuLyId
                  ? (nhanVienMap.get(String(ticket.nhanVienXuLyId)) ??
                    `NV #${ticket.nhanVienXuLyId}`)
                  : "Chưa gán"}
              </div>
              {ticket.ngayDong && (
                <div>
                  <span className="text-gray-500">Ngày đóng:</span>{" "}
                  {formatDateTime(ticket.ngayDong)}
                </div>
              )}
              {ticket.lyDoDong && (
                <div className="col-span-2">
                  <span className="text-gray-500">Lý do đóng:</span>{" "}
                  {ticket.lyDoDong}
                </div>
              )}
            </div>

            {/* ✅ Form chỉnh sửa trạng thái + ưu tiên + ngày hẹn */}
            <form onSubmit={handleSaveEdit} className="border-t pt-4 space-y-3">
              <h4 className="font-semibold text-gray-700">
                Cập nhật trạng thái xử lý
              </h4>
              <div className="grid grid-cols-2 gap-3">
                <div>
                  <label className="block text-xs font-medium text-gray-600 mb-1">
                    Trạng thái
                  </label>
                  <select
                    value={editForm.trangThai}
                    onChange={(e) =>
                      setEditForm((f) => ({ ...f, trangThai: e.target.value }))
                    }
                    disabled={isClosed}
                    className="w-full border rounded-lg px-3 py-2 text-sm disabled:bg-gray-100"
                  >
                    {TICKET_STATUS_OPTIONS.map((o) => (
                      <option key={o.value} value={o.value}>
                        {o.label}
                      </option>
                    ))}
                  </select>
                </div>
                <div>
                  <label className="block text-xs font-medium text-gray-600 mb-1">
                    Mức ưu tiên
                  </label>
                  <select
                    value={editForm.mucDoUuTien}
                    onChange={(e) =>
                      setEditForm((f) => ({
                        ...f,
                        mucDoUuTien: e.target.value,
                      }))
                    }
                    disabled={isClosed}
                    className="w-full border rounded-lg px-3 py-2 text-sm disabled:bg-gray-100"
                  >
                    {TICKET_PRIORITY_OPTIONS.map((o) => (
                      <option key={o.value} value={o.value}>
                        {o.label}
                      </option>
                    ))}
                  </select>
                </div>
                <div className="col-span-2">
                  <label className="block text-xs font-medium text-gray-600 mb-1">
                    Ngày hẹn xử lý
                  </label>
                  <input
                    type="datetime-local"
                    value={editForm.ngayHenXuLy}
                    onChange={(e) =>
                      setEditForm((f) => ({
                        ...f,
                        ngayHenXuLy: e.target.value,
                      }))
                    }
                    disabled={isClosed}
                    className="w-full border rounded-lg px-3 py-2 text-sm disabled:bg-gray-100"
                  />
                </div>
              </div>

              <div className="flex gap-2 items-center">
                <Badge
                  label={TICKET_STATUS[ticket.trangThai] ?? ticket.trangThai}
                  colorClass={
                    TICKET_STATUS_COLOR[ticket.trangThai] ??
                    "bg-gray-100 text-gray-600"
                  }
                />
                <Badge
                  label={
                    TICKET_PRIORITY[ticket.mucDoUuTien] ?? ticket.mucDoUuTien
                  }
                  colorClass={
                    TICKET_PRIORITY_COLOR[ticket.mucDoUuTien] ??
                    "bg-gray-100 text-gray-600"
                  }
                />
                <span className="text-xs text-gray-400">(hiện tại đã lưu)</span>
              </div>

              {!isClosed && (
                <div className="flex gap-2">
                  <button
                    type="submit"
                    disabled={savingEdit}
                    className="bg-blue-600 text-white px-4 py-2 rounded-lg text-sm font-medium hover:bg-blue-700 disabled:opacity-50"
                  >
                    {savingEdit ? "Đang lưu..." : "Lưu thay đổi"}
                  </button>
                  <button
                    type="button"
                    onClick={handleClose}
                    className="px-4 py-2 text-sm bg-gray-700 text-white rounded-lg hover:bg-gray-800"
                  >
                    Đóng ticket
                  </button>
                </div>
              )}
              {isClosed && (
                <p className="text-xs text-gray-400">
                  Ticket đã đóng, không thể chỉnh sửa thêm.
                </p>
              )}
            </form>

            {/* Phản hồi timeline */}
            <div className="border-t pt-4">
              <h4 className="font-semibold text-gray-700 mb-3">
                Lịch sử phản hồi ({ticket.phanHois?.length ?? 0})
              </h4>
              <div className="space-y-3 max-h-48 overflow-y-auto pr-1">
                {(ticket.phanHois ?? []).map((ph) => (
                  <div
                    key={ph.id}
                    className="bg-gray-50 rounded-lg p-3 text-sm"
                  >
                    <div className="flex items-center justify-between mb-1">
                      <span className="font-medium text-gray-700">
                        {nhanVienMap.get(String(ph.nguoiPhanHoiId)) ??
                          `NV #${ph.nguoiPhanHoiId}`}
                        {" — "}
                        <span className="text-xs text-gray-500">
                          {TICKET_PHAN_HOI_LOAI_OPTIONS.find(
                            (o) => o.value === ph.loaiPhanHoi,
                          )?.label ?? ph.loaiPhanHoi}
                        </span>
                      </span>
                      <span className="text-xs text-gray-400">
                        {formatDateTime(ph.createdAt)}
                      </span>
                    </div>
                    <p className="text-gray-700">{ph.noiDung}</p>
                    {(ph.trangThaiTruoc || ph.trangThaiSau) && (
                      <p className="text-xs text-gray-400 mt-1">
                        {TICKET_STATUS[ph.trangThaiTruoc] ?? ph.trangThaiTruoc}{" "}
                        → {TICKET_STATUS[ph.trangThaiSau] ?? ph.trangThaiSau}
                      </p>
                    )}
                  </div>
                ))}
                {(!ticket.phanHois || ticket.phanHois.length === 0) && (
                  <p className="text-gray-400 text-sm">Chưa có phản hồi nào</p>
                )}
              </div>
            </div>

            {/* Add phản hồi */}
            {!isClosed && (
              <form
                onSubmit={handleAddPhanHoi}
                className="border-t pt-4 space-y-3"
              >
                <h4 className="font-semibold text-gray-700">Thêm phản hồi</h4>
                <select
                  value={phanHoiForm.loaiPhanHoi}
                  onChange={(e) =>
                    setPhanHoiForm((f) => ({
                      ...f,
                      loaiPhanHoi: e.target.value,
                    }))
                  }
                  className="w-full border rounded-lg px-3 py-2 text-sm"
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
                  className="w-full border rounded-lg px-3 py-2 text-sm resize-none"
                />
                <button
                  type="submit"
                  disabled={submittingPhanHoi || !phanHoiForm.noiDung.trim()}
                  className="bg-blue-600 text-white px-4 py-2 rounded-lg text-sm font-medium hover:bg-blue-700 disabled:opacity-50"
                >
                  {submittingPhanHoi ? "Đang gửi..." : "Gửi phản hồi"}
                </button>
              </form>
            )}
          </div>
        ) : (
          <div className="p-8 text-center text-red-400">
            Không tìm thấy ticket
          </div>
        )}
      </div>
    </div>
  );
}

export default function TicketListPage() {
  const { user } = useAuthStore();
  // ✅ Khớp với backend: Delete chỉ Policies.ManagerOnly
  const canDelete = user?.role === ROLES.Manager;

  const [items, setItems] = useState([]);
  const [nhanVienList, setNhanVienList] = useState([]);
  const [customers, setCustomers] = useState([]);
  const [selectedTicketId, setSelectedTicketId] = useState(null);
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState("");
  const [success, setSuccess] = useState("");

  const [search, setSearch] = useState("");
  const [filterStatus, setFilterStatus] = useState("");
  const [filterPriority, setFilterPriority] = useState("");
  const [pageNumber, setPageNumber] = useState(1);
  const [totalPages, setTotalPages] = useState(1);
  const [totalCount, setTotalCount] = useState(0);
  const pageSize = 10;

  const emptyForm = {
    tieuDe: "",
    moTa: "",
    khachHangId: "",
    loaiTicketId: "",
    mucDoUuTien: "TrungBinh",
    nguonTiepNhan: "Phone",
    nhanVienXuLyId: "",
    ngayHenXuLy: "",
  };
  const [form, setForm] = useState(emptyForm);
  const [showForm, setShowForm] = useState(false);
  const [submitting, setSubmitting] = useState(false);
  const [formError, setFormError] = useState("");

  const nhanVienMap = useMemo(
    () =>
      new Map(
        nhanVienList.map((nv) => [String(nv.id), nv.hoTen ?? `NV #${nv.id}`]),
      ),
    [nhanVienList],
  );

  const loadTickets = async () => {
    setLoading(true);
    setError("");
    try {
      const res = await ticketApi.getAll({
        pageNumber,
        pageSize,
        search: search.trim() || undefined,
        trangThai: filterStatus || undefined,
        mucDoUuTien: filterPriority || undefined,
      });
      const paged = res.data;
      setItems(paged?.items ?? []);
      setTotalPages(paged?.totalPages ?? 1);
      setTotalCount(paged?.totalCount ?? 0);
    } catch (err) {
      setError(err?.message || "Tải danh sách thất bại");
    } finally {
      setLoading(false);
    }
  };

  const loadNhanVien = async () => {
    try {
      const res = await authApi.getStaffList();
      setNhanVienList(res.data ?? []);
    } catch {}
  };

  const loadCustomers = async () => {
    try {
      const res = await customerApi.getAll({ pageSize: 100 });
      setCustomers(res.data?.items ?? []);
    } catch {}
  };

  useEffect(() => {
    loadTickets();
  }, [pageNumber, filterStatus, filterPriority]);
  useEffect(() => {
    loadNhanVien();
    loadCustomers();
  }, []);

  const handleSubmitCreate = async (e) => {
    e.preventDefault();
    if (!form.tieuDe.trim() || !form.khachHangId) {
      setFormError("Tiêu đề và khách hàng là bắt buộc");
      return;
    }
    setSubmitting(true);
    setFormError("");
    try {
      await ticketApi.create({
        tieuDe: form.tieuDe.trim(),
        moTa: form.moTa.trim() || null,
        khachHangId: Number(form.khachHangId),
        loaiTicketId: form.loaiTicketId ? Number(form.loaiTicketId) : null,
        mucDoUuTien: form.mucDoUuTien,
        nguonTiepNhan: form.nguonTiepNhan,
        nhanVienXuLyId: form.nhanVienXuLyId
          ? Number(form.nhanVienXuLyId)
          : null,
        ngayHenXuLy: form.ngayHenXuLy || null,
      });
      setSuccess("Tạo ticket thành công");
      setForm(emptyForm);
      setShowForm(false);
      await loadTickets();
    } catch (err) {
      setFormError(err?.message || "Không thể tạo ticket");
    } finally {
      setSubmitting(false);
    }
  };

  const handleDelete = async (id) => {
    if (!window.confirm("Xóa ticket này?")) return;
    try {
      await ticketApi.delete(id);
      setSuccess("Xóa ticket thành công");
      await loadTickets();
    } catch (err) {
      setError(err?.message || "Không thể xóa");
    }
  };

  return (
    <div className="space-y-6">
      {selectedTicketId && (
        <TicketDetailModal
          ticketId={selectedTicketId}
          nhanVienMap={nhanVienMap}
          onClose={() => {
            setSelectedTicketId(null);
            loadTickets();
          }}
        />
      )}

      <div className="flex flex-col sm:flex-row sm:items-center sm:justify-between gap-4">
        <div>
          <p className="text-xs text-gray-500 uppercase tracking-wide">
            CRM / Hỗ trợ
          </p>
          <h1 className="text-2xl font-bold text-gray-800">
            Quản lý Ticket hỗ trợ
          </h1>
        </div>
        <div className="flex flex-wrap gap-2">
          <input
            type="search"
            placeholder="Tìm theo mã, tiêu đề..."
            value={search}
            onChange={(e) => setSearch(e.target.value)}
            onKeyDown={(e) => {
              if (e.key === "Enter") {
                setPageNumber(1);
                loadTickets();
              }
            }}
            className="border rounded-lg px-3 py-2 text-sm w-56 focus:outline-none focus:ring-2 focus:ring-blue-400"
          />
          <select
            value={filterStatus}
            onChange={(e) => {
              setFilterStatus(e.target.value);
              setPageNumber(1);
            }}
            className="border rounded-lg px-3 py-2 text-sm focus:outline-none focus:ring-2 focus:ring-blue-400"
          >
            <option value="">Tất cả trạng thái</option>
            {TICKET_STATUS_OPTIONS.map((o) => (
              <option key={o.value} value={o.value}>
                {o.label}
              </option>
            ))}
          </select>
          <select
            value={filterPriority}
            onChange={(e) => {
              setFilterPriority(e.target.value);
              setPageNumber(1);
            }}
            className="border rounded-lg px-3 py-2 text-sm focus:outline-none focus:ring-2 focus:ring-blue-400"
          >
            <option value="">Tất cả mức ưu tiên</option>
            {TICKET_PRIORITY_OPTIONS.map((o) => (
              <option key={o.value} value={o.value}>
                {o.label}
              </option>
            ))}
          </select>
          <button
            onClick={() => setShowForm(!showForm)}
            className="bg-blue-600 text-white px-4 py-2 rounded-lg text-sm font-medium hover:bg-blue-700"
          >
            {showForm ? "Ẩn form" : "+ Tạo ticket"}
          </button>
        </div>
      </div>

      {showForm && (
        <form
          onSubmit={handleSubmitCreate}
          className="bg-white rounded-xl border shadow-sm p-6 space-y-4"
        >
          <h2 className="font-semibold text-gray-800">Tạo ticket mới</h2>
          <div className="grid grid-cols-1 sm:grid-cols-2 gap-4">
            <div className="sm:col-span-2">
              <label className="block text-sm font-medium mb-1">
                Tiêu đề <span className="text-red-500">*</span>
              </label>
              <input
                value={form.tieuDe}
                onChange={(e) =>
                  setForm((f) => ({ ...f, tieuDe: e.target.value }))
                }
                placeholder="Mô tả ngắn vấn đề..."
                className="w-full border rounded-lg px-3 py-2 text-sm"
              />
            </div>
            <div>
              <label className="block text-sm font-medium mb-1">
                Khách hàng <span className="text-red-500">*</span>
              </label>
              <select
                value={form.khachHangId}
                onChange={(e) =>
                  setForm((f) => ({ ...f, khachHangId: e.target.value }))
                }
                className="w-full border rounded-lg px-3 py-2 text-sm"
              >
                <option value="">-- Chọn khách hàng --</option>
                {customers.map((c) => (
                  <option key={c.id} value={c.id}>
                    {c.tenKhachHang} ({c.maKhachHang})
                  </option>
                ))}
              </select>
            </div>
            <div>
              <label className="block text-sm font-medium mb-1">
                Nhân viên xử lý
              </label>
              <select
                value={form.nhanVienXuLyId}
                onChange={(e) =>
                  setForm((f) => ({ ...f, nhanVienXuLyId: e.target.value }))
                }
                className="w-full border rounded-lg px-3 py-2 text-sm"
              >
                <option value="">-- Chưa gán --</option>
                {nhanVienList.map((nv) => (
                  <option key={nv.id} value={nv.id}>
                    {nv.hoTen ?? `NV #${nv.id}`}
                  </option>
                ))}
              </select>
            </div>
            <div>
              <label className="block text-sm font-medium mb-1">
                Mức ưu tiên
              </label>
              <select
                value={form.mucDoUuTien}
                onChange={(e) =>
                  setForm((f) => ({ ...f, mucDoUuTien: e.target.value }))
                }
                className="w-full border rounded-lg px-3 py-2 text-sm"
              >
                {TICKET_PRIORITY_OPTIONS.map((o) => (
                  <option key={o.value} value={o.value}>
                    {o.label}
                  </option>
                ))}
              </select>
            </div>
            <div>
              <label className="block text-sm font-medium mb-1">
                Nguồn tiếp nhận
              </label>
              <select
                value={form.nguonTiepNhan}
                onChange={(e) =>
                  setForm((f) => ({ ...f, nguonTiepNhan: e.target.value }))
                }
                className="w-full border rounded-lg px-3 py-2 text-sm"
              >
                {TICKET_SOURCE_OPTIONS.map((o) => (
                  <option key={o.value} value={o.value}>
                    {o.label}
                  </option>
                ))}
              </select>
            </div>
            <div>
              <label className="block text-sm font-medium mb-1">
                Ngày hẹn xử lý
              </label>
              <input
                type="datetime-local"
                value={form.ngayHenXuLy}
                onChange={(e) =>
                  setForm((f) => ({ ...f, ngayHenXuLy: e.target.value }))
                }
                className="w-full border rounded-lg px-3 py-2 text-sm"
              />
            </div>
            <div className="sm:col-span-2">
              <label className="block text-sm font-medium mb-1">
                Mô tả chi tiết
              </label>
              <textarea
                value={form.moTa}
                onChange={(e) =>
                  setForm((f) => ({ ...f, moTa: e.target.value }))
                }
                rows={3}
                placeholder="Mô tả chi tiết vấn đề..."
                className="w-full border rounded-lg px-3 py-2 text-sm resize-none"
              />
            </div>
          </div>
          {formError && (
            <div className="text-sm text-red-600 bg-red-50 rounded p-2">
              {formError}
            </div>
          )}
          <div className="flex gap-2">
            <button
              type="submit"
              disabled={submitting}
              className="bg-blue-600 text-white px-6 py-2 rounded-lg text-sm font-medium hover:bg-blue-700 disabled:opacity-50"
            >
              {submitting ? "Đang tạo..." : "Tạo ticket"}
            </button>
            <button
              type="button"
              onClick={() => {
                setForm(emptyForm);
                setShowForm(false);
              }}
              className="border rounded-lg px-4 py-2 text-sm hover:bg-gray-50"
            >
              Hủy
            </button>
          </div>
        </form>
      )}

      {error && (
        <div className="text-sm text-red-600 bg-red-50 rounded-lg p-3">
          {error}
        </div>
      )}
      {success && (
        <div className="text-sm text-green-700 bg-green-50 rounded-lg p-3">
          {success}
        </div>
      )}

      <div className="bg-white rounded-xl border shadow-sm overflow-hidden">
        <div className="px-6 py-4 border-b flex items-center justify-between">
          <div>
            <h2 className="font-semibold text-gray-800">Danh sách Ticket</h2>
            <p className="text-xs text-gray-400">
              Trang {pageNumber} / {totalPages} — {totalCount} ticket
            </p>
          </div>
          {loading && (
            <span className="text-xs text-gray-400 animate-pulse">
              Đang tải...
            </span>
          )}
        </div>
        <div className="overflow-x-auto">
          <table className="w-full text-sm">
            <thead className="bg-gray-50 text-xs text-gray-500 uppercase">
              <tr>
                <th className="px-4 py-3 text-left">Mã Ticket</th>
                <th className="px-4 py-3 text-left">Tiêu đề</th>
                <th className="px-4 py-3 text-left">Trạng thái</th>
                <th className="px-4 py-3 text-left">Ưu tiên</th>
                <th className="px-4 py-3 text-left">NV xử lý</th>
                <th className="px-4 py-3 text-left">Ngày tạo</th>
                <th className="px-4 py-3 text-left">Hành động</th>
              </tr>
            </thead>
            <tbody className="divide-y divide-gray-100">
              {items.length === 0 ? (
                <tr>
                  <td colSpan="7" className="text-center py-10 text-gray-400">
                    {loading ? "Đang tải..." : "Không có dữ liệu"}
                  </td>
                </tr>
              ) : (
                items.map((item) => (
                  <tr key={item.id} className="hover:bg-gray-50">
                    <td className="px-4 py-3">
                      <span className="font-mono text-blue-600 text-xs font-semibold">
                        {item.maTicket}
                      </span>
                    </td>
                    <td className="px-4 py-3 max-w-xs">
                      <div className="font-medium text-gray-900 truncate">
                        {item.tieuDe}
                      </div>
                      <div className="text-xs text-gray-400">
                        {item.nguonTiepNhan}
                      </div>
                    </td>
                    <td className="px-4 py-3">
                      <Badge
                        label={TICKET_STATUS[item.trangThai] ?? item.trangThai}
                        colorClass={
                          TICKET_STATUS_COLOR[item.trangThai] ??
                          "bg-gray-100 text-gray-600"
                        }
                      />
                    </td>
                    <td className="px-4 py-3">
                      <Badge
                        label={
                          TICKET_PRIORITY[item.mucDoUuTien] ?? item.mucDoUuTien
                        }
                        colorClass={
                          TICKET_PRIORITY_COLOR[item.mucDoUuTien] ??
                          "bg-gray-100 text-gray-600"
                        }
                      />
                    </td>
                    <td className="px-4 py-3 text-gray-700">
                      {item.nhanVienXuLyId ? (
                        (nhanVienMap.get(String(item.nhanVienXuLyId)) ??
                        `NV #${item.nhanVienXuLyId}`)
                      ) : (
                        <span className="text-gray-400">Chưa gán</span>
                      )}
                    </td>
                    <td className="px-4 py-3 text-xs text-gray-400">
                      {formatDateTime(item.createdAt)}
                    </td>
                    <td className="px-4 py-3">
                      <div className="flex gap-2">
                        <button
                          onClick={() => setSelectedTicketId(item.id)}
                          className="text-blue-600 hover:underline text-xs font-medium"
                        >
                          Xem / Sửa
                        </button>
                        {canDelete && (
                          <button
                            onClick={() => handleDelete(item.id)}
                            className="text-red-500 hover:underline text-xs font-medium"
                          >
                            Xóa
                          </button>
                        )}
                      </div>
                    </td>
                  </tr>
                ))
              )}
            </tbody>
          </table>
        </div>
        <div className="px-6 py-4 border-t">
          <Pagination
            pageNumber={pageNumber}
            totalPages={totalPages}
            onPageChange={setPageNumber}
          />
        </div>
      </div>
    </div>
  );
}
