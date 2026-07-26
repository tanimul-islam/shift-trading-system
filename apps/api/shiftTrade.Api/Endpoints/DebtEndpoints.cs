using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using shiftTrade.Api.Data;
using shiftTrade.Api.Extensions;



namespace shiftTrade.Api.Endpoints;

public static class DebtEndpoints
{
    public static IEndpointRouteBuilder MapDebtEndpoints(
        this IEndpointRouteBuilder app
    )
    {
        var debts = app.MapGroup("/api/debts").WithTags("Debts").RequireAuthorization();

        debts.MapGet("/mine", async(ClaimsPrincipal principal, ApplicationDbContext db)=>{
              
              if (!principal.TryGetCurrentUser(out var organizationId, out var userId))
                {
                    return Results.Unauthorized();
                }   

                var myDebts = await db.HoursDebts
                .Where(debt =>
                    debt.OrganizationId == organizationId &&
                    debt.Status == "Active" &&
                    (debt.CreditorUserId == userId ||
                     debt.DebitorUserId == userId))
                .OrderByDescending(debt => debt.CreateAtUtc)
                .Select(debt => new
                {
                    debt.Id,
                    debt.ShiftId,
                    debt.CreditorUserId,
                    debt.DebitorUserId,
                    debt.HoursOwed,
                    debt.RemainingHours,
                    debt.Status,
                    debt.CreateAtUtc
                })
                .ToListAsync();

                var hoursOwedToYou = myDebts
                .Where(debt => debt.CreditorUserId == userId)
                .Sum(debt=> debt.RemainingHours);

                var HoursYouOwe = myDebts
                .Where(debt => debt.DebitorUserId == userId)
                .Sum(debt=> debt.RemainingHours);

                return Results.Ok(new
                {
                    HoursYouOwe,
                    hoursOwedToYou,
                    activeDebts = myDebts
                });
            
        });
        return app;
    }
}
