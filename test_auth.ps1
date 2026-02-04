[System.Net.ServicePointManager]::ServerCertificateValidationCallback = {$true}

$LoginUrl = "https://localhost:62197/api/auth/login"
$UsersUrl = "https://localhost:62197/api/auth/users"
$Body = @{
    username = "admin"
    password = "admin123"
} | ConvertTo-Json

try {
    Write-Host "Logging in..."
    $LoginResponse = Invoke-RestMethod -Uri $LoginUrl -Method Post -Body $Body -ContentType "application/json"
    $Token = $LoginResponse.token
    
    if (-not $Token) {
        Write-Error "No token received. Response: $($LoginResponse | Out-String)"
        exit
    }
    
    Write-Host "Token received: $($Token.Substring(0, 20))..."

    Write-Host "Fetching users..."
    $Headers = @{
        Authorization = "Bearer $Token"
    }
    $UsersResponse = Invoke-RestMethod -Uri $UsersUrl -Method Get -Headers $Headers
    Write-Host "Users received:"
    $UsersResponse | Format-Table
} catch {
    Write-Error $_.Exception.Message
    if ($_.Exception.Response) {
        $Reader = New-Object System.IO.StreamReader($_.Exception.Response.GetResponseStream())
        Write-Error $Reader.ReadToEnd()
    }
}