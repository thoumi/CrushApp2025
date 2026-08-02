# Script PowerShell simple pour gérer la navigation MkDocs
param(
    [Parameter(Position=0)]
    [ValidateSet("generate", "show", "help")]
    [string]$Command = "help"
)

function Show-Help {
    Write-Host "Gestionnaire de Navigation MkDocs" -ForegroundColor Cyan
    Write-Host ""
    Write-Host "Usage: .\nav-manager.ps1 [command]" -ForegroundColor Yellow
    Write-Host ""
    Write-Host "Commandes disponibles:" -ForegroundColor Green
    Write-Host "  generate - Genere la navigation a partir de navigation.yml"
    Write-Host "  show     - Affiche la navigation actuelle"
    Write-Host "  help     - Affiche cette aide"
    Write-Host ""
    Write-Host "Exemples:" -ForegroundColor Green
    Write-Host "  .\nav-manager.ps1 generate"
    Write-Host "  .\nav-manager.ps1 show"
}

function Generate-Navigation {
    Write-Host "Generation de la navigation..." -ForegroundColor Yellow
    
    if (Test-Path "generate_navigation.py") {
        python generate_navigation.py
        if ($LASTEXITCODE -eq 0) {
            Write-Host "Navigation generee avec succes!" -ForegroundColor Green
        } else {
            Write-Host "Erreur lors de la generation" -ForegroundColor Red
        }
    } else {
        Write-Host "Script generate_navigation.py non trouve" -ForegroundColor Red
    }
}

function Show-Navigation {
    Write-Host "Navigation actuelle:" -ForegroundColor Cyan
    
    if (Test-Path "mkdocs-simple.yml") {
        $content = Get-Content "mkdocs-simple.yml" -Raw
        $navStart = $content.IndexOf("nav:")
        $navEnd = $content.IndexOf("# Copyright")
        
        if ($navStart -ne -1 -and $navEnd -ne -1) {
            $navContent = $content.Substring($navStart, $navEnd - $navStart)
            Write-Host $navContent -ForegroundColor White
        } else {
            Write-Host "Section nav non trouvee" -ForegroundColor Red
        }
    } else {
        Write-Host "Fichier mkdocs-simple.yml non trouve" -ForegroundColor Red
    }
}

# Traitement des commandes
switch ($Command) {
    "generate" { Generate-Navigation }
    "show" { Show-Navigation }
    "help" { Show-Help }
    default { Show-Help }
}

Write-Host ""
Write-Host "Pour plus d'informations, utilisez: .\nav-manager.ps1 help" -ForegroundColor Cyan
