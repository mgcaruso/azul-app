# Prueba de concurrencia del alta pública: dispara N POST /api/providers a la vez y cuenta las respuestas.
# Requiere PowerShell 7 (pwsh), la API corriendo en local y el límite por IP desactivado.
#
# Uso:
#   pwsh .\scripts\concurrencia-alta.ps1 -Caso 1          # mismo Instagram: esperado 1x201 y 19x409
#   pwsh .\scripts\concurrencia-alta.ps1 -Caso 2          # mismo celular, sin Instagram: esperado 20x201
#   pwsh .\scripts\concurrencia-alta.ps1 -Caso 3          # datos distintos: esperado 20x201
#   pwsh .\scripts\concurrencia-alta.ps1 -Caso 1 -Cantidad 50 -OfferingId 4

param(
    [ValidateSet(1, 2, 3)][int]$Caso = 1,
    [int]$Cantidad = 20,
    [int]$OfferingId = 1,                       # un id de servicio que exista en tu base
    [string]$Url = "http://localhost:5297/api/providers"
)

$sufijo = Get-Date -Format "HHmmss"             # evita chocar con corridas anteriores

$resultados = 1..$Cantidad | ForEach-Object -Parallel {
    $i = $_
    $caso = $using:Caso
    $body = @{
        type          = "Individual"
        displayName   = "Prueba Concurrencia $using:sufijo"
        phoneNumber   = "2241555123"
        offeringIds   = @($using:OfferingId)
        acceptedTerms = $true
    }
    switch ($caso) {
        1 { $body.instagramUsername = "prueba_conc_$using:sufijo" }
        2 { }
        3 {
            $body.displayName = "Prueba Concurrencia $using:sufijo $i"
            $body.phoneNumber = "22415$('{0:D5}' -f $i)"
            $body.instagramUsername = "prueba_conc_${using:sufijo}_$i"
        }
    }
    try {
        (Invoke-WebRequest -Uri $using:Url -Method Post -ContentType "application/json" `
            -Body ($body | ConvertTo-Json)).StatusCode
    }
    catch {
        if ($_.Exception.Response) { [int]$_.Exception.Response.StatusCode } else { "sin respuesta" }
    }
} -ThrottleLimit $Cantidad

Write-Host "Caso $Caso, $Cantidad pedidos:"
$resultados | Group-Object | Sort-Object Name | Select-Object @{n = "Codigo"; e = { $_.Name } }, Count | Format-Table -AutoSize
Write-Host "Ahora mirá en la base cuántas fichas 'Prueba Concurrencia $sufijo' quedaron y borralas."
