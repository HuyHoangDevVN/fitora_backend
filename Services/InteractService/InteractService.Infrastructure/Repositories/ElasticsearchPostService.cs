using Elastic.Clients.Elasticsearch;
using Elastic.Clients.Elasticsearch.Core.Bulk;
using Elastic.Transport;
using InteractService.Application.Services.IServices;

namespace InteractService.Infrastructure.Repositories;

public class ElasticsearchPostService : IElasticsearchPostService
{
    private readonly ElasticsearchClient _client;
    private const string IndexName = "post-index";

    public ElasticsearchPostService(string elasticUri, string username, string password)
    {
        var settings = new ElasticsearchClientSettings(new Uri(elasticUri))
            .Authentication(new BasicAuthentication(username, password));
        _client = new ElasticsearchClient(settings);
    }

    // Create or Update
    public async Task IndexPostAsync(Post post)
    {
        var response = await _client.IndexAsync(post, idx => idx.Index(IndexName).Id(post.Id));
        if (!response.IsValidResponse)
            throw new Exception("Indexing failed: " + response.DebugInformation);
    }

    // Read by Id
    public async Task<Post?> GetPostByIdAsync(Guid id)
    {
        var response = await _client.GetAsync<Post>(id.ToString(), idx => idx.Index(IndexName));
        return response.Found ? response.Source : null;
    }

    // Update
    public async Task UpdatePostAsync(Post post)
    {
        await IndexPostAsync(post);
    }

    // Delete
    public async Task DeletePostAsync(Guid id)
    {
        var response = await _client.DeleteAsync<Post>(id.ToString(), d => d.Index(IndexName));
        if (!response.IsValidResponse)
            throw new Exception("Delete failed: " + response.DebugInformation);
    }

    // Search by Content
    public async Task<List<Post>> SearchByContentAsync(string keyword, int size = 10)
    {
        var searchResponse = await _client.SearchAsync<Post>(s => s
            .Index(IndexName)
            .Size(size)
            .Query(q => q
                .Match(m => m
                    .Field(f => f.Content)
                    .Query(keyword)
                )
            )
        );
        return searchResponse.Documents.ToList();
    }


    public async Task BulkIndexPostsAsync(IEnumerable<Post> posts)
    {
        var operations = new List<IBulkOperation>();
        foreach (var post in posts)
        {
            operations.Add(new BulkIndexOperation<Post>(post)
            {
                Index = IndexName,
                Id = post.Id.ToString()
            });
        }
        var response = await _client.BulkAsync(new BulkRequest(IndexName)
        {
            Operations = operations
        });
        if (!response.IsValidResponse)
        {
            throw new Exception($"Bulk index failed: {response.DebugInformation}");
        }
    }
}