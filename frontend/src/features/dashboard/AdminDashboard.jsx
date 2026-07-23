import { useEffect, useState } from "react";
import { useNavigate } from "react-router-dom";
import { Users, ShieldAlert, UserX, Settings, ScrollText, UserCog } from "lucide-react";
import userManagementApi from "../../api/userManagementApi";
import Card from "../../components/common/Card";
import StatCard from "../../components/common/StatCard";
import Button from "../../components/common/Button";
import DashboardAlertsCard from "./DashboardAlertsCard";

export default function AdminDashboard() {
  const navigate = useNavigate();
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState("");
  const [stats, setStats] = useState(null);

  useEffect(() => {
    let cancelled = false;

    (async () => {
      setLoading(true);
      setError("");
      try {
        const res = await userManagementApi.getAll();
        const users = res.data ?? [];
        if (cancelled) return;
        setStats({
          tong: users.length,
          biKhoa: users.filter((u) => u.trangThai === "Locked").length,
          chuaGanVaiTro: users.filter((u) => !u.roleId).length,
        });
      } catch (err) {
        if (!cancelled)
          setError(err?.message || "Không thể tải dữ liệu dashboard");
      } finally {
        if (!cancelled) setLoading(false);
      }
    })();

    return () => {
      cancelled = true;
    };
  }, []);

  return (
    <div className="space-y-5">
      <div>
        <p className="text-xs text-ink-400 uppercase tracking-wide mb-0.5">
          CRM / Tổng quan
        </p>
        <h1 className="text-xl font-semibold text-ink-900">
          Dashboard quản trị hệ thống
        </h1>
      </div>

      {loading && (
        <div className="text-sm text-ink-400 py-10 text-center">
          Đang tải dashboard...
        </div>
      )}

      {error && (
        <div className="text-sm text-danger-600 bg-danger-50 rounded-lg p-4">
          {error}
        </div>
      )}

      {stats && !loading && (
        <div className="grid grid-cols-1 sm:grid-cols-3 gap-3">
          <StatCard
            label="Tổng người dùng"
            value={stats.tong}
            icon={Users}
          />
          <StatCard
            label="Tài khoản bị khóa"
            value={stats.biKhoa}
            tone={stats.biKhoa > 0 ? "warning" : "default"}
            icon={UserX}
          />
          <StatCard
            label="Chưa gán vai trò"
            value={stats.chuaGanVaiTro}
            tone={stats.chuaGanVaiTro > 0 ? "warning" : "default"}
            icon={ShieldAlert}
          />
        </div>
      )}

      <div className="grid grid-cols-1 lg:grid-cols-3 gap-4 items-start">
        <div className="lg:col-span-2 space-y-4">
          <Card title="Lối tắt quản trị">
            <div className="grid grid-cols-2 sm:grid-cols-3 gap-2">
              <Button
                size="sm"
                variant="secondary"
                icon={UserCog}
                onClick={() => navigate("/users")}
              >
                Người dùng & Nhân sự
              </Button>
              <Button
                size="sm"
                variant="secondary"
                icon={Settings}
                onClick={() => navigate("/settings")}
              >
                Cài đặt danh mục
              </Button>
              <Button
                size="sm"
                variant="secondary"
                icon={ScrollText}
                onClick={() => navigate("/audit-log")}
              >
                Nhật ký hệ thống
              </Button>
            </div>
          </Card>
        </div>

        <DashboardAlertsCard />
      </div>
    </div>
  );
}
