using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using shiftTrade.Api.Contracts.Shifts;
using shiftTrade.Api.Data;
using shiftTrade.api.models;
using shiftTrade.Api.Extensions;

namespace shiftTrade.Api.Endpoints;

public static class ShiftEndpoints
{
    public static IEndpointRouteBuilder MapShiftEndpoints(
        this IEndpointRouteBuilder app)
    {
        var shifts = app.MapGroup("/api/shifts")
            .WithTags("Shifts")
            .RequireAuthorization();

        shifts.MapPost("/", async (
            CreateShiftRequest request,
            ClaimsPrincipal principal,
            ApplicationDbContext db) =>
        {
           if (!principal.TryGetCurrentUser(out var organizationId, out var userId))
{
    return Results.Unauthorized();
}

            if (request.ScheduleEndUtc <= request.ScheduleStartUtc)
            {
                return Results.BadRequest(new
                {
                    message = "The scheduled end time must be after the scheduled start time."
                });
            }

            var locationExists = await db.Locations.AnyAsync(location =>
                location.Id == request.LocationId &&
                location.OrganizationId == organizationId);

            if (!locationExists)
            {
                return Results.BadRequest(new
                {
                    message = "The selected location does not belong to your organization."
                });
            }

            var shift = new Shifts
            {
                OrganizationId = organizationId,
                LocationId = request.LocationId,
                PostedByUserId = userId,
                ScheduleStartUtc = request.ScheduleStartUtc,
                ScheduleEndUtc = request.ScheduleEndUtc,
                Status = "Open"
            };

            db.Shifts.Add(shift);
            await db.SaveChangesAsync();

            return Results.Created($"/api/shifts/{shift.Id}", new
            {
                shift.Id,
                shift.LocationId,
                shift.ScheduleStartUtc,
                shift.ScheduleEndUtc,
                shift.Status,
                shift.CreatedAtUtc
            });
        });

    shifts.MapGet("/", async (
    Guid? locationId,
    ClaimsPrincipal principal,
    ApplicationDbContext db) =>
{
    if (!principal.TryGetCurrentUser(out var organizationId, out var userId))
    {
        return Results.Unauthorized();
    }

    if (locationId.HasValue)
    {
        var locationBelongsToOrganization = await db.Locations.AnyAsync(location =>
            location.Id == locationId.Value &&
            location.OrganizationId == organizationId);

        if (!locationBelongsToOrganization)
        {
            return Results.BadRequest(new
            {
                message = "The selected location does not belong to your organization."
            });
        }
    }

    var query = db.Shifts
        .Where(shift =>
            shift.OrganizationId == organizationId &&
            shift.Status == "Open" &&
            shift.PostedByUserId != userId);

    if (locationId.HasValue)
    {
        query = query.Where(shift => shift.LocationId == locationId.Value);
    }

    var openShifts = await query
        .OrderBy(shift => shift.ScheduleStartUtc)
        .Select(shift => new
        {
            shift.Id,
            shift.LocationId,
            shift.PostedByUserId,
            shift.ScheduleStartUtc,
            shift.ScheduleEndUtc,
            shift.Status,
            shift.CreatedAtUtc
        })
        .ToListAsync();

    return Results.Ok(openShifts);
});



//get one shift by id
shifts.MapGet("/{shiftId:guid}",async(
    Guid shiftId,
    ClaimsPrincipal principal,
    ApplicationDbContext db
) =>
{
    if(!principal.TryGetCurrentUser(out var organizationId, out var userId))
    {
        return Results.Unauthorized();
    }

    var shift = await db.Shifts
                    .Where(shift =>
                    shift.Id == shiftId &&
                    shift.OrganizationId == organizationId &&
                    shift.Status == "open" &&
                    shift.PostedByUserId != userId        
                    ).Select(shift => new
                    {
                        shift.Id,
                        shift.LocationId,
                        shift.PostedByUserId,
                        shift.CreatedAtUtc,
                        shift.ScheduleStartUtc,
                        shift.ScheduleEndUtc,
                        shift.Status
                    }).FirstOrDefaultAsync();

                    if(shift is null)
    {
        return Results.NotFound(new
        {
            message ="The open shift was found are no longer avilable"
        });
    }

    return Results.Ok(shift);
});


    shifts.MapPost("/{shiftId:guid}/accept", async (
        Guid shiftId,
        ClaimsPrincipal principal,
        ApplicationDbContext db
    )=>
    {
        if (!principal.TryGetCurrentUser(out var organizationId, out var userId))
            {
                return Results.Unauthorized();
            }

        await using var transaction = await db.Database.BeginTransactionAsync();

        var shift = await db.Shifts.FirstOrDefaultAsync(shift => 
        shift.Id == shiftId &&
        shift.OrganizationId == organizationId);

        if (shift is null)
        {
            return Results.NotFound(new
            {
                message ="Shift Not Found"
            });

        }

        if(shift.Status != "Open")
        {
            return Results.BadRequest(new
            {
                message ="This shift has already been accepted or is no longer open."
            });
        }

        if (shift.PostedByUserId == userId)
        {
            return Results.BadRequest(
                new
                {
                    message = "You cannot accept you own shift!"
                }
            );
        }


        var hoursOwed = decimal.Round((decimal)(shift.ScheduleEndUtc-shift.ScheduleStartUtc).TotalHours,2);

        shift.Status ="Accepted";
        shift.AcceptedByUserId = userId;
        shift.AcceptedAtUtc = DateTime.UtcNow;

        var newDebt = new HoursDebt
        {
            OrganizationId = organizationId,
            ShiftId = shift.Id,
            CreditorUserId = shift.PostedByUserId,
            DebitorUserId = userId,
            HoursOwed = hoursOwed,
            RemainingHours = hoursOwed,
            Status ="Active"
        };

        db.HoursDebts.Add(newDebt);

         // Find older debts in the opposite direction.
         var debtsToRepay = await db.HoursDebts
          .Where(debt =>
            debt.OrganizationId == organizationId &&
            debt.Status == "Active" &&
            debt.CreditorUserId == userId &&
            debt.DebitorUserId == shift.PostedByUserId)
        .OrderBy(debt => debt.CreateAtUtc)
        .ToListAsync();

foreach (var oldDebt in debtsToRepay)
        {
            if(newDebt.RemainingHours <= 0)
            {
                break;
            }

            var hoursApplied =Math.Min(newDebt.RemainingHours,oldDebt.RemainingHours);

            oldDebt.RemainingHours-= hoursApplied;
            newDebt.RemainingHours -= hoursApplied;


            if(oldDebt.RemainingHours == 0)
            {
                oldDebt.Status= "Settled";
            }

            db.DebtSettlements.Add(new DebtSettlement
            {
                OrganizationId =organizationId,
                SourceDebtId = newDebt.Id,
                TargetedDebtId = oldDebt.Id,
                HoursApplied= hoursApplied
            });


            if (newDebt.RemainingHours == 0){
                newDebt.Status ="Settled";
            }

        }

        await db.SaveChangesAsync();
        await transaction.CommitAsync();


        return Results.Ok(new
        {
            message="Shift Accepted Successfully",
            shiftId=shift.Id,
            shiftStatus = shift.Status,
            shiftAccepted = shift.AcceptedByUserId,
            debtId= newDebt.Id,
            Creditor = newDebt.CreditorUserId,
            debitor = newDebt.DebitorUserId,
            remainingHours = newDebt.RemainingHours,
            owed= newDebt.HoursOwed,
            debtStatus = newDebt.Status

        });
    });



    shifts.MapDelete("/{shiftId:guid}",async(
        Guid shiftId,
        ClaimsPrincipal principal,
        ApplicationDbContext db
    ) =>
    {
        if(!principal.TryGetCurrentUser(out var organizationId, out var userId)){
            return Results.Unauthorized();
        };
        var shift = await db.Shifts.FirstOrDefaultAsync(
            shift => shift.Id == shiftId &&
            shift.OrganizationId == organizationId
        );

        if (shift is null)
        {
            return Results.NotFound(new
            {
                message =" No Shifts Found!"
            });
        }

        if(shift.PostedByUserId != userId)
        {
            return Results.Forbid();
        }

        if(shift.Status != "open")
        {
            return Results.BadRequest(new
            {
                message ="Only an Open shift can be cancelled"
            });
        }

        shift.Status = "Cancelled";

        await db.SaveChangesAsync();

        return Results.Ok(new
        {
            message =" Shift Cancelled Successfully",
            shift.Id,
            shift.Status
        });





    });




        return app;
    }
}