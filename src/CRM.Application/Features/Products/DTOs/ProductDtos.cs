namespace CRM.Application.Features.Products.DTOs;

public class ProductDto
{
    public uint Id { get; set; }
    public uint? LoaiSanPhamId { get; set; }
    public string? TenLoai { get; set; }
    public string MaSP { get; set; } = string.Empty;
    public string TenSP { get; set; } = string.Empty;
    public string? DonVi { get; set; }
    public decimal? GiaBan { get; set; }
    public int SoLuongTon { get; set; }
    public bool DangKinhDoanh { get; set; }
    public string? AnhDaiDien { get; set; }
    public DateTime? CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}

public class ProductTypeDto
{
    public uint Id { get; set; }
    public string TenLoai { get; set; } = string.Empty;
    public string? MoTa { get; set; }
}

public class ProductImageDto
{
    public ulong Id { get; set; }
    public uint SanPhamId { get; set; }
    public string UrlHinhAnh { get; set; } = string.Empty;
    public bool IsMain { get; set; }
}

public class CreateProductRequestDto
{
    public uint? LoaiSanPhamId { get; set; }
    public string MaSP { get; set; } = string.Empty;
    public string TenSP { get; set; } = string.Empty;
    public string? DonVi { get; set; }
    public decimal GiaBan { get; set; }

    //Số lượng tồn ban đầu khi tạo mới (sẽ tự sinh 1 phiếu nhập kho
    //loại NhapMua nếu > 0, để có lịch sử kho ngay từ đầu)
    public int SoLuongTonBanDau { get; set; }
}

public class UpdateProductRequestDto
{
    public uint? LoaiSanPhamId { get; set; }
    public string TenSP { get; set; } = string.Empty;
    public string? DonVi { get; set; }
    public decimal GiaBan { get; set; }
    public bool DangKinhDoanh { get; set; }
}

public class AdjustStockRequestDto
{
    public string LoaiGiaoDich { get; set; } = string.Empty;
    public int SoLuong { get; set; } // luôn nhập số dương, dấu +/- do LoaiGiaoDich quyết định
    public string? MaChungTu { get; set; }
    public string? GhiChu { get; set; }
}

public class StockTransactionDto
{
    public ulong Id { get; set; }
    public uint SanPhamId { get; set; }
    public string? MaChungTu { get; set; }
    public string LoaiGiaoDich { get; set; } = string.Empty;
    public int SoLuongThayDoi { get; set; }
    public int TonCuoi { get; set; }
    public DateTime NgayGiaoDich { get; set; }
    public uint? NguoiThucHienId { get; set; }
    public string? TenNguoiThucHien { get; set; }
    public string? GhiChu { get; set; }
}

public class StockTransactionResultDto
{
    public int TonTruoc { get; set; }
    public int TonSau { get; set; }
    public StockTransactionDto Transaction { get; set; } = null!;
}