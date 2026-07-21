using System.Security.Claims;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using shiftTrade.Api.Contracts.Employees;
using shiftTrade.Api.Data;
using shiftTrade.api.models;


namespace shiftTrade.Api.Endpoints;


public static class EmployeeEndpoints
{
    
    public static IEndpointRouteBuilder MapEmployeeEndpoints(
        this IEndpointRouteBuilder app
    )
    {
        var  employees = app.MapGroup("api/employees").WithTags("Employees").RequireAuthorization();

        employees.MapPost("/", async (CreateEmployeeRequest request,
        ClaimsPrincipal principal,
        UserManager<ApplicationUser> userManager,
        ApplicationDbContext db
        ) =>
        {
            var organizationIdValue  = principal.FindFirst("organization_id")?.Value;

            var organizationRole = principal.FindFirst("organization_role")?.Value;

            if(!string.Equals(
                organizationRole,"Owner",
                StringComparison.OrdinalIgnoreCase
            ))
            {
                return Results.Forbid();
            }

            if (!Guid.TryParse(organizationIdValue, out var organizationId))
            {
                return Results.Unauthorized();
            }

        await using var transaction = await db.Database.BeginTransactionAsync();

        var employee = new ApplicationUser
        {
            UserName = request.EmailAddress,
            Email = request.EmailAddress,
            displayName = request.DisplayName
        };

        var result = await userManager.CreateAsync(employee, request.password);

            if (!result.Succeeded)
            {
                var errors = result.Errors.GroupBy(error => error.Code)
                .ToDictionary(
                    group => group.Key,
                    group => group.Select(error => error.Description).ToArray()
                );
                return Results.ValidationProblem(errors);
            }

            var membership = new OrganizationMembership
            {
                userId = employee.Id,
                OrganizationId = organizationId,
                Role ="Employee"

            };

            db.OrganizationMemberships.Add(membership);
            await db.SaveChangesAsync();
            await transaction.CommitAsync();

            return Results.Created($"api/employees/{employee.Id}", new
            {
                employee.Id,
                employee.displayName,
                employee.Email,
                    membership.Role
            });
        });
        return app;
    }
}