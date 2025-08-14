namespace ChoSV.Services.Interfaces
{
    public interface IImageService
    {
        Task<string> UploadImageAsync(IFormFile file);
        bool DeleteImageByUrl(string imageUrl);
    }
}
