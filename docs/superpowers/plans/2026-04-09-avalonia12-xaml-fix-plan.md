# Avalonia 12 XAML 编译修复实现计划

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** 修复 Avalonia 12 升级后 22 处 XAML 编译错误（AVLN），实现零错误构建。

**Architecture:** 逐文件修复 XAML 编译问题——为 QuickAccessTemplate 关闭 compiled bindings、修正绑定路径语法、迁移跨项目附加属性到共享位置。

**Tech Stack:** Avalonia 12, XAML, C#, .NET 10

---

## 文件结构

| 文件 | 操作 | 说明 |
|------|------|------|
| `AvaloniaUI.Ribbon/QuickAccessProperties.cs` | **新建** | 跨平台附加属性类，存放 `IsCheckedProperty` |
| `AvaloniaUI.Ribbon/Styles/Fluent/Controls/RibbonButton.axaml` | 修改 | QuickAccessTemplate 关闭 compiled bindings |
| `AvaloniaUI.Ribbon/Styles/Fluent/Controls/RibbonDropDownButton.axaml` | 修改 | QuickAccessTemplate 关闭 compiled bindings |
| `AvaloniaUI.Ribbon/Styles/Fluent/Controls/RibbonSplitButton.axaml` | 修改 | QuickAccessTemplate + Popup 关闭 compiled bindings |
| `AvaloniaUI.Ribbon/Styles/Fluent/Controls/RibbonToggleButton.axaml` | 修改 | QuickAccessTemplate 关闭 compiled bindings |
| `AvaloniaUI.Ribbon/Styles/Fluent/Controls/SplitButtonControl.axaml` | 修改 | 3 个 ControlTemplate 关闭 compiled bindings |
| `AvaloniaUI.Ribbon/Styles/Fluent/Controls/RibbonGroupBox.axaml` | 修改 | DisplayMode 绑定改用 $parent |
| `AvaloniaUI.Ribbon/Styles/Fluent/Controls/Gallery.axaml` | 修改 | TemplatedParent 改为 FindAncestor |
| `AvaloniaUI.Ribbon/Styles/Fluent/Res.axaml` | 修改 | 使用 QuickAccessProperties 替代 QuickAccessToolbar |
| `AvaloniaUI.Ribbon.Desktop/QuickAccessToolbar.cs` | 修改 | 委托 IsChecked 到 QuickAccessProperties |

---

## Task 1: 创建跨平台附加属性类 QuickAccessProperties

**Files:**
- Create: `AvaloniaUI.Ribbon/QuickAccessProperties.cs`

**背景**: `QuickAccessToolbar.IsChecked` 是附加属性，定义在 Desktop 项目中。Res.axaml（跨平台项目）引用它导致编译错误。需要将其移到跨平台项目。

- [ ] **Step 1: 创建 QuickAccessProperties.cs**

```csharp
using Avalonia;
using Avalonia.Controls;

namespace AvaloniaUI.Ribbon;

/// <summary>
/// Attached properties for QuickAccess functionality.
/// Shared across platform-specific and cross-platform projects.
/// </summary>
public static class QuickAccessProperties
{
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
```

- [ ] **Step 2: 验证编译**

```bash
dotnet build AvaloniaUI.Ribbon/AvaloniaUI.Ribbon.csproj --no-incremental
```

期望：编译成功（可能仍有 CS 警告，但无 AVLN 错误新增）。

- [ ] **Step 3: Commit**

```bash
git add AvaloniaUI.Ribbon/QuickAccessProperties.cs
git commit -m "feat: add QuickAccessProperties attached property class for cross-platform use"
```

---

## Task 2: 更新 RibbonButton.axaml — QuickAccessTemplate 关闭 compiled bindings

**Files:**
- Modify: `AvaloniaUI.Ribbon/Styles/Fluent/Controls/RibbonButton.axaml`

- [ ] **Step 1: 修改 QuickAccessTemplate**

找到 `<ControlTemplate>`（QuickAccessTemplate 的值），在其上添加 `x:CompileBindings="False"`：

```xaml
<!-- Before (line ~59) -->
<Setter Property="QuickAccessTemplate">
    <ControlTemplate>
        <Button Classes="quickAccessButton" Command="{Binding Command}"

<!-- After -->
<Setter Property="QuickAccessTemplate">
    <ControlTemplate x:CompileBindings="False">
        <Button Classes="quickAccessButton" Command="{Binding Command}"
```

- [ ] **Step 2: 验证编译**

```bash
dotnet build AvaloniaUI.Ribbon/AvaloniaUI.Ribbon.csproj --no-incremental 2>&1 | findstr "AVLN2100.*RibbonButton"
```

期望：无输出（错误已修复）。

- [ ] **Step 3: Commit**

```bash
git add AvaloniaUI.Ribbon/Styles/Fluent/Controls/RibbonButton.axaml
git commit -m "fix: disable compiled bindings in RibbonButton QuickAccessTemplate for Avalonia 12"
```

---

## Task 3: 更新 RibbonDropDownButton.axaml

**Files:**
- Modify: `AvaloniaUI.Ribbon/Styles/Fluent/Controls/RibbonDropDownButton.axaml`

- [ ] **Step 1: 修改 QuickAccessTemplate**

```xaml
<!-- Before (line ~14) -->
<Setter Property="QuickAccessTemplate">
    <ControlTemplate>
        <RibbonDropDownButton

<!-- After -->
<Setter Property="QuickAccessTemplate">
    <ControlTemplate x:CompileBindings="False">
        <RibbonDropDownButton
```

- [ ] **Step 2: 验证编译**

```bash
dotnet build AvaloniaUI.Ribbon/AvaloniaUI.Ribbon.csproj --no-incremental 2>&1 | findstr "AVLN2100.*RibbonDropDownButton"
```

期望：无输出。

- [ ] **Step 3: Commit**

```bash
git add AvaloniaUI.Ribbon/Styles/Fluent/Controls/RibbonDropDownButton.axaml
git commit -m "fix: disable compiled bindings in RibbonDropDownButton QuickAccessTemplate for Avalonia 12"
```

---

## Task 4: 更新 RibbonSplitButton.axaml

**Files:**
- Modify: `AvaloniaUI.Ribbon/Styles/Fluent/Controls/RibbonSplitButton.axaml`

- [ ] **Step 1: 修改 QuickAccessTemplate**

```xaml
<!-- Before (line ~10) -->
<Setter Property="QuickAccessTemplate">
    <ControlTemplate>
        <RibbonSplitButton

<!-- After -->
<Setter Property="QuickAccessTemplate">
    <ControlTemplate x:CompileBindings="False">
        <RibbonSplitButton
```

- [ ] **Step 2: 修改 3 个 ControlTemplate（Large/Medium/Small 尺寸）**

每个 `ControlTemplate` 根元素添加 `x:CompileBindings="False"`：

```xaml
<!-- Before (Large template, line ~37) -->
<ControlTemplate>
    <Border Classes="RibbonButtonBackgroundBorder">
        <StackPanel Orientation="Vertical">
            ...
            <Popup Name="PART_Popup" ...>

<!-- After -->
<ControlTemplate x:CompileBindings="False">
    <Border Classes="RibbonButtonBackgroundBorder">
        <StackPanel Orientation="Vertical">
            ...
            <Popup Name="PART_Popup" ...>
```

```xaml
<!-- Before (Medium template, line ~96) -->
<ControlTemplate>
    <Border Classes="RibbonButtonBackgroundBorder">

<!-- After -->
<ControlTemplate x:CompileBindings="False">
    <Border Classes="RibbonButtonBackgroundBorder">
```

```xaml
<!-- Before (Small template, line ~150) -->
<ControlTemplate>
    <Border Classes="RibbonButtonBackgroundBorder">

<!-- After -->
<ControlTemplate x:CompileBindings="False">
    <Border Classes="RibbonButtonBackgroundBorder">
```

- [ ] **Step 3: 验证编译**

```bash
dotnet build AvaloniaUI.Ribbon/AvaloniaUI.Ribbon.csproj --no-incremental 2>&1 | findstr "AVLN"
```

期望：不再有 RibbonSplitButton 相关的 AVLN 错误。

- [ ] **Step 4: Commit**

```bash
git add AvaloniaUI.Ribbon/Styles/Fluent/Controls/RibbonSplitButton.axaml
git commit -m "fix: disable compiled bindings in RibbonSplitButton templates for Avalonia 12"
```

---

## Task 5: 更新 RibbonToggleButton.axaml

**Files:**
- Modify: `AvaloniaUI.Ribbon/Styles/Fluent/Controls/RibbonToggleButton.axaml`

- [ ] **Step 1: 修改 QuickAccessTemplate**

```xaml
<!-- Before (line ~48) -->
<Setter Property="QuickAccessTemplate">
    <ControlTemplate>
        <ToggleButton Classes="quickAccessButton" Command="{Binding Command}"

<!-- After -->
<Setter Property="QuickAccessTemplate">
    <ControlTemplate x:CompileBindings="False">
        <ToggleButton Classes="quickAccessButton" Command="{Binding Command}"
```

- [ ] **Step 2: 验证编译**

```bash
dotnet build AvaloniaUI.Ribbon/AvaloniaUI.Ribbon.csproj --no-incremental 2>&1 | findstr "AVLN2100.*RibbonToggleButton"
```

期望：无输出。

- [ ] **Step 3: Commit**

```bash
git add AvaloniaUI.Ribbon/Styles/Fluent/Controls/RibbonToggleButton.axaml
git commit -m "fix: disable compiled bindings in RibbonToggleButton QuickAccessTemplate for Avalonia 12"
```

---

## Task 6: 更新 SplitButtonControl.axaml

**Files:**
- Modify: `AvaloniaUI.Ribbon/Styles/Fluent/Controls/SplitButtonControl.axaml`

这个文件有 3 个 ControlTemplate（LargeSplitButton, MediumSplitButton, SmallSplitButton），每个都包含 QuickAccessTemplate。

- [ ] **Step 1: 修改 LargeSplitButton ControlTemplate**

```xaml
<!-- Before (line ~30) -->
<Setter Property="Template">
    <ControlTemplate>
        <Grid>

<!-- After -->
<Setter Property="Template">
    <ControlTemplate x:CompileBindings="False">
        <Grid>
```

- [ ] **Step 2: 修改 LargeSplitButton QuickAccessTemplate**

```xaml
<!-- Before (line ~78) -->
<Setter Property="QuickAccessTemplate">
    <ControlTemplate>
        <local:SplitButtonControl

<!-- After -->
<Setter Property="QuickAccessTemplate">
    <ControlTemplate x:CompileBindings="False">
        <local:SplitButtonControl
```

- [ ] **Step 3: 修改 MediumSplitButton ControlTemplate**

```xaml
<!-- Before (line ~110) -->
<Setter Property="Template">
    <ControlTemplate>
        <Grid>

<!-- After -->
<Setter Property="Template">
    <ControlTemplate x:CompileBindings="False">
        <Grid>
```

- [ ] **Step 4: 修改 MediumSplitButton QuickAccessTemplate**

```xaml
<!-- Before (line ~178) -->
<Setter Property="QuickAccessTemplate">
    <ControlTemplate>
        <local:SplitButtonControl

<!-- After -->
<Setter Property="QuickAccessTemplate">
    <ControlTemplate x:CompileBindings="False">
        <local:SplitButtonControl
```

- [ ] **Step 5: 修改 SmallSplitButton ControlTemplate**

```xaml
<!-- Before (line ~208) -->
<Setter Property="Template">
    <ControlTemplate>
        <Grid>

<!-- After -->
<Setter Property="Template">
    <ControlTemplate x:CompileBindings="False">
        <Grid>
```

- [ ] **Step 6: 修改 SmallSplitButton QuickAccessTemplate**

```xaml
<!-- Before (line ~278) -->
<Setter Property="QuickAccessTemplate">
    <ControlTemplate>
        <local:SplitButtonControl

<!-- After -->
<Setter Property="QuickAccessTemplate">
    <ControlTemplate x:CompileBindings="False">
        <local:SplitButtonControl
```

- [ ] **Step 7: 验证编译**

```bash
dotnet build AvaloniaUI.Ribbon/AvaloniaUI.Ribbon.csproj --no-incremental 2>&1 | findstr "AVLN"
```

期望：不再有 SplitButtonControl 相关的 AVLN 错误。

- [ ] **Step 8: Commit**

```bash
git add AvaloniaUI.Ribbon/Styles/Fluent/Controls/SplitButtonControl.axaml
git commit -m "fix: disable compiled bindings in SplitButtonControl templates for Avalonia 12"
```

---

## Task 7: 更新 RibbonGroupBox.axaml — DisplayMode 绑定修复

**Files:**
- Modify: `AvaloniaUI.Ribbon/Styles/Fluent/Controls/RibbonGroupBox.axaml`

- [ ] **Step 1: 修改 DisplayMode 绑定**

找到 `RibbonGroupWrapPanel`（约187行），修改绑定方式：

```xaml
<!-- Before -->
<ItemsPresenter.ItemsPanel>
    <ItemsPanelTemplate>
        <RibbonGroupWrapPanel
            x:Name="PART_ItemsPanel"
            HorizontalAlignment="Stretch"
            VerticalAlignment="Stretch"
            DisplayMode="{Binding DisplayMode, RelativeSource={RelativeSource TemplatedParent}}" />
    </ItemsPanelTemplate>
</ItemsPresenter.ItemsPanel>

<!-- After -->
<ItemsPresenter.ItemsPanel>
    <ItemsPanelTemplate>
        <RibbonGroupWrapPanel
            x:Name="PART_ItemsPanel"
            HorizontalAlignment="Stretch"
            VerticalAlignment="Stretch"
            DisplayMode="{Binding $parent[local|RibbonGroupBox].DisplayMode}" />
    </ItemsPanelTemplate>
</ItemsPresenter.ItemsPanel>
```

- [ ] **Step 2: 验证编译**

```bash
dotnet build AvaloniaUI.Ribbon/AvaloniaUI.Ribbon.csproj --no-incremental 2>&1 | findstr "AVLN2000.*DisplayMode"
```

期望：无输出。

- [ ] **Step 3: Commit**

```bash
git add AvaloniaUI.Ribbon/Styles/Fluent/Controls/RibbonGroupBox.axaml
git commit -m "fix: use $parent binding for RibbonGroupBox DisplayMode in ItemsPanelTemplate"
```

---

## Task 8: 更新 Gallery.axaml — TemplatedParent 改为 FindAncestor

**Files:**
- Modify: `AvaloniaUI.Ribbon/Styles/Fluent/Controls/Gallery.axaml`

- [ ] **Step 1: 修改 Style Selector 中的 BorderBrush 绑定**

找到约345行的 Style，修改绑定：

```xaml
<!-- Before -->
<Style Selector="local|Gallery /template/ RepeatButton,
              local|Gallery /template/ ToggleButton">
    <Setter Property="BorderBrush"
            Value="{Binding BorderBrush, RelativeSource={RelativeSource Mode=TemplatedParent}}" />

<!-- After -->
<Style Selector="local|Gallery /template/ RepeatButton,
              local|Gallery /template/ ToggleButton">
    <Setter Property="BorderBrush"
            Value="{Binding $parent[local|Gallery].BorderBrush}" />
```

- [ ] **Step 2: 验证编译**

```bash
dotnet build AvaloniaUI.Ribbon/AvaloniaUI.Ribbon.csproj --no-incremental 2>&1 | findstr "AVLN2100.*Gallery"
```

期望：无输出。

- [ ] **Step 3: Commit**

```bash
git add AvaloniaUI.Ribbon/Styles/Fluent/Controls/Gallery.axaml
git commit -m "fix: replace TemplatedParent with $parent[Gallery] in Gallery style binding"
```

---

## Task 9: 更新 Res.axaml — 使用 QuickAccessProperties

**Files:**
- Modify: `AvaloniaUI.Ribbon/Styles/Fluent/Res.axaml`

- [ ] **Step 1: 确认 namespace 声明**

文件顶部已有 `xmlns:local="clr-namespace:AvaloniaUI.Ribbon;assembly=AvaloniaUI.Ribbon"`，无需添加新 namespace（`QuickAccessProperties` 在同命名空间）。

- [ ] **Step 2: 替换 QuickAccessToolbar 为 QuickAccessProperties**

```xaml
<!-- Before (line ~39) -->
<ContentPresenter Name="PART_IconPresenter"
                  Content="{TemplateBinding Icon}"
                  IsVisible="{Binding !(local:QuickAccessToolbar.IsChecked), RelativeSource={RelativeSource Mode=TemplatedParent}}" />

<!-- After -->
<ContentPresenter Name="PART_IconPresenter"
                  Content="{TemplateBinding Icon}"
                  IsVisible="{Binding !(local:QuickAccessProperties.IsChecked), RelativeSource={RelativeSource Mode=TemplatedParent}}" />
```

```xaml
<!-- Before (line ~43) -->
<Path x:Name="CheckGlyph"
    Fill="{TemplateBinding Foreground}"
    Data="M1507 31L438 1101L-119 543L-29 453L438 919L1417 -59L1507 31Z"
    IsVisible="{Binding (local:QuickAccessToolbar.IsChecked), RelativeSource={RelativeSource Mode=TemplatedParent}}"
    Width="9"

<!-- After -->
<Path x:Name="CheckGlyph"
    Fill="{TemplateBinding Foreground}"
    Data="M1507 31L438 1101L-119 543L-29 453L438 919L1417 -59L1507 31Z"
    IsVisible="{Binding (local:QuickAccessProperties.IsChecked), RelativeSource={RelativeSource Mode=TemplatedParent}}"
    Width="9"
```

- [ ] **Step 3: 验证编译**

```bash
dotnet build AvaloniaUI.Ribbon/AvaloniaUI.Ribbon.csproj --no-incremental 2>&1 | findstr "AVLN2000.*QuickAccess"
```

期望：无输出。

- [ ] **Step 4: Commit**

```bash
git add AvaloniaUI.Ribbon/Styles/Fluent/Res.axaml
git commit -m "fix: use QuickAccessProperties instead of QuickAccessToolbar in Res.axaml"
```

---

## Task 10: 更新 QuickAccessToolbar.cs — 委托给 QuickAccessProperties

**Files:**
- Modify: `AvaloniaUI.Ribbon.Desktop/QuickAccessToolbar.cs`

- [ ] **Step 1: 移除 IsCheckedProperty 定义，添加委托**

```csharp
// Before (在 Fields 区域，约第138行)
public static readonly AttachedProperty<bool> IsCheckedProperty =
    AvaloniaProperty.RegisterAttached<QuickAccessToolbar, MenuItem, bool>("IsChecked");

public static bool GetIsChecked(MenuItem element)
{
    return element.GetValue(IsCheckedProperty);
}

public static void SetIsChecked(MenuItem element, bool value)
{
    element.SetValue(IsCheckedProperty, value);
}

// After — 替换为委托
/// <summary>
/// Deprecated. Use <see cref="QuickAccessProperties.IsCheckedProperty"/> instead.
/// Kept for API compatibility.
/// </summary>
[Obsolete("Use QuickAccessProperties.IsCheckedProperty instead.")]
public static readonly AttachedProperty<bool> IsCheckedProperty = QuickAccessProperties.IsCheckedProperty;

[Obsolete("Use QuickAccessProperties.GetIsChecked instead.")]
public static bool GetIsChecked(MenuItem element)
{
    return QuickAccessProperties.GetIsChecked(element);
}

[Obsolete("Use QuickAccessProperties.SetIsChecked instead.")]
public static void SetIsChecked(MenuItem element, bool value)
{
    QuickAccessProperties.SetIsChecked(element, value);
}
```

- [ ] **Step 2: 更新 static 构造函数中对 IsCheckedProperty 的引用**

找到 static 构造函数中：

```csharp
// Before (约第157行)
RibbonProperty.Changed.AddClassHandler<QuickAccessToolbar>((sender, e) =>
{
    if (sender.Ribbon != null)
        sender._collapseRibbonItem[!IsCheckedProperty] = sender.Ribbon[!DesktopRibbon.IsCollapsedProperty];
    else
        SetIsChecked(sender._collapseRibbonItem, false);
});

// After — 改用 QuickAccessProperties.IsCheckedProperty
RibbonProperty.Changed.AddClassHandler<QuickAccessToolbar>((sender, e) =>
{
    if (sender.Ribbon != null)
        sender._collapseRibbonItem[!QuickAccessProperties.IsCheckedProperty] = sender.Ribbon[!DesktopRibbon.IsCollapsedProperty];
    else
        QuickAccessProperties.SetIsChecked(sender._collapseRibbonItem, false);
});
```

- [ ] **Step 3: 构建整个解决方案**

```bash
dotnet build AvaloniaRibbon.sln --no-incremental
```

期望：零 AVLN 错误。CS 警告可存在。

- [ ] **Step 4: Commit**

```bash
git add AvaloniaUI.Ribbon.Desktop/QuickAccessToolbar.cs
git commit -m "refactor: delegate QuickAccess IsChecked to shared QuickAccessProperties"
```

---

## Task 11: 全解决方案构建验证

**Files:**
- 全部项目

- [ ] **Step 1: 完整构建**

```bash
dotnet build AvaloniaRibbon.sln --no-incremental
```

- [ ] **Step 2: 验证无 AVLN 错误**

```bash
dotnet build AvaloniaRibbon.sln --no-incremental 2>&1 | findstr "error AVLN"
```

期望：**无输出**。如果有 AVLN 错误输出，回到对应 Task 修复。

- [ ] **Step 3: 确认构建成功**

```bash
dotnet build AvaloniaRibbon.sln --no-incremental 2>&1 | findstr "Build succeeded"
```

期望：输出包含 `Build succeeded.`

- [ ] **Step 4: 最终 Commit**

```bash
git status
git add -A
git commit -m "fix: resolve all Avalonia 12 XAML compilation errors (AVLN)"
```

- [ ] **Step 5: 确认 git 状态干净**

```bash
git status
```

---

## 自审

### Spec Coverage 检查

| 设计文档要求 | 对应 Task | 状态 |
|-------------|-----------|------|
| AVLN2100/2101 - Compiled Binding（13处） | Task 2,3,4,5,6 | ✅ 覆盖 |
| AVLN2000 - DisplayMode（1处） | Task 7 | ✅ 覆盖 |
| AVLN2000 - Deferred Content（6处） | Task 4,6 | ✅ 覆盖 |
| AVLN2100 - TemplatedParent（1处） | Task 8 | ✅ 覆盖 |
| AVLN2000 - QuickAccessToolbar（2处） | Task 1,9,10 | ✅ 覆盖 |
| 验收标准：零 AVLN 错误 | Task 11 | ✅ 覆盖 |

### Placeholder 扫描
- 无 TBD/TODO
- 所有步骤包含完整代码
- 所有步骤包含具体命令和预期输出

### 类型一致性
- `QuickAccessProperties.IsCheckedProperty` 在 Task 1 定义，在 Task 9（Res.axaml）和 Task 10（QuickAccessToolbar.cs）中一致引用 ✅
- `local|RibbonGroupBox` 在 Task 7 的 `$parent` 绑定中使用正确 ✅
- `local|Gallery` 在 Task 8 的 `$parent` 绑定中使用正确 ✅

---

计划完整。请确认执行方式：

**方案 1: Subagent-Driven（推荐）** — 每个 Task 分派独立 agent 执行，任务间人工审查，快速迭代。

**方案 2: Inline Execution** — 在当前会话顺序执行所有 Task，批量处理带检查点。

选择哪个？
