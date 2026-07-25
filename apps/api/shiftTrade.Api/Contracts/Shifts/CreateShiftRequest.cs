namespace  shiftTrade.Api.Contracts.Shifts;


public class CreateShiftRequest
{
    public Guid LocationId {get;set;}
    public DateTime ScheduleStartUtc {get;set;}
    public DateTime ScheduleEndUtc {get;set;}
}