using System.Threading.Tasks;
using Avalonia.Controls;
using MsBox.Avalonia;
using MsBox.Avalonia.Enums;

namespace MicroNotes.MessageBox;

public class MessageBoxService : IMessageBoxService
{
    private readonly MainWindow _mainWindow;

    public MessageBoxService(MainWindow mainWindow)
    {
        _mainWindow = mainWindow;
    }

    public async Task<bool> AskForUpdate()
    {
        var result = await MessageBoxManager.GetMessageBoxStandard("Update available",
                "An update is available.\nDo you want to install it now? The application will restart automatically after installation.",
                ButtonEnum.YesNo, Icon.None, WindowStartupLocation.CenterOwner)
            .ShowWindowDialogAsync(_mainWindow);

        return result == ButtonResult.Yes;
    }

    public async Task<bool> ConfirmDiscardUnsavedChanges()
    {
        var result = await MessageBoxManager.GetMessageBoxStandard("Unsaved changes",
                "You have unsaved changes. These will be lost if you close the application now.\nDo you really want to quit?",
                ButtonEnum.YesNo, Icon.None, WindowStartupLocation.CenterOwner)
            .ShowWindowDialogAsync(_mainWindow);

        return result == ButtonResult.Yes;
    }

    public async Task<bool> ConfirmDelete(string title)
    {
        var result = await MessageBoxManager.GetMessageBoxStandard("Delete?",
                $"Do you want to delete the note called '{title}'?", ButtonEnum.YesNo,
                Icon.None, WindowStartupLocation.CenterOwner)
            .ShowWindowDialogAsync(_mainWindow);

        return result == ButtonResult.Yes;
    }

    public async Task WarnForInvalidFilename()
    {
        await MessageBoxManager.GetMessageBoxStandard("Invalid file name.",
                "The file name is invalid (maybe it contains invalid characters?)", ButtonEnum.Ok, Icon.None,
                WindowStartupLocation.CenterOwner)
            .ShowWindowDialogAsync(_mainWindow);
    }

    public async Task WarnForExistingFilename()
    {
        await MessageBoxManager.GetMessageBoxStandard("File already exists.",
                "A file with this title already exists. Please choose another title.", ButtonEnum.Ok, Icon.None,
                WindowStartupLocation.CenterOwner)
            .ShowWindowDialogAsync(_mainWindow);
    }
}