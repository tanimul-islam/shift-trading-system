using System.ComponentModel.DataAnnotations;

namespace shiftTrade.Api.Contracts.Auth;


public sealed class RegisterRequest
{
    [Required]
    [StringLength(100, MinimumLength =2)]
    public string DisplayName{get;init;} = String.Empty;


    [Required]
    [EmailAddress]
    public string EmailAddress {get;init;} = String.Empty;

    [Required]
    [StringLength(100,MinimumLength =8)]
    public string Password{get;init;} = String.Empty;
}