

using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using shiftTrade.Api.Data;
using shiftTrade.Api.Extensions;

namespace shiftTrade.Api.Endpoints;

public static class DashboardEndpoints
{
    public static IEndpointRouteBuilder MapDashboardEndpoints(
        this IEndpointRouteBuilder app
    )
    {
        var dashboard = app.MapGroup("/api/dashboard").WithTags("Dashboard").RequireAuthorization();

        dashboard.MapGet("/", async (
            ClaimsPrincipal principal,
            ApplicationDbContext db
        ) =>
        {
            if(!principal.TryGetCurrentUser(out var organizationId, out var userId)){
                return Results.Unauthorized();
        }

        var activeDebts = await db.HoursDebts.AsNoTracking().Where( debt =>
        debt.OrganizationId == organizationId &&
        debt.Status == "Open" &&
        debt.RemainingHours > 0 &&
        (debt.CreditorUserId == userId || debt.DebitorUserId == userId)
        ).Select(
            debt => new
            {
                debt.CreditorUserId,
                debt.DebitorUserId,
                debt.RemainingHours
            }
        ).ToListAsync();

        var myOpenShifts = await db.Shifts.AsNoTracking().
        CountAsync (
            shift =>
            shift.OrganizationId == organizationId &&
            shift.PostedByUserId == userId &&
            shift.Status == "Open"
        );
    var availableShifts = await db.Shifts.AsNoTracking()
                            .Where(shift => 
                            shift.OrganizationId == organizationId &&
                            shift.Status == "Open" &&
                            shift.PostedByUserId != userId
                            ).OrderBy(shift => shift.ScheduleStartUtc)
                            .Take(5)
                            .Select(shift => new
                            {
                                shift.Id,
                                shift.LocationId,
                                shift.PostedByUserId,
                                shift.ScheduleEndUtc,
                                shift.ScheduleStartUtc
                            }).ToListAsync();


            var hoursYouOwe = activeDebts
            .Where(debt => 
            debt.DebitorUserId == userId)
            .Sum(debt=> debt.RemainingHours);


             var hoursOwedToYou = activeDebts
                .Where(debt => debt.CreditorUserId == userId)
                .Sum(debt => debt.RemainingHours);

            return Results.Ok(new
            {
                hoursYouOwe,
                hoursOwedToYou,
                activeDebtCount = activeDebts.Count,
                myOpenShifts,
                availableShiftCount = availableShifts.Count,
                upcomingAvailableShifts = availableShifts
            });

        });

return app;


    }
}