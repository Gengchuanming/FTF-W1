# Filee MSIX 完整打包脚本
# 此脚本将 Filee 应用发布并打包为 MSIX 格式

param(
    [string]$Version = "1.0.0.0",
    [string]$OutputDir = "AppPackages",
    [switch]$Clean = $false,
    [switch]$SkipBuild = $false
)

$ErrorActionPreference = "Stop"

Write-Host "========================================" -ForegroundColor Cyan
Write-Host "Filee MSIX 完整打包工具" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""

# 1. 检查环境
Write-Host "[步骤 1/5] 检查构建环境..." -ForegroundColor Yellow

$checks = @(
    @{Name = ".NET SDK"; Command = "dotnet --version"; MinVersion = "8.0.0"},
    @{Name = "MSBuild"; Command = "msbuild -version"},
    @{Name = "PowerShell"; Command = "powershell -Command '$PSVersionTable.PSVersion.ToString()'"}
)

$envOk = $true
foreach ($check in $checks) {
    try {
        $output = Invoke-Command -ScriptBlock { & $check.Command 2>&1 } -ErrorAction Stop
        $version = $output.Trim()

        if ($check.MinVersion -and $version) {
            $versionObj = [version]$version
            $minVersionObj = [version]$check.MinVersion
            if ($versionObj -lt $minVersionObj) {
                Write-Host "  ✗ $($check.Name): 需要 $($check.MinVersion) 但检测到 $version" -ForegroundColor Red
                $envOk = $false
            } else {
                Write-Host "  ✓ $($check.Name): $version" -ForegroundColor Green
            }
        } else {
            Write-Host "  ✓ $($check.Name): 已安装" -ForegroundColor Green
        }
    } catch {
        Write-Host "  ✗ $($check.Name): 未安装" -ForegroundColor Red
        $envOk = $false
    }
}

if (-not $envOk) {
    Write-Host ""
    Write-Error "缺少必要工具，请先安装："
    Write-Host "  - .NET 8.0 SDK: https://dotnet.microsoft.com/download/dotnet/8.0" -ForegroundColor Cyan
    Write-Host "  - Visual Studio 2022: https://visualstudio.microsoft.com/vs/" -ForegroundColor Cyan
    exit 1
}

Write-Host ""

# 2. 清理输出目录
if ($Clean -and (Test-Path $OutputDir)) {
    Write-Host "[步骤 2/5] 清理输出目录..." -ForegroundColor Yellow
    Remove-Item -Path $OutputDir -Recurse -Force
    Write-Host "  ✓ 清理完成" -ForegroundColor Green
    Write-Host ""
}

# 3. 发布应用
if (-not $SkipBuild) {
    Write-Host "[步骤 3/5] 发布 Filee App..." -ForegroundColor Yellow

    $publishParams = @(
        "publish",
        "src/Filee.App/Filee.App.csproj",
        "-c", "Release",
        "-r", "win-x64",
        "-p:FileeRelease=true",
        "--self-contained", "true",
        "-o", "$OutputDir\publish"
    )

    Write-Host "  执行命令: dotnet publish $($publishParams -join ' ')" -ForegroundColor White

    try {
        & dotnet publish @publishParams -v quiet

        if ($LASTEXITCODE -eq 0) {
            Write-Host "  ✓ 应用发布成功" -ForegroundColor Green
        } else {
            Write-Host "  ✗ 应用发布失败 (退出代码: $LASTEXITCODE)" -ForegroundColor Red
            exit 1
        }
    } catch {
        Write-Host "  ✗ 应用发布失败: $_" -ForegroundColor Red
        exit 1
    }

    Write-Host ""

    # 检查发布输出
    if (Test-Path "$OutputDir\publish\FTF.exe") {
        $fileSize = [math]::Round((Get-Item "$OutputDir\publish\FTF.exe").Length / 1MB, 2)
        Write-Host "  应用大小: $fileSize MB" -ForegroundColor Green
        Write-Host ""
    } else {
        Write-Warning "  未找到 FTF.exe，检查发布输出目录..."
        if (Test-Path "$OutputDir\publish") {
            $files = Get-ChildItem -Path "$OutputDir\publish" -File | Measure-Object | Select-Object -ExpandProperty Count
            Write-Host "  发布文件数量: $files" -ForegroundColor Yellow
        }
        Write-Host ""
    }
}

# 4. 复制 MSIX 资源
Write-Host "[步骤 4/5] 准备 MSIX 资源..." -ForegroundColor Yellow

$msixDir = "$OutputDir\msix-temp"
if (Test-Path $msixDir) {
    Remove-Item $msixDir -Recurse -Force
}
New-Item -Path $msixDir -ItemType Directory -Force | Out-Null

# 复制发布文件
Write-Host "  复制发布文件到临时目录..." -ForegroundColor White
$publishFiles = Get-ChildItem -Path "$OutputDir\publish" -Recurse
$count = 0
foreach ($file in $publishFiles) {
    $destPath = $file.FullName.Replace("$OutputDir\publish\", "$msixDir\")
    $destDir = Split-Path $destPath -Parent
    if (-not (Test-Path $destDir)) {
        New-Item -Path $destDir -ItemType Directory -Force | Out-Null
    }
    Copy-Item -Path $file.FullName -Destination $destPath -Force
    $count++
}
Write-Host "  ✓ 复制了 $count 个文件" -ForegroundColor Green

# 复制图标
if (Test-Path "src/Filee.App/Assets/Icons/ftf.png") {
    Copy-Item -Path "src/Filee.App/Assets/Icons/ftf.png" -Destination "$msixDir/Assets/Icons/" -Force
    Write-Host "  ✓ 复制图标文件" -ForegroundColor Green
}

# 复制清单文件
if (Test-Path "build/msix/AppxManifest.xml") {
    Copy-Item -Path "build/msix/AppxManifest.xml" -Destination "$msixDir/AppxManifest.xml" -Force
    Write-Host "  ✓ 复制清单文件" -ForegroundColor Green
} else {
    Write-Warning "  未找到 AppxManifest.xml，创建默认清单..."
    $manifest = @"
<?xml version="1.0" encoding="utf-8"?>
<Package xmlns="http://schemas.microsoft.com/appx/manifest/foundation/windows10" xmlns:uap="http://schemas.microsoft.com/appx/manifest/uap/windows10" xmlns:uap3="http://schemas.microsoft.com/appx/manifest/uap/windows10/3" xmlns:rescap="http://schemas.microsoft.com/appx/manifest/foundation/windows10/restrictedCapabilities" IgnorableNamespaces="uap uap3 rescap">
  <Identity Name="FileeFTF" Publisher="CN=Filee" Version="$Version" />
  <Properties>
    <DisplayName>Filee FTF</DisplayName>
    <PublisherDisplayName>Filee FTF</PublisherDisplayName>
    <Logo>Assets\Icons\ftf.png</Logo>
  </Properties>
  <Resources>
    <Resource Language="en-us" />
    <Resource Language="zh-cn" />
  </Resources>
  <Applications>
    <Application Id="FileeFTF" Executable="FTF.exe" EntryPoint="Windows.FullTrustApplication">
      <uap:VisualElements BackgroundColor="#1e1e1e" DisplayName="Filee FTF" Square150x150Logo="Assets\Icons\ftf.png" Square44x44Logo="Assets\Icons\ftf.png" ShowNameOnAllTaskbars="true" />
      <uap3:Extensions>
        <uap3:Extension Category="windows.startupTask" Executable="FTF.exe" EntryPoint="Windows.FullTrustApplication">
          <uap3:StartupTask TaskId="FileeFTFStartup" Enabled="false" DisplayName="Filee FTF" />
        </uap3:Extension>
      </uap3:Extensions>
    </Application>
  </Applications>
  <Capabilities>
    <rescap:Capability Name="runFullTrust" />
    <uap:Capability Name="documentsLibrary" />
    <uap:Capability Name="enterpriseCloud" />
    <uap:Capability Name="internetClient" />
    <uap:Capability Name="internetClientServer" />
  </Capabilities>
  <Extensions>
    <Extension Category="windows.appExecutionAlias" Executable="FTF.exe" EntryPoint="Windows.FullTrustApplication">
      <AppExecutionAlias Alias="ftf.exe" />
    </Extension>
  </Extensions>
</Package>
"@
    $manifest | Out-File -FilePath "$msixDir/AppxManifest.xml" -Encoding utf8 -Force
    Write-Host "  ✓ 创建默认清单文件" -ForegroundColor Green
}

Write-Host ""

# 5. 创建 MSIX 包
Write-Host "[步骤 5/5] 创建 MSIX 包..." -ForegroundColor Yellow

# 检查 MSIX 工具
$msixToolsPath = "$env:USERPROFILE\.msix-packaging-tools"
$msixBuilderPath = "$msixToolsPath\Tools\Microsoft.MsixBuilder.App\Microsoft.MsixBuilder.App.exe"

if (Test-Path $msixBuilderPath) {
    Write-Host "  使用 MSIX Builder 打包..." -ForegroundColor Cyan

    $msixOutput = "$OutputDir\FileeFTF_$Version.msix"

    $process = Start-Process -FilePath $msixBuilderPath `
        -ArgumentList @(
            "pack",
            "--manifest", "$msixDir\AppxManifest.xml",
            "--app", $msixDir,
            "--output", $msixOutput,
            "--verbosity", "detailed"
        ) `
        -Wait `
        -PassThru

    if ($process.ExitCode -eq 0 -and (Test-Path $msixOutput)) {
        $sizeMB = [math]::Round((Get-Item $msixOutput).Length / 1MB, 2)
        Write-Host "  ✓ MSIX 包创建成功!" -ForegroundColor Green
        Write-Host "    文件: $msixOutput" -ForegroundColor Cyan
        Write-Host "    大小: $sizeMB MB" -ForegroundColor Cyan
    } else {
        Write-Warning "  MSIX Builder 打包失败，尝试备用方案..."
    }
}

# 6. 使用 Visual Studio MSIX 打包（推荐方法）
Write-Host ""
Write-Host "========================================" -ForegroundColor Cyan
Write-Host "MSIX 打包完成（备用方法）" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""

Write-Host "由于某些限制，建议使用以下方法之一创建 MSIX 包：" -ForegroundColor Yellow
Write-Host ""

Write-Host "方法 1: 使用 Visual Studio 2022（推荐）" -ForegroundColor White
Write-Host "  1. 打开 Visual Studio 2022" -ForegroundColor White
Write-Host "  2. 打开解决方案: Filee.slnx" -ForegroundColor White
Write-Host "  3. 右键点击 src/Filee.App/Filee.App.csproj" -ForegroundColor White
Write-Host "  4. 选择 'Pack'" -ForegroundColor White
Write-Host "  5. 在 MSIX 打包向导中:" -ForegroundColor White
Write-Host "     - 包类型: MSIX" -ForegroundColor White
Write-Host "     - 目标平台: Windows 10 或 Windows 11" -ForegroundColor White
Write-Host "     - 平台: x64" -ForegroundColor White
Write-Host "     - 应用类型: 桌面应用" -ForegroundColor White
Write-Host "     - 体系结构: Desktop" -ForegroundColor White
Write-Host "  6. 等待打包完成" -ForegroundColor White
Write-Host ""

Write-Host "方法 2: 使用 MSIX CLI 工具" -ForegroundColor White
Write-Host "  1. 下载 MSIX Packaging Tools:" -ForegroundColor White
Write-Host "     https://github.com/microsoft/msix-packaging/releases" -ForegroundColor Green
Write-Host "  2. 解压并运行: msix" -ForegroundColor White
Write-Host "  3. 按照 CLI 提示打包应用" -ForegroundColor White
Write-Host ""

Write-Host "打包完成后，MSIX 文件将位于:" -ForegroundColor Yellow
Write-Host "  $OutputDir\FileeFTF_*.msix" -ForegroundColor Green
Write-Host ""

# 查找所有 MSIX 文件
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

    Write-Host "下一步:" -ForegroundColor Yellow
    Write-Host "  1. 如果有证书，签名 MSIX 包:" -ForegroundColor White
    Write-Host "     signtool sign /f certificate.pfx /p password $OutputDir\FileeFTF_*.msix" -ForegroundColor Green
    Write-Host ""
    Write-Host "  2. 上传到 Microsoft Store:" -ForegroundColor White
    Write-Host "     https://partner.microsoft.com/dashboard" -ForegroundColor Green
    Write-Host ""
} else {
    Write-Host "⚠️  未找到生成的 MSIX 文件" -ForegroundColor Yellow
    Write-Host "  请使用方法 1（Visual Studio）或方法 2（MSIX CLI）创建 MSIX 包" -ForegroundColor Yellow
    Write-Host ""
    Write-Host "MSIX 包信息将保存到:" -ForegroundColor Cyan
    Write-Host "  $OutputDir\" -ForegroundColor Green
    Write-Host ""
}

# 清理临时目录
if (Test-Path $msixDir) {
    Remove-Item $msixDir -Recurse -Force
    Write-Host "临时文件已清理" -ForegroundColor Gray
}

Write-Host ""
Write-Host "========================================" -ForegroundColor Cyan
Write-Host "打包流程完成!" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""
Write-Host "详细文档: build/msix/README.md" -ForegroundColor White
Write-Host "快速指南: build/msix/使用说明.md" -ForegroundColor White
Write-Host ""
