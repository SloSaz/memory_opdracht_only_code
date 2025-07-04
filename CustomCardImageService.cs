using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Business.Models;

namespace Business.Services
{
    /// <summary>
    /// Service for managing custom card images
    /// </summary>
    public class CustomCardImageService
    {
        private readonly ICustomCardImageRepository _repository;
        private readonly string _imageDirectory;

        public CustomCardImageService(ICustomCardImageRepository repository, string imageDirectory)
        {
            _repository = repository ?? throw new ArgumentNullException(nameof(repository));
            _imageDirectory = imageDirectory ?? throw new ArgumentNullException(nameof(imageDirectory));

            // Create directory if it doesn't exist
            if (!Directory.Exists(_imageDirectory))
            {
                Directory.CreateDirectory(_imageDirectory);
            }
        }

        public List<CustomCardImage> GetAllImages()
        {
            return _repository.GetAllImages();
        }

        public CustomCardImage GetImageById(string id)
        {
            return _repository.GetImageById(id);
        }

        public async Task<CustomCardImage> AddImageAsync(string name, Stream imageStream, string fileExtension)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException("Image name cannot be empty", nameof(name));

            if (imageStream == null)
                throw new ArgumentNullException(nameof(imageStream));

            // Generate a unique id
            string id = Guid.NewGuid().ToString();

            // Create file path
            string fileName = $"{id}{fileExtension}";
            string filePath = Path.Combine(_imageDirectory, fileName);

            // Save image file
            using (var fileStream = File.Create(filePath))
            {
                await imageStream.CopyToAsync(fileStream);
            }

            // Create custom card image and save to repository
            var customCardImage = new CustomCardImage(id, name, filePath);
            _repository.SaveImage(customCardImage);

            return customCardImage;
        }

        public void DeleteImage(string id)
        {
            _repository.DeleteImage(id);
        }
    }
}