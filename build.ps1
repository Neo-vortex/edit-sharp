# build.ps1 - Build script for Windows

Write-Host "Edit Sharp Build Script" -ForegroundColor Cyan
Write-Host "=======================" -ForegroundColor Cyan
Write-Host ""

# Build Trimmed AOT version
Write-Host "Building Trimmed AOT Edition..." -ForegroundColor Yellow
dotnet publish edit-sharp/edit-sharp.csproj `
    --configuration Release-Trimmed `
    --runtime win-x64 `
    --self-contained true `
    --output ./build/trimmed-aot-win-x64 `
    /p:PublishSingleFile=true `
    /p:PublishReadyToRun=true `
    /p:IncludeNativeLibrariesForSelfExtract=true `
    /p:IncludeAllContentForSelfExtract=true

if ($LASTEXITCODE -eq 0) {
    Write-Host "✅ Trimmed AOT build complete: ./build/trimmed-aot-win-x64/" -ForegroundColor Green
} else {
    Write-Host "❌ Trimmed AOT build failed" -ForegroundColor Red
    exit 1
}

Write-Host ""

# Build Full version
Write-Host "Building Full Edition..." -ForegroundColor Yellow
dotnet publish edit-sharp/edit-sharp.csproj `
    --configuration Release-Full `
    --runtime win-x64 `
    --self-contained true `
    --output ./build/full-win-x64 `
    /p:PublishSingleFile=true `
    /p:PublishReadyToRun=true `
    /p:IncludeNativeLibrariesForSelfExtract=true `
    /p:IncludeAllContentForSelfExtract=true

if ($LASTEXITCODE -eq 0) {
    Write-Host "✅ Full build complete: ./build/full-win-x64/" -ForegroundColor Green
} else {
    Write-Host "❌ Full build failed" -ForegroundColor Red
    exit 1
}

Write-Host ""
Write-Host "Build Summary" -ForegroundColor Cyan
Write-Host "=============" -ForegroundColor Cyan
Write-Host "Trimmed AOT: ./build/trimmed-aot-win-x64/edit-sharp.exe"
Write-Host "Full:        ./build/full-win-x64/edit-sharp.exe"
Write-Host ""

# Show file sizes
Write-Host "File Sizes:" -ForegroundColor Cyan
if (Test-Path "./build/trimmed-aot-win-x64/edit-sharp.exe") {
    $trimmedSize = (Get-Item "./build/trimmed-aot-win-x64/edit-sharp.exe").Length / 1MB
    Write-Host "  Trimmed AOT: $([math]::Round($trimmedSize, 2)) MB"
}

if (Test-Path "./build/full-win-x64/edit-sharp.exe") {
    $fullSize = (Get-Item "./build/full-win-x64/edit-sharp.exe").Length / 1MB
    Write-Host "  Full:        $([math]::Round($fullSize, 2)) MB"
}

Write-Host ""
Write-Host "To run:" -ForegroundColor Cyan
Write-Host "  Trimmed: ./build/trimmed-aot-win-x64/edit-sharp.exe"
Write-Host "  Full:    ./build/full-win-x64/edit-sharp.exe"