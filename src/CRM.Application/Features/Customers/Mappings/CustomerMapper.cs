using CRM.Application.Features.Customers.DTOs;
using CRM.Domain.Entities.Customers;

namespace CRM.Application.Features.Customers.Mappings;

public static class CustomerMapper
{
    public static CustomerDto ToDto(KhachHang customer) =>
        new()
        {
            Id = customer.Id,
            MaKhachHang = customer.MaKhachHang,
            TenKhachHang = customer.TenKhachHang,
            LoaiKhachHangId = customer.LoaiKhachHangId,
            TinhTrangId = customer.TinhTrangId,
            Email = customer.Email,
            SoDienThoai = customer.SoDienThoai,
            MaSoThue = customer.MaSoThue,
            NhanVienPhuTrachId = customer.NhanVienPhuTrachId,
            CreatedAt = customer.CreatedAt,
            UpdatedAt = customer.UpdatedAt
        };
}
