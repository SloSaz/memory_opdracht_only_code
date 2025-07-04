using System.Collections.Generic;
using Business.Models;

namespace Business
{
    /// <summary>
    /// Repository interface for custom card image data access
    /// </summary>
    public interface ICustomCardImageRepository
    {
        List<CustomCardImage> GetAllImages();
        CustomCardImage GetImageById(string id);
        void SaveImage(CustomCardImage image);
        void DeleteImage(string id);
    }
}