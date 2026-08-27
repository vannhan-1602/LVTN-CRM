import { useEffect, useState } from "react";
import {
  KeyRound,
  Plus,
  Lock,
  Unlock,
  RefreshCw,
  ListChecks,
} from "lucide-react";
import contractApi from "../../api/contractApi";
import Card from "../../components/common/Card";
import Badge from "../../components/common/Badge";
import Button from "../../components/common/Button";
import EmptyState from "../../components/common/EmptyState";
import { formatDate, getApiErrorMessage } from "../../utils/formatters";

const LICENSE_STATUS_META = {
  DangHoatDong: { label: "Đang hoạt động", tone: "success" },
  TamKhoa: { label: "Tạm khóa", tone: "warning" },
  HetHan: { label: "Hết hạn", tone: "danger" },
};

const emptyForm = {
  sanPhamId: "",
  soLuongUser: 1,
  phienBan: "",
  moiTruongTrienKhai: "Cloud",
};

/**
 * License phần mềm (HD_License) — nhúng vào ContractDetailPage.
 * Chỉ Manager được cấp/gia hạn/khóa (ManagerOnly ở ContractController); Sale/Accountant chỉ xem.
 *
 * Ràng buộc nghiệp vụ (đã thống nhất, xem CreateLicenseCommand/RenewLicenseCommand ở backend):
 *  - Hợp đồng ChinhThuc: cấp License mới cho chính hợp đồng này (chặn nếu chưa có mốc Bàn giao
 *    đã khách xác nhận — thông báo lỗi từ backend sẽ hiện ra nếu chưa đủ điều kiện).
 *  - Hợp đồng GiaHan: không cấp License mới — hiển thị License của hợp đồng GỐC (hopDongGocId)
 *    để Manager chọn Gia hạn (nối dài NgayHetHan, giữ nguyên MaLicenseKey).
 *  - Hợp đồng BaoTri: không có thao tác License nào — không render section này.
 *
 * Props:
 *  - hopDongId, loaiHopDong, hopDongGocId, trangThai: thông tin hợp đồng đang xem
 *  - isManager: true nếu role hiện tại là Manager
 *  - isFinal: hợp đồng đã Thanh lý/Hết hạn — khóa cấp mới License
 *  - sanPhamTrongHopDong: danh sách dòng sản phẩm của CHÍNH hợp đồng này (lấy từ báo giá gốc,
 *    ContractDetailPage đã load sẵn) — dùng để (1) quyết định có hiện Card này không, và
 *    (2) giới hạn dropdown "Chọn sản phẩm" chỉ còn đúng những sản phẩm dạng License nằm
 *    TRONG hợp đồng, thay vì liệt kê toàn bộ catalog sản phẩm của hệ thống.
 */
export default function LicenseSection({
  hopDongId,
  loaiHopDong,
  hopDongGocId,
  isManager,
  isFinal,
  sanPhamTrongHopDong = [],
}) {
  const isGiaHan = loaiHopDong === "GiaHan";
  // ChinhThuc: xem/cấp license của chính hợp đồng này.
  // GiaHan: xem license của hợp đồng GỐC để chọn gia hạn (license không tự chuyển hợp đồng).
  const licenseSourceHopDongId = isGiaHan ? hopDongGocId : hopDongId;

  // Sản phẩm dạng License nằm trong hợp đồng này (loại bỏ trùng SanPhamId nếu báo giá có
  // nhiều dòng cùng 1 sản phẩm) — khớp đúng ràng buộc HinhThuc="License" ở CreateLicenseCommand.
  const products = Array.from(
    new Map(
      sanPhamTrongHopDong
        .filter((sp) => sp.hinhThuc === "License")
        .map((sp) => [sp.sanPhamId, sp]),
    ).values(),
  );
  const hasLicenseProduct = products.length > 0;

  const [licenses, setLicenses] = useState([]);
  const [loading, setLoading] = useState(true);
  const [showForm, setShowForm] = useState(false);
  const [form, setForm] = useState(emptyForm);
  const [submitting, setSubmitting] = useState(false);
  const [busyId, setBusyId] = useState(null);
  const [error, setError] = useState("");

  const load = async () => {
    if (!licenseSourceHopDongId) {
      setLicenses([]);
      setLoading(false);
      return;
    }
    setLoading(true);
    try {
      const res = await contractApi.getLicenses(licenseSourceHopDongId);
      setLicenses(res.data ?? []);
    } catch {
      setError("Không thể tải danh sách License");
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => {
    load();
    // Ăn theo nhịp làm mới của trang cha (ContractDetailPage poll 5s) — tránh trường hợp
    // License vừa được cấp/gia hạn/khóa ở nơi khác mà người đang xem không thấy cập nhật
    // nếu không tự F5.
    const timer = setInterval(load, 5000);
    return () => clearInterval(timer);
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [licenseSourceHopDongId]);

  const resetForm = () => {
    setForm(emptyForm);
    setShowForm(false);
    setError("");
  };

  const handleCreate = async (e) => {
    e.preventDefault();
    if (!form.sanPhamId) {
      setError("Vui lòng chọn sản phẩm.");
      return;
    }
    setSubmitting(true);
    setError("");
    try {
      await contractApi.createLicense(hopDongId, {
        sanPhamId: Number(form.sanPhamId),
        soLuongUser: Number(form.soLuongUser) || 1,
        phienBan: form.phienBan.trim() || null,
        moiTruongTrienKhai: form.moiTruongTrienKhai,
      });
      await load();
      resetForm();
    } catch (err) {
      setError(getApiErrorMessage(err, "Không thể cấp License"));
    } finally {
      setSubmitting(false);
    }
  };

  const handleRenew = async (licenseId) => {
    setBusyId(licenseId);
    setError("");
    try {
      await contractApi.renewLicense(licenseId, { hopDongGiaHanId: hopDongId });
      await load();
    } catch (err) {
      setError(getApiErrorMessage(err, "Không thể gia hạn License"));
    } finally {
      setBusyId(null);
    }
  };

  const handleToggleLock = async (license) => {
    const khoa = license.trangThai !== "TamKhoa";
    setBusyId(license.id);
    setError("");
    try {
      await contractApi.toggleLicenseLock(license.id, khoa);
      await load();
    } catch (err) {
      setError(
        getApiErrorMessage(err, "Không thể cập nhật trạng thái License"),
      );
    } finally {
      setBusyId(null);
    }
  };

  // Hợp đồng Bảo trì: không có thao tác License nào liên quan.
  if (loaiHopDong === "BaoTri") return null;

  // Chỉ hiện Card này khi: hợp đồng (ChinhThuc) có sản phẩm dạng License, HOẶC đã có License
  // được cấp/thuộc hợp đồng gốc (trường hợp GiaHan). Còn lại (hợp đồng thuần dịch vụ, chưa từng
  // có License) thì ẩn hẳn — tránh hiện khung rỗng gây hiểu nhầm hợp đồng có phần mềm.
  if (!loading && licenses.length === 0 && !(hasLicenseProduct && !isGiaHan))
    return null;

  const canCreate = isManager && !isGiaHan && !isFinal && hasLicenseProduct;

  return (
    <Card
      title={`License (${licenses.length})`}
      action={
        canCreate &&
        !showForm && (
          <Button
            size="sm"
            variant="secondary"
            icon={Plus}
            onClick={() => setShowForm(true)}
          >
            Cấp License
          </Button>
        )
      }
    >
      {isGiaHan && (
        <p className="text-xs text-ink-400 mb-3">
          Danh sách License thuộc hợp đồng gốc — chọn "Gia hạn" để nối dài thời
          hạn theo hợp đồng này.
        </p>
      )}

      {showForm && (
        <form
          onSubmit={handleCreate}
          className="border border-ink-100 rounded-lg p-4 bg-surface-alt space-y-3 mb-4"
        >
          <div>
            <label className="block text-xs font-medium text-ink-500 mb-1">
              Sản phẩm (License)
            </label>
            <select
              value={form.sanPhamId}
              onChange={(e) =>
                setForm((f) => ({ ...f, sanPhamId: e.target.value }))
              }
              className="w-full border border-ink-200 rounded-lg px-3 py-2 text-sm"
            >
              <option value="">-- Chọn sản phẩm --</option>
              {products.map((p) => (
                <option key={p.sanPhamId} value={p.sanPhamId}>
                  {p.tenSP} ({p.maSP})
                </option>
              ))}
            </select>
            {products.length === 0 && (
              <p className="text-xs text-warning-600 mt-1">
                Hợp đồng này không có sản phẩm loại License.
              </p>
            )}
          </div>

          <div className="grid grid-cols-2 gap-3">
            <div>
              <label className="block text-xs font-medium text-ink-500 mb-1">
                Số lượng user
              </label>
              <input
                type="number"
                min="1"
                value={form.soLuongUser}
                onChange={(e) =>
                  setForm((f) => ({ ...f, soLuongUser: e.target.value }))
                }
                className="w-full border border-ink-200 rounded-lg px-3 py-2 text-sm"
              />
            </div>
            <div>
              <label className="block text-xs font-medium text-ink-500 mb-1">
                Môi trường triển khai
              </label>
              <select
                value={form.moiTruongTrienKhai}
                onChange={(e) =>
                  setForm((f) => ({ ...f, moiTruongTrienKhai: e.target.value }))
                }
                className="w-full border border-ink-200 rounded-lg px-3 py-2 text-sm"
              >
                <option value="Cloud">Cloud</option>
                <option value="OnPremise">OnPremise</option>
              </select>
            </div>
          </div>

          <div>
            <label className="block text-xs font-medium text-ink-500 mb-1">
              Phiên bản
            </label>
            <input
              value={form.phienBan}
              onChange={(e) =>
                setForm((f) => ({ ...f, phienBan: e.target.value }))
              }
              placeholder="VD: 2026.1"
              className="w-full border border-ink-200 rounded-lg px-3 py-2 text-sm"
            />
          </div>

          {error && (
            <div className="text-sm text-danger-600 bg-danger-50 rounded-lg p-2">
              {error}
            </div>
          )}

          <div className="flex gap-2">
            <Button type="submit" size="sm" disabled={submitting}>
              {submitting ? "Đang cấp..." : "Cấp License"}
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
      ) : licenses.length === 0 ? (
        <EmptyState
          icon={ListChecks}
          title="Chưa có License nào"
          description={
            isGiaHan
              ? "Hợp đồng gốc chưa có License nào để gia hạn."
              : "Cấp License sau khi hợp đồng đã có mốc Bàn giao được khách xác nhận."
          }
        />
      ) : (
        <div className="space-y-2">
          {licenses.map((lic) => {
            const meta = LICENSE_STATUS_META[lic.trangThai] ?? {
              label: lic.trangThai,
              tone: "neutral",
            };
            const isHetHan = lic.trangThai === "HetHan";
            return (
              <div
                key={lic.id}
                className="border border-ink-100 rounded-lg p-3.5 flex items-start gap-3"
              >
                <div className="w-8 h-8 rounded-full bg-ink-50 flex items-center justify-center text-ink-500 shrink-0 mt-0.5">
                  <KeyRound size={15} />
                </div>
                <div className="flex-1 min-w-0">
                  <div className="flex items-center gap-2 flex-wrap mb-1">
                    <span className="text-sm font-medium text-ink-900">
                      {lic.tenSanPham}
                    </span>
                    <Badge label={meta.label} tone={meta.tone} />
                  </div>
                  <p className="text-xs text-ink-400 font-mono">
                    {lic.maLicenseKey}
                  </p>
                  <div className="flex flex-wrap gap-x-4 mt-1 text-xs text-ink-400">
                    <span>{lic.moiTruongTrienKhai}</span>
                    {lic.phienBan && <span>Phiên bản {lic.phienBan}</span>}
                    <span>{lic.soLuongUser} user</span>
                    {lic.ngayHetHan && (
                      <span>Hết hạn {formatDate(lic.ngayHetHan)}</span>
                    )}
                  </div>
                </div>
                {isManager && (
                  <div className="flex gap-3 shrink-0">
                    {isGiaHan ? (
                      <button
                        onClick={() => handleRenew(lic.id)}
                        disabled={busyId === lic.id}
                        title={
                          lic.trangThai === "TamKhoa"
                            ? "License đang bị khóa — Gia hạn chỉ nối dài ngày hết hạn, KHÔNG tự mở khóa. Vào hợp đồng gốc để mở khóa nếu cần."
                            : undefined
                        }
                        className="text-xs font-medium text-info-600 hover:underline inline-flex items-center gap-1 disabled:opacity-50"
                      >
                        <RefreshCw size={12} /> Gia hạn
                      </button>
                    ) : (
                      !isHetHan && (
                        <button
                          onClick={() => handleToggleLock(lic)}
                          disabled={busyId === lic.id}
                          className="text-xs font-medium text-ink-500 hover:underline inline-flex items-center gap-1 disabled:opacity-50"
                        >
                          {lic.trangThai === "TamKhoa" ? (
                            <>
                              <Unlock size={12} /> Mở khóa
                            </>
                          ) : (
                            <>
                              <Lock size={12} /> Khóa
                            </>
                          )}
                        </button>
                      )
                    )}
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
