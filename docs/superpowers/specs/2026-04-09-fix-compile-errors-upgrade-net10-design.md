# 设计文档：统一升级到 Avalonia 12 + net10.0 修复编译错误

## 问题

`AvaloniaUI.Ribbon.Desktop` 项目目标框架为 `netstandard2.0`，引用了 Avalonia 11.3.9。它通过 `ProjectReference` 引用了 `AvaloniaUI.Ribbon` 项目，后者目标框架为 `net10.0`，使用 Avalonia 12.0.0。框架不兼容导致 NU1201 错误。

此外，`System.Text.Json` 10.0.0 不兼容 `netstandard2.0`，触发 NU1510 警告。

## 方案

将所有项目统一到 `net10.0` + Avalonia 12.0.0 生态。

## 变更清单

### 1. AvaloniaUI.Ribbon.Desktop.csproj ✅

| 属性 | 旧值 | 新值 |
|------|------|------|
| TargetFrameworks | netstandard2.0 | net10.0 |
| Avalonia | 11.3.9 | 12.0.0 |
| Xaml.Behaviors.Avalonia | 11.3.9 | 12.0.0 |
| System.Text.Json | 10.0.0 (删除) | — |

### 2. AvaloniaUI.Ribbon.csproj ✅

- 删除 `System.Text.Json` 10.0.5（代码中未使用）

### 3. AvaloniaUI.Ribbon.Demo.csproj ✅

- `Avalonia.Diagnostics` 保持 11.3.13（12.0.0 稳定版尚未发布）

### 4. 代码兼容性修复 ✅

#### Ribbon.cs
- `IInputRoot.AddHandler/RemoveHandler` → `WindowBase.AddHandler/RemoveHandler`

#### RibbonMenu.cs
- `TitleBar`（Avalonia 12 已移除）→ 改用 `topLevel.Bounds.Height`
- 移除 `Avalonia.Controls.Chrome` using

#### TopLevelExtensions.cs
- `TitleBar` 引用移除

#### RibbonWindow.cs (Desktop)
- `SystemDecorationsProperty` → `WindowDecorationsProperty`
- `SystemDecorations` → `WindowDecorations`
- `ExtendClientAreaChromeHints` → `WindowDecorations = WindowDecorations.None`
- `GetVisualRoot()` → `TopLevel.GetTopLevel(this)`

### 5. XAML 兼容性修复 ✅

#### RibbonGroupBox.axaml (主项目)
- `$parent[local|RibbonGroupBox]` → `$parent[RibbonGroupBox]`
- `clr-namespace:` → `using:` 语法

### 6. 剩余 XAML 错误（Desktop 项目）⚠️

`RibbonWindow.axaml` 和 `QuickAccessToolbar.axaml` 使用 Avalonia 12 已移除的类型：
- `TitleBar`（`Avalonia.Controls.Chrome` 命名空间已移除）
- `CaptionButtons`（`Avalonia.Controls.Chrome` 命名空间已移除）
- `ChromeOverlayLayer`（`VisualLayerManager` 的子元素已变更）
- `SystemDecorations` 选择器 → 需改为 `WindowDecorations`

这些需要大规模 XAML 重构，超出原始编译错误修复范围。

## 成功标准

- [x] `dotnet build` 零 NUxxx 错误 ✅
- [x] 主项目（AvaloniaUI.Ribbon）零 CS 错误 ✅
- [ ] Desktop 项目零 XAML 错误 ⚠️（需额外 XAML 重构工作）
- [x] 所有项目目标框架统一为 net10.0 ✅
- [x] 所有 Avalonia 相关包版本统一为 12.0.0（Diagnostics 除外）✅

## 编译状态

- **AvaloniaUI.Ribbon**（主项目）：✅ 编译通过（仅有 nullable 警告）
- **AvaloniaUI.Ribbon.Desktop**：⚠️ XAML 编译错误（TitleBar、CaptionButtons 等已移除类型）
- **AvaloniaUI.Ribbon.Demo***：取决于 Desktop 项目
