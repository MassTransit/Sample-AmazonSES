FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY ["src/BounceMonitor.Service/BounceMonitor.Service.csproj", "src/BounceMonitor.Service/"]
COPY ["src/BounceMonitor.Components/BounceMonitor.Components.csproj", "src/BounceMonitor.Components/"]

RUN dotnet restore "src/BounceMonitor.Service/BounceMonitor.Service.csproj"
COPY . .
WORKDIR "/src/src/BounceMonitor.Service"
RUN dotnet build "BounceMonitor.Service.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "BounceMonitor.Service.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "BounceMonitor.Service.dll"]
