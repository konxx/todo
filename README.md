# Todo Desk

一个原生 Windows 待办应用，保留现有界面风格和布局，但不再依赖 Shiny、浏览器窗口或 R 运行时。程序现在直接由 WinForms 桌面应用承载，默认无 Windows 边框，并提供系统托盘入口。

## 功能

- 添加、完成、删除待办事项
- 点击有备注的任务标题可展开或折叠备注
- 中文 / English 双语切换
- `Ctrl+N` 快捷键快速添加任务
- 置顶状态切换
- 右下角系统托盘支持显示、隐藏和退出
- 本地数据保存到 `%APPDATA%\TodoDesk\tasks.json`
- 设置保存到 `%APPDATA%\TodoDesk\settings.json`

## 目录结构

```text
desktop-host/
  Program.cs            程序入口和基础控件
  Models.cs             数据模型和文案
  Drawing.cs            圆角与窗口拖拽辅助
  TodoDeskForm.cs       主窗口
  TaskItemControl.cs    任务列表项控件
  TaskDialogForm.cs     添加待办弹窗
launchers/
  start-dev.cmd         开发环境启动脚本
  start-todo.vbs        安装后兼容启动脚本
packaging/
  build-host.ps1        编译原生 exe
  build-installer.ps1   构建安装包
  TodoDesk.iss          Inno Setup 脚本
VERSION
```

## 本地运行

```cmd
.\launchers\start-dev.cmd
```

该脚本会先编译 `TodoDesk.exe`，然后直接启动原生桌面程序。

## 打包安装程序

### 依赖

- Windows 自带的 .NET Framework 编译器 `csc.exe`
- Inno Setup 6

### 构建命令

```powershell
powershell -ExecutionPolicy Bypass -File .\packaging\build-installer.ps1
```

如果 `ISCC.exe` 不在默认路径，可额外传入：

```powershell
-IsccPath "C:\Program Files (x86)\Inno Setup 6\ISCC.exe"
```

### 构建结果

- 中间目录：`build/stage`
- 安装包输出：`build/installer/TodoDesk-Setup-<version>.exe`

## 技术说明

- UI：WinForms 自绘无边框窗口
- 持久化：JSON
- 托盘：`NotifyIcon`
- 打包：Inno Setup
