using shiftTrade.api.models;

namespace shiftTrade.api.models;


public class Location
{
    public Guid Id {get;set;} = Guid.NewGuid();
    public string Name {get;set;} = string.Empty;
    public Guid OrganizationId {get;set;}
    public DateTime CreatedAtUtc {get;set;} = DateTime.UtcNow;
    public Organization Organization {get;set;}= null!;
}