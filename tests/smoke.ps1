param([switch]$SkipBuild)
$ErrorActionPreference = 'Stop'
$projectRoot = Split-Path $PSScriptRoot -Parent
Push-Location $projectRoot
$server = $null
$testPrefix = Join-Path ([IO.Path]::GetTempPath()) ('studynotes-' + [guid]::NewGuid().ToString('N'))
$database = $testPrefix + '.db'
$originalHash = if (Test-Path notes.db) { (Get-FileHash notes.db).Hash } else { $null }
$listener = [Net.Sockets.TcpListener]::new([Net.IPAddress]::Loopback, 0)
$listener.Start()
$port = $listener.LocalEndpoint.Port
$listener.Stop()
$baseUrl = "http://127.0.0.1:$port"

function Assert($condition, $message) {
    if (-not $condition) { throw $message }
}
function Start-TestServer {
    $dll = Join-Path $projectRoot 'bin\Debug\net8.0\Notes webapp (ASP.NET CORE).dll'
    $script:server = Start-Process dotnet -ArgumentList @('"' + $dll + '"', '--urls', $baseUrl, '"--ConnectionStrings:Notes=Data Source=' + $database + '"') -WindowStyle Hidden -PassThru -RedirectStandardOutput ($testPrefix + '.out.log') -RedirectStandardError ($testPrefix + '.err.log')
    for ($attempt = 0; $attempt -lt 60; $attempt++) {
        if ($script:server.HasExited) { throw (Get-Content ($testPrefix + '.err.log') -Raw) }
        try { $null = Invoke-RestMethod "$baseUrl/api/notes"; return } catch { Start-Sleep -Milliseconds 200 }
    }
    throw 'Server did not become ready.'
}
function Stop-TestServer {
    if ($script:server -and -not $script:server.HasExited) {
        Stop-Process -Id $script:server.Id
        $script:server.WaitForExit()
    }
}
function Request($method, $path, $body, $expectedStatus) {
    $parameters = @{ Uri = "$baseUrl$path"; Method = $method; SkipHttpErrorCheck = $true }
    if ($null -ne $body) { $parameters.Body = $body; $parameters.ContentType = 'application/json' }
    $response = Invoke-WebRequest @parameters
    Assert ($response.StatusCode -eq $expectedStatus) "$method $path expected $expectedStatus, got $($response.StatusCode)"
    if ($response.Content) { return $response.Content | ConvertFrom-Json }
}
try {
    if (-not $SkipBuild) {
        dotnet build --no-restore --nologo
        Assert ($LASTEXITCODE -eq 0) 'Build failed.'
    }
    Start-TestServer
    $empty = Request GET '/api/notes' $null 200
    Assert ($empty.total -eq 0) 'New database is not empty.'
    $page = Invoke-WebRequest $baseUrl
    Assert ($page.Content.Contains('cancelEdit')) 'Frontend was not served.'
    $note = Request POST '/api/notes' '{"title":" First ","content":" Content ","tag":" study "}' 201
    Assert ($note.title -eq 'First' -and $note.tag -eq 'study') 'Create did not trim/save fields.'
    $id = $note.id
    $null = Request POST '/api/notes' '{"title":" ","content":"text"}' 400
    $null = Request POST '/api/notes' '{"title":null,"content":"text"}' 400
    $null = Request POST '/api/notes' '{' 400
    $null = Request POST '/api/notes' (@{title=('x' * 201);content='text'} | ConvertTo-Json) 400
    $null = Request POST '/api/notes' (@{title='text';content=('x' * 20001)} | ConvertTo-Json) 400
    $null = Request POST '/api/notes' (@{title='text';content='text';tag=('x' * 51)} | ConvertTo-Json) 400
    $updated = Request PUT "/api/notes/$id" '{"title":"Edited","content":"Updated content","tag":"new"}' 200
    Assert ($updated.tag -eq 'new' -and $updated.createdAt -eq $note.createdAt) 'Update changed creation time or lost tag.'
    $null = Request PUT "/api/notes/$id" '{"title":"","content":"text"}' 400
    Stop-TestServer
    Start-TestServer
    $saved = Request GET "/api/notes/$id" $null 200
    Assert ($saved.title -eq 'Edited' -and $saved.content -eq 'Updated content' -and $saved.tag -eq 'new') 'Data did not survive restart.'
    $cleared = Request PUT "/api/notes/$id" '{"title":"Edited","content":"Updated content","tag":" "}' 200
    Assert ($null -eq $cleared.tag) 'Blank tag was not cleared.'
    $null = Request GET '/api/notes/2147483647' $null 404
    $null = Request PUT '/api/notes/2147483647' '{"title":"x","content":"y"}' 404
    $null = Request DELETE "/api/notes/$id" $null 204
    $null = Request DELETE "/api/notes/$id" $null 404
    $null = Request GET "/api/notes/$id" $null 404
    $empty = Request GET '/api/notes' $null 200
    Assert ($empty.total -eq 0) 'Delete did not remove note.'
    if ($originalHash) { Assert ((Get-FileHash notes.db).Hash -eq $originalHash) 'Original database changed.' }
    Write-Host 'PASS: CRUD, tags, validation, missing notes, frontend, restart persistence, original database unchanged.'
} finally {
    Stop-TestServer
    foreach ($suffix in @('.db', '.db-wal', '.db-shm', '.out.log', '.err.log')) {
        $file = $testPrefix + $suffix
        if (Test-Path -LiteralPath $file) { Remove-Item -LiteralPath $file }
    }
    Pop-Location
}
