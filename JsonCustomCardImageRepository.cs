using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using Business;
using Business.Models;

namespace DataAccess
{
    /// <summary>
    /// Repository implementation that stores custom card images metadata in a JSON file
    /// </summary>
    public class JsonCustomCardImageRepository : ICustomCardImageRepository
    {
        private readonly string _filePath;
        private readonly string _imageDirectory;
        private List<CustomCardImage> _images;

        public JsonCustomCardImageRepository(string filePath, string imageDirectory)
        {
            _filePath = filePath ?? throw new ArgumentNullException(nameof(filePath));
            _imageDirectory = imageDirectory ?? throw new ArgumentNullException(nameof(imageDirectory));
            
            // Create directory if it doesn't exist
            if (!Directory.Exists(_imageDirectory))
            {
                Directory.CreateDirectory(_imageDirectory);
            }
            
            _images = LoadImages();
        }

        private List<CustomCardImage> LoadImages()
        {
            try
            {
                if (File.Exists(_filePath))
                {
                    string json = File.ReadAllText(_filePath);
                    return JsonSerializer.Deserialize<List<CustomCardImage>>(json) ?? new List<CustomCardImage>();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading custom card images: {ex.Message}");
            }

            return new List<CustomCardImage>();
        }

        private void SaveImages()
        {
            try
            {
                string directoryPath = Path.GetDirectoryName(_filePath);
                if (!string.IsNullOrEmpty(directoryPath) && !Directory.Exists(directoryPath))
                {
                    Directory.CreateDirectory(directoryPath);
                }

                string json = JsonSerializer.Serialize(_images, new JsonSerializerOptions { WriteIndented = true });
                File.WriteAllText(_filePath, json);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error saving custom card images: {ex.Message}");
            }
        }

        public List<CustomCardImage> GetAllImages()
        {
            return _images.ToList();
        }

        public CustomCardImage GetImageById(string id)
        {
            return _images.FirstOrDefault(i => i.Id == id);
        }

        public void SaveImage(CustomCardImage image)
        {
            if (image == null)
                throw new ArgumentNullException(nameof(image));

            // Check if image already exists
            var existingImage = _images.FirstOrDefault(i => i.Id == image.Id);
            if (existingImage != null)
            {
                // Update existing image
                _images.Remove(existingImage);
            }

            // Add the new image
            _images.Add(image);

            // Save to file
            SaveImages();
        }

        public void DeleteImage(string id)
        {
            var image = _images.FirstOrDefault(i => i.Id == id);
            if (image != null)
            {
                // Remove from list
                _images.Remove(image);

                // Try to delete the file
                try
                {
                    if (File.Exists(image.FilePath))
                    {
                        File.Delete(image.FilePath);
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error deleting image file: {ex.Message}");
                }

                // Save to file
                SaveImages();
            }
        }
    }
}