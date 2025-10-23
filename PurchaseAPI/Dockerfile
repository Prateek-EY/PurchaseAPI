# See https://aka.ms/customizecontainer
FROM mcr.microsoft.com/dotnet/aspnet:6.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src

# Copy projects
COPY ["PurchaseAPI/PurchaseAPI.csproj", "PurchaseAPI/"]
COPY ["Purchase.Infrastructure/Purchase.Infrastructure.csproj", "Purchase.Infrastructure/"]
COPY ["Purchase.Core/Purchase.Core.csproj", "Purchase.Core/"]

# Restore, build, publish
RUN dotnet restore "./PurchaseAPI/PurchaseAPI.csproj"
COPY . .
WORKDIR "/src/PurchaseAPI"
RUN dotnet build "./PurchaseAPI.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./PurchaseAPI.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app

# Copy published files
COPY --from=publish /app/publish .

# Copy local dev certs if present
COPY ./certs ./certs
COPY ./certs/*.crt /usr/local/share/ca-certificates/

# Update CA certificates
RUN update-ca-certificates --fresh
ENTRYPOINT ["dotnet", "PurchaseAPI.dll"]
