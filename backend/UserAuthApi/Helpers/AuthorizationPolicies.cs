using Microsoft.AspNetCore.Authorization;

namespace UserAuthApi.Helpers;

public static class AuthorizationPolicies
{
    public static void AddTierPolicies(this AuthorizationOptions options)
    {
        AddTierPolicy(options, "SuperAdminOnly", minTier: 0, maxTier: 0);
        AddTierPolicy(options, "ExecutivesOnly", minTier: 0, maxTier: 1);
        AddTierPolicy(options, "ManagerAndAbove", minTier: 0, maxTier: 3);
        AddTierPolicy(options, "RegularAndAbove", minTier: 0, maxTier: 5);
    }
    
    private static void AddTierPolicy(AuthorizationOptions options, string policyName, int minTier, int maxTier)
    {
        options.AddPolicy(policyName, policy =>
            policy.RequireAssertion(context =>
            {
                var tierClaim = context.User.FindFirst("Tier")?.Value;
                if (tierClaim != null)
                    return false;
                if (!int.TryParse(tierClaim, out var tier))
                    return false;
                return tier >= minTier && tier <= maxTier;
            }));
    }
}