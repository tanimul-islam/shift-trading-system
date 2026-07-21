using System.ComponentModel.DataAnnotations;

namespace shiftTrade.Api.Contracts.Employees;

public class CreateEmployeeRequest
{
    [Required]
    public string DisplayName {get;set;} = string.Empty;

    [Required]
    [EmailAddress]
    public string EmailAddress {get;set;} =string.Empty;
    [Required]
    public string password {get;set;}=string.Empty;

}