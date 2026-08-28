using CRM.Application.Features.Activities.Commands.CreateActivity;
using CRM.Application.Features.Opportunities.Commands.CreateOpportunity;
using FluentValidation.TestHelper;
using Xunit;

namespace CRM.Application.Tests.Features.OpportunitiesAndActivities;

/// <summary>
/// Cơ hội bán hàng (Opportunity) và Hoạt động chăm sóc (Activity) đều phải gắn với ĐÚNG MỘT
/// trong hai: Khách hàng HOẶC Lead (không cả hai, không bỏ trống cả hai) — dùng XOR (^), không
/// phải OR (||). CreateActivityCommandValidator từng dùng nhầm OR (đã fix); test này khóa cả 2
/// validator lại cùng 1 bộ case để không lệch nhau lần nữa.
/// </summary>
public class XorValidatorsTests
{
    private static CreateOpportunityCommand OpportunityCmd(ulong? khachHangId, ulong? leadId) =>
        new("Thương vụ test", khachHangId, leadId, 50, null, null, null);

    private static CreateActivityCommand ActivityCmd(ulong? khachHangId, ulong? leadId) =>
        new(khachHangId, leadId, "GoiDien", "Ghi chú test", DateTime.UtcNow);

    [Fact]
    public void Opportunity_ChiChonKhachHang_HopLe()
    {
        var result = new CreateOpportunityCommandValidator().TestValidate(OpportunityCmd(1, null));
        result.ShouldNotHaveValidationErrorFor(x => x);
    }

    [Fact]
    public void Opportunity_ChiChonLead_HopLe()
    {
        var result = new CreateOpportunityCommandValidator().TestValidate(OpportunityCmd(null, 1));
        result.ShouldNotHaveValidationErrorFor(x => x);
    }

    [Fact]
    public void Opportunity_ChonCaHai_BiTuChoi()
    {
        var result = new CreateOpportunityCommandValidator().TestValidate(OpportunityCmd(1, 1));
        result.ShouldHaveValidationErrorFor(x => x);
    }

    [Fact]
    public void Opportunity_BoTrongCaHai_BiTuChoi()
    {
        var result = new CreateOpportunityCommandValidator().TestValidate(OpportunityCmd(null, null));
        result.ShouldHaveValidationErrorFor(x => x);
    }

    [Fact]
    public void Activity_ChiChonKhachHang_HopLe()
    {
        var result = new CreateActivityCommandValidator().TestValidate(ActivityCmd(1, null));
        result.ShouldNotHaveValidationErrorFor(x => x);
    }

    [Fact]
    public void Activity_ChiChonLead_HopLe()
    {
        var result = new CreateActivityCommandValidator().TestValidate(ActivityCmd(null, 1));
        result.ShouldNotHaveValidationErrorFor(x => x);
    }

    [Fact]
    public void Activity_ChonCaHai_BiTuChoi()
    {
        // Trước fix: validator dùng OR nên case này SAI SÓT đi qua được — test này đảm bảo
        // hành vi đúng (XOR) không bị quay lại kiểu cũ.
        var result = new CreateActivityCommandValidator().TestValidate(ActivityCmd(1, 1));
        result.ShouldHaveValidationErrorFor(x => x);
    }

    [Fact]
    public void Activity_BoTrongCaHai_BiTuChoi()
    {
        var result = new CreateActivityCommandValidator().TestValidate(ActivityCmd(null, null));
        result.ShouldHaveValidationErrorFor(x => x);
    }
}
