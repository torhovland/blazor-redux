cd src\Client
dotnet fable webpack -- -p
cd ..\Server
dotnet build
cd ..\..