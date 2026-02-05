# Script para verificar que las plantillas de email se copian correctamente

Write-Host "🔍 Verificando plantillas de email..." -ForegroundColor Cyan

# Rutas
$projectPath = "ManageMyMoney.Infrastructure.Shared"
$templatesSource = Join-Path $projectPath "Email\Templates"
$outputPath = Join-Path $projectPath "bin\Debug\net8.0\Email\Templates"

# Verificar carpeta fuente
if (Test-Path $templatesSource) {
    $sourceTemplates = Get-ChildItem -Path $templatesSource -Recurse -Filter "*.html"
    Write-Host "✅ Plantillas encontradas en fuente: $($sourceTemplates.Count)" -ForegroundColor Green
    
    # Listar algunas plantillas
    Write-Host "`nAlgunas plantillas:" -ForegroundColor Yellow
    $sourceTemplates | Select-Object -First 5 | ForEach-Object {
        Write-Host "  - $($_.FullName.Replace($templatesSource, 'Email\Templates'))" -ForegroundColor Gray
    }
} else {
    Write-Host "❌ No se encontró la carpeta de plantillas en: $templatesSource" -ForegroundColor Red
    exit 1
}

# Build del proyecto
Write-Host "`n🔨 Building proyecto..." -ForegroundColor Cyan
dotnet build $projectPath -c Debug --nologo --verbosity quiet

if ($LASTEXITCODE -eq 0) {
    Write-Host "✅ Build completado" -ForegroundColor Green
} else {
    Write-Host "❌ Error en build" -ForegroundColor Red
    exit 1
}

# Verificar carpeta de salida
if (Test-Path $outputPath) {
    $outputTemplates = Get-ChildItem -Path $outputPath -Recurse -Filter "*.html"
    Write-Host "✅ Plantillas copiadas al output: $($outputTemplates.Count)" -ForegroundColor Green
    
    if ($outputTemplates.Count -eq $sourceTemplates.Count) {
        Write-Host "✅ Todas las plantillas se copiaron correctamente!" -ForegroundColor Green
    } else {
        Write-Host "⚠️  Faltan plantillas en el output" -ForegroundColor Yellow
        Write-Host "   Fuente: $($sourceTemplates.Count) | Output: $($outputTemplates.Count)" -ForegroundColor Yellow
    }
} else {
    Write-Host "❌ No se encontró la carpeta de salida: $outputPath" -ForegroundColor Red
    Write-Host "💡 Intenta hacer un build primero" -ForegroundColor Yellow
}

Write-Host "`n✨ Verificación completada" -ForegroundColor Cyan
