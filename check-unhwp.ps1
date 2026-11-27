# 检查Unhwp包支持的框架
$client = New-Object System.Net.WebClient
$url = "https://api.nuget.org/v3-flatcontainer/unhwp/0.13.0/unhwp.0.13.0.nuspec"
$output = "unhwp.nuspec"

Write-Host "Downloading Unhwp package manifest..."
$client.DownloadFile($url, $output)

Get-Content $output | Select-String "framework"
Remove-Item $output
