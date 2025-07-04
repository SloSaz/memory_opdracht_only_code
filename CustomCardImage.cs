using System;

namespace Business.Models
{
    /// <summary>
    /// Represents a custom image that can be used for memory cards
    /// </summary>
    public class CustomCardImage
    {
        public string Id { get; }
        public string Name { get; }
        public string FilePath { get; }
        public DateTime DateAdded { get; }

        public CustomCardImage(string id, string name, string filePath)
        {
            Id = id ?? Guid.NewGuid().ToString();
            Name = name ?? throw new ArgumentNullException(nameof(name));
            FilePath = filePath ?? throw new ArgumentNullException(nameof(filePath));
            DateAdded = DateTime.Now;
        }
    }
}