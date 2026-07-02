import { useEffect, useMemo, useState } from "react";
import { useNavigate } from "react-router-dom";
import {
  Target,
  Plus,
  Search,
  Eye,
  Pencil,
  Trash2,
  ArrowRightLeft,
  PlayCircle,
  StopCircle,
} from "lucide-react";
import leadApi from "../../api/leadApi";
import authApi from "../../api/authApi";
import useAuthStore from "../auth/authStore";
import Pagination from "../../components/common/Pagination";
import PageHeader from "../../components/common/PageHeader";
import StatCard from "../../components/common/StatCard";
import EmptyState from "../../components/common/EmptyState";
import RowMenu from "../../components/common/RowMenu";
import Badge from "../../components/common/Badge";
import Button from "../../components/common/Button";
import LeadFormModal from "./LeadFormModal";
import {
  ROLES,
  LEAD_TINH_TRANG_LABEL,
  LEAD_TINH_TRANG_COLOR,
} from "../../utils/constants";
import { formatDateTime } from "../../utils/formatters";

export default function LeadListPage() {
  const { user } = useAuthStore();
  const navigate = useNavigate();
  const canDelete = user?.role === ROLES.Manager;
  const canEdit = [ROLES.Sale, ROLES.Manager].includes(user?.role);

  const [items, setItems] = useState([]);
  const [nhanVienList, setNhanVienList] = useState([]);
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState("");
  const [success, setSuccess] = useState("");

  const [showCreateModal, setShowCreateModal] = useState(false);
  const [editingLead, setEditingLead] = useState(null);

  const [search, setSearch] = useState("");
  const [pageNumber, setPageNumber] = useState(1);
  const [totalPages, setTotalPages] = useState(1);
  const [totalCount, setTotalCount] = useState(0);
  const pageSize = 10;

  const nhanVienMap = useMemo(
    () =>
      new Map(
        nhanVienList.map((nv) => [String(nv.id), nv.hoTen ?? `NV #${nv.id}`]),
      ),
    [nhanVienList],
  );

  const stats = useMemo(
    () => ({
      total: totalCount,
      moi: items.filter((i) => i.tinhTrang === "Moi").length,
      dangChamSoc: items.filter((i) => i.tinhTrang === "DangChamSoc").length,
    }),
    [items, totalCount],
  );

  const loadLeads = async () => {
    setLoading(true);
    setError("");
    try {
      const res = await leadApi.getAll({
        pageNumber,
        pageSize,
        search: search.trim() || undefined,
      });
      setItems(res.data?.items ?? []);
      setTotalPages(res.data?.totalPages ?? 1);
      setTotalCount(res.data?.totalCount ?? 0);
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
    } catch {
      /* không có quyền xem danh sách nhân viên, bỏ qua */
    }
  };

  useEffect(() => {
    loadLeads();
  }, [pageNumber]);
  useEffect(() => {
    loadNhanVien();
  }, []);

  const handleDelete = async (id) => {
    if (!window.confirm("Xóa lead này?")) return;
    try {
      await leadApi.delete(id);
      setSuccess("Xóa lead thành công");
      await loadLeads();
    } catch (err) {
      setError(err?.message || "Không thể xóa lead");
    }
  };

  // Chuyển trạng thái: Mới → Đang chăm sóc
  const handleBatDauChamSoc = async (item) => {
    try {
      await leadApi.update(item.id, { ...item, tinhTrang: "DangChamSoc" });
      setSuccess("Đã chuyển sang Đang chăm sóc");
      await loadLeads();
    } catch (err) {
      setError(err?.message || "Không thể cập nhật trạng thái");
    }
  };

  // Ngừng chăm sóc: DangChamSoc → ThatBai
  const handleNgungChamSoc = async (item) => {
    if (!window.confirm("Ngừng chăm sóc lead này?")) return;
    try {
      await leadApi.update(item.id, { ...item, tinhTrang: "ThatBai" });
      setSuccess("Đã ngừng chăm sóc lead");
      await loadLeads();
    } catch (err) {
      setError(err?.message || "Không thể cập nhật trạng thái");
    }
  };

  // Chuyển đổi thành KH: gọi API trực tiếp, không cần form
  const handleConvert = async (item) => {
    if (!window.confirm(`Chuyển đổi "${item.tenLead}" thành khách hàng?`))
      return;
    try {
      await leadApi.convert(item.id, {
        tenKhachHang: item.tenLead,
        email: item.email || null,
        soDienThoai: item.soDienThoai || null,
        nhanVienPhuTrachId: item.nhanVienPhuTrachId || null,
      });
      setSuccess("Chuyển đổi thành khách hàng thành công!");
      await loadLeads();
    } catch (err) {
      setError(err?.message || "Chuyển đổi thất bại");
    }
  };

  return (
    <div className="space-y-5">
      {showCreateModal && (
        <LeadFormModal
          nhanVienList={nhanVienList}
          onClose={() => setShowCreateModal(false)}
          onSaved={() => {
            setShowCreateModal(false);
            setSuccess("Thêm lead thành công");
            loadLeads();
          }}
        />
      )}
      {editingLead && (
        <LeadFormModal
          lead={editingLead}
          nhanVienList={nhanVienList}
          onClose={() => setEditingLead(null)}
          onSaved={() => {
            setEditingLead(null);
            setSuccess("Cập nhật lead thành công");
            loadLeads();
          }}
        />
      )}

      <PageHeader
        breadcrumb="CRM / Kinh doanh"
        title="Quản lý Lead"
        actions={
          canEdit && (
            <Button icon={Plus} onClick={() => setShowCreateModal(true)}>
              Thêm Lead
            </Button>
          )
        }
      />

      <div className="grid grid-cols-1 sm:grid-cols-3 gap-3">
        <StatCard
          label="Tổng lead (trang này)"
          value={stats.total}
          icon={Target}
        />
        <StatCard
          label="Lead mới"
          value={stats.moi}
          tone="info"
          icon={Target}
        />
        <StatCard
          label="Đang chăm sóc"
          value={stats.dangChamSoc}
          tone="warning"
          icon={Target}
        />
      </div>

      {error && (
        <div className="text-sm text-danger-600 bg-danger-50 rounded-lg p-3">
          {error}
        </div>
      )}
      {success && (
        <div className="text-sm text-success-700 bg-success-50 rounded-lg p-3">
          {success}
        </div>
      )}

      <div className="bg-surface rounded-card border border-ink-100 overflow-hidden">
        <div className="px-5 py-3.5 border-b border-ink-100">
          <div className="relative w-72">
            <Search
              size={15}
              className="absolute left-3 top-1/2 -translate-y-1/2 text-ink-400"
            />
            <input
              type="search"
              placeholder="Tìm theo tên, email, SĐT..."
              value={search}
              onChange={(e) => setSearch(e.target.value)}
              onKeyDown={(e) => {
                if (e.key === "Enter") {
                  setPageNumber(1);
                  loadLeads();
                }
              }}
              className="border border-ink-200 rounded-lg pl-9 pr-3 py-2 text-sm w-full focus:outline-none focus:ring-2 focus:ring-accent-400/40 focus:border-accent-400"
            />
          </div>
        </div>

        <div className="overflow-x-auto">
          <table className="w-full text-sm">
            <thead>
              <tr className="bg-surface-alt">
                <th className="px-5 py-3 text-left text-xs font-medium text-ink-400 uppercase tracking-wide">
                  Tên Lead
                </th>
                <th className="px-5 py-3 text-left text-xs font-medium text-ink-400 uppercase tracking-wide">
                  Liên hệ
                </th>
                <th className="px-5 py-3 text-left text-xs font-medium text-ink-400 uppercase tracking-wide">
                  Công ty
                </th>
                <th className="px-5 py-3 text-left text-xs font-medium text-ink-400 uppercase tracking-wide">
                  Tình trạng
                </th>
                <th className="px-5 py-3 text-left text-xs font-medium text-ink-400 uppercase tracking-wide">
                  NV phụ trách
                </th>
                <th className="px-5 py-3 text-left text-xs font-medium text-ink-400 uppercase tracking-wide">
                  Ngày tạo
                </th>
                <th className="w-12"></th>
              </tr>
            </thead>
            <tbody className="divide-y divide-ink-100">
              {items.length === 0 ? (
                <tr>
                  <td colSpan={7}>
                    <EmptyState
                      icon={Target}
                      title={loading ? "Đang tải..." : "Chưa có lead nào"}
                      description={
                        !loading
                          ? "Thêm lead mới để bắt đầu chăm sóc khách hàng tiềm năng."
                          : undefined
                      }
                    />
                  </td>
                </tr>
              ) : (
                items.map((item) => (
                  <tr
                    key={item.id}
                    className="hover:bg-surface-alt cursor-pointer transition-colors"
                    onClick={() => navigate(`/leads/${item.id}`)}
                  >
                    <td className="px-5 py-3.5 font-medium text-ink-900">
                      {item.tenLead}
                    </td>
                    <td className="px-5 py-3.5 text-ink-700">
                      <div>{item.email || "—"}</div>
                      {item.soDienThoai && (
                        <div className="text-xs text-ink-400">
                          {item.soDienThoai}
                        </div>
                      )}
                    </td>
                    <td className="px-5 py-3.5 text-ink-700">
                      {item.tenCongTy || "—"}
                    </td>
                    <td className="px-5 py-3.5">
                      {item.tinhTrang ? (
                        <Badge
                          label={
                            LEAD_TINH_TRANG_LABEL[item.tinhTrang] ??
                            item.tinhTrang
                          }
                          colorClass={LEAD_TINH_TRANG_COLOR[item.tinhTrang]}
                        />
                      ) : (
                        "—"
                      )}
                    </td>
                    <td className="px-5 py-3.5 text-ink-700">
                      {item.nhanVienPhuTrachId
                        ? (nhanVienMap.get(String(item.nhanVienPhuTrachId)) ??
                          `NV #${item.nhanVienPhuTrachId}`)
                        : "—"}
                    </td>
                    <td className="px-5 py-3.5 text-xs text-ink-400">
                      {formatDateTime(item.createdAt)}
                    </td>
                    <td
                      className="px-3 py-3.5 text-center"
                      onClick={(e) => e.stopPropagation()}
                    >
                      <RowMenu
                        items={[
                          {
                            label: "Xem chi tiết",
                            icon: Eye,
                            onClick: () => navigate(`/leads/${item.id}`),
                          },
                          // Chỉ được sửa khi chưa chuyển đổi
                          ...(canEdit && item.tinhTrang !== "DaChuyenDoi"
                            ? [
                                {
                                  label: "Sửa",
                                  icon: Pencil,
                                  onClick: () => setEditingLead(item),
                                },
                              ]
                            : []),
                          // Mới → Đang chăm sóc
                          ...(canEdit && item.tinhTrang === "Moi"
                            ? [
                                {
                                  label: "Bắt đầu chăm sóc",
                                  icon: PlayCircle,
                                  onClick: () => handleBatDauChamSoc(item),
                                },
                              ]
                            : []),
                          // Đang chăm sóc → Ngừng / Chuyển KH
                          ...(canEdit && item.tinhTrang === "DangChamSoc"
                            ? [
                                {
                                  label: "Ngừng chăm sóc",
                                  icon: StopCircle,
                                  onClick: () => handleNgungChamSoc(item),
                                },
                                {
                                  label: "Chuyển thành khách hàng",
                                  icon: ArrowRightLeft,
                                  onClick: () => handleConvert(item),
                                },
                              ]
                            : []),
                          ...(canDelete
                            ? [
                                {
                                  label: "Xóa",
                                  icon: Trash2,
                                  danger: true,
                                  onClick: () => handleDelete(item.id),
                                },
                              ]
                            : []),
                        ]}
                      />
                    </td>
                  </tr>
                ))
              )}
            </tbody>
          </table>
        </div>

        <div className="px-5 py-3.5 border-t border-ink-100 flex items-center justify-between">
          <p className="text-xs text-ink-400">{totalCount} lead</p>
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
