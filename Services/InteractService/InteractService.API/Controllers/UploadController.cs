using Microsoft.AspNetCore.Authorization;
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
        private const long MaxFileSizeBytes = 50 * 1024 * 1024; // khớp MAX_FILE_SIZE ở FE (PostBox.tsx)

        public UploadController(IWebHostEnvironment env, IConfiguration configuration)
        {
            _env = env;
            _configuration = configuration;
        }

        /// <summary>Không [Authorize] trước đây => ai cũng upload được không cần đăng nhập (spam disk/mã độc).</summary>
        [HttpPost("file")]
        [Authorize]
        [Consumes("multipart/form-data")]
        public async Task<IActionResult> UploadFile(IFormFile file)
        {
            if (file != null && file.Length > MaxFileSizeBytes)
            {
                return BadRequest("Tệp quá lớn. Kích thước tối đa 50MB.");
            }
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

            var fileUrl = $"/api/interact/upload/file/{uniqueFileName}";
            return Ok(new { url = fileUrl });
        }


        [HttpGet("file/{fileName}")]
        public IActionResult DownloadFile(string fileName)
        {
            // Chặn path traversal: fileName chỉ được là tên file thuần (Path.Combine trước đây
            // không validate, nên "../../appsettings.json" có thể đọc file ngoài thư mục uploads).
            if (string.IsNullOrWhiteSpace(fileName) || fileName != Path.GetFileName(fileName))
            {
                return BadRequest("Tên file không hợp lệ.");
            }

            string uploadRootPath = _configuration["UploadSettings:ExternalFolder"];
            if (string.IsNullOrEmpty(uploadRootPath))
            {
                uploadRootPath = Path.Combine(_env.WebRootPath, "uploads");
            }
            var filePath = Path.Combine(uploadRootPath, fileName);
            var fullUploadRoot = Path.GetFullPath(uploadRootPath);
            var fullFilePath = Path.GetFullPath(filePath);
            if (!fullFilePath.StartsWith(fullUploadRoot, StringComparison.OrdinalIgnoreCase))
            {
                return BadRequest("Tên file không hợp lệ.");
            }
            if (!System.IO.File.Exists(filePath))
                return NotFound("Không tìm thấy file");

            var provider = new Microsoft.AspNetCore.StaticFiles.FileExtensionContentTypeProvider();
            string contentType;
            if (!provider.TryGetContentType(filePath, out contentType))
            {
                contentType = "application/octet-stream";
            }

            return PhysicalFile(filePath, contentType); // Không truyền fileName để không force download
        }
    }
}
