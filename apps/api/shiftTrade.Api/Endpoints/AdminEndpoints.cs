using System.Security.Claims;
using Microsoft.AspNetCore.Routing.Tree;
using Microsoft.EntityFrameworkCore;
using shiftTrade.Api.Data;
using shiftTrade.Api.Extensions;
namespace shiftTrade.Api.Endpoints;


public static class AdminEndpoints
{
    public static IEndpointRouteBuilder MapAdminEndpoints(
        this IEndpointRouteBuilder app
    )
    {
        var admin = app.MapGroup("/api/admin").WithTags("Admin").RequireAuthorization();

        app.MapGet("/shifts", async(
            string? status,
            Guid? locationId,
            ClaimsPrincipal principal,
            ApplicationDbContext db
        ) =>
        {
            if(!principal.TryGetCurrentUser(out var organizationId,out var userId)){
                return Results.Unauthorized();
        }

        var organizationRole = principal.FindFirst("organization_role")?.Value;

        if(!string.Equals(
            organizationRole,"Owner",StringComparison.OrdinalIgnoreCase
        ))
            {
                return Results.Forbid();
            }
            if (locationId.HasValue)
            {
                var locationExists = await db.Locations.AnyAsync(location =>
                  location.Id == locationId.Value &&
                    location.OrganizationId == organizationId
                );

                if (!locationExists)
                {
                    return Results.BadRequest(new
                    {
                        message = "The selected location does not belong to your organization."
                    });
                }
            }

            var query = db.Shifts
                .AsNoTracking()
                .Where(shift =>
                    shift.OrganizationId == organizationId);

            if (!string.IsNullOrWhiteSpace(status))
            {
                query = query.Where(shift => 
                shift.Status.ToLower() == status.ToLower());
            }

            if (locationId.HasValue)
            {
                query = query.Where(shift=>
                shift.LocationId ==locationId.Value);
            }

            var shiftList = await query
            .OrderByDescending(shift => shift.CreatedAtUtc)
            .Select (shift  => new
            {
                shift.Id,
                    shift.LocationId,
                    shift.PostedByUserId,
                    shift.AcceptedByUserId,
                    shift.ScheduleStartUtc,
                    shift.ScheduleEndUtc,
                    shift.Status,
                    shift.CreatedAtUtc,
                    shift.AcceptedAtUtc
            }).ToListAsync();

            return Results.Ok(shiftList);

        });


      app.MapGet("/debts", async(
            string? status,
            Guid? locationId,
            ClaimsPrincipal principal,
            ApplicationDbContext db
        ) =>
        {
            if(!principal.TryGetCurrentUser(out var organizationId,out var userId)){
                return Results.Unauthorized();
        }

        var organizationRole = principal.FindFirst("organization_role")?.Value;

        if(!string.Equals(
            organizationRole,"Owner",StringComparison.OrdinalIgnoreCase))
            {
                return Results.Forbid();
            }


            var query = db.HoursDebts
                .AsNoTracking()
                .Where(debt =>
                    debt.OrganizationId == organizationId);

            if (!string.IsNullOrWhiteSpace(status))
            {
                query = query.Where(debt => 
                debt.Status.ToLower() == status.ToLower());
            }

           if (!string.IsNullOrWhiteSpace(userId))
    {
        query = query.Where(debt =>
            debt.CreditorUserId == userId ||
            debt.DebitorUserId == userId);
    }

            var debtList = await query
            .OrderByDescending(debt => debt.CreateAtUtc)
            .Select (debt  => new
            {
                    debt.Id,
                    debt.ShiftId,
                    debt.CreditorUserId,
                    debt.DebitorUserId,
                    debt.HoursOwed,
                    debt.RemainingHours,
                    repaidHours = debt.HoursOwed - debt.RemainingHours,
                    debt.Status,
                    debt.CreateAtUtc
            }).ToListAsync();

            return Results.Ok(debtList);

        });


        return app;
    }

     
}