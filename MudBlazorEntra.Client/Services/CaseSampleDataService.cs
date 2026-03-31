using MudBlazorEntra.Client.Models.Cases;
using MudBlazorEntra.Client.Models.Authentication;
using MudBlazorEntra.Client.Models.Policies;
using MudBlazorEntra.Client.Models.Users;

namespace MudBlazorEntra.Client.Services;

public static class CaseSampleDataService
{
    public static IReadOnlyList<CaseListItem> Cases { get; } =
    [
        new() { YourReference = "ENQ-24001", CaseId = "C-100245", Address = "18 Westbridge Road, Bristol", Status = "Draft", IsMine = true, LastUpdatedDate = DateTime.Today.AddDays(-1), CreatedDate = DateTime.Today.AddDays(-4) },
        new() { YourReference = "ENQ-24002", CaseId = "C-100246", Address = "4 Station View, Leeds", Status = "Issued", IsMine = true, LastUpdatedDate = DateTime.Today.AddDays(-2), CreatedDate = DateTime.Today.AddDays(-11) },
        new() { YourReference = "ENQ-24003", CaseId = "C-100247", Address = "77 Market Street, York", Status = "Cancelled", IsMine = false, LastUpdatedDate = DateTime.Today.AddDays(-9), CreatedDate = DateTime.Today.AddDays(-12) },
        new() { YourReference = "ENQ-24004", CaseId = "C-100248", Address = "1 Harbour Lane, Southampton", Status = "Issued", IsMine = false, LastUpdatedDate = DateTime.Today.AddDays(-6), CreatedDate = DateTime.Today.AddDays(-18) },
        new() { YourReference = "ENQ-24005", CaseId = "C-100249", Address = "15 Elm Court, Nottingham", Status = "Draft", IsMine = true, LastUpdatedDate = DateTime.Today.AddDays(-3), CreatedDate = DateTime.Today.AddDays(-3) },
        new() { YourReference = "ENQ-24006", CaseId = "C-100250", Address = "9 Willow Close, Reading", Status = "Issued", IsMine = false, LastUpdatedDate = DateTime.Today.AddDays(-14), CreatedDate = DateTime.Today.AddDays(-32) },
        new() { YourReference = "ENQ-24007", CaseId = "C-100251", Address = "42 Queens Avenue, Manchester", Status = "Draft", IsMine = true, LastUpdatedDate = DateTime.Today.AddDays(-5), CreatedDate = DateTime.Today.AddDays(-7) },
        new() { YourReference = "ENQ-24008", CaseId = "C-100252", Address = "3 Highfield Park, Exeter", Status = "Cancelled", IsMine = true, LastUpdatedDate = DateTime.Today.AddDays(-8), CreatedDate = DateTime.Today.AddDays(-21) },
        new() { YourReference = "ENQ-24009", CaseId = "C-100253", Address = "28 Riverbank Way, Cambridge", Status = "Issued", IsMine = true, LastUpdatedDate = DateTime.Today.AddDays(-15), CreatedDate = DateTime.Today.AddDays(-40) },
        new() { YourReference = "ENQ-24010", CaseId = "C-100254", Address = "60 Cedar Rise, Birmingham", Status = "Draft", IsMine = false, LastUpdatedDate = DateTime.Today.AddDays(-20), CreatedDate = DateTime.Today.AddDays(-65) },
        new() { YourReference = "ENQ-24011", CaseId = "C-100255", Address = "6 Victoria Square, Sheffield", Status = "Issued", IsMine = true, LastUpdatedDate = DateTime.Today.AddDays(-27), CreatedDate = DateTime.Today.AddDays(-85) },
        new() { YourReference = "ENQ-24012", CaseId = "C-100256", Address = "12 Meadow Bank, Norwich", Status = "Cancelled", IsMine = false, LastUpdatedDate = DateTime.Today.AddDays(-33), CreatedDate = DateTime.Today.AddDays(-120) }
    ];

    public static CaseListItem? FindByCaseId(string caseId)
    {
        return Cases.FirstOrDefault(x => string.Equals(x.CaseId, caseId, StringComparison.OrdinalIgnoreCase));
    }

    public static PolicyDetailsResponse? GetFallbackPolicyDetails(string policyId)
    {
        var policy = FindByCaseId(policyId);
        if (policy is null)
        {
            return null;
        }

        return new PolicyDetailsResponse
        {
            PolicyId = policy.CaseId,
            YourReference = policy.YourReference,
            Address = policy.Address,
            Status = policy.Status,
            IsMine = policy.IsMine,
            LastUpdatedDate = policy.LastUpdatedDate,
            CreatedDate = policy.CreatedDate,
            OfficeId = policy.IsMine ? "OF-001" : "OF-002",
            OfficeName = policy.IsMine ? "Bristol Office" : "Leeds Office",
            Panels = policy.IsMine ? ["Retail", "Broker"] : ["Commercial"]
        };
    }

    public static UserDetailsResponse GetFallbackUserDetails(CurrentUserInfoResponse? currentUser)
    {
        return new UserDetailsResponse
        {
            UserId = currentUser?.UserId ?? "sample-user-id",
            DisplayName = string.IsNullOrWhiteSpace(currentUser?.DisplayName) ? "Sample User" : currentUser.DisplayName,
            Email = currentUser?.Email ?? string.Empty,
            IntroducerId = "INT-0001",
            OfficeId = "OF-001",
            OfficeName = "Bristol Office",
            Panels = ["Retail", "Broker", "Renewals"]
        };
    }
}
