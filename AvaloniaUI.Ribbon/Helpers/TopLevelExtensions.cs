using System.Linq;
using Avalonia.Controls;
using Avalonia.VisualTree;

namespace AvaloniaUI.Ribbon.Helpers;

public static class TopLevelExtensions
{
    public static Canvas GetCanvasFromUsableArea(this TopLevel topLevel)
    {
        if (topLevel == null) return null;
        var descendants = topLevel.GetVisualDescendants();

        return null;
    }
}