import { useRef } from "react";

/**
 * Input số tiền dạng text: tự thêm dấu chấm phân cách nghìn khi gõ (vd 15.000.000)
 * để dễ đọc và soát số 0 hơn <input type="number">. Bên ngoài vẫn dùng như input
 * số bình thường — value là number (hoặc ""), onChange trả về number (hoặc "").
 *
 * Dùng thay cho:
 *   <input type="number" value={soTien} onChange={(e) => setSoTien(e.target.value)} />
 * bằng:
 *   <MoneyInput value={soTien} onChange={(n) => setSoTien(n)} />
 */
export default function MoneyInput({
  value,
  onChange,
  placeholder = "Số tiền",
  className = "",
  disabled = false,
}) {
  const inputRef = useRef(null);

  const digitsOnly = (s) => (s ?? "").toString().replace(/[^\d]/g, "");
  const format = (digits) =>
    digits === "" ? "" : Number(digits).toLocaleString("vi-VN");

  const displayValue = format(digitsOnly(value));

  const handleChange = (e) => {
    const raw = e.target.value;
    const caret = e.target.selectionStart ?? raw.length;
    // Đếm số ký tự số trước vị trí con trỏ để đặt lại đúng vị trí sau khi
    // format (thêm/bớt dấu chấm làm thay đổi độ dài chuỗi hiển thị).
    const digitsBeforeCaret = raw.slice(0, caret).replace(/[^\d]/g, "").length;

    const digits = digitsOnly(raw);
    const formatted = format(digits);

    onChange(digits === "" ? "" : Number(digits));

    requestAnimationFrame(() => {
      const el = inputRef.current;
      if (!el) return;
      let pos = formatted.length;
      if (digitsBeforeCaret === 0) {
        pos = 0;
      } else {
        let seen = 0;
        for (let i = 0; i < formatted.length; i++) {
          if (/\d/.test(formatted[i])) seen++;
          if (seen === digitsBeforeCaret) {
            pos = i + 1;
            break;
          }
        }
      }
      el.setSelectionRange(pos, pos);
    });
  };

  return (
    <input
      ref={inputRef}
      type="text"
      inputMode="numeric"
      autoComplete="off"
      value={displayValue}
      onChange={handleChange}
      placeholder={placeholder}
      disabled={disabled}
      className={className}
    />
  );
}
