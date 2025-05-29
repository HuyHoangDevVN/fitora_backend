using System.Net.Http.Headers;
using BuildingBlocks.Security;

namespace InteractService.Infrastructure.Repositories;

public class BearerTokenHandler : DelegatingHandler
{
    private readonly IAuthorizeExtension _authorizeExtension;

    public BearerTokenHandler(IAuthorizeExtension authorizeExtension)
    {
        _authorizeExtension = authorizeExtension;
    }

    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        // Lấy token từ IAuthorizeExtension
        var token = _authorizeExtension.GetToken();

        if (!string.IsNullOrEmpty(token))
        {
            // Thêm token vào header Authorization với dạng "Bearer"
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        }

        // Gửi yêu cầu với token đã được thêm
        return await base.SendAsync(request, cancellationToken);
    }
}