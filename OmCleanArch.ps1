# Function to log messages with timestamp
function Log {
    param ([string]$message)
    $timestamp = Get-Date -Format "yyyy-MM-dd HH:mm:ss"
    Write-Host "[$timestamp] $message"
}

# Get the current script directory
$scriptDir = $PSScriptRoot

# Set paths (relative to script directory)
$angularClientPath = Join-Path $scriptDir "client"                         # Angular frontend
$WebApiPath = Join-Path $scriptDir "src\Web.Api"                           # Main Web API
$DummyApiPath = Join-Path $scriptDir "other-services\Dummy.Api"            # Test/Demo API
$SearchApiPath = Join-Path $scriptDir "other-services\SearchService"       # Search microservice
$IdentityApiPath = Join-Path $scriptDir "other-services\IdentityService"   # Identity/Auth service
$GatewayPath = Join-Path $scriptDir "other-services\GatewayService"        # API Gateway

# Start Angular Client
Log "Checking Angular dependencies in '$angularClientPath'..."
cd $angularClientPath
if (!(Test-Path "node_modules")) {
    Log "node_modules not found. Running npm install..."
    npm install --legacy-peer-deps
}

Log "Starting Angular client with npm run dev..."
wt -w 0 -d "$angularClientPath" --title "Angular Client" pwsh -NoExit -Command "ng serve --port 4201"	

# Start .NET API in a new terminal
Log "Starting main Web API..."
wt -w 0 -d "$WebApiPath" --title "WebApi" pwsh -NoExit -Command "dotnet watch"
Start-Sleep -Milliseconds 500

Log "Core development environment started (Angular Client + Web API)."
Log ""

# Ask if user wants to start additional services
$startAdditionalServices = Read-Host "Diğer servisleri de başlatmak ister misiniz? (Y/N)"

if ($startAdditionalServices -eq "Y" -or $startAdditionalServices -eq "y") {
    Log "Starting additional services..."
    Start-Sleep -Milliseconds 100
    wt -w 0 -d "$DummyApiPath" --title "DummyApi" pwsh -NoExit -Command "dotnet watch"
    Start-Sleep -Milliseconds 100
    wt -w 0 -d "$SearchApiPath" --title "SearchApi" pwsh -NoExit -Command "dotnet watch"
    Start-Sleep -Milliseconds 100
    wt -w 0 -d "$IdentityApiPath" --title "IdentityService" pwsh -NoExit -Command "dotnet watch"
    Start-Sleep -Milliseconds 100
    wt -w 0 -d "$GatewayPath" --title "GatewayService" pwsh -NoExit -Command "dotnet watch"
    
    Log "All services started successfully (Full Stack)."
} else {
    Log "Only core services started (Angular Client + Web API)."
}
Read-Host "Press Enter to exit"