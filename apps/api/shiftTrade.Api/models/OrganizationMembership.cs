using System.Runtime.CompilerServices;

namespace shiftTrade.api.models;

public class OrganizationMembership
{
    public Guid Id {get;set;} = Guid.NewGuid();

    public string userId {get;set;} = string.Empty;

    public Guid OrganizationId {get;set;}
    public String Role {get;set;} = "Employee";
    public DateTime CreatedAtUtc {get;set;}= DateTime.UtcNow;
    public ApplicationUser User {get;set;} = null!;
    public Organization Organization {get;set;} = null!; 
}