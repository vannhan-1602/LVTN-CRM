using CRM.Application.Features.Customers.DTOs;
using MediatR;

namespace CRM.Application.Features.Customers.Commands.UpdateCustomer;

public record UpdateCustomerCommand(
    ulong Id,
    string TenKhachHang,
    ushort? LoaiKhachHangId,
    ushort? TinhTrangId,
    string? Email,
    string? SoDienThoai,
    string? MaSoThue,
    uint? NhanVienPhuTrachId) : IRequest<CustomerDto>;
