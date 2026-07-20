using System.ComponentModel.DataAnnotations;

namespace shiftTrade.Api.Contracts.Auth;

public sealed class RegisterOrganizationRequest
{
[Required]
[StringLength(150, MinimumLength =2)]
public string OrganizationName {get;init;} = string.Empty;

[Required]
[StringLength(150, MinimumLength =2)]
public string LocationName {get;init;} = string.Empty;

[Required]
[StringLength(100, MinimumLength =2)]
public string DisplayName {get;init;} = string.Empty;

[Required]
[EmailAddress]
public string EmailAddress {get;init;} = string.Empty;

[Required]
[StringLength(100, MinimumLength =8)]
public string Password {get;init;} = string.Empty;
}