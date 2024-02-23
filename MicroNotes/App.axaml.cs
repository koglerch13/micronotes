using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using MicroNotes.Logging;
using MicroNotes.MessageBox;
using MicroNotes.UpdateManager;

namespace MicroNotes;

public class App : Application
{
    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
    }

    public override void OnFrameworkInitializationCompleted()
    {
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            var mainWindow = new MainWindow();
            var logger = LoggingSetup.GetLogger();
            var messageBoxService = new MessageBoxService(mainWindow);
            var updateManagerService = new UpdateManagerService(logger, messageBoxService);
            
            var mainWindowViewModel = new MainWindowViewModel(mainWindow, desktop, messageBoxService, logger, updateManagerService);
            mainWindow.DataContext = mainWindowViewModel;
            desktop.MainWindow = mainWindow;
        }

        base.OnFrameworkInitializationCompleted();
    }
}