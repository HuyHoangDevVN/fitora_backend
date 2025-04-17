using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using UserService.Application.Services.IServices;

namespace UserService.Infrastructure.Repositories;

public class InteractApiService : IInteractApiService
{
    private readonly HttpClient _httpClient;

    public InteractApiService(IHttpClientFactory httpClientFactory)
    {
        _httpClient = httpClientFactory.CreateClient("InteractService");
    }

    public async Task<bool> ApproveGroupPost(Guid groupId, Guid postId, CancellationToken cancellationToken)
    {
        var payload = new StringContent(
            JsonSerializer.Serialize(new { IsApproved = true }),
            Encoding.UTF8,
            "application/json"
        );

        var response = await _httpClient.PutAsync($"api/post/update-post/{postId}", payload, cancellationToken);

        if (response.IsSuccessStatusCode)
        {
            var result = await response.Content.ReadFromJsonAsync<bool>(cancellationToken: cancellationToken);
            return result;
        }

        throw new Exception("Error while approving post");
    }
}