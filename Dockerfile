FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build-env
WORKDIR /app

COPY *.sln ./
COPY TaskManager/*.csproj ./TaskManager/

RUN dotnet restore TaskManagerMVC.sln

COPY . ./

WORKDIR /app/TaskManager

RUN dotnet publish TaskManager.csproj -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app

COPY --from=build-env /app/publish .

EXPOSE 80

ENTRYPOINT ["dotnet", "TaskManager.dll"]