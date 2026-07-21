using System.Security.Claims;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using shiftTrade.Api.Contracts.Auth;
using shiftTrade.Api.Data;
using shiftTrade.Api.Services.Auth;
using shiftTrade.api.models;

namespace shiftTrade.Api.Endpoints;

public static class AuthEndpoints
{
    public static IEndpointRouteBuilder MapAuthEndpoints(this IEndpointRouteBuilder app)
    {
        var auth = app.MapGroup("/api/auth")
            .WithTags("Authentication");

        auth.MapPost("/register", async (
            RegisterOrganizationRequest request,
            UserManager<ApplicationUser> userManager,
            ApplicationDbContext db) =>
        {
            await using var transaction = await db.Database.BeginTransactionAsync();

            var user = new ApplicationUser
            {
                UserName = request.EmailAddress,
                Email = request.EmailAddress,
                displayName = request.DisplayName
            };

            var result = await userManager.CreateAsync(user, request.Password);

            if (!result.Succeeded)
            {
                var errors = result.Errors
                    .GroupBy(error => error.Code)
                    .ToDictionary(
                        group => group.Key,
                        group => group.Select(error => error.Description).ToArray());

                return Results.ValidationProblem(errors);
            }

            var organization = new Organization
            {
                Name = request.OrganizationName
            };

            var location = new Location
            {
                Name = request.LocationName,
                OrganizationId = organization.Id
            };

            var membership = new OrganizationMembership
            {
                userId = user.Id,
                OrganizationId = organization.Id,
                Role = "Owner"
            };

            db.Organizations.Add(organization);
            db.Locations.Add(location);
            db.OrganizationMemberships.Add(membership);

            await db.SaveChangesAsync();
            await transaction.CommitAsync();

            return Results.Created($"/api/organizations/{organization.Id}", new
            {
                organization.Id,
                organization.Name,
                location = new
                {
                    location.Id,
                    location.Name
                },
                owner = new
                {
                    user.Id,
                    user.displayName,
                    user.Email
                }
            });
        })
        .AllowAnonymous();

        auth.MapPost("/login", async (
            LoginEmployeeRequest request,
            UserManager<ApplicationUser> userManager,
            ApplicationDbContext db,
            JwtTokenService jwtTokenService) =>
        {
            var user = await userManager.FindByEmailAsync(request.EmailAddress);

            if (user is null)
            {
                return Results.Unauthorized();
            }

            var passwordIsValid = await userManager.CheckPasswordAsync(
                user,
                request.Password);

            if (!passwordIsValid)
            {
                return Results.Unauthorized();
            }

            var membership = await db.OrganizationMemberships
                .AsNoTracking()
                .FirstOrDefaultAsync(membership => membership.userId == user.Id);

            if (membership is null)
            {
                return Results.Unauthorized();
            }

            var accessToken = jwtTokenService.CreateToken(user, membership);

            return Results.Ok(new
            {
                accessToken,
                tokenType = "Bearer",
                expiresIn = 7200,
                user = new
                {
                    user.Id,
                    user.displayName,
                    user.Email
                },
                organization = new
                {
                    membership.OrganizationId,
                    membership.Role
                }
            });
        })
        .AllowAnonymous();

        auth.MapGet("/me", (HttpContext httpContext) =>
        {
            var user = httpContext.User;

            return Results.Ok(new
            {
                userId = user.FindFirst(ClaimTypes.NameIdentifier)?.Value,
                email = user.FindFirst(ClaimTypes.Email)?.Value,
                displayName = user.FindFirst("display_name")?.Value,
                organizationId = user.FindFirst("organization_id")?.Value,
                organizationRole = user.FindFirst("organization_role")?.Value
            });
        })
        .RequireAuthorization();

        return app;
    }
}