using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using MicroNotes.MessageBox;

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
            var messageBoxService = new MessageBoxService(mainWindow);
            
            var mainWindowViewModel = new MainWindowViewModel(mainWindow, desktop, messageBoxService);
            mainWindow.DataContext = mainWindowViewModel;
            desktop.MainWindow = mainWindow;
        }

        base.OnFrameworkInitializationCompleted();
    }
}