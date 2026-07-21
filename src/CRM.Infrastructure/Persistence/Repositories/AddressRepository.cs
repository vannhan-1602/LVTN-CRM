using CRM.Application.Features.Addresses.DTOs;
using CRM.Application.Interfaces.Addresses;
using CRM.Infrastructure.Persistence.Contexts;
using CRM.Infrastructure.Persistence.Entities;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace CRM.Infrastructure.Persistence.Repositories;

public class AddressRepository : IAddressRepository
{
    private readonly CrmDbContext _ctx;
    public AddressRepository(CrmDbContext ctx) => _ctx = ctx;

    public async Task<List<AddressDto>> GetByKhachHangAsync(ulong khachHangId, CancellationToken ct = default)
    {
        var list = await (
            from dc in _ctx.KhDiaChis.AsNoTracking()
            where dc.KhachHang_Id == khachHangId
            join t in _ctx.DmTinhThanhs on dc.TinhThanh_Id equals t.Id into tJ
            from t in tJ.DefaultIfEmpty()
            join p in _ctx.DmPhuongXas on dc.PhuongXa_Id equals p.Id into pJ
            from p in pJ.DefaultIfEmpty()
            orderby dc.IsDefault descending
            select new AddressDto
            {
                Id = dc.Id,
                KhachHangId = dc.KhachHang_Id,
                DiaChiChiTiet = dc.DiaChiChiTiet,
                TinhThanhId = dc.TinhThanh_Id,
                TinhThanh = t != null ? t.TenTinhThanh : null,
                PhuongXaId = dc.PhuongXa_Id,
                PhuongXa = p != null ? p.TenPhuongXa : null,
                LoaiDiaChi = dc.LoaiDiaChi,
                IsDefault = dc.IsDefault
            }
        ).ToListAsync(ct);

        return list;
    }

    public async Task<AddressDto?> GetByIdAsync(ulong id, CancellationToken ct = default)
    {
        var query =
            from dc in _ctx.KhDiaChis.AsNoTracking()
            where dc.Id == id
            join t in _ctx.DmTinhThanhs on dc.TinhThanh_Id equals t.Id into tJ
            from t in tJ.DefaultIfEmpty()
            join p in _ctx.DmPhuongXas on dc.PhuongXa_Id equals p.Id into pJ
            from p in pJ.DefaultIfEmpty()
            select new AddressDto
            {
                Id = dc.Id,
                KhachHangId = dc.KhachHang_Id,
                DiaChiChiTiet = dc.DiaChiChiTiet,
                TinhThanhId = dc.TinhThanh_Id,
                TinhThanh = t != null ? t.TenTinhThanh : null,
                PhuongXaId = dc.PhuongXa_Id,
                PhuongXa = p != null ? p.TenPhuongXa : null,
                LoaiDiaChi = dc.LoaiDiaChi,
                IsDefault = dc.IsDefault
            };
        return await query.FirstOrDefaultAsync(ct);
    }

    public async Task<AddressDto> AddAsync(ulong khachHangId, string? diaChiChiTiet, uint? tinhThanhId, uint? phuongXaId, string loaiDiaChi, bool isDefault, CancellationToken ct = default)
    {
        if (isDefault)
            await ClearDefaultAsync(khachHangId, loaiDiaChi, ct);

        var e = new KhDiaChiEntity
        {
            KhachHang_Id = khachHangId,
            DiaChiChiTiet = diaChiChiTiet,
            TinhThanh_Id = tinhThanhId,
            PhuongXa_Id = phuongXaId,
            LoaiDiaChi = loaiDiaChi,
            IsDefault = isDefault
        };
        _ctx.KhDiaChis.Add(e);
        await _ctx.SaveChangesAsync(ct);
        return (await GetByIdAsync(e.Id, ct))!;
    }

    public async Task<AddressDto> UpdateAsync(ulong id, string? diaChiChiTiet, uint? tinhThanhId, uint? phuongXaId, string loaiDiaChi, bool isDefault, CancellationToken ct = default)
    {
        var e = await _ctx.KhDiaChis.FindAsync(new object[] { id }, ct)
            ?? throw new InvalidOperationException($"DiaChi {id} not found.");

        if (isDefault && !e.IsDefault)
            await ClearDefaultAsync(e.KhachHang_Id, loaiDiaChi, ct);

        e.DiaChiChiTiet = diaChiChiTiet;
        e.TinhThanh_Id = tinhThanhId;
        e.PhuongXa_Id = phuongXaId;
        e.LoaiDiaChi = loaiDiaChi;
        e.IsDefault = isDefault;
        await _ctx.SaveChangesAsync(ct);

        return (await GetByIdAsync(id, ct))!;
    }

    public async Task<bool> DeleteAsync(ulong id, CancellationToken ct = default)
    {
        var e = await _ctx.KhDiaChis.FindAsync(new object[] { id }, ct);
        if (e is null) return false;
        _ctx.KhDiaChis.Remove(e);
        await _ctx.SaveChangesAsync(ct);
        return true;
    }

    private async Task ClearDefaultAsync(ulong khachHangId, string loaiDiaChi, CancellationToken ct)
    {
        var existing = await _ctx.KhDiaChis
            .Where(x => x.KhachHang_Id == khachHangId && x.LoaiDiaChi == loaiDiaChi && x.IsDefault)
            .ToListAsync(ct);
        existing.ForEach(x => x.IsDefault = false);
    }
}