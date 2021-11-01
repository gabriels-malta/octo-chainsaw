dotnet build BaaS.TechCase.FundTransfer.sln

wt --title API dotnet run -p .\TechCase.Services.Api.FundTransfer\TechCase.Services.Api.FundTransfer.csproj --no-build

wt --title FundTransferStarter dotnet run -p ./TechCase.Services.Worker.FundTransferStarter/TechCase.Services.Worker.FundTransferStarter.csproj --no-build

wt --title AccountDiscovery dotnet run dotnet run -p ./TechCase.Services.Worker.AccountDiscovery/TechCase.Services.Worker.AccountDiscovery.csproj --no-build

wt --title FundTransferTriggered dotnet run -p ./TechCase.Services.Worker.FundTransferTriggered/TechCase.Services.Worker.FundTransferTriggered.csproj --no-build

wt --title AccountUpdate dotnet run -p ./TechCase.Services.Worker.AccountUpdate/TechCase.Services.Worker.AccountUpdate.csproj --no-build

wt --title FundTransferFinished dotnet run -p ./TechCase.Services.Worker.FundTransferFinished/TechCase.Services.Worker.FundTransferFinished.csproj --no-build

wt --title FundTransferFailed dotnet run -p ./TechCase.Services.Worker.FundTransferFailed/TechCase.Services.Worker.FundTransferFailed.csproj --no-build