using CRM.Application.Features.Customers.DTOs;
using CRM.Application.Features.Customers.Mappings;
using MediatR;

namespace CRM.Application.Features.Customers.Commands.CreateCustomer;

public record CreateCustomerCommand(
    string TenKhachHang,
    ushort? LoaiKhachHangId,
    ushort? TinhTrangId,
    string? Email,
    string? SoDienThoai,
    string? MaSoThue,
    uint? NhanVienPhuTrachId) : IRequest<CustomerDto>;
