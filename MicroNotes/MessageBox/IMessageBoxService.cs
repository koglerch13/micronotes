using System.Threading.Tasks;

namespace MicroNotes;

public interface IMessageBoxService
{
    Task<bool> ConfirmDiscardUnsavedChanges();
    Task<bool> ConfirmDelete(string title);
    Task WarnForInvalidFilename();
    Task WarnForExistingFilename();
}