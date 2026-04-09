# 设计文档：统一升级到 Avalonia 12 + net10.0 修复编译错误

## 问题

`AvaloniaUI.Ribbon.Desktop` 项目目标框架为 `netstandard2.0`，引用了 Avalonia 11.3.9。它通过 `ProjectReference` 引用了 `AvaloniaUI.Ribbon` 项目，后者目标框架为 `net10.0`，使用 Avalonia 12.0.0。框架不兼容导致 NU1201 错误。

此外，`System.Text.Json` 10.0.0 不兼容 `netstandard2.0`，触发 NU1510 警告。

## 方案

将所有项目统一到 `net10.0` + Avalonia 12.0.0 生态。

## 变更清单

### 1. AvaloniaUI.Ribbon.Desktop.csproj

| 属性 | 旧值 | 新值 |
|------|------|------|
| TargetFrameworks | netstandard2.0 | net10.0 |
| Avalonia | 11.3.9 | 12.0.0 |
| Xaml.Behaviors.Avalonia | 11.3.9 | 12.0.0 |
| System.Text.Json | 10.0.0 (删除) | — |

### 2. Demo 项目检查

检查以下项目的目标框架和 Avalonia 版本，确保一致：
- AvaloniaUI.Ribbon.Demo
- AvaloniaUI.Ribbon.Demo.Desktop
- AvaloniaUI.Ribbon.Demo.Browser

若有版本不一致，升级到 Avalonia 12.0.0 + net10.0。

### 3. 代码兼容性验证

升级后重新构建，检查是否有 Avalonia 11→12 的 breaking changes 导致的 CS 编译错误。

## 成功标准

- `dotnet build` 零错误、零 NUxxx 警告
- 所有项目目标框架统一为 net10.0
- 所有 Avalonia 相关包版本统一为 12.0.0
