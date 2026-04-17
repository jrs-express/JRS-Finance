using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Server;

namespace JrsExpressAccounting.Web.Infrastructure;

public class IdentityRevalidatingAuthenticationStateProvider(ILoggerFactory loggerFactory)
    : RevalidatingServerAuthenticationStateProvider(loggerFactory)
{
    protected override TimeSpan RevalidationInterval => TimeSpan.FromMinutes(30);

    protected override Task<bool> ValidateAuthenticationStateAsync(AuthenticationState authenticationState, CancellationToken cancellationToken)
    {
        return Task.FromResult(authenticationState.User.Identity?.IsAuthenticated == true);
    }
}
