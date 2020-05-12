#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/core/aspnet:3.1-buster-slim AS base
WORKDIR /app
EXPOSE 80

FROM mcr.microsoft.com/dotnet/core/sdk:3.1-buster AS build
WORKDIR /src
COPY ["compile.me.api/compile.me.api.csproj", "compile.me.api/"]
COPY ["compile.me.shared/compile.me.shared.csproj", "compile.me.shared/"]
RUN dotnet restore "compile.me.api/compile.me.api.csproj"
COPY . .
WORKDIR "/src/compile.me.api"
RUN dotnet build "compile.me.api.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "compile.me.api.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "compile.me.api.dll"]