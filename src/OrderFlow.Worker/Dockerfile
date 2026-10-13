
# Compila o Worker utilizando o SDK do .NET 10.
FROM mcr.microsoft.com/dotnet/sdk:10.0-alpine AS build

WORKDIR /source

COPY . .

RUN dotnet restore "src/OrderFlow.Worker/OrderFlow.Worker.csproj"

RUN dotnet publish "src/OrderFlow.Worker/OrderFlow.Worker.csproj" \
    -c Release \
    -o /app/publish \
    --no-restore \
    /p:UseAppHost=false

# O Worker precisa somente do runtime .NET.
FROM mcr.microsoft.com/dotnet/runtime:10.0-alpine AS final

WORKDIR /app

COPY --from=build /app/publish .

ENTRYPOINT ["dotnet", "OrderFlow.Worker.dll"]
