namespace shiftTrade.Api.Contracts.Admin;

public class AdminShiftResponse
{
    
    public Guid Id { get; set; }

    public Guid LocationId { get; set; }

    public string PostedByUserId { get; set; } = string.Empty;

    public string? AcceptedByUserId { get; set; }

    public DateTime ScheduleStartUtc { get; set; }

    public DateTime ScheduleEndUtc { get; set; }

    public string Status { get; set; } = string.Empty;

    public DateTime CreatedAtUtc { get; set; }

    public DateTime? AcceptedAtUtc { get; set; }

}