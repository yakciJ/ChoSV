using ChoSV.Models.DTOs.Image;
using ChoSV.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ChoSV.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Policy = "UserPolicy")]
    public class ImageController : ControllerBase
    {
        private readonly IImageService _imageService;

        public ImageController(IImageService imageService)
        {
            _imageService = imageService;
        }

        [HttpPost("upload")]
        public async Task<IActionResult> UploadImage([FromForm] UploadImageDTO uploadImageDTO)
        {
            if (uploadImageDTO.File == null || uploadImageDTO.File.Length == 0)
            {
                return BadRequest(new { success = false, error = "Không có file nào được tải lên!" });
            }

            var imageUrl = await _imageService.UploadImageAsync(uploadImageDTO.File);
            return Ok(new { imageUrl = imageUrl, message = "Tải ảnh thành công!" });
        }

        [HttpDelete]
        public IActionResult DeleteImage([FromQuery] string imageUrl)
        {
            try
            {
                if (string.IsNullOrEmpty(imageUrl))
                {
                    return BadRequest(new { message = "URL ảnh không được để trống!" });
                }

                var success = _imageService.DeleteImageByUrl(imageUrl);

                if (success)
                {
                    return Ok(new { message = "Xóa ảnh thành công!" });
                }
                else
                {
                    return NotFound(new { message = "Không tìm thấy ảnh hoặc ảnh không hợp lệ!" });
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Đã có lỗi xảy ra khi xóa ảnh!" });
            }
        }

        [HttpPost("upload-multiple")]
        public async Task<IActionResult> UploadMultipleImages([FromForm] List<IFormFile> files)
        {
            if (files == null || !files.Any())
            {
                return BadRequest(new { success = false, error = "Không có file nào được tải lên!" });
            }

            if (files.Count > 6)
            {
                return BadRequest(new { success = false, error = $"Số lượng hình ảnh không được vượt quá 6. Bạn đã tải lên {files.Count} hình ảnh." });
            }

            var imageUrls = new List<string>();
            var errors = new List<string>();

            foreach (var file in files)
            {
                if (file != null && file.Length > 0)
                {
                    try
                    {
                        var imageUrl = await _imageService.UploadImageAsync(file);
                        imageUrls.Add(imageUrl);
                    }
                    catch (ArgumentException ex)
                    {
                        // Rollback: Clean up any previously uploaded files
                        foreach (var uploadedUrl in imageUrls)
                        {
                            _imageService.DeleteImageByUrl(uploadedUrl);
                        }

                        errors.Add($"File {file?.FileName}: {ex.Message}");
                        return BadRequest(new { success = false, error = "Upload thất bại, tất cả file đã được dọn dẹp.", details = errors });
                    }
                }
            }

            return Ok(new
            {
                imageUrls = imageUrls,
                message = $"Tải thành công {imageUrls.Count} ảnh!"
            });
        }
    }
}
