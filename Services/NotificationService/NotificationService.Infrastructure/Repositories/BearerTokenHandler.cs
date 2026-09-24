using System.Net.Http.Headers;
using BuildingBlocks.Security;

namespace NotificationService.Infrastructure.Repositories;

public class BearerTokenHandler : DelegatingHandler
{
    private readonly IAuthorizeExtension _authorizeExtension;

    public BearerTokenHandler(IAuthorizeExtension authorizeExtension)
    {
        _authorizeExtension = authorizeExtension;
    }

    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        var token = _authorizeExtension.GetToken();
        if (!string.IsNullOrEmpty(token))
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        return await base.SendAsync(request, cancellationToken);
    }
}
