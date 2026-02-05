FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copiar archivos de proyecto
COPY ["ManageMyMoney.Presentation.Api/ManageMyMoney.Presentation.Api.csproj", "ManageMyMoney.Presentation.Api/"]
COPY ["ManageMyMoney.Core.Application/ManageMyMoney.Core.Application.csproj", "ManageMyMoney.Core.Application/"]
COPY ["ManageMyMoney.Core.Domain/ManageMyMoney.Core.Domain.csproj", "ManageMyMoney.Core.Domain/"]
COPY ["ManageMyMoney.Infrastructure.Persistence/ManageMyMoney.Infrastructure.Persistence.csproj", "ManageMyMoney.Infrastructure.Persistence/"]
COPY ["ManageMyMoney.Infrastructure.Shared/ManageMyMoney.Infrastructure.Shared.csproj", "ManageMyMoney.Infrastructure.Shared/"]

# Restaurar dependencias
RUN dotnet restore "ManageMyMoney.Presentation.Api/ManageMyMoney.Presentation.Api.csproj"

# Copiar todo el código
COPY . .

# Build
WORKDIR "/src/ManageMyMoney.Presentation.Api"
RUN dotnet build "ManageMyMoney.Presentation.Api.csproj" -c Release -o /app/build

# Publish
FROM build AS publish
RUN dotnet publish "ManageMyMoney.Presentation.Api.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Final
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "ManageMyMoney.Presentation.Api.dll"]
