
# Compila a aplicação utilizando o SDK do .NET 10.
FROM mcr.microsoft.com/dotnet/sdk:10.0-alpine AS build

WORKDIR /source

COPY . .

RUN dotnet restore "src/OrderFlow.Api/OrderFlow.Api.csproj"

RUN dotnet publish "src/OrderFlow.Api/OrderFlow.Api.csproj" \
    -c Release \
    -o /app/publish \
    --no-restore \
    /p:UseAppHost=false

# Imagem utilizada para executar a API.
FROM mcr.microsoft.com/dotnet/aspnet:10.0-alpine AS final

WORKDIR /app

# Utilizado pelo Health Check do Docker.
RUN apk add --no-cache curl

COPY --from=build /app/publish .

ENV ASPNETCORE_HTTP_PORTS=8080

EXPOSE 8080

ENTRYPOINT ["dotnet", "OrderFlow.Api.dll"]