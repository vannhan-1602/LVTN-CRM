import Card from "../../components/common/Card";

export default function WelcomeDashboard() {
  return (
    <Card>
      <h2 className="text-lg font-semibold text-ink-900 mb-1.5">Dashboard</h2>
      <p className="text-ink-500 text-sm">
        Chào mừng đến với CRM System. Chọn module từ menu bên trái.
      </p>
    </Card>
  );
}
