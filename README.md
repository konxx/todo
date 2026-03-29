# 桃神自用

一个原生 Windows 待办小工具，使用 WinForms 自绘界面实现，无浏览器窗口、无 R 运行时依赖。当前界面为无边框圆角样式，支持系统托盘、置顶和中英文切换，适合常驻桌面快速记录。

说明：
当前程序显示名为“桃神自用”，但可执行文件名和本地数据目录仍沿用 `TodoDesk`。

## 功能

- 添加、完成、删除待办事项
- 点击有备注的任务标题可展开或折叠备注
- 中文 / English 切换
- `Ctrl+N` 快捷键快速添加任务
- 置顶状态切换
- 右下角系统托盘支持显示、隐藏和退出
- 列表时间显示为任务添加时间，勾选完成后不会修改
- 本地数据保存到 `%APPDATA%\TodoDesk\tasks.json`
- 设置保存到 `%APPDATA%\TodoDesk\settings.json`

## 目录结构

```text
desktop-host/
  Program.cs            程序入口和基础控件
  Models.cs             数据模型、文案与配色
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
build/
  dev/                  默认开发输出
  verify/               临时验证输出
VERSION
```

## 本地运行

```cmd
.\launchers\start-dev.cmd
```

该脚本会先编译 `build\dev\TodoDesk.exe`，然后直接启动桌面程序。

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

- UI：WinForms 自绘无边框圆角窗口
- 持久化：JSON
- 托盘：`NotifyIcon`
- 打包：Inno Setup
