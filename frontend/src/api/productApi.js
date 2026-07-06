import axiosClient from "./axiosClient";

const productApi = {
  getAll: (params) => axiosClient.get("/Product", { params }),
  getById: (id) => axiosClient.get(`/Product/${id}`),
  getTypes: () => axiosClient.get("/Product/types"),
  create: (data) => axiosClient.post("/Product", data),
  update: (id, data) => axiosClient.put(`/Product/${id}`, data),
  delete: (id) => axiosClient.delete(`/Product/${id}`),
  adjustStock: (id, data) => axiosClient.post(`/Product/${id}/stock`, data),
  getImages: (id) => axiosClient.get(`/Product/${id}/images`),
  uploadImage: (id, file, isMain) => {
    const formData = new FormData();
    formData.append("file", file);
    formData.append("isMain", isMain ? "true" : "false");
    return axiosClient.post(`/Product/${id}/images`, formData, {
      headers: { "Content-Type": "multipart/form-data" },
    });
  },
  setMainImage: (id, imageId) =>
    axiosClient.put(`/Product/${id}/images/${imageId}/set-main`),
  deleteImage: (id, imageId) =>
    axiosClient.delete(`/Product/${id}/images/${imageId}`),
};

export default productApi;
