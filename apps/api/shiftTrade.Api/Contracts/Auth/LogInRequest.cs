using System.ComponentModel.DataAnnotations;


namespace shiftTrade.Api.Contracts.Auth;


public sealed class LogInRequest
{
    [Required]
    [EmailAddress]
    public string EmailAddress {get;init;} = string.Empty;

    [Required]
    public string Password {get;init;} = string.Empty;

}