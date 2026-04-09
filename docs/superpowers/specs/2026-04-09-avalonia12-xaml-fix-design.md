# Avalonia 12 XAML 编译修复设计文档

**日期**: 2026-04-09  
**状态**: 待实现  

## 背景

项目从 Avalonia 11 升级到 Avalonia 12 后，XAML 编译器启用了 compiled bindings（`x:CompileBindings` 默认 `True`）。原有 XAML 中有 22 处编译错误，分布在 8 个 `.axaml` 文件中。

## 错误分类

### 1. AVLN2100/AVLN2101 — Compiled Binding 缺少 x:DataType（13 处）

**根因**: Avalonia 12 默认启用 `x:CompileBindings="True"`，`ControlTemplate` 内的 `{Binding}` 无法在编译期推断 DataContext 类型。

**影响文件**:
- `AvaloniaUI.Ribbon/Styles/Fluent/Controls/RibbonButton.axaml` — QuickAccessTemplate（61-63行）
- `AvaloniaUI.Ribbon/Styles/Fluent/Controls/RibbonDropDownButton.axaml` — QuickAccessTemplate（16-22行）
- `AvaloniaUI.Ribbon/Styles/Fluent/Controls/RibbonSplitButton.axaml` — QuickAccessTemplate（17行）
- `AvaloniaUI.Ribbon/Styles/Fluent/Controls/RibbonToggleButton.axaml` — QuickAccessTemplate（53-55行）
- `AvaloniaUI.Ribbon/Styles/Fluent/Controls/SplitButtonControl.axaml` — 3 个 QuickAccessTemplate（92, 193, 292行）

**修复方式**: 在 `ControlTemplate` 元素上添加 `x:CompileBindings="False"`。  
**理由**: `QuickAccessTemplate` 的 DataContext 是运行时绑定的（来自 `TemplateBinding` 的父控件），编译期无法推断类型。关闭编译绑定是最安全的方式。

### 2. AVLN2000 — ItemsPresenter 无 DisplayMode 属性（1 处）

**根因**: `RibbonGroupBox.axaml` 第187行将 `DisplayMode` 绑定到 `ItemsPresenter` 元素上，但 `ItemsPresenter` 类没有此属性。`DisplayMode` 是 `RibbonGroupWrapPanel` 的属性。

**影响文件**:
- `AvaloniaUI.Ribbon/Styles/Fluent/Controls/RibbonGroupBox.axaml`（187行）

**修复方式**:  
```xaml
<!-- Before (错误：绑定在 ItemsPresenter 上) -->
<ItemsPresenter.ItemsPanel>
    <ItemsPanelTemplate>
        <RibbonGroupWrapPanel DisplayMode="{Binding DisplayMode, RelativeSource={RelativeSource TemplatedParent}}" />
    </ItemsPanelTemplate>
</ItemsPresenter.ItemsPanel>

<!-- After (正确：通过 $parent 绑定到 RibbonGroupBox) -->
<ItemsPresenter.ItemsPanel>
    <ItemsPanelTemplate>
        <RibbonGroupWrapPanel DisplayMode="{Binding $parent[local|RibbonGroupBox].DisplayMode}" />
    </ItemsPanelTemplate>
</ItemsPresenter.ItemsPanel>
```

### 3. AVLN2000 — Deferred Content 结构破坏（6 处）

**根因**: `SplitButtonControl.axaml` 和 `RibbonSplitButton.axaml` 中，`<Popup>` 嵌套在 `<Grid>` 或 `<DockPanel>` 内部，Avalonia 12 的 deferred loading 对嵌套内容中的 compiled binding 处理有变化，导致 AST 结构在转换时丢失 object initialization node。

**影响文件**:
- `AvaloniaUI.Ribbon/Styles/Fluent/Controls/SplitButtonControl.axaml`（84, 185, 285行）
- `AvaloniaUI.Ribbon/Styles/Fluent/Controls/RibbonSplitButton.axaml`（11行及类似位置）

**修复方式**: 在每个 `ControlTemplate` 根元素添加 `x:CompileBindings="False"`。  
**理由**: 这些 ControlTemplate 混合使用 `{TemplateBinding ...}` 和 `{Binding ...}`，结构复杂。关闭编译绑定避免 deferred content 解析问题。

### 4. AVLN2100 — TemplatedParent Binding 不在 ControlTemplate 内（1 处）

**根因**: `Gallery.axaml` 第345行，Style selector 中使用 `{Binding BorderBrush, RelativeSource={RelativeSource Mode=TemplatedParent}}`，但该 binding 不在 `ControlTemplate` 内部，`TemplatedParent` 不可用。

**影响文件**:
- `AvaloniaUI.Ribbon/Styles/Fluent/Controls/Gallery.axaml`（345行）

**修复方式**:
```xaml
<!-- Before -->
<Style Selector="local|Gallery /template/ RepeatButton, local|Gallery /template/ ToggleButton">
    <Setter Property="BorderBrush" Value="{Binding BorderBrush, RelativeSource={RelativeSource Mode=TemplatedParent}}" />

<!-- After -->
<Style Selector="local|Gallery /template/ RepeatButton, local|Gallery /template/ ToggleButton">
    <Setter Property="BorderBrush" Value="{Binding BorderBrush, RelativeSource={RelativeSource Mode=FindAncestor, AncestorType=local|Gallery}}" />
```

### 5. AVLN2000 — QuickAccessToolbar 类型无法解析（2 处）

**根因**: `Res.axaml`（跨平台项目）使用 `local:QuickAccessToolbar.IsChecked`，但 `QuickAccessToolbar` 类定义在 `AvaloniaUI.Ribbon.Desktop`（Desktop 项目）中。跨平台项目无法引用 Desktop 项目类型（会造成循环依赖）。

**影响文件**:
- `AvaloniaUI.Ribbon/Styles/Fluent/Res.axaml`（39, 43行）
- `AvaloniaUI.Ribbon.Desktop/QuickAccessToolbar.cs` — 需要移除 IsCheckedProperty
- **新建** `AvaloniaUI.Ribbon/QuickAccessProperties.cs` — 存放共享的附加属性

**修复方式**:
1. 在 `AvaloniaUI.Ribbon` 中创建 `QuickAccessProperties` 类，定义 `IsCheckedProperty` 附加属性
2. `Res.axaml` 使用 `xmlns:local="...AvaloniaUI.Ribbon"`（已有）引用 `local:QuickAccessProperties.IsChecked`
3. `QuickAccessToolbar.cs` 中的 `GetIsChecked/SetIsChecked` 方法改为委托给 `QuickAccessProperties`
4. `QuickAccessToolbar.cs` 的 `static` 构造函数中对 `IsCheckedProperty` 的引用改为 `QuickAccessProperties.IsCheckedProperty`

## 修复范围确认

- **包含**: 22 处 XAML 编译错误（AVLN 开头）
- **不包含**: CS86xx nullability 警告（约50+个）、CS0169 未使用字段警告
- **目标**: `dotnet build` 零 XAML 编译错误

## 文件变更清单

| 文件 | 变更类型 | 说明 |
|------|---------|------|
| `AvaloniaUI.Ribbon/Styles/Fluent/Controls/RibbonButton.axaml` | 修改 | QuickAccessTemplate 添加 `x:CompileBindings="False"` |
| `AvaloniaUI.Ribbon/Styles/Fluent/Controls/RibbonDropDownButton.axaml` | 修改 | QuickAccessTemplate 添加 `x:CompileBindings="False"` |
| `AvaloniaUI.Ribbon/Styles/Fluent/Controls/RibbonSplitButton.axaml` | 修改 | QuickAccessTemplate + ControlTemplate 添加 `x:CompileBindings="False"` |
| `AvaloniaUI.Ribbon/Styles/Fluent/Controls/RibbonToggleButton.axaml` | 修改 | QuickAccessTemplate 添加 `x:CompileBindings="False"` |
| `AvaloniaUI.Ribbon/Styles/Fluent/Controls/SplitButtonControl.axaml` | 修改 | 3 个 ControlTemplate 添加 `x:CompileBindings="False"` |
| `AvaloniaUI.Ribbon/Styles/Fluent/Controls/RibbonGroupBox.axaml` | 修改 | DisplayMode 绑定改用 `$parent` |
| `AvaloniaUI.Ribbon/Styles/Fluent/Controls/Gallery.axaml` | 修改 | TemplatedParent 改为 FindAncestor |
| `AvaloniaUI.Ribbon/Styles/Fluent/Res.axaml` | 修改 | `local:QuickAccessToolbar` → `local:QuickAccessProperties` |
| `AvaloniaUI.Ribbon/QuickAccessProperties.cs` | **新建** | 跨平台附加属性类 |
| `AvaloniaUI.Ribbon.Desktop/QuickAccessToolbar.cs` | 修改 | 移除 IsCheckedProperty，委托给 QuickAccessProperties |

## 风险点

1. **QuickAccessProperties 迁移**: 需要确保 `QuickAccessToolbar.cs` 中所有对 `IsCheckedProperty` 的引用都更新，否则运行时行为可能不一致。
2. **Gallery.axaml FindAncestor 绑定**: 需确认 `RepeatButton`/`ToggleButton` 的视觉树中确实能找到 `Gallery` 祖先。

## 验收标准

- `dotnet build --no-incremental` 零 AVLN 错误
- XAML 警告（NU1510 等）可存在
- CS 警告不在此范围
