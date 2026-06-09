# Build Stage
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Sadece csproj dosyalarını kopyalayarak restore işlemini önbelleğe alıyoruz
COPY ["src/UrbanCommunity.Api/UrbanCommunity.Api.csproj", "src/UrbanCommunity.Api/"]
COPY ["src/UrbanCommunity.Application/UrbanCommunity.Application.csproj", "src/UrbanCommunity.Application/"]
COPY ["src/UrbanCommunity.Domain/UrbanCommunity.Domain.csproj", "src/UrbanCommunity.Domain/"]
COPY ["src/UrbanCommunity.Infrastructure/UrbanCommunity.Infrastructure.csproj", "src/UrbanCommunity.Infrastructure/"]

RUN dotnet restore "src/UrbanCommunity.Api/UrbanCommunity.Api.csproj"

# Tüm kaynak kodunu kopyalıyoruz
COPY . .
WORKDIR "/src/src/UrbanCommunity.Api"
RUN dotnet build "UrbanCommunity.Api.csproj" -c Release -o /app/build

# Publish Stage
FROM build AS publish
RUN dotnet publish "UrbanCommunity.Api.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Runtime Stage
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app
COPY --from=publish /app/publish .

# Upload ve temp klasörlerini oluştur
RUN mkdir -p /app/wwwroot/uploads /app/temp/uploads \
    && chown -R $APP_UID:$APP_UID /app

USER $APP_UID

EXPOSE 8080
ENTRYPOINT ["dotnet", "UrbanCommunity.Api.dll"]
