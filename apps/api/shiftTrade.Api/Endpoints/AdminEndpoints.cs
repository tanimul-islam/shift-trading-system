using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using shiftTrade.Api.Data;
using shiftTrade.Api.Extensions;

namespace shiftTrade.Api.Endpoints;

public static class AdminEndpoints
{
    public static IEndpointRouteBuilder MapAdminEndpoints(
        this IEndpointRouteBuilder app)
    {
        var admin = app.MapGroup("/api/admin")
            .WithTags("Admin")
            .RequireAuthorization();

        admin.MapGet("/shifts", async (
            ClaimsPrincipal principal,
            ApplicationDbContext db,
            string? status,
            Guid? locationId,
            int page = 1,
            int pageSize = 20) =>
        {
            if (!principal.TryGetCurrentUser(
                    out var organizationId,
                    out _))
            {
                return Results.Unauthorized();
            }

            var organizationRole =
                principal.FindFirst("organization_role")?.Value;

            if (!string.Equals(
                    organizationRole,
                    "Owner",
                    StringComparison.OrdinalIgnoreCase))
            {
                return Results.Forbid();
            }

            page = Math.Max(page, 1);
            pageSize = Math.Clamp(pageSize, 1, 100);

            if (locationId.HasValue)
            {
                var locationExists = await db.Locations
                    .AnyAsync(location =>
                        location.Id == locationId.Value &&
                        location.OrganizationId == organizationId);

                if (!locationExists)
                {
                    return Results.BadRequest(new
                    {
                        message =
                            "The selected location does not belong to your organization."
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
                query = query.Where(shift =>
                    shift.LocationId == locationId.Value);
            }

            var totalCount = await query.CountAsync();

            var shiftList = await query
                .OrderByDescending(shift => shift.CreatedAtUtc)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(shift => new
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
                })
                .ToListAsync();

            return Results.Ok(new
            {
                page,
                pageSize,
                totalCount,
                totalPages = (int)Math.Ceiling(
                    totalCount / (double)pageSize),
                items = shiftList
            });
        });

        admin.MapGet("/debts", async (
            ClaimsPrincipal principal,
            ApplicationDbContext db,
            string? status,
            string? userId,
            int page = 1,
            int pageSize = 20) =>
        {
            if (!principal.TryGetCurrentUser(
                    out var organizationId,
                    out _))
            {
                return Results.Unauthorized();
            }

            var organizationRole =
                principal.FindFirst("organization_role")?.Value;

            if (!string.Equals(
                    organizationRole,
                    "Owner",
                    StringComparison.OrdinalIgnoreCase))
            {
                return Results.Forbid();
            }

            page = Math.Max(page, 1);
            pageSize = Math.Clamp(pageSize, 1, 100);

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

            var totalCount = await query.CountAsync();

            var debtList = await query
                .OrderByDescending(debt => debt.CreateAtUtc)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(debt => new
                {
                    debt.Id,
                    debt.ShiftId,
                    debt.CreditorUserId,
                    debt.DebitorUserId,
                    debt.HoursOwed,
                    debt.RemainingHours,
                    repaidHours =
                        debt.HoursOwed - debt.RemainingHours,
                    debt.Status,
                    debt.CreateAtUtc
                })
                .ToListAsync();

            return Results.Ok(new
            {
                page,
                pageSize,
                totalCount,
                totalPages = (int)Math.Ceiling(
                    totalCount / (double)pageSize),
                items = debtList
            });
        });

        admin.MapGet("/employees/{employeeId}/summary", async (
            string employeeId,
            ClaimsPrincipal principal,
            ApplicationDbContext db) =>
        {
            if (!principal.TryGetCurrentUser(
                    out var organizationId,
                    out _))
            {
                return Results.Unauthorized();
            }

            var organizationRole =
                principal.FindFirst("organization_role")?.Value;

            if (!string.Equals(
                    organizationRole,
                    "Owner",
                    StringComparison.OrdinalIgnoreCase))
            {
                return Results.Forbid();
            }

            var employee = await db.OrganizationMemberships
                .AsNoTracking()
                .Where(membership =>
                    membership.OrganizationId == organizationId &&
                    membership.userId == employeeId)
                .Select(membership => new
                {
                    membership.userId,
                    membership.Role
                })
                .FirstOrDefaultAsync();

            if (employee is null)
            {
                return Results.NotFound(new
                {
                    message =
                        "Employee not found in this organization."
                });
            }

            var postedShiftCount = await db.Shifts
                .AsNoTracking()
                .CountAsync(shift =>
                    shift.OrganizationId == organizationId &&
                    shift.PostedByUserId == employeeId);

            var acceptedShiftsCount = await db.Shifts
                .AsNoTracking()
                .CountAsync(shift =>
                    shift.OrganizationId == organizationId &&
                    shift.AcceptedByUserId == employeeId &&
                    shift.Status == "Accepted");

            var openShiftCount = await db.Shifts
                .AsNoTracking()
                .CountAsync(shift =>
                    shift.OrganizationId == organizationId &&
                    shift.PostedByUserId == employeeId &&
                    shift.Status == "Open");

            var activeDebts = await db.HoursDebts
                .AsNoTracking()
                .Where(debt =>
                    debt.OrganizationId == organizationId &&
                    debt.Status == "Active" &&
                    debt.RemainingHours > 0 &&
                    (debt.CreditorUserId == employeeId ||
                     debt.DebitorUserId == employeeId))
                .Select(debt => new
                {
                    debt.CreditorUserId,
                    debt.DebitorUserId,
                    debt.RemainingHours
                })
                .ToListAsync();

            var hoursEmployeeOwes = activeDebts
                .Where(debt =>
                    debt.DebitorUserId == employeeId)
                .Sum(debt =>
                    debt.RemainingHours);

            var hoursOwedToEmployee = activeDebts
                .Where(debt =>
                    debt.CreditorUserId == employeeId)
                .Sum(debt =>
                    debt.RemainingHours);

            return Results.Ok(new
            {
                employee.userId,
                employee.Role,
                postedShiftCount,
                acceptedShiftsCount,
                openShiftCount,
                hoursEmployeeOwes,
                hoursOwedToEmployee,
                activeDebtCount = activeDebts.Count
            });
        });

        return app;
    }
}