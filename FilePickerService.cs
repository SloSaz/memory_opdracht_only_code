using System.Threading.Tasks;

namespace MAUI.Services
{
    public class FilePickerService : IFilePickerService
    {
        public async Task<FileResult> PickImageAsync()
        {
            try
            {
                var options = new PickOptions
                {
                    PickerTitle = "Select an image",
                    FileTypes = FilePickerFileType.Images
                };

                return await FilePicker.PickAsync(options);
            }
            catch (Exception)
            {
                // The user canceled or something went wrong
                return null;
            }
        }
    }
}