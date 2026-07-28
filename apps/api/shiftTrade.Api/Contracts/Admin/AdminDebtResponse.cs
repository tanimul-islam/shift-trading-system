namespace shiftTrade.Api.Contracts.Admin;

public class AdminDebtResponse
{
    
    public Guid Id { get; set; }

    public Guid ShiftId { get; set; }

    public string CreditorUserId { get; set; } = string.Empty;

    public string DebitorUserId { get; set; } = string.Empty;

    public decimal HoursOwed { get; set; }

    public decimal RemainingHours { get; set; }

    public decimal RepaidHours {get;set;}

    public string Status { get; set; } = string.Empty;

    public DateTime CreateAtUtc { get; set; }


}