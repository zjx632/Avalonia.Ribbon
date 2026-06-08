# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Build & Run

```bash
# Build everything
dotnet build AvaloniaRibbon.sln

# Build specific project
dotnet build AvaloniaUI.Ribbon/AvaloniaUI.Ribbon.csproj

# Run desktop demo
dotnet run --project AvaloniaUI.Ribbon.Demo.Desktop/AvaloniaUI.Ribbon.Demo.Desktop.csproj

# Publish WASM demo
dotnet publish AvaloniaUI.Ribbon.Demo.Browser/AvaloniaUI.Ribbon.Demo.Browser.csproj

# Clean build artifacts
dotnet clean AvaloniaRibbon.sln
```

**SDK required**: .NET 10.0+ (global.json permits rollForward to latest major).
**No tests exist** in this repo.

## Project Structure

5 projects in one solution:

| Project | TFM | Purpose |
|---------|-----|---------|
| `AvaloniaUI.Ribbon` | `net10.0` | Cross-platform controls library. NuGet: `AvaloniaControls.Ribbon` |
| `AvaloniaUI.Ribbon.Desktop` | `net10.0` | Desktop-only controls (`RibbonWindow`, `QuickAccessToolbar`, `DesktopRibbon`). NuGet: `AvaloniaControls.Ribbon.Desktop` |
| `AvaloniaUI.Ribbon.Demo` | `net10.0-windows;net10.0-browser` | Shared demo views, viewmodels, models |
| `AvaloniaUI.Ribbon.Demo.Desktop` | `net10.0-windows` | Desktop demo entry (WinExe) |
| `AvaloniaUI.Ribbon.Demo.Browser` | `net10.0-browser` | WASM demo entry point |

## Architecture

### Controls Library Layout (`AvaloniaUI.Ribbon/`)

- **Contracts/** — Interfaces: `IRibbon`, `IKeyTipHandler`, `IRibbonControl`, `IRibbonCommand`, `IRibbonCommandControl`, `IRibbonInputControl`, `IRibbonMenu`, `ICanAddToQuickAccess`
- **Models/** — Enums: `RibbonControlSize` (Small/Medium/Large), `GroupDisplayMode`
- **Converters/** — Value converters: `BoolAndConverter`, `BoundsPointToAdjustedPointConverter`, `DoubleArithmeticConverter`, `HeaderHeightConverter`, `IsNullConverter`, `MathAddConverter`
- **Helpers/** — `RibbonControlHelper<T>`, `RibbonControlExtensions`, `TopLevelExtensions`
- **Locale/** — Localized resources (`en-ca.axaml`, `ru-ru.axaml`)
- **Styles/Fluent/Controls/** — Control themes for each control (`.axaml` files)
- **Themes/Accents/** — Accent theme files (Fluent, Simple)

### Control Hierarchy

```
Ribbon (extends TabControl, implements IRibbon)         [cross-platform]
  └── DesktopRibbon (extends Ribbon)                    [desktop-only, adds QuickAccessToolbar]
RibbonTab (extends TabItem, implements IKeyTipHandler)  [cross-platform]
RibbonGroupBox (extends HeaderedItemsControl)           [cross-platform]
RibbonButton (extends Button, implements IRibbonInputControl + IRibbonCommand + ICanAddToQuickAccess)
RibbonToggleButton (extends ToggleButton, same interfaces)
RibbonSplitButton (extends SplitButton, same interfaces)
RibbonDropDownButton
RibbonComboBox
RibbonGallery
GalleryItem
RibbonMenu (extends ItemsControl, implements IRibbonMenu)
RibbonMenuItem
KeyTip
RibbonContextualTabGroup
```

### Desktop-Only Controls (`AvaloniaUI.Ribbon.Desktop/`)

- `RibbonWindow` — Ribbon-enabled Window
- `QuickAccessToolbar` — Quick Access Toolbar (QAT)
- `DesktopRibbon` — Ribbon with QAT support

### Key Patterns

- **ControlTheme architecture**: Each control has `.cs` + corresponding `.axaml` under `Styles/Fluent/Controls/`. Styles registered via `StyleInclude` in `AvaloniaRibbon.axaml`.
- **TemplatePart**: Controls declare expected named template parts via `[TemplatePart]` attributes and resolve them in `OnApplyTemplate`.
- **Static constructor property registration**: Static constructors register AvaloniaProperty instances using `AvaloniaProperty.Register`/`RegisterDirect`. `IRibbonControl` size properties registered via `RibbonControlHelper<T>.SetProperties()`.
- **KeyTip system**: ALT key opens key tips. Navigation through `IKeyTipHandler` interface chain: Ribbon → RibbonTab → Group controls.
- **Ribbon collapse**: When collapsed, clicking a tab shows selected groups in a popup. Toggle via `IsCollapsed` property.
- **MVVM in demo**: Uses `CommunityToolkit.Mvvm` (not ReactiveUI, despite `ReactiveUI.Avalonia` dependency in main lib).

### Key Dependencies

- Avalonia 12.0.0
- ReactiveUI.Avalonia 11.4.12 (in main lib)
- CommunityToolkit.Mvvm 8.4.2 (in demo)
- Xaml.Behaviors.Interactions 12.0.0
- Material.Avalonia 3.15.1 (in demo)

## Known Issues

- **Desktop project XAML errors**: `AvaloniaUI.Ribbon.Desktop` uses removed Avalonia 12 types (`TitleBar`, `CaptionButtons`, `ChromeOverlayLayer` from `Avalonia.Controls.Chrome`). `RibbonWindow.axaml` and `QuickAccessToolbar.axaml` need XAML refactoring. Main library builds clean.
- `Avalonia.Diagnostics` locked to 11.3.13 (no 12.0.0 stable version referenced).

## Package Publishing

NuGet packages: `AvaloniaControls.Ribbon`, `AvaloniaControls.Ribbon.Desktop` (v0.9.9). Project URL: https://github.com/SachiHarshitha/Avalonia.Ribbon
