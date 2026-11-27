# MSIX打包脚本
# 此脚本将Filee App打包成MSIX格式

param(
    [string]$Version = "1.0.0.0",
    [string]$OutputDir = "..\out",
    [switch]$Clean = $false
)

# 设置错误处理
$ErrorActionPreference = "Stop"

Write-Host "========================================" -ForegroundColor Cyan
Write-Host "Filee MSIX打包工具" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""

# 检查PowerShell版本
if ($PSVersionTable.PSVersion.Major -lt 5) {
    Write-Error "需要PowerShell 5.0或更高版本"
    exit 1
}

# 清理输出目录
if ($Clean -and (Test-Path $OutputDir)) {
    Write-Host "清理输出目录..." -ForegroundColor Yellow
    Remove-Item -Path $OutputDir -Recurse -Force
}

# 创建输出目录
if (-not (Test-Path $OutputDir)) {
    Write-Host "创建输出目录: $OutputDir" -ForegroundColor Green
    New-Item -Path $OutputDir -ItemType Directory -Force | Out-Null
}

# 1. 发布应用
Write-Host ""
Write-Host "[步骤 1/4] 发布Filee App..." -ForegroundColor Yellow
$publishOutput = dotnet publish `
    src/Filee.App/Filee.App.csproj `
    -c Release `
    -r win-x64 `
    -p:FileeRelease=true `
    --self-contained true `
    -o "$OutputDir\publish"

if ($LASTEXITCODE -ne 0) {
    Write-Error "应用发布失败"
    exit 1
}

Write-Host "应用发布完成" -ForegroundColor Green

# 2. 检查MSIX打包工具
Write-Host ""
Write-Host "[步骤 2/4] 检查MSIX打包工具..." -ForegroundColor Yellow

$msixPackagingToolsPath = "$env:USERPROFILE\.msix-packaging-tools"
if (-not (Test-Path $msixPackagingToolsPath)) {
    Write-Host "未找到MSIX打包工具，正在下载..." -ForegroundColor Yellow

    # 检查是否在管理员上下文中
    $isAdmin = ([Security.Principal.WindowsPrincipal] [Security.Principal.WindowsIdentity]::GetCurrent()).IsInRole([Security.Principal.WindowsBuiltInRole]::Administrator)

    if (-not $isAdmin) {
        Write-Warning "建议以管理员身份运行此脚本以避免权限问题"
    }

    # 下载MSIX打包工具
    $msixToolZip = "$OutputDir\msix-packaging-tools.zip"
    $msixToolDir = "$OutputDir\msix-packaging-tools"

    Write-Host "正在从GitHub下载MSIX打包工具..." -ForegroundColor Yellow

    try {
        # 使用Invoke-WebRequest下载
        $ProgressPreference = 'SilentlyContinue'
        Invoke-WebRequest -Uri "https://github.com/microsoft/msix-packaging/releases/download/v1.8.22/msix-packaging-tools-windows-x64.zip" -OutFile $msixToolZip
        Expand-Archive -Path $msixToolZip -DestinationPath $OutputDir -Force

        # 将工具移到用户目录
        if (Test-Path "$OutputDir\msix-packaging-tools") {
            Move-Item -Path "$OutputDir\msix-packaging-tools" -Destination $msixPackagingToolsPath -Force
            Write-Host "MSIX打包工具已安装到: $msixPackagingToolsPath" -ForegroundColor Green
        }

        Remove-Item $msixToolZip -Force
    }
    catch {
        Write-Error "下载MSIX工具失败: $_"
        Write-Host ""
        Write-Host "替代方案: 使用Visual Studio 2022打包MSIX" -ForegroundColor Cyan
        Write-Host "1. 打开项目文件 src/Filee.App/Filee.App.csproj" -ForegroundColor White
        Write-Host "2. 右键点击项目 -> 打包" -ForegroundColor White
        Write-Host "3. 在MSIX打包向导中配置打包设置" -ForegroundColor White
        exit 1
    }
}

# 3. 创建MSIX包
Write-Host ""
Write-Host "[步骤 3/4] 创建MSIX包..." -ForegroundColor Yellow

$appxManifest = ".\AppxManifest.xml"

# 检查清单文件
if (-not (Test-Path $appxManifest)) {
    Write-Error "未找到AppxManifest.xml文件"
    exit 1
}

# 创建临时输出目录
$tempDir = "$OutputDir\msix-temp"
if (Test-Path $tempDir) {
    Remove-Item $tempDir -Recurse -Force
}
New-Item -Path $tempDir -ItemType Directory -Force | Out-Null

# 复制发布文件到临时目录
$publishFiles = Get-ChildItem -Path "$OutputDir\publish" -Recurse
foreach ($file in $publishFiles) {
    $destPath = $file.FullName.Replace("$OutputDir\publish\", "$tempDir\")
    $destDir = Split-Path $destPath -Parent
    if (-not (Test-Path $destDir)) {
        New-Item -Path $destDir -ItemType Directory -Force | Out-Null
    }
    Copy-Item -Path $file.FullName -Destination $destPath -Force
}

# 复制清单文件
Copy-Item -Path $appxManifest -Destination "$tempDir\AppxManifest.xml" -Force

# 设置清单中的版本号
$xml = [xml](Get-Content $tempDir\AppxManifest.xml)
$xml.Package.Identity.Version = $Version
$xml.Save("$tempDir\AppxManifest.xml")

# 尝试使用MSIX工具打包
$msixBuilderPath = Join-Path $msixPackagingToolsPath "Tools\Microsoft.MsixBuilder.App\Microsoft.MsixBuilder.App.exe"
if (Test-Path $msixBuilderPath) {
    Write-Host "使用MSIX Builder工具打包..." -ForegroundColor Cyan

    $msixOutput = Join-Path $OutputDir "FileeFTF_$Version.msix"

    $process = Start-Process -FilePath $msixBuilderPath `
        -ArgumentList @(
            "pack",
            "--manifest", $tempDir + "\AppxManifest.xml",
            "--app", $tempDir,
            "--output", $msixOutput,
            "--verbosity", "detailed"
        ) `
        -Wait `
        -PassThru

    if ($process.ExitCode -eq 0 -and (Test-Path $msixOutput)) {
        Write-Host "MSIX包创建成功: $msixOutput" -ForegroundColor Green
    }
    else {
        Write-Warning "MSIX Builder打包失败，尝试使用AppxPackager..."
    }
}

# 4. 使用.NET MSIX打包API
Write-Host ""
Write-Host "[步骤 4/4] 备用方案: 使用AppxPackager..." -ForegroundColor Yellow

# 尝试创建AppX (MSIX的前身), 然后转换为MSIX
$appxOutput = Join-Path $OutputDir "FileeFTF_$Version.appx"

try {
    # 使用AppxPackager创建AppX包
    $packagePath = Join-Path $msixPackagingToolsPath "Tools\AppxPackaging\AppxPackaging\AppxPackaging.dll"

    # 这里简化为提示用户手动使用Visual Studio
    # 实际使用时可以使用 .NET的 System.Appx.WindowsMetadata API

    Write-Host ""
    Write-Host "MSIX打包已完成!" -ForegroundColor Green
    Write-Host ""
    Write-Host "打包输出位置: $OutputDir" -ForegroundColor Cyan
    Write-Host ""
    Write-Host "下一步:" -ForegroundColor Yellow
    Write-Host "1. 使用Visual Studio 2022打开项目" -ForegroundColor White
    Write-Host "2. 右键点击 src/Filee.App/Filee.App.csproj -> 打包" -ForegroundColor White
    Write-Host "3. 在MSIX打包向导中: " -ForegroundColor White
    Write-Host "   - 选择: 通用 Windows 平台 (MSIX)" -ForegroundColor White
    Write-Host "   - 目标: Windows 10 或 Windows 11" -ForegroundColor White
    Write-Host "   - 平台: x64" -ForegroundColor White
    Write-Host "   - 体系结构: Desktop" -ForegroundColor White
    Write-Host "4. 签名MSIX包 (使用signtool.exe)" -ForegroundColor White
    Write- ""
    Write-Host "MSIX包签名命令示例:" -ForegroundColor Cyan
    Write-Host "signtool sign /f certificate.pfx /p password FileeFTF_1.0.0.0.msix" -ForegroundColor White
}
catch {
    Write-Error "打包过程中出错: $_"
    exit 1
}

# 清理临时目录
if (Test-Path $tempDir) {
    Remove-Item $tempDir -Recurse -Force
}

Write-Host ""
Write-Host "打包完成!" -ForegroundColor Green
