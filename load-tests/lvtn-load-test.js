import http from 'k6/http';
import { check, sleep, group } from 'k6';
import { Trend } from 'k6/metrics';

// Chỉnh BASE_URL theo môi trường đang test (localhost khi test trước/sau tối ưu trên máy dev,
// hoặc domain thật trên Render khi muốn đo tải production). USERNAME/PASSWORD là tài khoản
// test có sẵn trong DB — không dùng tài khoản thật để tránh tạo rác dữ liệu khi test lặp lại.
const BASE_URL = __ENV.BASE_URL || 'https://localhost:7071';
const USERNAME = __ENV.USERNAME || 'admin';
const PASSWORD = __ENV.PASSWORD || 'changeme';

const loginLatency = new Trend('login_latency', true);
const customerListLatency = new Trend('customer_list_latency', true);
const dashboardTrendsLatency = new Trend('dashboard_trends_latency', true);
const danhMucLatency = new Trend('danh_muc_latency', true);

export const options = {
  scenarios: {
    ramping_load: {
      executor: 'ramping-vus',
      startVUs: 0,
      stages: [
        { duration: '30s', target: 20 },
        { duration: '1m', target: 50 },
        { duration: '1m', target: 100 },
        { duration: '30s', target: 0 },
      ],
    },
  },
  thresholds: {
    http_req_duration: ['p(95)<800'],
    http_req_failed: ['rate<0.01'],
    // Riêng danh mục PHẢI nhanh hơn hẳn vì đã bật OutputCache 10 phút — nếu p95 không rơi
    // xuống dưới ngưỡng này, nghĩa là cache không hit như kỳ vọng, đáng để điều tra lại.
    danh_muc_latency: ['p(95)<100'],
  },
};

// Đăng nhập 1 lần mỗi VU lúc khởi tạo (setup per-VU), không lặp lại lúc mỗi iteration — đúng
// hành vi thật (user login 1 lần rồi dùng access token cho nhiều request, không login lại
// liên tục). Access token 15 phút là đủ sống hết vòng đời test (tổng ~3 phút).
export function setup() {
  const loginRes = http.post(
    `${BASE_URL}/api/Auth/login`,
    JSON.stringify({ username: USERNAME, password: PASSWORD }),
    { headers: { 'Content-Type': 'application/json' } },
  );

  check(loginRes, { 'login status 200': (r) => r.status === 200 });
  loginLatency.add(loginRes.timings.duration);

  const token = loginRes.json('data.accessToken');
  if (!token) {
    throw new Error('Login thất bại — kiểm tra lại USERNAME/PASSWORD hoặc BASE_URL.');
  }
  return { token };
}

export default function (data) {
  const headers = {
    headers: {
      'Content-Type': 'application/json',
      Authorization: `Bearer ${data.token}`,
    },
  };

  group('Danh sách khách hàng (phân trang)', function () {
    const res = http.get(`${BASE_URL}/api/Customer?pageNumber=1&pageSize=20`, headers);
    customerListLatency.add(res.timings.duration);
    check(res, { 'Customer list 200': (r) => r.status === 200 });
  });

  group('Dashboard trends (đã tối ưu N+1 -> COUNT)', function () {
    const res = http.get(`${BASE_URL}/api/analytics/dashboard-trends`, headers);
    dashboardTrendsLatency.add(res.timings.duration);
    check(res, { 'Dashboard trends 200': (r) => r.status === 200 });
  });

  group('Danh mục loại khách hàng (đã bật OutputCache)', function () {
    const res = http.get(`${BASE_URL}/api/danh-muc/loai-khach-hang`, headers);
    danhMucLatency.add(res.timings.duration);
    check(res, { 'DanhMuc 200': (r) => r.status === 200 });
  });

  sleep(1);
}