using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using InteractService.Application.Services.IServices;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace InteractService.Infrastructure.Repositories;

public class UserApiService : IUserApiService
{
    private readonly HttpClient _httpClient;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly ILogger<UserApiService> _logger;

    public UserApiService(
        IHttpClientFactory httpClientFactory, 
        IHttpContextAccessor httpContextAccessor, 
        ILogger<UserApiService> logger)
    {
        _httpClient = httpClientFactory.CreateClient("UserService");
        _httpContextAccessor = httpContextAccessor;
        _logger = logger;
    }

    public async Task<bool> CreateGroupPost(Guid groupId, Guid postId, bool isApproved, CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation("Calling CreateGroupPost API for GroupId: {GroupId}, PostId: {PostId}", groupId, postId);

            // Lấy token từ HttpContext (nếu có)
            var token = _httpContextAccessor.HttpContext?.User?.FindFirst("access_token")?.Value
                ?? throw new InvalidOperationException("No access token found in the HttpContext.");

            // Thêm token vào header
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            var payload = new StringContent(
                JsonSerializer.Serialize(new { IsApproved = isApproved, GroupId = groupId, PostId = postId }),
                Encoding.UTF8,
                "application/json"
            );

            var response = await _httpClient.PostAsync("api/group/create-group-post/", payload, cancellationToken);

            if (response.IsSuccessStatusCode)
            {
                var result = await response.Content.ReadFromJsonAsync<bool>(cancellationToken: cancellationToken);
                _logger.LogInformation("CreateGroupPost API call succeeded with result: {Result}", result);
                return result;
            }

            var errorContent = await response.Content.ReadAsStringAsync(cancellationToken);
            _logger.LogError("CreateGroupPost API failed with status {StatusCode}: {ErrorContent}", response.StatusCode, errorContent);
            throw new HttpRequestException($"Failed to create group post. Status: {response.StatusCode}, Error: {errorContent}");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error while calling CreateGroupPost API");
            throw;
        }
    }
}