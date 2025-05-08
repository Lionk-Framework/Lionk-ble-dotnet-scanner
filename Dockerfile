FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /source

COPY *.csproj .
#RUN dotnet add package Linux.Bluetooth
RUN dotnet restore --disable-parallel

# Copy the rest of the code and build
COPY . .
RUN dotnet build -c Release -o /app

# Build runtime image
FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app
COPY --from=build /app .

RUN apt-get update && \
    apt-get install -y --no-install-recommends \
    bluez \
    libbluetooth-dev \
    && rm -rf /var/lib/apt/lists/*

ENTRYPOINT ["dotnet", "lionk-ble-net-scanner.dll"]
