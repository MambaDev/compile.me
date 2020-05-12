#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/core/runtime:3.1-buster-slim AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/core/sdk:3.1-buster AS build
WORKDIR /src
COPY ["compile.me.worker.service/compile.me.worker.service.csproj", "compile.me.worker.service/"]
COPY ["compile.me.shared/compile.me.shared.csproj", "compile.me.shared/"]
RUN dotnet restore "compile.me.worker.service/compile.me.worker.service.csproj"
COPY . .
WORKDIR "/src/compile.me.worker.service"
RUN dotnet build "compile.me.worker.service.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "compile.me.worker.service.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Compile.Me.Worker.Service.dll"]