import { useState } from "react";
import productApi from "../../api/productApi";
import Modal from "../../components/common/Modal";
import Button from "../../components/common/Button";

// Modal Thêm/Sửa sản phẩm. Có prop `product` => chế độ Sửa (không cho đổi Mã SP, Tồn ban đầu).
export default function ProductFormModal({ product, types = [], onClose, onSaved }) {
  const isEdit = Boolean(product);
  const [form, setForm] = useState(
    isEdit
      ? {
          loaiSanPhamId: product.loaiSanPhamId ?? "",
          maSP: product.maSP,
          tenSP: product.tenSP,
          donVi: product.donVi ?? "",
          giaBan: product.giaBan ?? "",
          soLuongTonBanDau: "0",
        }
      : { loaiSanPhamId: "", maSP: "", tenSP: "", donVi: "", giaBan: "", soLuongTonBanDau: "0" },
  );
  const [submitting, setSubmitting] = useState(false);
  const [error, setError] = useState("");

  const handleSubmit = async (e) => {
    e.preventDefault();
    if (!form.maSP.trim() || !form.tenSP.trim()) { setError("Mã và tên sản phẩm là bắt buộc"); return; }
    setSubmitting(true); setError("");
    try {
      if (isEdit) {
        await productApi.update(product.id, {
          loaiSanPhamId: form.loaiSanPhamId ? Number(form.loaiSanPhamId) : null,
          tenSP: form.tenSP.trim(),
          donVi: form.donVi.trim() || null,
          giaBan: Number(form.giaBan) || 0,
          dangKinhDoanh: true,
        });
      } else {
        await productApi.create({
          loaiSanPhamId: form.loaiSanPhamId ? Number(form.loaiSanPhamId) : null,
          maSP: form.maSP.trim(),
          tenSP: form.tenSP.trim(),
          donVi: form.donVi.trim() || null,
          giaBan: Number(form.giaBan) || 0,
          soLuongTonBanDau: Number(form.soLuongTonBanDau) || 0,
        });
      }
      onSaved();
    } catch (err) { setError(err?.message || "Không thể lưu sản phẩm"); }
    finally { setSubmitting(false); }
  };

  return (
    <Modal isOpen onClose={onClose} title={isEdit ? "Cập nhật sản phẩm" : "Thêm sản phẩm mới"} size="md">
      <form onSubmit={handleSubmit} className="space-y-4">
        <div>
          <label className="block text-sm font-medium text-ink-700 mb-1.5">
            Mã sản phẩm {!isEdit && <span className="text-danger-500">*</span>}
          </label>
          <input value={form.maSP} disabled={isEdit}
            onChange={(e) => setForm((f) => ({ ...f, maSP: e.target.value }))}
            placeholder="VD: SP0001"
            className="w-full border border-ink-200 rounded-lg px-3 py-2 text-sm disabled:bg-ink-100 disabled:text-ink-400 focus:outline-none focus:ring-2 focus:ring-accent-400/40 focus:border-accent-400" />
        </div>

        <div>
          <label className="block text-sm font-medium text-ink-700 mb-1.5">
            Tên sản phẩm <span className="text-danger-500">*</span>
          </label>
          <input value={form.tenSP} onChange={(e) => setForm((f) => ({ ...f, tenSP: e.target.value }))}
            className="w-full border border-ink-200 rounded-lg px-3 py-2 text-sm focus:outline-none focus:ring-2 focus:ring-accent-400/40 focus:border-accent-400" />
        </div>

        <div className="grid grid-cols-2 gap-3">
          <div>
            <label className="block text-sm font-medium text-ink-700 mb-1.5">Loại</label>
            <select value={form.loaiSanPhamId} onChange={(e) => setForm((f) => ({ ...f, loaiSanPhamId: e.target.value }))}
              className="w-full border border-ink-200 rounded-lg px-3 py-2 text-sm focus:outline-none focus:ring-2 focus:ring-accent-400/40 focus:border-accent-400">
              <option value="">-- Chọn --</option>
              {types.map((t) => <option key={t.id} value={t.id}>{t.tenLoai}</option>)}
            </select>
          </div>
          <div>
            <label className="block text-sm font-medium text-ink-700 mb-1.5">Đơn vị</label>
            <input value={form.donVi} onChange={(e) => setForm((f) => ({ ...f, donVi: e.target.value }))}
              placeholder="cái, gói, tháng..."
              className="w-full border border-ink-200 rounded-lg px-3 py-2 text-sm focus:outline-none focus:ring-2 focus:ring-accent-400/40 focus:border-accent-400" />
          </div>
        </div>

        <div className="grid grid-cols-2 gap-3">
          <div>
            <label className="block text-sm font-medium text-ink-700 mb-1.5">Giá bán (VNĐ)</label>
            <input type="number" min="0" value={form.giaBan} onChange={(e) => setForm((f) => ({ ...f, giaBan: e.target.value }))}
              className="w-full border border-ink-200 rounded-lg px-3 py-2 text-sm focus:outline-none focus:ring-2 focus:ring-accent-400/40 focus:border-accent-400" />
          </div>
          {!isEdit && (
            <div>
              <label className="block text-sm font-medium text-ink-700 mb-1.5">Tồn ban đầu</label>
              <input type="number" min="0" value={form.soLuongTonBanDau}
                onChange={(e) => setForm((f) => ({ ...f, soLuongTonBanDau: e.target.value }))}
                className="w-full border border-ink-200 rounded-lg px-3 py-2 text-sm focus:outline-none focus:ring-2 focus:ring-accent-400/40 focus:border-accent-400" />
            </div>
          )}
        </div>

        {error && <div className="text-sm text-danger-600 bg-danger-50 rounded-lg p-2.5">{error}</div>}

        <div className="flex gap-2 pt-1">
          <Button type="submit" disabled={submitting} className="flex-1">
            {submitting ? "Đang lưu..." : isEdit ? "Cập nhật" : "Thêm mới"}
          </Button>
          <Button type="button" variant="secondary" onClick={onClose}>Hủy</Button>
        </div>
      </form>
    </Modal>
  );
}
