using System.Threading.Tasks;

namespace MAUI.Services
{
    public interface IFilePickerService
    {
        Task<FileResult> PickImageAsync();
    }
}