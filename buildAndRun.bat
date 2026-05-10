if not exist "TankGL_fbo.slnx" (
    if not exist "src" (
        exit /b 1
    )
)

dotnet restore
if %errorlevel% neq 0 (
    exit /b 1
)

dotnet test
if %errorlevel% neq 0 (
    exit /b 1
)

dotnet build -c Release
if %errorlevel% neq 0 (
    exit /b 1
)

src\TankGL_fbo.WPF\bin\Release\net10.0-windows\win-x64\TankGL_fbo.WPF.exe