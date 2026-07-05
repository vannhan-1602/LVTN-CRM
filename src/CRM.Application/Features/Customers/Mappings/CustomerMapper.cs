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
            NgaySinh = customer.NgaySinh,
            NgayThanhLap = customer.NgayThanhLap,
            HangKhachHangId = customer.HangKhachHangId,
            CreatedAt = customer.CreatedAt,
            UpdatedAt = customer.UpdatedAt
        };

    
    public static CustomerDto ToDto(
        KhachHang customer,
        string? tenLoai,
        string? tenTinhTrang,
        string? tenNhanVien,
        string? tenHangKhachHang = null) =>
        new()
        {
            Id = customer.Id,
            MaKhachHang = customer.MaKhachHang,
            TenKhachHang = customer.TenKhachHang,
            LoaiKhachHangId = customer.LoaiKhachHangId,
            TenLoaiKhachHang = tenLoai,
            TinhTrangId = customer.TinhTrangId,
            TenTinhTrang = tenTinhTrang,
            Email = customer.Email,
            SoDienThoai = customer.SoDienThoai,
            MaSoThue = customer.MaSoThue,
            NhanVienPhuTrachId = customer.NhanVienPhuTrachId,
            TenNhanVienPhuTrach = tenNhanVien,
            NgaySinh = customer.NgaySinh,
            NgayThanhLap = customer.NgayThanhLap,
            HangKhachHangId = customer.HangKhachHangId,
            TenHangKhachHang = tenHangKhachHang,
            CreatedAt = customer.CreatedAt,
            UpdatedAt = customer.UpdatedAt
        };
}