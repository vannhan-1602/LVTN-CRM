using System;

namespace CRM.Application.Features.Leads.DTOs;

public class LeadDto
{
    public ulong Id { get; set; }
    public string TenLead { get; set; } = string.Empty;
    public string? TenCongTy { get; set; }
    public string? SoDienThoai { get; set; }
    public string? Email { get; set; }
    public string? NguonLead { get; set; }
    public string? TinhTrang { get; set; }
    public uint? NhanVienPhuTrachId { get; set; }
    public bool IsDeleted { get; set; }
    public DateTime? CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}

public class CreateLeadRequestDto
{
    public string TenLead { get; set; } = string.Empty;
    public string? TenCongTy { get; set; }
    public string? SoDienThoai { get; set; }
    public string? Email { get; set; }
    public string? NguonLead { get; set; }
    public uint? NhanVienPhuTrachId { get; set; }
}

public class UpdateLeadRequestDto
{
    public string TenLead { get; set; } = string.Empty;
    public string? TenCongTy { get; set; }
    public string? SoDienThoai { get; set; }
    public string? Email { get; set; }
    public string? NguonLead { get; set; }
    public string? TinhTrang { get; set; }
    public uint? NhanVienPhuTrachId { get; set; }
}

public class CreatePublicLeadRequestDto
{
    public string TenLead { get; set; } = string.Empty;
    public string? TenCongTy { get; set; }
    public string? SoDienThoai { get; set; }
    public string? Email { get; set; }

    /// <summary>
    /// Honeypot chống bot: field ẩn bằng CSS trên landing page (không hiển thị, không có label),
    /// người dùng thật sẽ luôn để trống. Bot tự động điền form thường điền vào mọi field kể cả field ẩn.
    /// Nếu có giá trị -> coi như spam, âm thầm bỏ qua (không báo lỗi để không lộ cơ chế cho bot).
    /// </summary>
    public string? Website { get; set; }
}