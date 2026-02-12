# Multi-stage build for DailyNotes.Api
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Copy csproj and restore
COPY ["src/DailyNotes.Api/DailyNotes.Api.csproj", "src/DailyNotes.Api/"]
COPY ["src/DailyNotes.Core/DailyNotes.Core.csproj", "src/DailyNotes.Core/"]
COPY ["src/DailyNotes.Infrastructure/DailyNotes.Infrastructure.csproj", "src/DailyNotes.Infrastructure/"]
RUN dotnet restore "src/DailyNotes.Api/DailyNotes.Api.csproj"

# Copy everything else and build
COPY . .
WORKDIR "/src/src/DailyNotes.Api"
RUN dotnet build "DailyNotes.Api.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "DailyNotes.Api.csproj" -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "DailyNotes.Api.dll"]
