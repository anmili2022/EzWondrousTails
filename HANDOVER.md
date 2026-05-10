# WondrousTailsSolver Handover

更新日期：2026-05-10

> 这份文档写给维护者，不是写给普通用户的。  
> README 会链接到这里，用来集中放维护、构建、发版与注意事项。

## 1. 项目概览

`WondrousTailsSolver` 是一个 Dalamud 插件，用于在 FFXIV 的天书（Wondrous Tails / `WeeklyBingo`）界面中实时显示：

- 当前棋盘的 1 线 / 2 线 / 3 线概率
- 当前贴纸数下，重排（Shuffle）后的平均参考概率

当前界面内显示方式为：

- 在天书左下角原说明文字末尾追加两行
- `概率：1:xx.xx% 2:xx.xx% 3:xx.xx%`
- `重排：1:xx.xx% 2:xx.xx% 3:xx.xx%`

并带有简单颜色提示：

- 大于 `50%`：绿色
- 小于 `30%`：红色

## 2. 当前仓库状态

- 工作目录：`E:\git\EzWondrousTails`
- 远程仓库：`origin = https://github.com/anmili2022/EzWondrousTails`
- 当前主分支：`main`
- 最新发版版本：`3.2.2.11`
- 最新发版标签：`v3.2.2.11`
- 当前提交请以本地命令为准：`git rev-parse --short HEAD`
- Dalamud API Level：`15`
- Target Framework：`net10.0-windows`

> 旧版交接记录里提到的 `master`、旧提交号和旧 tag 已经过时，以当前仓库状态为准。

## 3. 关键目录与文件

### 仓库顶层

- `README.md`：项目简介与交接文档入口
- `HANDOVER.md`：维护交接文档
- `.github/workflows/release.yml`：GitHub Actions 发版工作流
- `WondrousTailsSolver.sln`：解决方案
- `WondrousTailsSolver/`：主插件项目

### 主项目核心文件

- `WondrousTailsSolver/WondrousTailsSolverPlugin.cs`
  - 插件入口
  - 初始化 `PerfectTails` 与 `AddonWeeklyBingoController`

- `WondrousTailsSolver/AddonWeeklyBingoController.cs`
  - 监听 `WeeklyBingo` 生命周期
  - 读取游戏内贴纸状态
  - 查找并更新说明文本节点
  - 负责把概率信息写回游戏界面

- `WondrousTailsSolver/PerfectTails.cs`
  - 概率计算核心
  - 生成主窗口文本
  - 生成行内显示文本
  - 负责彩色 `SeString` 输出

- `WondrousTailsSolver/DalamudServices.cs`
  - 初始化并暴露 Dalamud 服务
  - 当前主要用到 `IAddonLifecycle` 和 `IGameGui`

- `WondrousTailsSolver/System.cs`
  - 轻量静态容器
  - 保存全局 `PerfectTails` 与 `AddonWeeklyBingoController`

## 4. 当前实现要点

### 4.1 概率显示注入逻辑

当前逻辑不再只依赖单一提示文案（如“空白处贴上印花”）进行识别，而是会兼容多种天书说明状态，例如：

- 初始可贴印花状态
- 已贴出故事线状态
- 可以领奖但仍有可贴空间状态
- 贴满 9 个印花后的说明状态

这样做是为了避免在说明文案变化后，概率文字完全不显示。

### 4.2 文本位置与显示风格

当前概率信息会：

- 追加到原说明文字末尾
- 使用更短的行内格式，减少占位
- 使用 `SeString` 写入 `AtkTextNode`
- 支持概率值着色显示

### 4.3 UI 安全性

当前实现**避免在 `PreFinalize` 和插件销毁阶段修改 `WeeklyBingo` 文本节点**。

原因：

- finalize 阶段继续改 UI 节点，容易引发崩溃或未定义行为

当前做法是在这些阶段只清理内部状态，不再对节点进行恢复性写入。

## 5. 本地构建与输出

### Debug 构建

在仓库根目录执行：

```powershell
dotnet build .\WondrousTailsSolver.sln -c Debug
```

主要输出：

```text
WondrousTailsSolver\output\
```

当前这个目录用于本地开发态加载，通常是最需要关注的输出位置。

### Release 构建

在仓库根目录执行：

```powershell
dotnet build .\WondrousTailsSolver.sln -c Release
```

当前本机实际主要输出：

```text
WondrousTailsSolver\bin\x64\Release\
```

打包后的关键文件通常位于：

```text
WondrousTailsSolver\bin\x64\Release\WondrousTailsSolver\
```

其中重要产物为：

- `latest.zip`
- `WondrousTailsSolver.json`

> 说明：仓库里可能仍能看到较早时期的 `WondrousTailsSolver\bin\Release\...` 产物。  
> 以**当前构建实际更新时间最新**的输出目录为准；本机上通常是 `bin\x64\Release\...`。

## 6. 本地开发建议

### 6.1 改动后优先看 Debug 输出

如果是在本机直接用开发插件方式测试，优先确认：

```text
WondrousTailsSolver\output\WondrousTailsSolver.dll
```

是否已经被最新 `Debug` 构建刷新。

### 6.2 UI 改动优先在游戏里看截图

这个插件的改动大多是“看起来很小、实际非常依赖具体布局”的 UI 细节：

- 文本是否显示
- 是否被裁切
- 是否被其他说明覆盖
- 插入位置是否自然
- 颜色是否明显

所以每次改说明文本相关逻辑，建议都配合游戏内截图验证。

## 7. 发版工作流

工作流文件：

```text
.github\workflows\release.yml
```

### 触发方式

1. 推送符合 `v*` 的 tag，例如 `v3.2.2.11`
2. 或在 GitHub Actions 中手动触发 `workflow_dispatch`

### 工作流行为

1. Checkout 指定 tag
2. 安装 .NET 10 SDK
3. 校验 tag 必须以 `v` 开头
4. 下载 Dalamud runtime
5. 构建 `WondrousTailsSolver\WondrousTailsSolver.csproj` 的 `Release`
6. 在 `WondrousTailsSolver\bin` 下递归查找打包资产目录
7. 上传：
   - `latest.zip`
   - `WondrousTailsSolver.json`

### 版本规则

- 项目版本写在：`WondrousTailsSolver\WondrousTailsSolver.csproj`
- `csproj` 里的版本号**不带** `v`
- Git tag **带** `v`

示例：

- csproj：`3.2.2.12`
- tag：`v3.2.2.12`

## 8. 快速发版流程

当你已经完成修复并确认可以发布时：

1. 更新版本号：

```xml
<Version>3.2.2.12</Version>
```

2. 本地构建验证：

```powershell
dotnet build .\WondrousTailsSolver.sln -c Release
```

3. 确认关键产物存在：

- `WondrousTailsSolver\bin\x64\Release\WondrousTailsSolver\latest.zip`
- `WondrousTailsSolver\bin\x64\Release\WondrousTailsSolver\WondrousTailsSolver.json`

4. 提交需要发版的文件，例如：

```powershell
git add WondrousTailsSolver/WondrousTailsSolver.csproj
git add WondrousTailsSolver/AddonWeeklyBingoController.cs
git add WondrousTailsSolver/PerfectTails.cs
git commit -m "Describe the release"
```

5. 推送分支：

```powershell
git push origin main
```

6. 创建并推送 tag：

```powershell
git -c tag.gpgSign=false tag v3.2.2.12
git push origin v3.2.2.12
```

7. 检查 GitHub Release 是否生成成功：

- Actions：`.github/workflows/release.yml`
- Releases：`https://github.com/anmili2022/EzWondrousTails/releases`

### 8.1 固定发版速查清单

下次发版时可以直接照这个顺序走：

1. 改 `WondrousTailsSolver.csproj` 里的 `<Version>`
2. 本地执行 `dotnet build .\WondrousTailsSolver.sln -c Release`
3. 检查：
   - `latest.zip`
   - `WondrousTailsSolver.json`
4. 提交代码与版本号变更
5. `git push origin main`
6. `git -c tag.gpgSign=false tag vX.Y.Z.N`
7. `git push origin vX.Y.Z.N`
8. 去 GitHub 看：
   - Build 是否成功
   - Release 是否成功
   - Release 页面资产是否齐全

## 9. 机器相关备注

### 9.1 GPG Tag

这台机器之前存在：

- `tag.gpgSign=true`

如果没有配置可无人值守使用的 GPG，直接执行：

```powershell
git tag v3.2.2.11
```

可能会卡住或失败。

更稳妥的方式是：

```powershell
git -c tag.gpgSign=false tag v3.2.2.11
```

### 9.2 本地 Markdown 忽略

如果你发现本机 `git status` 里看不到新建的 `.md` 文件，请检查：

```text
.git\info\exclude
```

当前本机可能使用了本地排除规则忽略 `*.md`。  
这是**本地 Git 配置**，不会自动同步到远程仓库。

## 10. 建议关注的后续方向

如果后面还要继续迭代，这几个方向优先级较高：

1. 继续观察不同分辨率、UI 缩放下的说明文本布局
2. 视需要把颜色阈值改成可配置项
3. 视需要给主窗口或设置窗口增加显示开关
4. 若 Dalamud 或 FFXIVClientStructs 升级，优先复查：
   - `IAddonLifecycle`
   - `IGameGui`
   - `AtkTextNode` 写入方式
   - `WeeklyBingo` 节点识别逻辑

## 11. 最后结论

当前项目已经处于：

- 可正常构建
- 可正常在天书界面显示概率
- 文本位置与可读性基本稳定
- 发版流程已可用

如果下一位维护者接手，建议先做三件事：

1. 读 `README.md`
2. 读本文档
3. 在游戏内打开天书界面，实际看一次当前显示效果
