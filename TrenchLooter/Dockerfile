FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Copy everything from the project folder (this includes NuGet.Config + nuget-feed/)
COPY . .

# Restore using your local NuGet feed
RUN dotnet restore "TrenchLooter.csproj" --configfile NuGet.Config

# Publish the API
RUN dotnet publish "TrenchLooter.csproj" -c Release -o /app/publish

# Runtime image
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app

COPY --from=build /app/publish .

ENTRYPOINT ["dotnet", "TrenchLooter.dll"]
