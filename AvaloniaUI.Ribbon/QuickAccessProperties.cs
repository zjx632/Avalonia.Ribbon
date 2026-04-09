using Avalonia;
using Avalonia.Controls;

namespace AvaloniaUI.Ribbon;

/// <summary>
/// Attached properties for QuickAccess functionality.
/// Shared across platform-specific and cross-platform projects.
/// </summary>
public class QuickAccessProperties
{
    private QuickAccessProperties()
    {
    }

    public static readonly AttachedProperty<bool> IsCheckedProperty =
        AvaloniaProperty.RegisterAttached<QuickAccessProperties, MenuItem, bool>("IsChecked");

    public static bool GetIsChecked(MenuItem element)
    {
        return element.GetValue(IsCheckedProperty);
    }

    public static void SetIsChecked(MenuItem element, bool value)
    {
        element.SetValue(IsCheckedProperty, value);
    }
}
