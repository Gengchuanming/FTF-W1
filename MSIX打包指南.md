# Filee MSIX 打包指南

## 当前状态

✅ .NET 8.0 SDK 已成功安装（版本 8.0.300）
❌ MSIX Packaging Tools 需要手动下载
⚠️ Visual Studio 2019 需要验证构建能力

## 方案 1: 手动下载 MSIX Tools 并打包（推荐）

### 步骤 1: 下载 MSIX Packaging Tools

**方法 A：直接下载（推荐）**
1. 访问：https://github.com/microsoft/msix-packaging/releases
2. 下载最新版本的 `msix-packaging-tools-windows-x64.zip`
3. 解压到：`C:\Users\耿\.msix-packaging-tools\`

**方法 B：使用浏览器**
1. 打开 https://github.com/microsoft/msix-packaging/releases
2. 下载文件
3. 解压到指定目录

### 步骤 2: 发布应用

```powershell
cd c:\Users\耿\FTF-W1
dotnet publish src/Filee.App/Filee.App.csproj `
    -c Release `
    -r win-x64 `
    -p:FileeRelease=true `
    --self-contained true `
    -o AppPackages\publish
```

### 步骤 3: 准备 MSIX 资源

```powershell
# 创建临时目录
$msixDir = "AppPackages\msix-temp"
if (Test-Path $msixDir) { Remove-Item $msixDir -Recurse -Force }
New-Item -Path $msixDir -ItemType Directory -Force | Out-Null

# 复制发布文件
Copy-Item -Path AppPackages\publish\* -Destination $msixDir -Recurse -Force

# 复制清单文件
Copy-Item -Path build\msix\AppxManifest.xml -Destination "$msixDir\AppxManifest.xml" -Force
```

### 步骤 4: 使用 MSIX Builder 打包

```powershell
$msixTools = "C:\Users\耿\.msix-packaging-tools\Tools\Microsoft.MsixBuilder.App\Microsoft.MsixBuilder.App.exe"
$msixOutput = "AppPackages\FileeFTF_1.0.0.0.msix"

& $msixTools pack `
    --manifest "$msixDir\AppxManifest.xml" `
    --app $msixDir `
    --output $msixOutput `
    --verbosity detailed
```

### 步骤 5: 签名 MSIX 包（可选）

```powershell
# 使用自签名证书签名
signtool sign /f certificate.pfx /p password AppPackages\FileeFTF_1.0.0.0.msix

# 或使用 Microsoft 提供的测试证书
signtool sign /t http://timestamp.digicert.com /fd sha256 AppPackages\FileeFTF_1.0.0.0.msix
```

## 方案 2: 使用 Visual Studio 2022（推荐用于最终发布）

### 步骤 1: 安装 Visual Studio 2022（如果未安装）
- 下载地址：https://visualstudio.microsoft.com/vs/
- 需要安装：.NET桌面开发工作负载

### 步骤 2: 打开项目
```powershell
cd c:\Users\耿\FTF-W1
code Filee.slnx
```

### 步骤 3: 配置打包
1. 在解决方案资源管理器中找到 `src/Filee.App`
2. 右键点击 → **打包 (Pack)**
3. 在 MSIX 打包向导中配置：
   - **打包类型**：MSIX
   - **目标平台**：Windows 10 或 Windows 11
   - **平台**：x64
   - **应用类型**：桌面应用
   - **体系结构**：Desktop
4. 点击 **确定**，等待打包完成

### 步骤 4: 查找 MSIX 文件
- 输出位置：`AppPackages\FileeFTF_1.0.0.0_x64_Release.msix`

## 方案 3: 使用 Visual Studio 2019（如果可用）

### 步骤 1: 验证构建环境
```powershell
# 打开 VS 2019 开发者命令提示符
# 在命令行中运行：
msbuild Filee.slnx /p:Configuration=Release /p:Platform=x64
```

### 步骤 2: 发布应用
```powershell
cd c:\Users\耿\FTF-W1
dotnet publish src/Filee.App/Filee.App.csproj -c Release -r win-x64 --self-contained true -o AppPackages\publish
```

### 步骤 3: 使用 MSIX Tools 打包
（同方案1的步骤3和4）

## 验证 MSIX 包

### 解压查看内容
```powershell
Expand-Archive -Path "AppPackages\FileeFTF_1.0.0.0.msix" -DestinationPath "extracted"
```

### 安装测试
```powershell
Add-AppxPackage -Path "AppPackages\FileeFTF_1.0.0.0.msix"
```

### 验证签名
```powershell
signtool verify /pa /v "AppPackages\FileeFTF_1.0.0.0.msix"
```

## 常见问题

### Q: 打包失败 - 找不到 MSIX Builder
A: 确保已下载并正确安装 MSIX Packaging Tools

### Q: 应用无法启动
A: 检查 AppxManifest.xml 中的配置，确保 Executable 名称正确

### Q: MSIX 包太大
A:
1. 使用 PublishSingleFile=true
2. 启用 PublishTrimmed=true
3. 移除不需要的资源

### Q: 签名失败
A:
1. 确保有有效的证书文件（.pfx）
2. 证书密码正确
3. 证书支持代码签名

## 下一步

1. 📖 选择一种打包方案
2. 🔧 执行相应的步骤
3. ✅ 验证打包结果
4. 📦 上传到 Microsoft Store（如果需要）

## 相关资源

- [MSIX 官方文档](https://docs.microsoft.com/windows/msix/)
- [MSIX Packaging Tools](https://github.com/microsoft/msix-packaging)
- [Visual Studio 2022 下载](https://visualstudio.microsoft.com/vs/)
- [.NET 下载](https://dotnet.microsoft.com/download/dotnet/8.0)
- [Partner Center](https://partner.microsoft.com/dashboard)

## 脚本说明

项目中已提供以下脚本：
- `build/msix/quick-pack.ps1` - 快速打包引导
- `build/msix/pack-msix.ps1` - 完整打包脚本
- `build/msix/打包MSIX完整版.ps1` - 中文完整版脚本

## 支持

如有问题，请：
1. 查看 README.md
2. 检查项目 Issue
3. 参考 MSIX 官方文档

---

**祝你打包顺利！** 🎉
