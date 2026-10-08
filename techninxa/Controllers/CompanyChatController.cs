using Microsoft.AspNetCore.Mvc;

namespace techninxa.Controllers
{
    public class CompanyChatController : Controller
    {
        private readonly IWebHostEnvironment _environment;

        public CompanyChatController(IWebHostEnvironment environment)
        {
            _environment = environment;
        }

        // Keep your existing CompanyChat() action here.
        // Keep your existing Messages() action here.
        // Keep your existing Logout() action here.


        [HttpPost]
        [RequestSizeLimit(10 * 1024 * 1024)]
        public async Task<IActionResult> UploadFile(IFormFile file)
        {
            // Check login
            if (HttpContext.Session.GetString("CompanyChatLoggedIn") != "true")
            {
                return Unauthorized(new
                {
                    success = false,
                    message = "You are not logged in."
                });
            }

            if (file == null || file.Length == 0)
            {
                return BadRequest(new
                {
                    success = false,
                    message = "Please select a file."
                });
            }

            // Maximum 10 MB
            if (file.Length > 10 * 1024 * 1024)
            {
                return BadRequest(new
                {
                    success = false,
                    message = "File size cannot exceed 10 MB."
                });
            }

            // Allowed extensions
            var allowedExtensions = new[]
            {
                ".jpg",
                ".jpeg",
                ".png",
                ".gif",
                ".webp",
                ".pdf",
                ".doc",
                ".docx",
                ".xls",
                ".xlsx",
                ".zip"
            };

            var extension =
                Path.GetExtension(file.FileName).ToLowerInvariant();

            if (!allowedExtensions.Contains(extension))
            {
                return BadRequest(new
                {
                    success = false,
                    message = "This file type is not allowed."
                });
            }

            // Create upload folder
            var uploadFolder = Path.Combine(
                _environment.WebRootPath,
                "uploads",
                "chat"
            );

            if (!Directory.Exists(uploadFolder))
            {
                Directory.CreateDirectory(uploadFolder);
            }

            // Never trust the original filename
            var safeFileName =
                $"{Guid.NewGuid():N}{extension}";

            var filePath = Path.Combine(
                uploadFolder,
                safeFileName
            );

            await using (var stream = new FileStream(
                filePath,
                FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            var originalFileName =
                Path.GetFileName(file.FileName);

            var fileUrl =
                $"/uploads/chat/{safeFileName}";

            var isImage =
                extension == ".jpg" ||
                extension == ".jpeg" ||
                extension == ".png" ||
                extension == ".gif" ||
                extension == ".webp";

            return Ok(new
            {
                success = true,
                fileName = originalFileName,
                fileUrl = fileUrl,
                isImage = isImage,
                fileSize = file.Length
            });
        }
    }
}