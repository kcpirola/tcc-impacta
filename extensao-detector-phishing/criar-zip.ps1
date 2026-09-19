# Gera o ZIP da extensao para enviar aos funcionarios.
# Uso:  .\criar-zip.ps1

$ErrorActionPreference = "Stop"

$pasta = Split-Path -Parent $MyInvocation.MyCommand.Definition
$zip = Join-Path $pasta "detector-phishing.zip"

$arquivos = @(
    "manifest.json",
    "config.js",
    "background.js",
    "content.js",
    "popup.html",
    "popup.css",
    "popup.js",
    "LEIA-ME.txt"
)

# Confere se a URL aponta para um endereco que nao serve para distribuicao:
# o exemplo original, a maquina de desenvolvimento ou o proprio PC do funcionario.
$config = Get-Content (Join-Path $pasta "config.js") -Raw
if ($config -match 'const SERVIDOR = "https?://(192\.168\.1\.100|localhost|127\.0\.0\.1)') {
    Write-Host "ATENCAO: config.js nao aponta para o servidor da empresa." -ForegroundColor Yellow
    Write-Host "         Hoje esta assim:" -ForegroundColor Yellow
    Write-Host "         $(($config -split "`n" | Select-String 'const SERVIDOR').Line.Trim())" -ForegroundColor Yellow
    Write-Host "         192.168.1.100 e o endereco de exemplo, e localhost seria o" -ForegroundColor Yellow
    Write-Host "         PC do proprio funcionario." -ForegroundColor Yellow
    Write-Host ""
    $resposta = Read-Host "Gerar o ZIP assim mesmo? (s/N)"
    if ($resposta -ne "s") {
        Write-Host "Cancelado."
        return
    }
}

$temp = Join-Path $env:TEMP ("extensao-phishing-" + [guid]::NewGuid().ToString("N"))
New-Item -ItemType Directory -Path $temp -Force | Out-Null

try {
    foreach ($arquivo in $arquivos) {
        $origem = Join-Path $pasta $arquivo
        if (-not (Test-Path $origem)) {
            throw "Arquivo nao encontrado: $arquivo"
        }
        Copy-Item $origem -Destination $temp
    }

    if (Test-Path $zip) { Remove-Item $zip -Force }

    Compress-Archive -Path (Join-Path $temp "*") -DestinationPath $zip

    $tamanho = [math]::Round((Get-Item $zip).Length / 1KB, 1)
    Write-Host ""
    Write-Host "ZIP gerado: $zip ($tamanho KB)" -ForegroundColor Green
    Write-Host "Envie esse arquivo aos funcionarios. As instrucoes estao no LEIA-ME.txt dentro dele."
}
finally {
    Remove-Item $temp -Recurse -Force -ErrorAction SilentlyContinue
}
