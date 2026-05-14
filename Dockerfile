# syntax=docker/dockerfile:1

ARG DOTNET_VERSION=10.0
ARG NUGET_CACHE_PATH=/home/jenkins/.nuget

FROM mcr.microsoft.com/dotnet/sdk:${DOTNET_VERSION} AS build
ARG NUGET_CACHE_PATH
WORKDIR /src

COPY Business/*.csproj Business/
COPY Domain/*.csproj Domain/
COPY Infrastructure/*.csproj Infrastructure/
COPY BillScanner/*.csproj BillScanner/

ENV NUGET_PACKAGES=${NUGET_CACHE_PATH}

RUN --mount=type=cache,id=nuget,target=${NUGET_CACHE_PATH} \
    dotnet restore BillScanner/BillScanner.csproj

COPY . .

RUN --mount=type=cache,id=nuget,target=${NUGET_CACHE_PATH} \
    dotnet publish BillScanner/BillScanner.csproj \
    --configuration Release \
    --property:WarningLevel=0 \
    --output /src/publish \
    --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:${DOTNET_VERSION}-alpine AS final
RUN apk add --no-cache curl

WORKDIR /app
COPY --from=build /src/publish .

EXPOSE 8080
ENTRYPOINT ["dotnet", "BillScanner.dll"]