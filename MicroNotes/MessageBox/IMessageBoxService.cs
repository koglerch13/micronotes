using System.Threading.Tasks;

namespace MicroNotes.MessageBox;

public interface IMessageBoxService
{
    Task<bool> ConfirmDiscardUnsavedChanges();
    Task<bool> ConfirmDelete(string title);
    Task WarnForInvalidFilename();
    Task WarnForExistingFilename();
    Task<bool> AskForUpdate();
}