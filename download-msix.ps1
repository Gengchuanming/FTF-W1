$ProgressPreference = 'SilentlyContinue'
try {
    $client = New-Object System.Net.WebClient
    $url = 'https://github.com/microsoft/msix-packaging/releases/download/v1.8.20/msix-packaging-tools-windows-x64.zip'
    $output = 'msix-tools.zip'

    Write-Host 'Downloading from: ' $url
    $client.DownloadFile($url, $output)

    $fileInfo = Get-Item $output
    Write-Host 'Download complete! Size: ' $fileInfo.Length ' bytes'
} catch {
    Write-Error 'Download failed: ' $_.Exception.Message
}
