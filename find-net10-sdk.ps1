# 查找.NET 10.0 SDK的正确下载链接
# 根据Microsoft的发布周期，.NET 10可能还不在预览阶段

Write-Host "检查.NET 10.0 SDK状态..."
Write-Host "建议：由于项目需要.NET 10.0 SDK，建议："
Write-Host "1. 等待.NET 10.0 SDK正式发布"
Write-Host "2. 或修改项目以支持.NET 9.0"

# 尝试查找.NET 10.0
$url = "https://api.nuget.org/v3-flatcontainer/microsoft.net.sdk.versions/10.0.0/microsoft.net.sdk.versions.10.0.0.nuspec"
Write-Host "检查.NET 10.0 NuGet包..."
