# Filee MSIX 打包指南

本文档说明如何将 Filee 应用打包成 MSIX 格式以发布到 Microsoft Store。

## 前提条件

1. **Windows 10 1809+ 或 Windows 11**
2. **.NET 8.0 SDK** (已安装)
3. **Visual Studio 2022** (推荐) 或 MSIX 打包工具
4. **有效的证书** 用于签名 MSIX 包 (如果没有证书,Microsoft Store 会生成一个)

## 快速开始

### 方法一: 使用快速打包脚本 (推荐，3分钟完成)

双击运行或执行：
```powershell
.\build\msix\快速打包.bat
```

或在 PowerShell 中：
```powershell
cd c:\Users\耿\FTF-W1
.\build\msix\打包MSIX完整版.ps1 -Clean
```

脚本会自动：
1. ✅ 检查环境 (.NET SDK, MSBuild)
2. ✅ 发布应用
3. ✅ 准备 MSIX 资源
4. ✅ 创建 MSIX 包

### 方法二: 使用 Visual Studio 2022 (推荐用于最终打包)

1. **打开项目**
   ```powershell
   cd c:\Users\耿\FTF-W1
   .\build\msix\pack-msix.ps1 -Clean
   ```

2. **在 Visual Studio 中打开项目**
   ```
   src/Filee.App/Filee.App.csproj
   ```

3. **打包 MSIX**
   - 右键点击 `Filee.App` 项目
   - 选择 "打包"
   - 在 MSIX 打包向导中选择:
     - **打包类型**: MSIX
     - **目标平台**: Windows 10 或 Windows 11
     - **应用类型**: 桌面应用
     - **体系结构**: x64

4. **等待打包完成**
   - 打包完成后,输出位置通常为:
     `C:\Users\耿\source\repos\Filee\AppPackages\FileeFTF_1.0.0.0_x64_Release.msix`

5. **签名 MSIX 包** (必需)
   ```powershell
   # 使用自签名证书
   signtool sign /f certificate.pfx /p yourpassword FileeFTF_1.0.0.0_x64_Release.msix

   # 或使用 Microsoft Store 提供的证书
   ```

### 方法二: 使用 MSIX 打包工具 CLI

1. **安装 MSIX 打包工具**
   ```powershell
   # 运行打包脚本会自动下载
   cd c:\Users\耿\FTF-W1
   .\build\msix\pack-msix.ps1 -Clean
   ```

2. **打包应用**
   ```powershell
   .\build\msix\pack-msix.ps1 -Version 1.0.0.0
   ```

### 方法三: 使用 .NET CLI (需要手动配置)

```powershell
# 发布应用
dotnet publish src/Filee.App/Filee.App.csproj `
    -c Release `
    -r win-x64 `
    -p:FileeRelease=true `
    --self-contained true `
    -o out\publish

# 然后使用 AppxPackaging 工具打包 (需要额外配置)
```

## MSIX 打包配置说明

### AppxManifest.xml

主清单文件定义了应用的基本信息:

- **Identity**: 应用名称、发布者、版本
- **Properties**: 显示名称、图标
- **Capabilities**: 所需权限 (文件访问、网络等)
- **Applications**: 应用入口点
- **Extensions**: 启动任务、应用别名

### 权限说明

清单中包含以下权限:

- `runFullTrust`: 完整信任运行
- `documentsLibrary`: 文档库访问
- `enterpriseCloud`: 企业云服务
- `internetClient`: 互联网访问
- `internetClientServer`: 服务器端网络访问

根据你的应用需求,可以调整这些权限。

## 验证 MSIX 包

使用 Windows 工具验证打包:

```powershell
# 解压 MSIX 包查看内容
Expand-Archive FileeFTF_1.0.0.0.msix -DestinationPath extracted

# 验证清单
Get-AppxManifest -Path extracted\AppxManifest.xml

# 如果有签名,验证签名
signtool verify /pa /v FileeFTF_1.0.0.0.msix
```

## 上传到 Microsoft Store

### 1. 准备工作

- 注册 [Microsoft Developer Account](https://developer.microsoft.com/store/register)
- 创建应用提交
- 准备以下材料:
  - 应用截图 (至少1张,最多10张)
  - 应用描述 (多种语言)
  - 应用分类和属性
  - 隐私策略 URL
  - 许可协议 URL

### 2. 上传 MSIX 包

### 1. 准备工作

- 注册 [Microsoft Developer Account](https://developer.microsoft.com/store/register)
- 创建应用提交
- 准备以下材料:
  - 应用截图 (至少1张,最多10张)
  - 应用描述 (多种语言)
  - 应用分类和属性
  - 隐私策略 URL
  - 许可协议 URL

### 2. 上传 MSIX 包

1. 登录 [Partner Center](https://partner.microsoft.com/dashboard)
2. 创建新应用或选择现有应用
3. 在 "管理" -> "应用包" 中上传 MSIX 文件
4. 填写应用详情:
   - **名称**: Filee FTF
   - **简短描述**: ...
   - **详细描述**: ...
   - **价格**: 免费
   - **年龄分级**: ...
   - **类别**: ...

### 3. 提交审核

- 确保所有必填字段已填写
- 确认隐私策略和许可协议链接有效
- 提交后等待审核 (通常1-7个工作日)

## 常见问题

### Q: 打包失败 "无法加载 AppxManifest.xml"
A: 确保 AppxManifest.xml 文件位于项目根目录,并且格式正确。

### Q: 如何修改应用版本号?
A: 在 AppxManifest.xml 中修改 `<Identity Version="1.0.0.0" />` 的值,或使用:
```powershell
.\build\msix\pack-msix.ps1 -Version 2.0.0.0
```

### Q: MSIX 包太大怎么办?
A:
1. 使用 `PublishSingleFile=true` (已配置)
2. 移除未使用的资源
3. 启用发布时修剪 (`PublishTrimmed=true`)
4. 压缩应用文件

### Q: 需要什么证书?
A:
- **开发阶段**: 可以使用自签名证书
- **发布到 Store**: Microsoft 会生成证书
- **发布到其他平台**: 需要购买信任的证书 (代码签名证书)

### Q: 如何更新已发布的应用?
A:
1. 修改版本号 (增加修订号,如 1.0.0.1)
2. 重新打包 MSIX
3. 在 Partner Center 中创建新版本提交
4. 上传新的 MSIX 包

## 相关链接

- [MSIX 官方文档](https://docs.microsoft.com/en-us/windows/msix/)
- [MSIX Packaging Tools](https://github.com/microsoft/msix-packaging)
- [Partner Center 帮助](https://docs.microsoft.com/en-us/windows/publish/)
- [应用包清单规范](https://docs.microsoft.com/en-us/windows/uwp/schemas/appxpackage/appx-package-manifest)

## 脚本参数

```powershell
.\build\msix\pack-msix.ps1
    -Version [string]      # 应用版本 (默认: 1.0.0.0)
    -OutputDir [string]    # 输出目录 (默认: ..\out)
    -Clean [switch]        # 清理输出目录
```

## 支持

如有问题,请:
1. 查看本文档的常见问题部分
2. 检查 [MSIX 官方文档](https://docs.microsoft.com/en-us/windows/msix/)
3. 提交 Issue 到项目仓库
