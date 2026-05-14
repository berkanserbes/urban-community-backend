# Build Stage
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Sadece csproj dosyalarını kopyalayarak restore işlemini önbelleğe alıyoruz
COPY ["backend/src/UrbanCommunity.Api/UrbanCommunity.Api.csproj", "backend/src/UrbanCommunity.Api/"]
COPY ["backend/src/UrbanCommunity.Application/UrbanCommunity.Application.csproj", "backend/src/UrbanCommunity.Application/"]
COPY ["backend/src/UrbanCommunity.Domain/UrbanCommunity.Domain.csproj", "backend/src/UrbanCommunity.Domain/"]
COPY ["backend/src/UrbanCommunity.Infrastructure/UrbanCommunity.Infrastructure.csproj", "backend/src/UrbanCommunity.Infrastructure/"]

RUN dotnet restore "backend/src/UrbanCommunity.Api/UrbanCommunity.Api.csproj"

# Tüm kaynak kodunu kopyalıyoruz
COPY . .
WORKDIR "/src/backend/src/UrbanCommunity.Api"
RUN dotnet build "UrbanCommunity.Api.csproj" -c Release -o /app/build

# Publish Stage
FROM build AS publish
RUN dotnet publish "UrbanCommunity.Api.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Runtime Stage
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app
COPY --from=publish /app/publish .

# Upload ve temp klasörlerini oluştur
RUN mkdir -p /app/wwwroot/uploads && mkdir -p /app/temp/uploads

EXPOSE 80
ENTRYPOINT ["dotnet", "UrbanCommunity.Api.dll"]
