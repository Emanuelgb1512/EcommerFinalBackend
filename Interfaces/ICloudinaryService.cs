namespace EcommerceAPI.Interfaces
{
    public interface ICloudinaryService
    {
        Task<(string Url, string PublicId)> UploadImageAsync(string base64Image);
        Task DeleteImageAsync(string publicId);
    }
}