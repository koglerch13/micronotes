using System.Threading.Tasks;

namespace MicroNotes;

public class MessageBoxService : IMessageBoxService
{
    public async Task<bool> ConfirmDiscardUnsavedChanges()
    {
        
    }
    
    public async Task<bool> ConfirmDelete(string title)
    {
        
    }

    public async Task WarnForInvalidFilename()
    {
        
    }

    public async Task WarnForExistingFilename()
    {
        
    }
}