# Filee MSIX 快速打包脚本
# 简化版本,推荐使用 Visual Studio 2022

param(
    [string]$OutputDir = "AppPackages",
    [switch]$Clean = $false
)

Write-Host "========================================" -ForegroundColor Cyan
Write-Host "Filee MSIX 快速打包" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""

# 清理输出目录
if ($Clean -and (Test-Path $OutputDir)) {
    Write-Host "清理输出目录..." -ForegroundColor Yellow
    Remove-Item -Path $OutputDir -Recurse -Force
}

# 创建输出目录
New-Item -Path $OutputDir -ItemType Directory -Force | Out-Null

# 打开 Visual Studio
Write-Host ""
Write-Host "请按以下步骤操作:" -ForegroundColor Yellow
Write-Host ""
Write-Host "1. 打开 Visual Studio 2022" -ForegroundColor White
Write-Host "2. 打开解决方案文件:" -ForegroundColor White
Write-Host "   Filee.slnx" -ForegroundColor Green
Write-Host ""
Write-Host "3. 在解决方案资源管理器中右键点击:" -ForegroundColor White
Write-Host "   src/Filee.App/Filee.App.csproj" -ForegroundColor Green
Write-Host ""
Write-Host "4. 选择 '打包' (Pack)" -ForegroundColor White
Write-Kind ""
Write-Host "5. 在 MSIX 打包向导中配置:" -ForegroundColor White
Write-Host "   - 目标平台: Windows 10 或 Windows 11" -ForegroundColor White
Write-Host "   - 平台: x64" -ForegroundColor White
Write-Host "   - 应用类型: 桌面应用" -ForegroundColor White
Write-Host ""
Write-Host "6. 点击 '确定' 等待打包完成" -ForegroundColor White
Write-Host ""
Write-Host "7. 打包完成后,在以下位置找到 MSIX 包:" -ForegroundColor Yellow
Write-Host "   $OutputDir\FileeFTF_*.msix" -ForegroundColor Green
Write-Host ""

# 等待用户确认
Read-Host "按 Enter 继续..."

# 签名提示
Write-Host ""
Write-Host "========================================" -ForegroundColor Cyan
Write-Host "签名 MSIX 包 (可选)" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""
Write-Host "如果你有证书,可以签名 MSIX 包:" -ForegroundColor White
Write-Host ""
Write-Host "signtool sign /f certificate.pfx /p password $OutputDir\FileeFTF_*.msix" -ForegroundColor Green
Write-Host ""

# 查找生成的文件
$msixFiles = Get-ChildItem -Path $OutputDir -Filter "*.msix" -Recurse

if ($msixFiles.Count -gt 0) {
    Write-Host "========================================" -ForegroundColor Cyan
    Write-Host "找到的 MSIX 文件:" -ForegroundColor Cyan
    Write-Host "========================================" -ForegroundColor Cyan
    Write-Host ""

    foreach ($file in $msixFiles) {
        $sizeMB = [math]::Round($file.Length / 1MB, 2)
        Write-Host "文件: $($file.Name)" -ForegroundColor Green
        Write-Host "路径: $($file.FullName)" -ForegroundColor White
        Write-Host "大小: $sizeMB MB" -ForegroundColor White
        Write-Host ""
    }
}
else {
    Write-Host ""
    Write-Warning "未找到生成的 MSIX 文件"
    Write-Host "请确保在 Visual Studio 中成功打包" -ForegroundColor Yellow
    Write-Host ""
    Write-Host "查看详细说明:" -ForegroundColor Cyan
    Write-Host "  .\build\msix\README.md" -ForegroundColor Green
}

Write-Host ""
Write-Host "打包完成!" -ForegroundColor Green
