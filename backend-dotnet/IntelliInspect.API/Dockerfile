# Build stage
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY backend-dotnet/IntelliInspect.API/IntelliInspect.API.csproj backend-dotnet/IntelliInspect.API/
RUN dotnet restore backend-dotnet/IntelliInspect.API/IntelliInspect.API.csproj

COPY backend-dotnet/IntelliInspect.API/ backend-dotnet/IntelliInspect.API/
RUN dotnet publish backend-dotnet/IntelliInspect.API/IntelliInspect.API.csproj -c Release -o /app/publish /p:UseAppHost=false

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
ENV ASPNETCORE_URLS=http://+:8080
COPY --from=build /app/publish .
EXPOSE 8080
ENTRYPOINT ["dotnet", "IntelliInspect.API.dll"]

