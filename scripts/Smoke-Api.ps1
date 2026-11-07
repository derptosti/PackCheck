param([string]$BaseUrl = 'http://localhost:5092')

$ErrorActionPreference = 'Stop'
$apiOrigin = $BaseUrl.TrimEnd('/')
$createdTrip = $null

try {
    $health = Invoke-RestMethod -Uri "$apiOrigin/health"
    if ($health.status -ne 'ready') { throw 'The API is not ready.' }

    $tripRequest = @{
        name = "PackCheck smoke $([guid]::NewGuid().ToString('N').Substring(0, 8))"
        departureDate = (Get-Date).Date.ToString('yyyy-MM-ddTHH:mm:ss')
        templateKey = 'camping'
    } | ConvertTo-Json
    $createdTrip = Invoke-RestMethod -Method Post -Uri "$apiOrigin/api/trips" -ContentType 'application/json' -Body $tripRequest
    $torch = $createdTrip.items | Where-Object { $_.categoryKey -eq 'torch' } | Select-Object -First 1
    if ($null -eq $torch -or $createdTrip.items.Count -lt 2) { throw 'The camping checklist was not created.' }

    $itemRequest = @{
        name = $torch.name
        categoryKey = $torch.categoryKey
        requiredQuantity = $torch.requiredQuantity
        packedQuantity = 1
        isEssential = $torch.isEssential
    } | ConvertTo-Json
    $null = Invoke-RestMethod -Method Put -Uri "$apiOrigin/api/trips/$($createdTrip.id)/items/$($torch.id)" -ContentType 'application/json' -Body $itemRequest

    $savedTrip = Invoke-RestMethod -Uri "$apiOrigin/api/trips/$($createdTrip.id)"
    $savedTorch = $savedTrip.items | Where-Object { $_.id -eq $torch.id } | Select-Object -First 1
    if ($savedTorch.packedQuantity -ne 1 -or $savedTrip.packedQuantity -ne 1 -or $savedTrip.progress -le 0) {
        throw 'The saved quantity or trip progress is incorrect.'
    }
    Write-Output 'PASS: API health, template creation, related items, saved quantity and trip progress.'
}
finally {
    # Remove only the test trip created by this invocation, even after an assertion fails.
    if ($null -ne $createdTrip -and $createdTrip.id -gt 0) {
        $null = Invoke-RestMethod -Method Delete -Uri "$apiOrigin/api/trips/$($createdTrip.id)"
        Write-Output 'Removed the smoke-test trip and its checklist.'
    }
}
