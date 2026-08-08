import { useEffect, useMemo, useState } from "react";
import { useParams, useNavigate } from "react-router-dom";
import {
  Printer,
  ArrowLeft,
  AlertTriangle,
  ChevronDown,
  ChevronUp,
  RotateCcw,
  Eye,
  Pencil,
  Send,
  CheckCircle2,
  XCircle,
} from "lucide-react";
import contractApi from "../../api/contractApi";
import customerApi from "../../api/customerApi";
import quoteApi from "../../api/quoteApi";
import Button from "../../components/common/Button";
import { formatDate } from "../../utils/formatters";

function formatMoney(n) {
  return n == null || n === "" ? "" : Number(n).toLocaleString("vi-VN") + " đ";
}

// Tự động co giãn chiều cao ô textarea theo đúng nội dung đang gõ, để không
// bị cắt/ẩn chữ trong khung nhỏ cố định — chỉ dùng cho các ô sửa điều khoản
// hợp đồng bên dưới (đọc kỹ để dò lại nội dung trước khi in).
function autoGrowTextarea(el) {
  if (!el) return;
  el.style.height = "auto";
  el.style.height = el.scrollHeight + "px";
}

// Đọc số tiền ra chữ (đơn vị: đồng) — thuật toán đọc số tiếng Việt theo nhóm 3
// chữ số (nghìn/triệu/tỷ), không phải nối chuỗi đơn giản, để đúng cách trình
// bày trên hợp đồng/hóa đơn thật.
const CHU_SO = [
  "không",
  "một",
  "hai",
  "ba",
  "bốn",
  "năm",
  "sáu",
  "bảy",
  "tám",
  "chín",
];
function docBaSo(so, daySo) {
  let tram = Math.floor(so / 100);
  let chuc = Math.floor((so % 100) / 10);
  let donvi = so % 10;
  let s = "";
  if (tram === 0 && !daySo) {
    // nhóm đầu tiên, không cần đọc "không trăm"
  } else {
    s += CHU_SO[tram] + " trăm ";
  }
  if (chuc === 0 && donvi > 0 && (tram > 0 || daySo)) s += "linh ";
  else if (chuc === 1) s += "mười ";
  else if (chuc > 1) s += CHU_SO[chuc] + " mươi ";
  if (chuc >= 2 && donvi === 1) s += "mốt";
  else if (chuc >= 1 && donvi === 5) s += "lăm";
  else if (donvi > 0) s += CHU_SO[donvi];
  return s.trim();
}
function soTienBangChu(n) {
  if (n == null || n === "" || isNaN(Number(n))) return "";
  let so = Math.round(Number(n));
  if (so === 0) return "Không đồng";
  const donVi = ["", "nghìn", "triệu", "tỷ"];
  const groups = [];
  while (so > 0) {
    groups.unshift(so % 1000);
    so = Math.floor(so / 1000);
  }
  let parts = [];
  groups.forEach((g, idx) => {
    if (g === 0) return;
    const daySo = idx > 0 && groups.slice(0, idx).some((x) => x > 0);
    const chu = docBaSo(g, daySo);
    const donViText = donVi[groups.length - 1 - idx];
    parts.push(`${chu}${donViText ? " " + donViText : ""}`);
  });
  const s = parts.join(" ").replace(/\s+/g, " ").trim();
  return s.charAt(0).toUpperCase() + s.slice(1) + " đồng";
}

const HINH_THUC_LABEL = {
  ThanhToanMotLan: "Thanh toán một lần",
  TraGop: "Thanh toán thành nhiều đợt (trả góp định kỳ)",
};

// Thông tin Bên A (bên cung cấp) — hệ thống chưa có bảng lưu thông tin công ty
// (không tạo bảng mới theo yêu cầu), nên để mặc định ở đây, sửa 1 lần trong code
// cho đúng công ty thật là dùng lại được cho mọi lần in. Có thể sửa tay tiếp trên
// form trước khi in mà không ảnh hưởng dữ liệu trong hệ thống.
const DEFAULT_BEN_A = {
  tenCongTy: "CÔNG TY TNHH GIẢI PHÁP PHẦN MỀM ...",
  diaChi: "",
  maSoThue: "",
  giayDkkd: "",
  dienThoai: "",
  email: "",
  soTaiKhoan: "",
  nganHang: "",
  nguoiDaiDien: "",
  chucVu: "Giám đốc",
  cccd: "",
};

// ─────────────────────────────────────────────────────────────────────────
// Nội dung các điều khoản (Điều 3 → Điều 14) được sinh mặc định từ dữ liệu
// hợp đồng + các tham số người dùng nhập (bảo hành, mức phạt, số bản...).
// Toàn bộ là state trên trình duyệt — KHÔNG lưu vào DB, KHÔNG gọi API nào
// khác ngoài việc tải dữ liệu hợp đồng để hiển thị. Người dùng có thể sửa
// tay từng điều trước khi in mà không ảnh hưởng dữ liệu hệ thống; nếu đã
// sửa tay thì điều đó không tự đồng bộ lại theo baoHanhThang/mucPhatViPham
// nữa (tránh ghi đè mất nội dung khách vừa gõ), bấm "Khôi phục mặc định"
// để đồng bộ lại.
function buildDefaultClauses({ contract, baoHanhThang, mucPhatViPham, soBan }) {
  return {
    dieu3Cham:
      "Trường hợp Bên B chậm thanh toán so với hạn tại mỗi đợt, Bên B phải trả thêm tiền lãi trên số tiền chậm trả tương ứng với thời gian chậm trả theo mức lãi suất áp dụng đối với nợ quá hạn trung bình trên thị trường tại thời điểm thanh toán, theo quy định tại Điều 306 Luật Thương mại 2005, trừ trường hợp hai bên có thỏa thuận khác bằng văn bản.",
    dieu4: `Hợp đồng có hiệu lực kể từ ngày ${
      contract.ngayKy ? formatDate(contract.ngayKy) : "…"
    }${contract.thoiHan ? `, thời hạn ${contract.thoiHan} tháng` : ""}${
      contract.ngayKetThuc
        ? `, đến ngày ${formatDate(contract.ngayKetThuc)}`
        : ""
    }. Tiến độ triển khai, đào tạo, bàn giao và nghiệm thu cụ thể theo Phụ lục/Biên bản mốc triển khai do hai bên xác nhận trong quá trình thực hiện.`,
    dieu5:
      "5.1. Cung cấp giải pháp/dịch vụ phần mềm đúng nội dung, số lượng, chất lượng và tiến độ đã thỏa thuận tại Điều 1, Điều 4.\n5.2. Hướng dẫn, đào tạo và hỗ trợ kỹ thuật cho Bên B trong quá trình triển khai và sử dụng.\n5.3. Có quyền yêu cầu Bên B thanh toán đầy đủ, đúng hạn theo Điều 3; tạm ngừng cung cấp dịch vụ nếu Bên B vi phạm nghĩa vụ thanh toán quá thời hạn đã thỏa thuận, sau khi đã có văn bản thông báo trước ít nhất 07 (bảy) ngày.",
    dieu6:
      "6.1. Thanh toán đầy đủ, đúng hạn theo Điều 3; cung cấp thông tin, phối hợp cần thiết để Bên A triển khai đúng tiến độ.\n6.2. Có quyền yêu cầu Bên A thực hiện đúng nội dung, chất lượng và tiến độ đã cam kết; yêu cầu khắc phục lỗi phát sinh trong thời gian bảo hành theo Điều 7.\n6.3. Không được sao chép, chuyển giao, phân phối lại, dịch ngược (reverse engineering) hoặc cho bên thứ ba thuê lại phần mềm khi chưa có sự đồng ý bằng văn bản của Bên A, trừ trường hợp pháp luật có quy định khác.",
    dieu7: `Thời gian bảo hành: ${
      baoHanhThang || "…"
    } tháng kể từ ngày nghiệm thu/bàn giao. Trong thời gian bảo hành, Bên A có trách nhiệm khắc phục miễn phí các lỗi phát sinh do phần mềm gây ra, không bao gồm lỗi do Bên B tự ý can thiệp, sửa đổi mã nguồn hoặc do hạ tầng/thiết bị/đường truyền của Bên B. Sau thời gian bảo hành, việc bảo trì (nếu có) thực hiện theo thỏa thuận riêng giữa hai bên.`,
    dieu8:
      "Bên A là chủ sở hữu quyền tác giả đối với mã nguồn, thiết kế, cơ sở dữ liệu và các thành phần kỹ thuật của phần mềm nền tảng theo quy định của Luật Sở hữu trí tuệ. Bên B được cấp quyền sử dụng (license) phần mềm theo phạm vi, thời hạn và số lượng người dùng đã thỏa thuận; việc cấp quyền này không phải là chuyển nhượng quyền sở hữu trí tuệ. Đối với các phần tùy chỉnh (customize) riêng theo yêu cầu của Bên B, nếu có, quyền sở hữu và quyền sử dụng thực hiện theo thỏa thuận riêng bằng văn bản giữa hai bên.",
    dieu9:
      "Hai bên cam kết giữ bí mật mọi thông tin, dữ liệu, tài liệu kỹ thuật và nội dung hợp đồng này, không tiết lộ cho bên thứ ba khi chưa có sự đồng ý bằng văn bản của bên còn lại, trừ trường hợp phải cung cấp theo yêu cầu của cơ quan nhà nước có thẩm quyền theo quy định pháp luật. Đối với dữ liệu cá nhân mà Bên A tiếp cận, xử lý thay mặt hoặc theo yêu cầu của Bên B trong quá trình cung cấp, vận hành phần mềm, Bên A cam kết tuân thủ Nghị định số 13/2023/NĐ-CP về bảo vệ dữ liệu cá nhân, chỉ xử lý dữ liệu đúng phạm vi, mục đích đã thỏa thuận và áp dụng các biện pháp bảo mật kỹ thuật phù hợp.",
    dieu10: `Bên vi phạm nghĩa vụ hợp đồng phải chịu phạt vi phạm với mức ${
      mucPhatViPham || 0
    }% giá trị phần nghĩa vụ hợp đồng bị vi phạm (không vượt quá 8% theo Điều 301 Luật Thương mại 2005). Nếu có thiệt hại thực tế xảy ra, bên vi phạm còn phải bồi thường thiệt hại theo quy định tại Điều 302 Luật Thương mại 2005 và Bộ luật Dân sự 2015; tổng trách nhiệm bồi thường của Bên A theo hợp đồng này, trừ trường hợp do lỗi cố ý, không vượt quá tổng giá trị hợp đồng đã thanh toán tại thời điểm phát sinh.`,
    dieu11:
      "Trường hợp việc thực hiện hợp đồng bị ảnh hưởng bởi sự kiện bất khả kháng theo quy định tại khoản 1 Điều 156 Bộ luật Dân sự 2015 (thiên tai, hỏa hoạn, dịch bệnh, chiến tranh, thay đổi chính sách pháp luật...), bên bị ảnh hưởng phải thông báo bằng văn bản cho bên còn lại trong thời hạn hợp lý và được miễn trách nhiệm đối với phần nghĩa vụ không thể thực hiện do sự kiện đó, sau khi đã áp dụng mọi biện pháp cần thiết trong khả năng cho phép để khắc phục hậu quả.",
    dieu12:
      "Mọi tranh chấp phát sinh từ hoặc liên quan đến hợp đồng này trước hết được giải quyết thông qua thương lượng, hòa giải giữa hai bên. Trường hợp không tự giải quyết được trong thời hạn 30 (ba mươi) ngày kể từ ngày phát sinh tranh chấp, một trong hai bên có quyền đưa vụ việc ra giải quyết tại Tòa án nhân dân có thẩm quyền theo quy định pháp luật Việt Nam. Trong thời gian giải quyết tranh chấp, các bên vẫn phải tiếp tục thực hiện các nghĩa vụ không liên quan trực tiếp đến nội dung đang tranh chấp.",
    dieu13:
      "13.1. Mỗi bên có quyền đơn phương chấm dứt hợp đồng nếu bên còn lại vi phạm nghĩa vụ cơ bản của hợp đồng và không khắc phục trong thời hạn 15 (mười lăm) ngày kể từ ngày nhận được thông báo bằng văn bản của bên bị vi phạm.\n13.2. Hai bên có thể thỏa thuận chấm dứt hợp đồng trước thời hạn bằng văn bản; nghĩa vụ thanh toán cho phần công việc, sản phẩm đã thực hiện đến thời điểm chấm dứt vẫn phải được các bên hoàn tất đầy đủ.\n13.3. Việc chấm dứt hợp đồng không làm mất hiệu lực của các điều khoản về bảo mật thông tin (Điều 9), quyền sở hữu trí tuệ (Điều 8) và giải quyết tranh chấp (Điều 12).",
    dieu14: `14.1. Hợp đồng có hiệu lực kể từ ngày ký và chấm dứt khi hai bên đã hoàn thành nghĩa vụ hoặc theo thỏa thuận chấm dứt/thanh lý của hai bên.\n14.2. Mọi sửa đổi, bổ sung hợp đồng phải được lập thành văn bản (phụ lục hợp đồng) và có chữ ký của cả hai bên.\n14.3. Hợp đồng được lập thành ${
      soBan || 2
    } (${soBan || 2}) bản có giá trị pháp lý như nhau, mỗi bên giữ ${Math.floor(
      (soBan || 2) / 2,
    )} bản để thực hiện.`,
  };
}

const CLAUSE_FIELDS = [
  { key: "dieu3Cham", label: "Điều 3 — Lãi chậm thanh toán" },
  { key: "dieu4", label: "Điều 4 — Thời hạn và tiến độ thực hiện" },
  { key: "dieu5", label: "Điều 5 — Quyền và nghĩa vụ Bên A" },
  { key: "dieu6", label: "Điều 6 — Quyền và nghĩa vụ Bên B" },
  { key: "dieu7", label: "Điều 7 — Bảo hành, bảo trì, hỗ trợ kỹ thuật" },
  { key: "dieu8", label: "Điều 8 — Quyền sở hữu trí tuệ" },
  { key: "dieu9", label: "Điều 9 — Bảo mật thông tin & dữ liệu cá nhân" },
  { key: "dieu10", label: "Điều 10 — Phạt vi phạm và bồi thường thiệt hại" },
  { key: "dieu11", label: "Điều 11 — Sự kiện bất khả kháng" },
  { key: "dieu12", label: "Điều 12 — Giải quyết tranh chấp" },
  { key: "dieu13", label: "Điều 13 — Chấm dứt hợp đồng" },
  { key: "dieu14", label: "Điều 14 — Hiệu lực và điều khoản chung" },
];

export default function ContractPrintPage() {
  const { id } = useParams();
  const navigate = useNavigate();

  const [contract, setContract] = useState(null);
  const [customer, setCustomer] = useState(null);
  const [quoteChiTiet, setQuoteChiTiet] = useState([]);
  const [lichThanhToans, setLichThanhToans] = useState([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState("");

  const [benA, setBenA] = useState(DEFAULT_BEN_A);
  const [benB, setBenB] = useState({
    diaChi: "",
    giayDkkd: "",
    nguoiDaiDien: "",
    chucVu: "",
    cccd: "",
  });
  const [diaDiemKy, setDiaDiemKy] = useState("");
  const [vatIncluded, setVatIncluded] = useState(true);
  const [mucPhatViPham, setMucPhatViPham] = useState(8);
  const [baoHanhThang, setBaoHanhThang] = useState(12);
  const [soBan, setSoBan] = useState(2);

  // Nội dung điều khoản tùy chỉnh — key -> text đã sửa tay. Điều nào không
  // có trong object này thì lấy theo mặc định tính tự động (xem clauses ở dưới).
  const [clauseOverrides, setClauseOverrides] = useState({});
  const [showClauseEditor, setShowClauseEditor] = useState(false);

  // false = đang điền/chỉnh thông tin; true = xem trước bản in sạch (đúng như
  // khi in ra) để dò lại nội dung trước khi thực sự bấm In / Xuất PDF.
  const [previewMode, setPreviewMode] = useState(false);

  // Gửi email hợp đồng cho khách — file PDF được sinh Ở SERVER (không phải bản in
  // trên trình duyệt) từ đúng dữ liệu đang hiển thị ở đây, nên khớp 100% với bản
  // đã xem trước. Lịch sử gửi lấy từ email log sẵn có, không cần lưu trạng thái mới.
  const [sendingEmail, setSendingEmail] = useState(false);
  const [sendMsg, setSendMsg] = useState(null);
  const [loiNhan, setLoiNhan] = useState("");
  const [emailHistory, setEmailHistory] = useState([]);

  useEffect(() => {
    (async () => {
      setLoading(true);
      setError("");
      try {
        const res = await contractApi.getById(id);
        const c = res.data;
        setContract(c);

        if (c?.khachHangId) {
          customerApi
            .getById(c.khachHangId)
            .then((r) => setCustomer(r.data))
            .catch(() => {});
        }
        if (c?.baoGiaId) {
          quoteApi
            .getById(c.baoGiaId)
            .then((r) => setQuoteChiTiet(r.data?.chiTiet ?? []))
            .catch(() => {});
        }
        if (c?.hinhThucThanhToan === "TraGop") {
          contractApi
            .getLichThanhToan(id)
            .then((r) => setLichThanhToans(r.data ?? []))
            .catch(() => {});
        }
        contractApi
          .getEmailHistory(id)
          .then((r) => setEmailHistory(r.data ?? []))
          .catch(() => {});
      } catch (err) {
        setError("Không thể tải thông tin hợp đồng để in");
      } finally {
        setLoading(false);
      }
    })();
  }, [id]);

  const defaultClauses = useMemo(
    () =>
      contract
        ? buildDefaultClauses({ contract, baoHanhThang, mucPhatViPham, soBan })
        : {},
    [contract, baoHanhThang, mucPhatViPham, soBan],
  );
  const clauses = { ...defaultClauses, ...clauseOverrides };

  function setClauseText(key, value) {
    setClauseOverrides((s) => ({ ...s, [key]: value }));
  }
  function resetClause(key) {
    setClauseOverrides((s) => {
      const next = { ...s };
      delete next[key];
      return next;
    });
  }

  // Nội dung điều khoản có thể đổi do bấm "Khôi phục mặc định" (không phải do
  // gõ tay), lúc đó sự kiện onInput của textarea không bắn ra — nên cần chạy
  // lại autoGrow ở đây mỗi khi nội dung hoặc trạng thái mở khung sửa đổi.
  useEffect(() => {
    if (!showClauseEditor) return;
    document.querySelectorAll(".clause-textarea").forEach(autoGrowTextarea);
  }, [clauses, showClauseEditor]);

  async function handleSendEmail() {
    const thieu = [];
    if (!benA.tenCongTy?.trim()) thieu.push("Tên công ty Bên A");
    if (!benA.diaChi?.trim()) thieu.push("Địa chỉ Bên A");
    if (!benA.maSoThue?.trim()) thieu.push("Mã số thuế Bên A");
    if (!benA.nguoiDaiDien?.trim()) thieu.push("Người đại diện Bên A");
    if (!benA.chucVu?.trim()) thieu.push("Chức vụ Bên A");
    if (!benB.diaChi?.trim()) thieu.push("Địa chỉ Bên B");
    if (!benB.nguoiDaiDien?.trim()) thieu.push("Người đại diện Bên B");
    if (!benB.chucVu?.trim()) thieu.push("Chức vụ Bên B");
    if (!diaDiemKy?.trim()) thieu.push("Địa điểm ký");
    if (thieu.length > 0) {
      setSendMsg({
        type: "error",
        text: `Chưa điền đủ thông tin bắt buộc để gửi: ${thieu.join(", ")}. Bấm "Quay lại chỉnh sửa" để điền.`,
      });
      return;
    }

    setSendingEmail(true);
    setSendMsg(null);
    try {
      const payload = {
        benA,
        benB,
        diaDiemKy,
        vatIncluded,
        baoHanhThang: Number(baoHanhThang) || 0,
        mucPhatViPham: Number(mucPhatViPham) || 0,
        soBan: Number(soBan) || 2,
        clauseTexts: {
          dieu3Cham: clauses.dieu3Cham,
          dieu4: clauses.dieu4,
          dieu5: clauses.dieu5,
          dieu6: clauses.dieu6,
          dieu7: clauses.dieu7,
          dieu8: clauses.dieu8,
          dieu9: clauses.dieu9,
          dieu10: clauses.dieu10,
          dieu11: clauses.dieu11,
          dieu12: clauses.dieu12,
          dieu13: clauses.dieu13,
          dieu14: clauses.dieu14,
        },
        loiNhan: loiNhan || null,
      };
      const res = await contractApi.sendEmail(id, payload);
      setSendMsg({
        type: "success",
        text: res.message || "Đã gửi hợp đồng cho khách.",
      });
      contractApi
        .getEmailHistory(id)
        .then((r) => setEmailHistory(r.data ?? []))
        .catch(() => {});
    } catch (err) {
      setSendMsg({
        type: "error",
        text: err?.message || "Gửi email thất bại, thử lại sau.",
      });
    } finally {
      setSendingEmail(false);
    }
  }

  if (loading) {
    return (
      <div className="text-sm text-ink-400 py-10 text-center">Đang tải...</div>
    );
  }
  if (error || !contract) {
    return (
      <div className="text-sm text-danger-600 bg-danger-50 rounded-lg p-4 m-6">
        {error || "Không tìm thấy hợp đồng."}
      </div>
    );
  }

  const tongTienSanPham = quoteChiTiet.reduce(
    (sum, l) => sum + (l.thanhTien ?? l.soLuong * l.donGia ?? 0),
    0,
  );
  const giaTriHopDong = contract.giaTri ?? tongTienSanPham;

  return (
    <div className="bg-ink-100 min-h-screen">
      {/* Thanh công cụ + form điền thông tin — ẩn khi in */}
      <div className="no-print sticky top-0 z-10 bg-white border-b border-ink-200 px-6 py-3 flex items-center justify-between">
        <Button
          variant="secondary"
          icon={previewMode ? Pencil : ArrowLeft}
          onClick={() => {
            if (previewMode) {
              setPreviewMode(false);
              return;
            }
            // Trang này thường được mở bằng window.open(url, "_blank") từ trang
            // Chi tiết hợp đồng, tức là mở ra một tab mới không có lịch sử điều
            // hướng trước đó — lúc đó navigate(-1) không có gì để quay lại nên
            // không phản hồi gì cả. Nếu tab hiện tại không có lịch sử, đóng tab
            // luôn (coi như quay lại); nếu có lịch sử thật thì mới điều hướng lùi.
            if (window.history.length <= 1) {
              window.close();
            } else {
              navigate(-1);
            }
          }}
        >
          {previewMode ? "Quay lại chỉnh sửa" : "Quay lại"}
        </Button>
        <div className="text-xs text-ink-500 max-w-md">
          {previewMode
            ? "Đây là nội dung đúng như khi in ra — dò kỹ lại rồi mới bấm In / Xuất PDF."
            : 'Điền các thông tin còn thiếu bên dưới, bấm "Xem trước" để dò lại nội dung trước khi in.'}
        </div>
        {previewMode ? (
          <Button icon={Printer} onClick={() => window.print()}>
            In / Xuất PDF
          </Button>
        ) : (
          <Button icon={Eye} onClick={() => setPreviewMode(true)}>
            Xem trước
          </Button>
        )}
      </div>

      {previewMode && (
        <div className="no-print max-w-4xl mx-auto mt-4 px-6">
          <div className="bg-white border border-ink-200 rounded-lg p-3">
            <div className="text-xs font-semibold text-ink-700 uppercase mb-2">
              Gửi hợp đồng cho khách hàng qua email
            </div>
            <div className="text-[11px] text-ink-400 mb-2">
              Hệ thống sẽ tự sinh file PDF ở server đúng theo nội dung đang hiển
              thị bên dưới (kể cả các điều khoản đã tinh chỉnh) và gửi kèm email
              tới địa chỉ email trong hồ sơ khách hàng.
            </div>
            <textarea
              value={loiNhan}
              onChange={(e) => setLoiNhan(e.target.value)}
              placeholder="Lời nhắn thêm gửi kèm khách (không bắt buộc)..."
              rows={2}
              className="w-full border border-ink-200 rounded-lg px-2 py-1.5 text-xs mb-2"
            />
            <div className="flex items-center gap-3">
              <Button
                icon={Send}
                onClick={handleSendEmail}
                disabled={sendingEmail}
              >
                {sendingEmail ? "Đang gửi..." : "Gửi email cho khách"}
              </Button>
              {sendMsg && (
                <div
                  className={`flex items-center gap-1.5 text-xs ${
                    sendMsg.type === "success"
                      ? "text-success-700"
                      : "text-danger-600"
                  }`}
                >
                  {sendMsg.type === "success" ? (
                    <CheckCircle2 size={14} />
                  ) : (
                    <XCircle size={14} />
                  )}
                  {sendMsg.text}
                </div>
              )}
            </div>
            {emailHistory.length > 0 && (
              <div className="mt-2 pt-2 border-t border-ink-100 text-[11px] text-ink-500 space-y-0.5">
                <div className="font-medium text-ink-600">Lịch sử đã gửi:</div>
                {emailHistory.slice(0, 5).map((h, idx) => (
                  <div key={idx} className="flex items-center gap-1.5">
                    {h.thanhCong ? (
                      <CheckCircle2 size={11} className="text-success-600" />
                    ) : (
                      <XCircle size={11} className="text-danger-500" />
                    )}
                    <span>
                      {h.createdAt
                        ? new Date(h.createdAt).toLocaleString("vi-VN")
                        : ""}{" "}
                      — {h.emailDen}
                      {!h.thanhCong && h.loiChiTiet ? ` (${h.loiChiTiet})` : ""}
                    </span>
                  </div>
                ))}
              </div>
            )}
          </div>
        </div>
      )}

      <div
        className={`no-print max-w-4xl mx-auto mt-4 space-y-4 px-6 ${previewMode ? "hidden" : ""}`}
      >
        <div className="grid grid-cols-2 gap-4">
          <div className="bg-white border border-ink-200 rounded-lg p-3 space-y-2">
            <div className="text-xs font-semibold text-ink-700 uppercase">
              Bên A — Bên cung cấp
            </div>
            <input
              value={benA.tenCongTy}
              onChange={(e) =>
                setBenA((s) => ({ ...s, tenCongTy: e.target.value }))
              }
              placeholder="Tên công ty"
              className="w-full border border-ink-200 rounded-lg px-2 py-1.5 text-xs"
            />
            <input
              value={benA.diaChi}
              onChange={(e) =>
                setBenA((s) => ({ ...s, diaChi: e.target.value }))
              }
              placeholder="Địa chỉ trụ sở"
              className="w-full border border-ink-200 rounded-lg px-2 py-1.5 text-xs"
            />
            <div className="grid grid-cols-2 gap-2">
              <input
                value={benA.maSoThue}
                onChange={(e) =>
                  setBenA((s) => ({ ...s, maSoThue: e.target.value }))
                }
                placeholder="Mã số thuế"
                className="w-full border border-ink-200 rounded-lg px-2 py-1.5 text-xs"
              />
              <input
                value={benA.giayDkkd}
                onChange={(e) =>
                  setBenA((s) => ({ ...s, giayDkkd: e.target.value }))
                }
                placeholder="Số GCN ĐKKD (nếu khác MST)"
                className="w-full border border-ink-200 rounded-lg px-2 py-1.5 text-xs"
              />
            </div>
            <div className="grid grid-cols-2 gap-2">
              <input
                value={benA.dienThoai}
                onChange={(e) =>
                  setBenA((s) => ({ ...s, dienThoai: e.target.value }))
                }
                placeholder="Điện thoại"
                className="w-full border border-ink-200 rounded-lg px-2 py-1.5 text-xs"
              />
              <input
                value={benA.email}
                onChange={(e) =>
                  setBenA((s) => ({ ...s, email: e.target.value }))
                }
                placeholder="Email"
                className="w-full border border-ink-200 rounded-lg px-2 py-1.5 text-xs"
              />
            </div>
            <div className="grid grid-cols-2 gap-2">
              <input
                value={benA.soTaiKhoan}
                onChange={(e) =>
                  setBenA((s) => ({ ...s, soTaiKhoan: e.target.value }))
                }
                placeholder="Số tài khoản (nhận thanh toán)"
                className="w-full border border-ink-200 rounded-lg px-2 py-1.5 text-xs"
              />
              <input
                value={benA.nganHang}
                onChange={(e) =>
                  setBenA((s) => ({ ...s, nganHang: e.target.value }))
                }
                placeholder="Tại ngân hàng"
                className="w-full border border-ink-200 rounded-lg px-2 py-1.5 text-xs"
              />
            </div>
            <div className="grid grid-cols-3 gap-2">
              <input
                value={benA.nguoiDaiDien}
                onChange={(e) =>
                  setBenA((s) => ({ ...s, nguoiDaiDien: e.target.value }))
                }
                placeholder="Người đại diện"
                className="col-span-1 w-full border border-ink-200 rounded-lg px-2 py-1.5 text-xs"
              />
              <input
                value={benA.chucVu}
                onChange={(e) =>
                  setBenA((s) => ({ ...s, chucVu: e.target.value }))
                }
                placeholder="Chức vụ"
                className="w-full border border-ink-200 rounded-lg px-2 py-1.5 text-xs"
              />
              <input
                value={benA.cccd}
                onChange={(e) =>
                  setBenA((s) => ({ ...s, cccd: e.target.value }))
                }
                placeholder="Số CCCD"
                className="w-full border border-ink-200 rounded-lg px-2 py-1.5 text-xs"
              />
            </div>
          </div>

          <div className="bg-white border border-ink-200 rounded-lg p-3 space-y-2">
            <div className="text-xs font-semibold text-ink-700 uppercase">
              Bên B — Bên sử dụng dịch vụ (bổ sung)
            </div>
            <div className="text-[11px] text-ink-400">
              Tên, MST, điện thoại, email lấy tự động từ hồ sơ khách hàng — chỉ
              cần điền thêm bên dưới.
            </div>
            <input
              value={benB.diaChi}
              onChange={(e) =>
                setBenB((s) => ({ ...s, diaChi: e.target.value }))
              }
              placeholder="Địa chỉ khách hàng"
              className="w-full border border-ink-200 rounded-lg px-2 py-1.5 text-xs"
            />
            <input
              value={benB.giayDkkd}
              onChange={(e) =>
                setBenB((s) => ({ ...s, giayDkkd: e.target.value }))
              }
              placeholder="Số GCN ĐKKD (nếu là công ty)"
              className="w-full border border-ink-200 rounded-lg px-2 py-1.5 text-xs"
            />
            <div className="grid grid-cols-3 gap-2">
              <input
                value={benB.nguoiDaiDien}
                onChange={(e) =>
                  setBenB((s) => ({ ...s, nguoiDaiDien: e.target.value }))
                }
                placeholder="Người đại diện"
                className="w-full border border-ink-200 rounded-lg px-2 py-1.5 text-xs"
              />
              <input
                value={benB.chucVu}
                onChange={(e) =>
                  setBenB((s) => ({ ...s, chucVu: e.target.value }))
                }
                placeholder="Chức vụ"
                className="w-full border border-ink-200 rounded-lg px-2 py-1.5 text-xs"
              />
              <input
                value={benB.cccd}
                onChange={(e) =>
                  setBenB((s) => ({ ...s, cccd: e.target.value }))
                }
                placeholder="Số CCCD"
                className="w-full border border-ink-200 rounded-lg px-2 py-1.5 text-xs"
              />
            </div>
          </div>
        </div>

        <div className="bg-white border border-ink-200 rounded-lg p-3">
          <div className="text-xs font-semibold text-ink-700 uppercase mb-2">
            Các điều khoản có giá trị pháp lý cụ thể
          </div>
          <div className="grid grid-cols-4 gap-3 items-end">
            <div>
              <label className="block text-[11px] text-ink-500 mb-1">
                Ký tại (địa điểm)
              </label>
              <input
                value={diaDiemKy}
                onChange={(e) => setDiaDiemKy(e.target.value)}
                placeholder="VD: TP. Hồ Chí Minh"
                className="w-full border border-ink-200 rounded-lg px-2 py-1.5 text-xs"
              />
            </div>
            <div>
              <label className="block text-[11px] text-ink-500 mb-1">
                Bảo hành (tháng)
              </label>
              <input
                type="number"
                min="0"
                value={baoHanhThang}
                onChange={(e) => setBaoHanhThang(e.target.value)}
                className="w-full border border-ink-200 rounded-lg px-2 py-1.5 text-xs"
              />
            </div>
            <div>
              <label className="block text-[11px] text-ink-500 mb-1">
                Mức phạt vi phạm (%)
              </label>
              <input
                type="number"
                min="0"
                max="8"
                value={mucPhatViPham}
                onChange={(e) => setMucPhatViPham(e.target.value)}
                className="w-full border border-ink-200 rounded-lg px-2 py-1.5 text-xs"
              />
            </div>
            <div>
              <label className="block text-[11px] text-ink-500 mb-1">
                Số bản hợp đồng
              </label>
              <input
                type="number"
                min="2"
                value={soBan}
                onChange={(e) => setSoBan(e.target.value)}
                className="w-full border border-ink-200 rounded-lg px-2 py-1.5 text-xs"
              />
            </div>
          </div>
          <label className="flex items-center gap-2 mt-2 text-xs text-ink-600">
            <input
              type="checkbox"
              checked={vatIncluded}
              onChange={(e) => setVatIncluded(e.target.checked)}
            />
            Giá trị hợp đồng đã bao gồm thuế GTGT
          </label>
          {Number(mucPhatViPham) > 8 && (
            <div className="mt-2 flex items-start gap-1.5 text-[11px] text-warning-700 bg-warning-50 border border-warning-100 rounded-lg p-2">
              <AlertTriangle size={13} className="shrink-0 mt-0.5" />
              Theo Điều 301 Luật Thương mại 2005, mức phạt vi phạm không được
              vượt quá 8% giá trị phần nghĩa vụ hợp đồng bị vi phạm. Đặt trên 8%
              có thể bị tuyên vô hiệu phần vượt nếu xảy ra tranh chấp.
            </div>
          )}
        </div>

        {/* Tùy chỉnh nội dung từng điều khoản — sửa trực tiếp trên trình
            duyệt, không ghi vào cơ sở dữ liệu, chỉ áp dụng cho lần in này. */}
        <div className="bg-white border border-ink-200 rounded-lg p-3">
          <button
            type="button"
            onClick={() => setShowClauseEditor((s) => !s)}
            className="w-full flex items-center justify-between text-xs font-semibold text-ink-700 uppercase"
          >
            <span>Tùy chỉnh nội dung điều khoản (Điều 3 – 14)</span>
            {showClauseEditor ? (
              <ChevronUp size={14} />
            ) : (
              <ChevronDown size={14} />
            )}
          </button>
          {showClauseEditor && (
            <div className="mt-3 space-y-3">
              <div className="text-[11px] text-ink-400">
                Nội dung mặc định được sinh theo số tháng bảo hành / mức phạt /
                số bản đã điền ở trên. Sửa tay điều nào thì điều đó sẽ không tự
                cập nhật theo các ô trên nữa — bấm "Khôi phục mặc định" để đồng
                bộ lại. Chỉ áp dụng cho bản in này, không thay đổi dữ liệu trong
                hệ thống.
              </div>
              {CLAUSE_FIELDS.map(({ key, label }) => (
                <div key={key}>
                  <div className="flex items-center justify-between mb-1">
                    <label className="text-[11px] font-medium text-ink-600">
                      {label}
                    </label>
                    {clauseOverrides[key] !== undefined && (
                      <button
                        type="button"
                        onClick={() => resetClause(key)}
                        className="flex items-center gap-1 text-[11px] text-primary-600 hover:underline"
                      >
                        <RotateCcw size={11} />
                        Khôi phục mặc định
                      </button>
                    )}
                  </div>
                  <textarea
                    ref={autoGrowTextarea}
                    value={clauses[key]}
                    onChange={(e) => setClauseText(key, e.target.value)}
                    onInput={(e) => autoGrowTextarea(e.target)}
                    rows={
                      key === "dieu5" ||
                      key === "dieu6" ||
                      key === "dieu13" ||
                      key === "dieu14"
                        ? 4
                        : 2
                    }
                    className="clause-textarea w-full border border-ink-200 rounded-lg px-2 py-1.5 text-xs font-mono resize-none overflow-hidden leading-relaxed"
                  />
                </div>
              ))}
            </div>
          )}
        </div>

        <div className="text-[11px] text-ink-400 bg-ink-50 rounded-lg p-2.5">
          Bản in này dựng theo cấu trúc hợp đồng dịch vụ/cung cấp giải pháp CNTT
          phổ biến, dựa trên Bộ luật Dân sự 2015, Luật Thương mại 2005, Luật Sở
          hữu trí tuệ và Nghị định 13/2023/NĐ-CP về bảo vệ dữ liệu cá nhân —
          không thay thế tư vấn của luật sư. Nên rà soát lại nội dung, đặc biệt
          các điều khoản sở hữu trí tuệ, bảo mật và phạt vi phạm, trước khi dùng
          cho giao dịch thật.
        </div>
      </div>

      {/* Nội dung hợp đồng — phần này mới được in */}
      <div
        className={`printable-area bg-white max-w-[210mm] mx-auto p-[18mm] text-sm text-ink-900 shadow print:shadow-none print:my-0 ${
          previewMode ? "my-6" : "hidden print:block"
        }`}
      >
        <div className="text-center mb-1">
          <div className="font-bold">CỘNG HÒA XÃ HỘI CHỦ NGHĨA VIỆT NAM</div>
          <div className="font-bold">Độc lập - Tự do - Hạnh phúc</div>
          <div className="mt-1">—————oOo—————</div>
        </div>

        <div className="text-center my-5">
          <div className="text-lg font-bold uppercase">
            Hợp đồng cung cấp giải pháp phần mềm
          </div>
          <div className="text-sm">Số: {contract.maHopDong}</div>
        </div>

        <p className="mb-1">Căn cứ:</p>
        <ul className="list-disc list-inside mb-2">
          <li>Bộ luật Dân sự số 91/2015/QH13 ngày 24/11/2015;</li>
          <li>Luật Thương mại số 36/2005/QH11 ngày 14/06/2005;</li>
          <li>
            Luật Sở hữu trí tuệ số 50/2005/QH11 (được sửa đổi, bổ sung các năm
            2009, 2019, 2022);
          </li>
          <li>Nhu cầu và khả năng thực tế của hai bên.</li>
        </ul>
        <p className="mb-3">
          Hôm nay, ngày {contract.ngayKy ? formatDate(contract.ngayKy) : "…"}
          {diaDiemKy ? `, tại ${diaDiemKy}` : ""}, chúng tôi gồm:
        </p>

        <div className="mb-3">
          <div className="font-bold">BÊN A (BÊN CUNG CẤP):</div>
          <div>{benA.tenCongTy || "……………………………………"}</div>
          <div>Địa chỉ: {benA.diaChi || "……………………………………"}</div>
          <div>
            Mã số thuế: {benA.maSoThue || "……………"}
            {benA.giayDkkd ? ` — GCN ĐKKD số: ${benA.giayDkkd}` : ""}
          </div>
          <div>
            Điện thoại: {benA.dienThoai || "……………"} — Email:{" "}
            {benA.email || "……………"}
          </div>
          {(benA.soTaiKhoan || benA.nganHang) && (
            <div>
              Số tài khoản: {benA.soTaiKhoan || "……………"}
              {benA.nganHang ? ` tại ${benA.nganHang}` : ""}
            </div>
          )}
          <div>
            Đại diện: {benA.nguoiDaiDien || "……………"} — Chức vụ:{" "}
            {benA.chucVu || "……………"}
            {benA.cccd ? ` — CCCD số: ${benA.cccd}` : ""}
          </div>
        </div>

        <div className="mb-4">
          <div className="font-bold">BÊN B (BÊN SỬ DỤNG DỊCH VỤ):</div>
          <div>{customer?.tenKhachHang ?? contract.tenKhachHang}</div>
          <div>Địa chỉ: {benB.diaChi || "……………………………………"}</div>
          <div>
            Mã số thuế: {customer?.maSoThue || "……………"}
            {benB.giayDkkd ? ` — GCN ĐKKD số: ${benB.giayDkkd}` : ""}
          </div>
          <div>
            Điện thoại: {customer?.soDienThoai || "……………"} — Email:{" "}
            {customer?.email || "……………"}
          </div>
          <div>
            Đại diện: {benB.nguoiDaiDien || "……………"} — Chức vụ:{" "}
            {benB.chucVu || "……………"}
            {benB.cccd ? ` — CCCD số: ${benB.cccd}` : ""}
          </div>
        </div>

        <p className="mb-3">
          Hai bên đồng ý ký kết hợp đồng với các điều khoản sau:
        </p>

        <div className="font-bold mb-1">
          Điều 1. Đối tượng và nội dung hợp đồng
        </div>
        {quoteChiTiet.length > 0 ? (
          <table className="w-full border-collapse mb-2 text-xs">
            <thead>
              <tr>
                <th className="border border-ink-400 px-2 py-1">STT</th>
                <th className="border border-ink-400 px-2 py-1 text-left">
                  Sản phẩm / Dịch vụ
                </th>
                <th className="border border-ink-400 px-2 py-1">ĐVT</th>
                <th className="border border-ink-400 px-2 py-1">SL</th>
                <th className="border border-ink-400 px-2 py-1">Đơn giá</th>
                <th className="border border-ink-400 px-2 py-1">Thành tiền</th>
              </tr>
            </thead>
            <tbody>
              {quoteChiTiet.map((l, idx) => (
                <tr key={l.id ?? idx}>
                  <td className="border border-ink-400 px-2 py-1 text-center">
                    {idx + 1}
                  </td>
                  <td className="border border-ink-400 px-2 py-1">{l.tenSP}</td>
                  <td className="border border-ink-400 px-2 py-1 text-center">
                    {l.donVi ?? ""}
                  </td>
                  <td className="border border-ink-400 px-2 py-1 text-center">
                    {l.soLuong}
                  </td>
                  <td className="border border-ink-400 px-2 py-1 text-right">
                    {formatMoney(l.donGia)}
                  </td>
                  <td className="border border-ink-400 px-2 py-1 text-right">
                    {formatMoney(l.thanhTien ?? l.soLuong * l.donGia)}
                  </td>
                </tr>
              ))}
            </tbody>
          </table>
        ) : (
          <p className="mb-2">
            Bên A cung cấp giải pháp/dịch vụ phần mềm theo thỏa thuận giữa hai
            bên, giá trị: {formatMoney(giaTriHopDong)}.
          </p>
        )}

        <div className="font-bold mb-1">Điều 2. Giá trị hợp đồng và thuế</div>
        <p className="mb-1">
          Tổng giá trị hợp đồng: {formatMoney(giaTriHopDong)} (
          {vatIncluded
            ? "đã bao gồm thuế GTGT theo quy định hiện hành"
            : "chưa bao gồm thuế GTGT; thuế GTGT tính theo quy định hiện hành tại thời điểm xuất hóa đơn"}
          ). Bên A có trách nhiệm xuất hóa đơn điện tử theo quy định tại Nghị
          định số 123/2020/NĐ-CP khi Bên B thanh toán từng đợt hoặc theo thỏa
          thuận cụ thể giữa hai bên.
        </p>
        <p className="mb-3 italic">Bằng chữ: {soTienBangChu(giaTriHopDong)}.</p>

        <div className="font-bold mb-1">
          Điều 3. Phương thức và tiến độ thanh toán
        </div>
        <p className="mb-2">
          Hình thức thanh toán:{" "}
          {HINH_THUC_LABEL[contract.hinhThucThanhToan] ??
            contract.hinhThucThanhToan}
          . Bên B thanh toán bằng hình thức chuyển khoản vào tài khoản của Bên A
          nêu tại phần thông tin các bên, hoặc tiền mặt theo thỏa thuận cụ thể
          giữa hai bên.
        </p>
        {contract.hinhThucThanhToan === "TraGop" &&
          lichThanhToans.length > 0 && (
            <table className="w-full border-collapse mb-3 text-xs">
              <thead>
                <tr>
                  <th className="border border-ink-400 px-2 py-1">Đợt</th>
                  <th className="border border-ink-400 px-2 py-1">Số tiền</th>
                  <th className="border border-ink-400 px-2 py-1">
                    Hạn thanh toán
                  </th>
                </tr>
              </thead>
              <tbody>
                {lichThanhToans.map((l) => (
                  <tr key={l.id}>
                    <td className="border border-ink-400 px-2 py-1 text-center">
                      Đợt {l.soDot}
                    </td>
                    <td className="border border-ink-400 px-2 py-1 text-right">
                      {formatMoney(l.soTien)}
                    </td>
                    <td className="border border-ink-400 px-2 py-1 text-center">
                      {formatDate(l.hanThanhToan)}
                    </td>
                  </tr>
                ))}
              </tbody>
            </table>
          )}
        <p className="mb-3 whitespace-pre-line">{clauses.dieu3Cham}</p>

        <div className="font-bold mb-1">
          Điều 4. Thời hạn và tiến độ thực hiện
        </div>
        <p className="mb-3 whitespace-pre-line">{clauses.dieu4}</p>

        <div className="font-bold mb-1">
          Điều 5. Quyền và nghĩa vụ của Bên A
        </div>
        <p className="mb-3 whitespace-pre-line">{clauses.dieu5}</p>

        <div className="font-bold mb-1">
          Điều 6. Quyền và nghĩa vụ của Bên B
        </div>
        <p className="mb-3 whitespace-pre-line">{clauses.dieu6}</p>

        <div className="font-bold mb-1">
          Điều 7. Bảo hành, bảo trì và hỗ trợ kỹ thuật
        </div>
        <p className="mb-3 whitespace-pre-line">{clauses.dieu7}</p>

        <div className="font-bold mb-1">Điều 8. Quyền sở hữu trí tuệ</div>
        <p className="mb-3 whitespace-pre-line">{clauses.dieu8}</p>

        <div className="font-bold mb-1">
          Điều 9. Bảo mật thông tin và dữ liệu cá nhân
        </div>
        <p className="mb-3 whitespace-pre-line">{clauses.dieu9}</p>

        <div className="font-bold mb-1">
          Điều 10. Phạt vi phạm và bồi thường thiệt hại
        </div>
        <p className="mb-3 whitespace-pre-line">{clauses.dieu10}</p>

        <div className="font-bold mb-1">Điều 11. Sự kiện bất khả kháng</div>
        <p className="mb-3 whitespace-pre-line">{clauses.dieu11}</p>

        <div className="font-bold mb-1">Điều 12. Giải quyết tranh chấp</div>
        <p className="mb-3 whitespace-pre-line">{clauses.dieu12}</p>

        <div className="font-bold mb-1">Điều 13. Chấm dứt hợp đồng</div>
        <p className="mb-3 whitespace-pre-line">{clauses.dieu13}</p>

        <div className="font-bold mb-1">
          Điều 14. Hiệu lực và điều khoản chung
        </div>
        <p className="mb-6 whitespace-pre-line">{clauses.dieu14}</p>

        <div className="grid grid-cols-2 text-center">
          <div>
            <div className="font-bold">ĐẠI DIỆN BÊN A</div>
            <div className="text-xs italic">(Ký, ghi rõ họ tên, đóng dấu)</div>
          </div>
          <div>
            <div className="font-bold">ĐẠI DIỆN BÊN B</div>
            <div className="text-xs italic">(Ký, ghi rõ họ tên, đóng dấu)</div>
          </div>
        </div>
      </div>

      <style>{`
        @media print {
          .no-print { display: none !important; }
          .printable-area { box-shadow: none !important; margin: 0 !important; max-width: 100% !important; }
          body { background: white !important; }
        }
        @page { size: A4; margin: 15mm; }
      `}</style>
    </div>
  );
}
