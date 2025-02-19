using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;
using System.Collections.Generic;
using System.Linq;

public class FileUploadOperationFilter : IOperationFilter
{
    public void Apply(OpenApiOperation operation, OperationFilterContext context)
    {
        // Kiểm tra xem phương thức có parameter nào kiểu IFormFile hoặc IFormFile[] không.
        var fileParams = context.MethodInfo.GetParameters()
            .Where(p => p.ParameterType == typeof(Microsoft.AspNetCore.Http.IFormFile) || 
                        p.ParameterType == typeof(Microsoft.AspNetCore.Http.IFormFile[]));

        if (!fileParams.Any())
            return;

        // Xóa các parameter hiện có nếu cần (tuỳ vào cách bạn thiết kế API)
        operation.Parameters.Clear();

        // Cấu hình RequestBody để hỗ trợ multipart/form-data với file upload
        operation.RequestBody = new OpenApiRequestBody
        {
            Content = new Dictionary<string, OpenApiMediaType>
            {
                ["multipart/form-data"] = new OpenApiMediaType
                {
                    Schema = new OpenApiSchema
                    {
                        Type = "object",
                        Properties = new Dictionary<string, OpenApiSchema>
                        {
                            ["file"] = new OpenApiSchema
                            {
                                Type = "string",
                                Format = "binary"
                            }
                        },
                        Required = new HashSet<string> { "file" }
                    }
                }
            }
        };
    }
}