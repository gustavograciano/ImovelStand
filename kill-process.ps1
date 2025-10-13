# Script para matar processos do ImovelStand.Api
Write-Host "Procurando processos ImovelStand.Api..."

$processes = Get-Process -Name "ImovelStand.Api" -ErrorAction SilentlyContinue

if ($processes) {
    foreach ($proc in $processes) {
        Write-Host "Matando processo: $($proc.Id) - $($proc.Name)"
        Stop-Process -Id $proc.Id -Force
    }
    Write-Host "Processos finalizados com sucesso!"
} else {
    Write-Host "Nenhum processo ImovelStand.Api encontrado."
}

Write-Host "`nVerificando processos dotnet na porta 5082..."
$netstat = netstat -ano | Select-String ":5082"
if ($netstat) {
    Write-Host $netstat
    $netstat | ForEach-Object {
        $pid = ($_ -split '\s+')[-1]
        if ($pid -match '^\d+$') {
            Write-Host "Matando processo na porta 5082: PID $pid"
            Stop-Process -Id $pid -Force -ErrorAction SilentlyContinue
        }
    }
}

Write-Host "`nPronto! Agora você pode executar o projeto no Visual Studio."
