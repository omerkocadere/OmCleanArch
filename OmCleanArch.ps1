# Function to log messages with timestamp
function Log {
    param ([string]$message)
    $timestamp = Get-Date -Format "yyyy-MM-dd HH:mm:ss"
    Write-Host "[$timestamp] $message"
}

# Set paths
$dotnetApiPath = "D:\Codes\Omer\CleanArchitecture\OmCleanArch\src\Web.Api"
$dotnetApi2Path = "D:\Codes\Omer\CleanArchitecture\OmCleanArch\other-services\Dummy.Api"
$dotnetApi3Path = "D:\Codes\Omer\CleanArchitecture\OmCleanArch\other-services\SearchService"
$angularClientPath = "D:\Codes\Omer\CleanArchitecture\OmCleanArch\client"

# Start .NET API in a new terminal
Log "Starting .NET API in '$dotnetApiPath'..."
# wt -w 0 nt -p PowerShell -d "$dotnetApiPath" -c dotnet watch -t ".NET API"
wt -w 0 -d "$dotnetApiPath" --title ".NET API" pwsh -NoExit -Command "dotnet watch"
wt -w 0 -d "$dotnetApi2Path" --title ".NET API" pwsh -NoExit -Command "dotnet watch"
wt -w 0 -d "$dotnetApi3Path" --title ".NET API" pwsh -NoExit -Command "dotnet watch"

# Start Angular Client
Log "Checking Angular dependencies in '$angularClientPath'..."
cd $angularClientPath
if (!(Test-Path "node_modules")) {
    Log "node_modules not found. Running npm install..."
    npm install --legacy-peer-deps
}

Log "Starting Angular client with npm run dev..."
wt -w 0 -d "$angularClientPath" --title "Angular Client" pwsh -NoExit -Command "npm start"

	
# npm start
# wt -w 0  -d "$angularClientPath" --title "React Client" pwsh -ExecutionPolicy Bypass -Command "npm run dev"

Log "All processes started successfully."
Read-Host "Press Enter to exit"