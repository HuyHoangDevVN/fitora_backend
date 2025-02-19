using Microsoft.AspNetCore.Mvc;

namespace InteractService.API.Controllers;

[Route("api/post")]
[ApiController]
public class PostController : ControllerBase
{
    [HttpPost("create-post")]
    public async Task<IActionResult> CreatePost()
    {
        return Ok("posts");
    }

    [HttpPut("update-post")]
    public async Task<IActionResult> UpdatePost()
    {
        return Ok("posts");
    }

    [HttpGet("get-posts")]
    public async Task<IActionResult> GetPosts()
    {
        return Ok("posts");
    }

    [HttpGet("get-post")]
    public async Task<IActionResult> GetPost()
    {
        return Ok("post");
    }

    [HttpDelete("delete-post")]
    public async Task<IActionResult> DeletePost()
    {
        return Ok("post");
    }
}