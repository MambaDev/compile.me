#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/core/runtime:3.1-buster-slim AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/core/sdk:3.1-buster AS build
WORKDIR /src
COPY ["Compile.Me.Worker.Management/Compile.Me.Worker.Management.csproj", "Compile.Me.Worker.Management/"]
COPY ["compile.me.worker.service/compile.me.worker.service.csproj", "compile.me.worker.service/"]
COPY ["compile.me.shared/compile.me.shared.csproj", "compile.me.shared/"]
RUN dotnet restore "Compile.Me.Worker.Management/Compile.Me.Worker.Management.csproj"
COPY . .
WORKDIR "/src/Compile.Me.Worker.Management"
RUN dotnet build "Compile.Me.Worker.Management.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Compile.Me.Worker.Management.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Compile.Me.Worker.Management.dll"]