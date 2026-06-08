using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;

using AvaloniaUI.Ribbon.Demo.ViewModels;
using AvaloniaUI.Ribbon.Demo.Views;

namespace AvaloniaUI.Ribbon.Demo;

public class App : Application
{
    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
    }

    public override void OnFrameworkInitializationCompleted()
    {
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
            desktop.MainWindow = new MainWindow
            {
                DataContext = new MainViewModel()
            };
        if (ApplicationLifetime is ISingleViewApplicationLifetime singleViewPlatform)
            singleViewPlatform.MainView = new MainView
            {
                DataContext = new MainViewModel()
            };

        base.OnFrameworkInitializationCompleted();
    }
}