namespace CRM.Application.Features.Opportunities.DTOs;

public class OpportunitySummaryDto
{
    public int TotalActive { get; set; }
    public int ThanhCong { get; set; }
    public int ThatBai { get; set; }
    public decimal TotalDoanhThuKyVong { get; set; }
    public decimal DoanhThuThanhCong { get; set; }
    public Dictionary<string, int> CountByStage { get; set; } = new();
    public double TyLeThanhCongTrungBinh { get; set; }
}