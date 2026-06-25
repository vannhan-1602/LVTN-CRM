using CRM.Application.Features.Addresses.DTOs;
using CRM.Application.Interfaces.Addresses;
using CRM.Infrastructure.Persistence.Contexts;
using CRM.Infrastructure.Persistence.Entities;
using Microsoft.EntityFrameworkCore;

namespace CRM.Infrastructure.Persistence.Repositories;

public class AddressRepository : IAddressRepository
{
    private readonly CrmDbContext _ctx;
    public AddressRepository(CrmDbContext ctx) => _ctx = ctx;

    public async Task<List<AddressDto>> GetByKhachHangAsync(ulong khachHangId, CancellationToken ct = default)
    {
        var list = await _ctx.Set<KhDiaChiEntity>().AsNoTracking()
            .Where(x => x.KhachHang_Id == khachHangId)
            .OrderByDescending(x => x.IsDefault)
            .ToListAsync(ct);
        return list.Select(ToDto).ToList();
    }

    public async Task<AddressDto?> GetByIdAsync(ulong id, CancellationToken ct = default)
    {
        var e = await _ctx.Set<KhDiaChiEntity>().FindAsync([id], ct);
        return e is null ? null : ToDto(e);
    }

    public async Task<AddressDto> AddAsync(
        ulong khachHangId,
        string? diaChiChiTiet,
        string? tinhThanh,
        string? quanHuyen,
        string? phuongXa,
        string loaiDiaChi,
        bool isDefault,
        CancellationToken ct = default)
    {
        // Nếu đặt làm mặc định → bỏ mặc định các địa chỉ khác cùng loại
        if (isDefault)
            await ClearDefaultAsync(khachHangId, loaiDiaChi, ct);

        var e = new KhDiaChiEntity
        {
            KhachHang_Id = khachHangId,
            DiaChiChiTiet = diaChiChiTiet,
            TinhThanh = tinhThanh,
            QuanHuyen = quanHuyen,
            PhuongXa = phuongXa,
            LoaiDiaChi = loaiDiaChi,
            IsDefault = isDefault
        };
        _ctx.Set<KhDiaChiEntity>().Add(e);
        await _ctx.SaveChangesAsync(ct);
        return (await GetByIdAsync(e.Id, ct))!;
    }

    public async Task<AddressDto> UpdateAsync(
        ulong id,
        string? diaChiChiTiet,
        string? tinhThanh,
        string? quanHuyen,
        string? phuongXa,
        string loaiDiaChi,
        bool isDefault,
        CancellationToken ct = default)
    {
        var e = await _ctx.Set<KhDiaChiEntity>().FindAsync([id], ct)
            ?? throw new InvalidOperationException($"DiaChi {id} not found.");

        if (isDefault && !e.IsDefault)
            await ClearDefaultAsync(e.KhachHang_Id, loaiDiaChi, ct);

        e.DiaChiChiTiet = diaChiChiTiet;
        e.TinhThanh = tinhThanh;
        e.QuanHuyen = quanHuyen;
        e.PhuongXa = phuongXa;
        e.LoaiDiaChi = loaiDiaChi;
        e.IsDefault = isDefault;
        await _ctx.SaveChangesAsync(ct);
        return (await GetByIdAsync(id, ct))!;
    }

    public async Task<bool> DeleteAsync(ulong id, CancellationToken ct = default)
    {
        var e = await _ctx.Set<KhDiaChiEntity>().FindAsync([id], ct);
        if (e is null) return false;
        _ctx.Set<KhDiaChiEntity>().Remove(e);
        await _ctx.SaveChangesAsync(ct);
        return true;
    }

    private async Task ClearDefaultAsync(ulong khachHangId, string loaiDiaChi, CancellationToken ct)
    {
        var existing = await _ctx.Set<KhDiaChiEntity>()
            .Where(x => x.KhachHang_Id == khachHangId && x.LoaiDiaChi == loaiDiaChi && x.IsDefault)
            .ToListAsync(ct);
        existing.ForEach(x => x.IsDefault = false);
    }

    private static AddressDto ToDto(KhDiaChiEntity e) => new()
    {
        Id = e.Id,
        KhachHangId = e.KhachHang_Id,
        DiaChiChiTiet = e.DiaChiChiTiet,
        TinhThanh = e.TinhThanh,
        QuanHuyen = e.QuanHuyen,
        PhuongXa = e.PhuongXa,
        LoaiDiaChi = e.LoaiDiaChi,
        IsDefault = e.IsDefault
    };
}