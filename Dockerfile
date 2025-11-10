FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
USER $APP_UID
WORKDIR /app
EXPOSE 80

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["ClientConsumerOrder/ClientConsumerOrder.csproj", "ClientConsumerOrder/"]
RUN dotnet restore "./ClientConsumerOrder/ClientConsumerOrder.csproj"
COPY . .
WORKDIR "/src/ClientConsumerOrder"
RUN dotnet build "./ClientConsumerOrder.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./ClientConsumerOrder.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
# Crear directorio para datos persistentes
RUN mkdir -p /app/data
VOLUME /app/data
ENTRYPOINT ["dotnet", "ClientConsumerOrder.dll"]