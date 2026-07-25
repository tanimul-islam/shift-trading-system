using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using shiftTrade.Api.Contracts.Shifts;
using shiftTrade.Api.Data;
using shiftTrade.api.models;

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
            var organizationIdValue = principal.FindFirst("organization_id")?.Value;
            var userId = principal.FindFirstValue(ClaimTypes.NameIdentifier)
                ?? principal.FindFirst("sub")?.Value;

            if (!Guid.TryParse(organizationIdValue, out var organizationId)
                || string.IsNullOrWhiteSpace(userId))
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


        return app;
    }
}