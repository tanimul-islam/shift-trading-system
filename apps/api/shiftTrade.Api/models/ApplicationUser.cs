using Microsoft.AspNetCore.Identity;

namespace shiftTrade.api.models;

public class ApplicationUser:IdentityUser
{
    public string displayName{get;set;} = String.Empty;
}