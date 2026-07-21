using System.ComponentModel.DataAnnotations;


namespace shiftTrade.Api.Contracts.Auth;


public sealed class LoginEmployeeRequest
{
    [Required]
    [EmailAddress]
    public string EmailAddress {get;init;} = string.Empty;

    [Required]
    public string Password {get;init;} = string.Empty;

}