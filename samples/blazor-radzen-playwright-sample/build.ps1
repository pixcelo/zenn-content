# Blazor Radzen Playwright Sample - Build Script

Write-Host "Building BlazorRadzenPlaywrightSample..." -ForegroundColor Cyan
dotnet build BlazorRadzenPlaywrightSample.sln

if ($LASTEXITCODE -eq 0) {
    Write-Host "Build succeeded!" -ForegroundColor Green
} else {
    Write-Host "Build failed!" -ForegroundColor Red
    exit 1
}
