namespace shiftTrade.api.models;


public class Shifts
{
    public Guid Id {get;set;} = Guid.NewGuid();
    public Guid OrganizationId {get;set;} 
    public Guid LocationId {get;set;}
    public string PostedByUserId{get;set;} = String.Empty;

    public DateTime ScheduleStartUtc {get;set;} 
    public DateTime ScheduleEndUtc {get;set;}
    public string Staus {get;set;}  ="Open";
    public string? AcceptedByUserId {get;set;} 
    public DateTime CreatedAtUtc {get;set;} = DateTime.UtcNow;
    public DateTime? AcceptedAtUtc { get; set; }
    }