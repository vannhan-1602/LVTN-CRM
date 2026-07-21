import { useState } from "react";
import {
  Sparkles,
  ShieldCheck,
  Zap,
  Headphones,
  CheckCircle2,
  ArrowRight,
  Building2,
  Phone,
  Mail,
  User,
} from "lucide-react";
import leadPublicApi from "../../api/leadPublicApi";
import Button from "../../components/common/Button";

export default function LandingPageDemo() {
  const [form, setForm] = useState({
    tenLead: "",
    tenCongTy: "",
    soDienThoai: "",
    email: "",
  });
  const [submitting, setSubmitting] = useState(false);
  const [message, setMessage] = useState("");
  const [error, setError] = useState("");

  const handleSubmit = async (e) => {
    e.preventDefault();
    setSubmitting(true);
    setMessage("");
    setError("");
    try {
      const res = await leadPublicApi.submitLandingPageLead(form);
      setMessage(
        res.message ||
          "Đăng ký thành công! Chuyên gia của chúng tôi sẽ liên hệ lại với bạn trong 24h tới.",
      );
      setForm({ tenLead: "", tenCongTy: "", soDienThoai: "", email: "" });
    } catch (err) {
      setError(
        err?.message || "Có lỗi xảy ra, vui lòng kiểm tra lại thông tin.",
      );
    } finally {
      setSubmitting(false);
    }
  };

  return (
    <div className="min-h-screen bg-slate-50 font-sans text-slate-800">
      {/* Header / Navbar */}
      <header className="sticky top-0 z-50 bg-white/80 backdrop-blur-md border-b border-slate-100">
        <div className="max-w-7xl mx-auto px-6 h-16 flex items-center justify-between">
          <div className="flex items-center gap-2 font-bold text-xl text-blue-600">
            <div className="w-9 h-9 rounded-xl bg-blue-600 text-white flex items-center justify-center shadow-lg shadow-blue-500/30">
              <Sparkles size={20} />
            </div>
            <span>TechCRM Pro</span>
          </div>
          <a
            href="#dang-ky"
            className="hidden sm:inline-flex items-center justify-center bg-blue-600 hover:bg-blue-700 text-white font-medium px-5 py-2 rounded-xl text-sm transition-all shadow-md shadow-blue-600/20"
          >
            Nhận tư vấn ngay
          </a>
        </div>
      </header>

      {/* Hero Section */}
      <section className="relative overflow-hidden py-20 lg:py-28 bg-gradient-to-b from-blue-50/50 via-white to-slate-50">
        <div className="max-w-7xl mx-auto px-6 grid grid-cols-1 lg:grid-cols-12 gap-12 items-center">
          <div className="lg:col-span-7 space-y-6 text-center lg:text-left">
            <span className="inline-flex items-center gap-1.5 px-3.5 py-1.5 rounded-full text-xs font-semibold bg-blue-50 text-blue-600 border border-blue-100">
              <Sparkles size={13} /> Nền tảng CRM toàn diện cho doanh nghiệp 4.0
            </span>
            <h1 className="text-4xl lg:text-6xl font-extrabold text-slate-900 tracking-tight leading-tight">
              Tối ưu doanh số, <br />
              <span className="text-transparent bg-clip-text bg-gradient-to-r from-blue-600 to-indigo-600">
                Bứt phá mọi giới hạn
              </span>
            </h1>
            <p className="text-lg text-slate-600 max-w-xl mx-auto lg:mx-0">
              Quản lý khách hàng tập trung, tự động hóa quy trình bán hàng và
              chăm sóc toàn diện chỉ trong một nền tảng thông minh duy nhất.
            </p>
            <div className="flex flex-wrap items-center justify-center lg:justify-start gap-4 pt-2">
              <div className="flex items-center gap-2 text-sm text-slate-600 font-medium">
                <CheckCircle2 size={17} className="text-emerald-500" /> Dùng thử
                miễn phí
              </div>
              <div className="flex items-center gap-2 text-sm text-slate-600 font-medium">
                <CheckCircle2 size={17} className="text-emerald-500" /> Cài đặt
                nhanh chóng
              </div>
              <div className="flex items-center gap-2 text-sm text-slate-600 font-medium">
                <CheckCircle2 size={17} className="text-emerald-500" /> Hỗ trợ
                24/7
              </div>
            </div>
          </div>

          {/* Form Đăng Ký Tư Vấn (Đổ trực tiếp về CRM Lead) */}
          <div id="dang-ky" className="lg:col-span-5">
            <div className="bg-white rounded-3xl shadow-2xl border border-slate-100 p-8 relative">
              <div className="absolute -top-3 right-8 bg-gradient-to-r from-blue-600 to-indigo-600 text-white text-xs font-semibold px-3 py-1 rounded-full shadow-md">
                Đăng ký ngay
              </div>
              <h3 className="text-xl font-bold text-slate-900 mb-2">
                Nhận Tư Vấn Miễn Phí
              </h3>
              <p className="text-xs text-slate-500 mb-6">
                Điền thông tin để chuyên gia của chúng tôi kết nối lại ngay lập
                tức.
              </p>

              <form onSubmit={handleSubmit} className="space-y-4">
                <div>
                  <label className="block text-xs font-semibold text-slate-700 uppercase tracking-wider mb-1.5">
                    Họ và tên *
                  </label>
                  <div className="relative">
                    <User
                      size={16}
                      className="absolute left-3.5 top-1/2 -translate-y-1/2 text-slate-400"
                    />
                    <input
                      type="text"
                      required
                      value={form.tenLead}
                      onChange={(e) =>
                        setForm({ ...form, tenLead: e.target.value })
                      }
                      className="w-full border border-slate-200 rounded-xl pl-10 pr-4 py-2.5 text-sm focus:ring-2 focus:ring-blue-500 focus:border-transparent outline-none transition-all"
                      placeholder="Nguyễn Văn A"
                    />
                  </div>
                </div>

                <div>
                  <label className="block text-xs font-semibold text-slate-700 uppercase tracking-wider mb-1.5">
                    Tên công ty
                  </label>
                  <div className="relative">
                    <Building2
                      size={16}
                      className="absolute left-3.5 top-1/2 -translate-y-1/2 text-slate-400"
                    />
                    <input
                      type="text"
                      value={form.tenCongTy}
                      onChange={(e) =>
                        setForm({ ...form, tenCongTy: e.target.value })
                      }
                      className="w-full border border-slate-200 rounded-xl pl-10 pr-4 py-2.5 text-sm focus:ring-2 focus:ring-blue-500 focus:border-transparent outline-none transition-all"
                      placeholder="Công ty Cổ phần ABC"
                    />
                  </div>
                </div>

                <div>
                  <label className="block text-xs font-semibold text-slate-700 uppercase tracking-wider mb-1.5">
                    Số điện thoại *
                  </label>
                  <div className="relative">
                    <Phone
                      size={16}
                      className="absolute left-3.5 top-1/2 -translate-y-1/2 text-slate-400"
                    />
                    <input
                      type="tel"
                      required
                      value={form.soDienThoai}
                      onChange={(e) =>
                        setForm({ ...form, soDienThoai: e.target.value })
                      }
                      className="w-full border border-slate-200 rounded-xl pl-10 pr-4 py-2.5 text-sm focus:ring-2 focus:ring-blue-500 focus:border-transparent outline-none transition-all"
                      placeholder="0901234567"
                    />
                  </div>
                </div>

                <div>
                  <label className="block text-xs font-semibold text-slate-700 uppercase tracking-wider mb-1.5">
                    Email liên hệ
                  </label>
                  <div className="relative">
                    <Mail
                      size={16}
                      className="absolute left-3.5 top-1/2 -translate-y-1/2 text-slate-400"
                    />
                    <input
                      type="email"
                      value={form.email}
                      onChange={(e) =>
                        setForm({ ...form, email: e.target.value })
                      }
                      className="w-full border border-slate-200 rounded-lg pl-10 pr-4 py-2.5 text-sm focus:ring-2 focus:ring-blue-500 focus:border-transparent outline-none transition-all"
                      placeholder="example@domain.com"
                    />
                  </div>
                </div>

                {message && (
                  <div className="text-xs text-emerald-700 bg-emerald-50 border border-emerald-100 p-3 rounded-xl font-medium">
                    {message}
                  </div>
                )}
                {error && (
                  <div className="text-xs text-rose-600 bg-rose-50 border border-rose-100 p-3 rounded-xl font-medium">
                    {error}
                  </div>
                )}

                <button
                  type="submit"
                  disabled={submitting}
                  className="w-full bg-blue-600 hover:bg-blue-700 active:scale-[0.98] text-white font-semibold py-3 rounded-xl text-sm transition-all shadow-lg shadow-blue-600/30 flex items-center justify-center gap-2 disabled:opacity-50"
                >
                  {submitting ? (
                    "Đang gửi thông tin..."
                  ) : (
                    <>
                      Gửi yêu cầu tư vấn <ArrowRight size={16} />
                    </>
                  )}
                </button>
              </form>
            </div>
          </div>
        </div>
      </section>

      {/* Về chúng tôi & Tính năng */}
      <section className="py-20 bg-white border-t border-slate-100">
        <div className="max-w-7xl mx-auto px-6">
          <div className="text-center max-w-2xl mx-auto mb-16 space-y-3">
            <h2 className="text-xs font-bold text-blue-600 uppercase tracking-widest">
              Về Chúng Tôi
            </h2>
            <h3 className="text-3xl font-extrabold text-slate-900">
              Giải pháp toàn diện cho mọi doanh nghiệp
            </h3>
            <p className="text-slate-600 text-sm">
              Chúng tôi cung cấp hệ sinh thái công nghệ giúp chuẩn hóa toàn bộ
              quy trình vận hành từ kinh doanh đến kế toán.
            </p>
          </div>

          <div className="grid grid-cols-1 md:grid-cols-3 gap-8">
            <div className="bg-slate-50 border border-slate-100 p-8 rounded-3xl space-y-4 hover:shadow-xl transition-all">
              <div className="w-12 h-12 rounded-2xl bg-blue-100 text-blue-600 flex items-center justify-center font-bold">
                <Zap size={24} />
              </div>
              <h4 className="text-lg font-bold text-slate-900">
                Quản Lý Lead Tự Động
              </h4>
              <p className="text-slate-600 text-sm leading-relaxed">
                Tự động thu thập dữ liệu khách hàng tiềm năng từ website,
                landing page và phân bổ trực tiếp cho đội ngũ sale chăm sóc kịp
                thời.
              </p>
            </div>

            <div className="bg-slate-50 border border-slate-100 p-8 rounded-3xl space-y-4 hover:shadow-xl transition-all">
              <div className="w-12 h-12 rounded-2xl bg-indigo-100 text-indigo-600 flex items-center justify-center font-bold">
                <ShieldCheck size={24} />
              </div>
              <h4 className="text-lg font-bold text-slate-900">
                Bảo Mật & Tin Cậy
              </h4>
              <p className="text-slate-600 text-sm leading-relaxed">
                Hạ tầng công nghệ bảo mật cao cấp, phân quyền chi tiết theo từng
                vai trò giúp bảo vệ dữ liệu khách hàng tuyệt đối.
              </p>
            </div>

            <div className="bg-slate-50 border border-slate-100 p-8 rounded-3xl space-y-4 hover:shadow-xl transition-all">
              <div className="w-12 h-12 rounded-2xl bg-emerald-100 text-emerald-600 flex items-center justify-center font-bold">
                <Headphones size={24} />
              </div>
              <h4 className="text-lg font-bold text-slate-900">
                Chăm Sóc Khách Hàng
              </h4>
              <p className="text-slate-600 text-sm leading-relaxed">
                Theo dõi lịch sử giao dịch, tích điểm khách hàng thân thiết
                (Loyalty) và tự động gửi email tri ân vào các dịp lễ tết, sinh
                nhật.
              </p>
            </div>
          </div>
        </div>
      </section>

      {/* Footer */}
      <footer className="bg-slate-900 text-slate-400 py-12 text-center text-sm border-t border-slate-800">
        <div className="max-w-7xl mx-auto px-6 space-y-4">
          <div className="flex items-center justify-center gap-2 font-bold text-lg text-white">
            <Sparkles size={18} className="text-blue-500" /> TechCRM System
          </div>
          <p>© 2026 TechCRM Solutions. Bảo lưu mọi quyền.</p>
        </div>
      </footer>
    </div>
  );
}
