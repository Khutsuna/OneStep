# Uploads product photos in bulk through the API, with the same checks as the admin panel
# (.jpg/.jpeg/.png/.webp, max 5 MB, at most 8 per product; the first photo of a product becomes its main photo).
#
# 1. Put the photos in the product-photos folder. Each file name starts with the product id:
#    2.png, 13.webp, 26-sony-wf.jpg ...
# 2. Start the API, then run from the repo root:
#    powershell -ExecutionPolicy Bypass -File tools\import-product-photos.ps1
param(
    [string]$Folder = (Join-Path $PSScriptRoot '..\product-photos'),
    [string]$Api = 'http://localhost:5091',
    [string]$Email = 'admin@onestep.ge',
    [string]$Password = 'Admin123!'
)

$body = @{ email = $Email; password = $Password } | ConvertTo-Json
$login = Invoke-RestMethod -Method Post -Uri "$Api/api/auth/login" -ContentType 'application/json' -Body $body
$token = $login.accessToken

$files = Get-ChildItem -Path $Folder -File | Where-Object { $_.Extension -in '.jpg', '.jpeg', '.png', '.webp' }
foreach ($file in $files) {
    if ($file.BaseName -notmatch '^(\d+)') {
        Write-Host "skipped $($file.Name): the name must start with the product id" -ForegroundColor Yellow
        continue
    }
    $id = $Matches[1]
    # curl.exe ships with Windows 10 and 11 and sends the file as multipart/form-data, like the browser does.
    $status = curl.exe -s -o NUL -w '%{http_code}' -X POST "$Api/api/products/$id/images" `
        -H "Authorization: Bearer $token" -F "file=@$($file.FullName)"
    $color = if ($status -eq '201') { 'Green' } else { 'Red' }
    Write-Host "$($file.Name) -> product $id : $status" -ForegroundColor $color
}
