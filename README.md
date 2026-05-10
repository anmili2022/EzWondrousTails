# WondrousTailsSolver

> 天书概率助手 / Inline Wondrous Tails probability helper for Dalamud.

`WondrousTailsSolver` 会在最终幻想 XIV 的天书界面（`Wondrous Tails` / `WeeklyBingo`）中，直接把 1 线、2 线、3 线概率显示在原说明文字附近，并提供重排（Shuffle）后的平均参考概率。

`WondrousTailsSolver` adds inline 1-line / 2-line / 3-line probabilities to the in-game Wondrous Tails window and also shows average reference odds after shuffle.

![math](https://github.com/user-attachments/assets/d4e00d8a-d3e9-4638-839a-2d93eb0ae928)

## Features / 功能

- 在天书界面内直接显示当前 1 线 / 2 线 / 3 线概率
  - Show current 1-line / 2-line / 3-line probabilities directly in the Wondrous Tails window
- 显示重排（Shuffle）后的平均参考概率
  - Show average reference probabilities after shuffle
- 把概率信息追加到原说明文字末尾，尽量减少界面占位
  - Append probability details to the end of the original instruction text to keep the UI compact
- 用颜色帮助快速判断结果好坏
  - Use color cues for faster reading
  - `> 50%`：绿色 / Green
  - `< 30%`：红色 / Red

## Download / 下载

- 最新发布页 / Latest release:
  https://github.com/anmili2022/EzWondrousTails/releases/latest
- 主要文件 / Main assets:
  - `latest.zip`：插件发布包，可直接用于安装或更新 / plugin release package for direct install or update
  - `WondrousTailsSolver.json`：插件源清单文件 / manifest file for plugin feeds

## Build / 构建

在仓库根目录执行 / Run from the repository root:

```powershell
dotnet build .\WondrousTailsSolver.sln -c Debug
dotnet build .\WondrousTailsSolver.sln -c Release
```

- Debug 输出目录 / Debug output: `WondrousTailsSolver\output\`
- Release 产物目录 / Release output: `WondrousTailsSolver\bin\x64\Release\WondrousTailsSolver\`

## Maintenance / 维护

- 维护交接文档 / Maintainer handover: [HANDOVER.md](HANDOVER.md)
- GitHub Actions: https://github.com/anmili2022/EzWondrousTails/actions
- GitHub Releases: https://github.com/anmili2022/EzWondrousTails/releases
- Dalamud API 文档 / Docs: https://dalamud.dev/api/

更详细的构建说明、发版流程和维护注意事项请查看 `HANDOVER.md`。

For detailed build notes, release steps, and maintenance guidance, see `HANDOVER.md`.

## Credits / 致谢

- Original authors: `daemitus`, `MidoriKami`