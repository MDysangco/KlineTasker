# ============================
# 1. Build stage
# ============================
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy everything (including Utility submodule)
COPY . .

# Restore dependencies
RUN dotnet restore "./TrenchLooter.csproj"

# Publish trimmed, ready for runtime
RUN dotnet publish "./TrenchLooter.csproj" -c Release -o /app/publish

# ============================
# 2. Runtime stage
# ============================
FROM mcr.microsoft.com/dotnet/runtime:8.0 AS final
WORKDIR /app

# Copy published output
COPY --from=build /app/publish .

# Run the Tasker
ENTRYPOINT ["dotnet", "TrenchLooter.dll"]
