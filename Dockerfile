# Build stage
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy project files for restoring dependencies
COPY ["Backend/Backend.csproj", "Backend/"]
COPY ["DbConnect/DbConnect.csproj", "DbConnect/"]
COPY ["LibraryManagement.Shared/LibraryManagement.Shared.csproj", "LibraryManagement.Shared/"]

# Restore dependencies
RUN dotnet restore "Backend/Backend.csproj"

# Copy everything else and build
COPY . .
WORKDIR "/src/Backend"
RUN dotnet publish "Backend.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Final stage
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
COPY --from=build /app/publish .

# --- HUGGING FACE SPECIFIC CONFIG ---
# Hugging Face requires port 7860
EXPOSE 7860
ENV ASPNETCORE_URLS=http://+:7860
# ------------------------------------

ENTRYPOINT ["dotnet", "Backend.dll"]
