FROM mcr.microsoft.com/dotnet/runtime:10.0 AS base
WORKDIR /app

# Build stage
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Copy NuGet.Config so restore knows about /root/nuget-feed
COPY NuGet.Config .

# Copy private NuGet feed into the container
COPY nuget-feed /root/nuget-feed

# Copy the rest of the source
COPY . .

# Publish
RUN dotnet publish "TrenchLooter/TrenchLooter.csproj" -c Release -o /app/publish

# Final stage
FROM base AS final
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "TrenchLooter.dll"]
