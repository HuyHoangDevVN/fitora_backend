using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;

namespace InteractService.API.Controllers
{
    [ApiController]
    [Route("api/interact/upload")]
    public class UploadController : ControllerBase
    {
        private readonly IWebHostEnvironment _env;
        private readonly IConfiguration _configuration;

        public UploadController(IWebHostEnvironment env, IConfiguration configuration)
        {
            _env = env;
            _configuration = configuration;
        }

        [HttpPost("file")]
        [Consumes("multipart/form-data")]
        public async Task<IActionResult> UploadFile(IFormFile file)
        {
            if (file == null || file.Length == 0)
            {
                return BadRequest("Không có tệp nào được upload.");
            }

            string uploadRootPath = _configuration["UploadSettings:ExternalFolder"];

            if (string.IsNullOrEmpty(uploadRootPath))
            {
                uploadRootPath = _env.WebRootPath;
                if (string.IsNullOrEmpty(uploadRootPath))
                {
                    uploadRootPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");
                    Directory.CreateDirectory(uploadRootPath);
                }
                uploadRootPath = Path.Combine(uploadRootPath, "uploads");
            }
            else
            {
                Directory.CreateDirectory(uploadRootPath);
            }

            var originalFileName = Path.GetFileName(file.FileName);
            var uniqueFileName = $"{Guid.NewGuid()}_{originalFileName}";
            var filePath = Path.Combine(uploadRootPath, uniqueFileName);

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            var fileUrl = $"{Request.Scheme}://{Request.Host}/interact/uploads/{uniqueFileName}";
            return Ok(new { url = fileUrl });
        }


        [HttpGet("file/{fileName}")]
        public IActionResult DownloadFile(string fileName)
        {
            string uploadRootPath = _configuration["UploadSettings:ExternalFolder"];
            if (string.IsNullOrEmpty(uploadRootPath))
            {
                uploadRootPath = Path.Combine(_env.WebRootPath, "uploads");
            }
            var filePath = Path.Combine(uploadRootPath, fileName);
            if (!System.IO.File.Exists(filePath))
                return NotFound("Không tìm thấy file");

            var contentType = "application/octet-stream";
            return PhysicalFile(filePath, contentType, fileName);
        }
    }
}