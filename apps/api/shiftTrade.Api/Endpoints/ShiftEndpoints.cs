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
            Guid locationId,
            ClaimsPrincipal principal,
            ApplicationDbContext db
        ) =>
        {
            var organizationIdValue = principal.FindFirst("organization_id")?.Value;
            if(!Guid.TryParse(organizationIdValue, out var organizationId))
            {
                return Results.Unauthorized();
            }
            
            var locationExists = await db.Locations.AnyAsync(location =>
            location.Id == locationId &&
            location.OrganizationId == organizationId);

            if (!locationExists)
            {
                 return Results.BadRequest(new
                {
                    message = "The selected location does not belong to your organization."
                });
            }
var openShifts = await db.Shifts
        .Where(shift =>
            shift.OrganizationId == organizationId &&
            shift.LocationId == locationId &&
            shift.Status == "Open")
        .OrderBy(shift => shift.ScheduleStartUtc)
        .Select(shift => new
        {
            shift.Id,
            shift.PostedByUserId,
            shift.ScheduleStartUtc,
            shift.ScheduleEndUtc,
            shift.Status,
            shift.CreatedAtUtc
        })
        .ToListAsync();

        return Results.Ok(openShifts);


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


        var hoursOwed = (decimal)(shift.ScheduleEndUtc-shift.ScheduleStartUtc).TotalHours;

        shift.Status ="Accepted";
        shift.AcceptedByUserId = userId;
        shift.AcceptedAtUtc = DateTime.UtcNow;

        var debt = new HoursDebt
        {
            OrganizationId = organizationId,
            ShiftId = shift.Id,
            CreditorUserId = shift.PostedByUserId,
            DebitorUserId = userId,
            HoursOwed = hoursOwed,
            Status ="Active"
        };

        db.HoursDebts.Add(debt);

        await db.SaveChangesAsync();
        await transaction.CommitAsync();


        return Results.Ok(new
        {
            message="Shift Accepted Successfully",
            shiftId=shift.Id,
            shiftStatus = shift.Status,
            shiftAccepted = shift.AcceptedByUserId,
            debtId= debt.Id,
            Creditor = debt.CreditorUserId,
            debitor = debt.DebitorUserId,
            owed= debt.HoursOwed,
            debtStatus = debt.Status

        });
    });




        return app;
    }
}