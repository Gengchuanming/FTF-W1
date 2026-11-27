@echo off
chcp 65001 >nul
title Filee MSIX 打包工具

echo ========================================
echo Filee MSIX 打包工具
echo ========================================
echo.

REM 检查PowerShell是否安装
where powershell >nul 2>&1
if %errorlevel% neq 0 (
    echo [错误] 未找到PowerShell，请先安装PowerShell
    pause
    exit /b 1
)

REM 检查dotnet是否安装
where dotnet >nul 2>&1
if %errorlevel% neq 0 (
    echo [警告] 未找到dotnet SDK
    echo 请安装 .NET 8.0 SDK
    echo 下载地址: https://dotnet.microsoft.com/download/dotnet/8.0
    echo.
    echo 按任意键继续打开Visual Studio...
    pause >nul
    start msbuild Filee.slnx
    exit /b 0
)

echo [信息] PowerShell 和 .NET SDK 已安装
echo.

REM 运行快速打包脚本
echo 正在启动打包向导...
echo.
powershell.exe -ExecutionPolicy Bypass -File "%~dp0quick-pack.ps1"

if %errorlevel% neq 0 (
    echo.
    echo [错误] 打包过程中出现问题
    echo 请查看错误信息或查看 build/msix/README.md
    echo.
    pause
    exit /b 1
)

echo.
echo ========================================
echo 打包完成!
echo ========================================
echo.
echo 详细信息请查看: build/msix/使用说明.md
echo.
pause
