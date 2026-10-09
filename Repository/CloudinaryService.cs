using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using EcommerceAPI.Interfaces;
using EcommerceAPI.Models;
using Microsoft.Extensions.Options;

namespace EcommerceAPI.Services
{
    public class CloudinaryService : ICloudinaryService
    {
        private readonly Cloudinary _cloudinary;

        public CloudinaryService(IOptions<CloudinarySettings> config)
        {
            var account = new Account(
                config.Value.CloudName,
                config.Value.ApiKey,
                config.Value.ApiSecret);

            _cloudinary = new Cloudinary(account);
            _cloudinary.Api.Secure = true;
        }

        // Imágenes de productos
        public Task<(string Url, string PublicId)> UploadImageAsync(string base64Image)
            => SubirAsync(base64Image, "sportzone/productos",
                new Transformation().Width(800).Height(800).Crop("limit"));

        // Fotos de perfil: cuadradas, centradas en la cara
        public Task<(string Url, string PublicId)> UploadAvatarAsync(string base64Image)
            => SubirAsync(base64Image, "sportzone/perfiles",
                new Transformation().Width(400).Height(400).Crop("fill").Gravity("face"));

        public async Task DeleteImageAsync(string publicId)
        {
            await _cloudinary.DestroyAsync(new DeletionParams(publicId));
        }

        private async Task<(string Url, string PublicId)> SubirAsync(
            string base64Image, string carpeta, Transformation transformacion)
        {
            if (string.IsNullOrWhiteSpace(base64Image))
                throw new ArgumentException("La imagen no puede estar vacía.");

            try
            {
                var base64Data = base64Image.Contains(",")
                    ? base64Image.Split(',')[1]
                    : base64Image;

                var imageBytes = Convert.FromBase64String(base64Data);
                await using var stream = new MemoryStream(imageBytes);

                var uploadParams = new ImageUploadParams
                {
                    File = new FileDescription("imagen", stream),
                    Folder = carpeta,
                    Transformation = transformacion
                };

                var result = await _cloudinary.UploadAsync(uploadParams);

                if (result.Error != null)
                    throw new Exception($"Error al subir la imagen: {result.Error.Message}");

                return (result.SecureUrl.ToString(), result.PublicId);
            }
            catch (FormatException)
            {
                throw new ArgumentException("El formato de la imagen Base64 no es válido.");
            }
        }
    }
}