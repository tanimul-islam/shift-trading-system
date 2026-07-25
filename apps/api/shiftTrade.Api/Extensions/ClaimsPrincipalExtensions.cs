using System.Security.Claims;

namespace shiftTrade.Api.Extensions;

public static class ClaimsPrincipalExtensions
{
    public static bool TryGetCurrentUser(
        this ClaimsPrincipal principal,
        out Guid organizationId,
        out string userId)
    {
        var organizationIdValue =
            principal.FindFirst("organization_id")?.Value;

        userId = principal.FindFirstValue(ClaimTypes.NameIdentifier)
            ?? principal.FindFirst("sub")?.Value
            ?? string.Empty;

        return Guid.TryParse(organizationIdValue, out organizationId)
            && !string.IsNullOrWhiteSpace(userId);
    }
}