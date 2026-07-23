import Card from "../../components/common/Card";
import DashboardAlertsCard from "./DashboardAlertsCard";

export default function WelcomeDashboard() {
  return (
    <div className="space-y-4">
      <Card>
        <h2 className="text-lg font-semibold text-ink-900 mb-1.5">Dashboard</h2>
        <p className="text-ink-500 text-sm">
          Chào mừng đến với CRM System. Chọn module từ menu bên trái.
        </p>
      </Card>
      <DashboardAlertsCard />
    </div>
  );
}
