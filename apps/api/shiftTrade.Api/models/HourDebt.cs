namespace shiftTrade.api.models;


public class HoursDebt
{
    public Guid Id {get;set;} = Guid.NewGuid();
    public Guid OrganizationId {get;set;} 
    public Guid ShiftId {get;set;} 
    public String CreditorUserId {get;set;}  = String.Empty;
    public String DebitorUserId {get;set;}  = String.Empty;
    public decimal HoursOwed {get;set;} 
    public string Status {get;set;}  = "Active";
    public DateTime CreateAtUtc {get;set;}  = DateTime.UtcNow;
}