# See https://aka.ms/customizecontainer to learn how to customize your debug container and how Visual Studio uses this Dockerfile to build your images for faster debugging.

# This stage is used when running from VS in fast mode (Default for Debug configuration)
FROM mcr.microsoft.com/dotnet/aspnet:6.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

# This stage is used to build the service project
FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["PurchaseAPI/PurchaseAPI.csproj", "PurchaseAPI/"]
COPY ["Purchase.Infrastructure/Purchase.Infrastructure.csproj", "Purchase.Infrastructure/"]
COPY ["Purchase.Core/Purchase.Core.csproj", "Purchase.Core/"]
RUN dotnet restore "./PurchaseAPI/PurchaseAPI.csproj"
COPY . .
WORKDIR "/src/PurchaseAPI"
RUN dotnet build "./PurchaseAPI.csproj" -c $BUILD_CONFIGURATION -o /app/build

# This stage is used to publish the service project to be copied to the final stage
FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./PurchaseAPI.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

# This stage is used in production or when running from VS in regular mode (Default when not using the Debug configuration)
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
# Copy the entire certs folder (contains both .crt and .p12)
COPY ./certs ./certs

# Install CA certificates and update the store (picks up any .crt files in /usr/local/share/ca-certificates/)
RUN apt-get update && \
    apt-get install -y --no-install-recommends ca-certificates && \
    # Copy all .crt files from your certs folder to system CA store
    cp /app/certs/*.crt /usr/local/share/ca-certificates/ 2>/dev/null || true && \
    update-ca-certificates && \
    rm -rf /var/lib/apt/lists/*

# Ensure .NET uses system SSL
ENV DOTNET_SYSTEM_NET_HTTP_USESOCKETSHTTPHANDLER=0
ENTRYPOINT ["dotnet", "PurchaseAPI.dll"]