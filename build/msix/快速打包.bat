@echo off
chcp 65001 >nul
title Filee MSIX 快速打包

echo ========================================
echo Filee MSIX 快速打包工具
echo ========================================
echo.

REM 检查 PowerShell
where powershell >nul 2>&1
if %errorlevel% neq 0 (
    echo [错误] 未找到 PowerShell，请先安装
    pause
    exit /b 1
)

REM 检查脚本是否存在
if not exist "build\msix\打包MSIX完整版.ps1" (
    echo [错误] 找不到打包脚本: build\msix\打包MSIX完整版.ps1
    pause
    exit /b 1
)

echo [信息] 正在启动打包流程...
echo.
echo 此脚本将:
echo   1. 检查构建环境 (.NET SDK, MSBuild)
echo   2. 发布应用 (Release + self-contained)
echo   3. 准备 MSIX 资源
echo   4. 尝试创建 MSIX 包
echo.
echo 打包完成后，您需要使用 Visual Studio 完成最终 MSIX 包的创建。
echo.

pause

REM 运行打包脚本
powershell.exe -ExecutionPolicy Bypass -File "%~dp0打包MSIX完整版.ps1" -Clean

if %errorlevel% neq 0 (
    echo.
    echo [错误] 打包过程中出现错误
    echo.
    pause
    exit /b 1
)

echo.
echo ========================================
echo 完成!
echo ========================================
echo.
echo MSIX 包信息将位于: AppPackages\
echo.
pause
