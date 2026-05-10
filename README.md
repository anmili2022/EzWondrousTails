# WondrousTailsSolver

> 天书概率助手 / A Dalamud plugin that shows Wondrous Tails row probabilities inline.

`WondrousTailsSolver` 会在最终幻想 XIV 的天书界面（`Wondrous Tails` / `WeeklyBingo`）中实时显示 1 线、2 线、3 线概率，并提供重排（Shuffle）后的平均参考概率。  
This plugin adds real-time 1-line / 2-line / 3-line probabilities to the in-game Wondrous Tails display and also shows the average outcome after shuffle.

![math](https://github.com/user-attachments/assets/d4e00d8a-d3e9-4638-839a-2d93eb0ae928)

## Features / 功能

- 在天书界面内直接显示当前 1 线 / 2 线 / 3 线概率  
  Show current 1-line / 2-line / 3-line probabilities directly in the Wondrous Tails window
- 显示重排（Shuffle）后的平均参考概率  
  Show average reference probabilities after shuffle
- 概率文本追加在原说明文字末尾，减少额外占位  
  Append probability text to the end of the original instruction text to reduce UI clutter
- 提供颜色提示，便于快速判断  
  Provide color cues for quick reading
  - `> 50%`：绿色 / Green
  - `< 30%`：红色 / Red

## Download / 下载

- 最新发布页 / Latest release:  
  https://github.com/anmili2022/EzWondrousTails/releases/latest
- 主要文件 / Main assets:
  - `latest.zip`：插件发布包 / plugin release package
  - `WondrousTailsSolver.json`：仓库清单 / repository-feed manifest

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
