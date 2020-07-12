#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/core/aspnet:3.1-buster-slim AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/core/sdk:3.1-buster AS build
WORKDIR /src
COPY ["DominosLocationMap.WebAPI/DominosLocationMap.WebAPI.csproj", "DominosLocationMap.WebAPI/"]
COPY ["DominosLocationMap.Business/DominosLocationMap.Business.csproj", "DominosLocationMap.Business/"]
COPY ["DominosLocationMap.DataAccess/DominosLocationMap.DataAccess.csproj", "DominosLocationMap.DataAccess/"]
COPY ["DominosLocationMap.Entities/DominosLocationMap.Entities.csproj", "DominosLocationMap.Entities/"]
COPY ["DominosLocationMap.Core/DominosLocationMap.Core.csproj", "DominosLocationMap.Core/"]
RUN dotnet restore "DominosLocationMap.WebAPI/DominosLocationMap.WebAPI.csproj"
COPY . .
WORKDIR "/src/DominosLocationMap.WebAPI"
RUN dotnet build "DominosLocationMap.WebAPI.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "DominosLocationMap.WebAPI.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "DominosLocationMap.WebAPI.dll"]
