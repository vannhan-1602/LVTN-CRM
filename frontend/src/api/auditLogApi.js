import axiosClient from "./axiosClient";

const auditLogApi = {
  getAll: (params) => axiosClient.get("/AuditLog", { params }),
  getTableNames: () => axiosClient.get("/AuditLog/table-names"),
};

export default auditLogApi;
