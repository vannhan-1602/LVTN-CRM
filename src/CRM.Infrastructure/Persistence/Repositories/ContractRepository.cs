using CRM.Application.Common.Models;
using CRM.Application.Features.Contracts.DTOs;
using CRM.Application.Interfaces.Contracts;
using CRM.Domain.Entities.Sales;
using CRM.Infrastructure.Persistence.Contexts;
using CRM.Infrastructure.Persistence.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;

namespace CRM.Infrastructure.Persistence.Repositories;

public class ContractRepository : IContractRepository
{
    private readonly CrmDbContext _context;
    public ContractRepository(CrmDbContext context) => _context = context;
    private async Task<List<ContractDto>> ExecuteEnrichedQueryAsync(
        string? whereClause, object?[] parameters, int? skip, int? take,
        CancellationToken ct)
    {
        var sql = $"""
            SELECT
                hd.`Id`,
                hd.`MaHopDong`,
                hd.`KhachHang_Id`  AS KhachHangId,
                kh.`TenKhachHang`,
                hd.`BaoGia_Id`     AS BaoGiaId,
                bg.`MaBaoGia`,
                bg.`TongTien`      AS GiaTri,
                hd.`NgayKy`,
                hd.`ThoiHan`,
                hd.`TrangThai`,
                hd.`CreatedAt`,
                hd.`UpdatedAt`
            FROM `HD_HopDong` hd
            LEFT JOIN `KH_KhachHang` kh ON kh.`Id` = hd.`KhachHang_Id`
            LEFT JOIN `HD_BaoGia`    bg ON bg.`Id` = hd.`BaoGia_Id`
            {(string.IsNullOrWhiteSpace(whereClause) ? "" : "WHERE " + whereClause)}
            ORDER BY hd.`Id` DESC
            {(skip.HasValue ? $"LIMIT {take ?? 20} OFFSET {skip}" : "")}
            """;

        var conn = _context.Database.GetDbConnection();
        var openedHere = conn.State != System.Data.ConnectionState.Open;
        if (openedHere)
            await conn.OpenAsync(ct);
        try
        {
            using var cmd = conn.CreateCommand();
            cmd.CommandText = sql;
            cmd.Transaction = _context.Database.CurrentTransaction?.GetDbTransaction();
            foreach (var (p, i) in parameters.Select((p, i) => (p, i)))
            {
                var param = cmd.CreateParameter();
                param.ParameterName = $"@p{i}";
                param.Value = p ?? DBNull.Value;
                cmd.Parameters.Add(param);
            }

            using var reader = await cmd.ExecuteReaderAsync(ct);
            var result = new List<ContractDto>();
            while (await reader.ReadAsync(ct))
            {
                result.Add(new ContractDto
                {
                    Id = (ulong)reader.GetInt64(reader.GetOrdinal("Id")),
                    MaHopDong = reader.GetString(reader.GetOrdinal("MaHopDong")),
                    KhachHangId = (ulong)reader.GetInt64(reader.GetOrdinal("KhachHangId")),
                    TenKhachHang = reader.IsDBNull(reader.GetOrdinal("TenKhachHang")) ? null : reader.GetString(reader.GetOrdinal("TenKhachHang")),
                    BaoGiaId = reader.IsDBNull(reader.GetOrdinal("BaoGiaId")) ? null : (ulong)reader.GetInt64(reader.GetOrdinal("BaoGiaId")),
                    MaBaoGia = reader.IsDBNull(reader.GetOrdinal("MaBaoGia")) ? null : reader.GetString(reader.GetOrdinal("MaBaoGia")),
                    GiaTri = reader.IsDBNull(reader.GetOrdinal("GiaTri")) ? null : reader.GetDecimal(reader.GetOrdinal("GiaTri")),
                    NgayKy = reader.IsDBNull(reader.GetOrdinal("NgayKy")) ? null : DateOnly.FromDateTime(reader.GetDateTime(reader.GetOrdinal("NgayKy"))),
                    ThoiHan = reader.IsDBNull(reader.GetOrdinal("ThoiHan")) ? null : reader.GetInt32(reader.GetOrdinal("ThoiHan")),
                    TrangThai = reader.GetString(reader.GetOrdinal("TrangThai")),
                    CreatedAt = reader.IsDBNull(reader.GetOrdinal("CreatedAt")) ? null : reader.GetDateTime(reader.GetOrdinal("CreatedAt")),
                    UpdatedAt = reader.IsDBNull(reader.GetOrdinal("UpdatedAt")) ? null : reader.GetDateTime(reader.GetOrdinal("UpdatedAt")),
                });
            }
            return result;
        }
        finally
        {
            if (openedHere && conn.State == System.Data.ConnectionState.Open)
                await conn.CloseAsync();
        }
    }

    private async Task<int> ExecuteCountAsync(string? whereClause, object?[] parameters, CancellationToken ct)
    {
        var sql = $"""
            SELECT COUNT(*) FROM `HD_HopDong` hd
            {(string.IsNullOrWhiteSpace(whereClause) ? "" : "WHERE " + whereClause)}
            """;

        var conn = _context.Database.GetDbConnection();
        var openedHere = conn.State != System.Data.ConnectionState.Open;
        if (openedHere)
            await conn.OpenAsync(ct);
        try
        {
            using var cmd = conn.CreateCommand();
            cmd.CommandText = sql;
            cmd.Transaction = _context.Database.CurrentTransaction?.GetDbTransaction();
            foreach (var (p, i) in parameters.Select((p, i) => (p, i)))
            {
                var param = cmd.CreateParameter();
                param.ParameterName = $"@p{i}";
                param.Value = p ?? DBNull.Value;
                cmd.Parameters.Add(param);
            }
            var scalar = await cmd.ExecuteScalarAsync(ct);
            return Convert.ToInt32(scalar);
        }
        finally
        {
            if (openedHere && conn.State == System.Data.ConnectionState.Open)
                await conn.CloseAsync();
        }
    }

    // ── Public interface ──────────────────────────────────────────────────────
    public async Task<HopDong?> GetByIdAsync(ulong id, CancellationToken ct = default)
    {
        var e = await _context.HdHopDongs.FirstOrDefaultAsync(x => x.Id == id, ct);
        return e is null ? null : MapToDomain(e);
    }

    public async Task<ContractDto?> GetByIdEnrichedAsync(ulong id, CancellationToken ct = default)
    {
        var rows = await ExecuteEnrichedQueryAsync("hd.Id = @p0", [id], null, null, ct);
        return rows.FirstOrDefault();
    }

    public async Task<PagedResult<ContractDto>> GetPagedAsync(
        int pageNumber, int pageSize, string? search, string? trangThai,
        ulong? khachHangId, CancellationToken ct = default)
    {
        var conditions = new List<string>();
        var parameters = new List<object?>();
        int pIdx = 0;

        if (!string.IsNullOrWhiteSpace(search))
        {
            conditions.Add($"hd.`MaHopDong` LIKE @p{pIdx}");
            parameters.Add($"%{search.Trim()}%");
            pIdx++;
        }
        if (!string.IsNullOrWhiteSpace(trangThai))
        {
            conditions.Add($"hd.`TrangThai` = @p{pIdx}");
            parameters.Add(trangThai);
            pIdx++;
        }
        if (khachHangId.HasValue)
        {
            conditions.Add($"hd.`KhachHang_Id` = @p{pIdx}");
            parameters.Add(khachHangId.Value);
        }

        var where = conditions.Count > 0 ? string.Join(" AND ", conditions) : null;
        var args = parameters.ToArray();

        var total = await ExecuteCountAsync(where, args, ct);
        var items = await ExecuteEnrichedQueryAsync(where, args, (pageNumber - 1) * pageSize, pageSize, ct);

        return new PagedResult<ContractDto>
        {
            Items = items,
            PageNumber = pageNumber,
            PageSize = pageSize,
            TotalCount = total
        };
    }

    public async Task<HopDong> AddAsync(HopDong contract, CancellationToken ct = default)
    {
        var entity = MapToEntity(contract);
        _context.HdHopDongs.Add(entity);
        await _context.SaveChangesAsync(ct);
        contract.Id = entity.Id;
        return contract;
    }

    public async Task UpdateStatusAsync(ulong id, string trangThai, CancellationToken ct = default)
    {
        var entity = await _context.HdHopDongs.FindAsync([id], ct);
        if (entity is null) return;
        entity.TrangThai = trangThai;
        entity.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync(ct);
    }

    public async Task<bool> DeleteAsync(ulong id, CancellationToken ct = default)
    {
        var entity = await _context.HdHopDongs.FindAsync([id], ct);
        if (entity is null) return false;
        _context.HdHopDongs.Remove(entity);
        return true;
    }

    public async Task<string> GenerateMaHopDongAsync(CancellationToken ct = default)
    {
        var last = await _context.HdHopDongs.OrderByDescending(x => x.Id).FirstOrDefaultAsync(ct);
        var next = (last?.Id ?? 0) + 1;
        return $"HD{next:D5}";
    }

    public Task<bool> ExistsForBaoGiaAsync(ulong baoGiaId, CancellationToken ct = default) =>
        _context.HdHopDongs.AnyAsync(x => x.BaoGiaId == baoGiaId, ct);

    public Task<bool> HasActiveContractAsync(ulong khachHangId, CancellationToken ct = default) =>
        _context.HdHopDongs.AnyAsync(
            x => x.KhachHangId == khachHangId && x.TrangThai == CRM.Domain.Enums.ContractStatus.DangThucHien, ct);

    // ── Mappers ───────────────────────────────────────────────────────────────
    private static HopDong MapToDomain(HdHopDongEntity e) => new()
    {
        Id = e.Id,
        MaHopDong = e.MaHopDong,
        KhachHangId = e.KhachHangId,
        BaoGiaGocId = e.BaoGiaId,
        NgayKy = e.NgayKy,
        ThoiHan = e.ThoiHan,
        TrangThai = e.TrangThai,
        CreatedAt = e.CreatedAt,
        UpdatedAt = e.UpdatedAt
    };

    private static HdHopDongEntity MapToEntity(HopDong d) => new()
    {
        MaHopDong = d.MaHopDong,
        KhachHangId = d.KhachHangId,
        BaoGiaId = d.BaoGiaGocId,
        NgayKy = d.NgayKy,
        ThoiHan = d.ThoiHan,
        TrangThai = d.TrangThai,
        CreatedAt = d.CreatedAt,
        UpdatedAt = d.UpdatedAt
    };
}