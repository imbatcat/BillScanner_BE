# syntax=docker/dockerfile:1

ARG DOTNET_VERSION=10.0
ARG NUGET_CACHE_PATH=/home/jenkins/.nuget

FROM mcr.microsoft.com/dotnet/sdk:${DOTNET_VERSION} AS build
WORKDIR /src

RUN ls

COPY BillScanner.deploy.slnx .
COPY Business/*.csproj Business/
COPY Domain/*.csproj Domain/
COPY Infrastructure/*.csproj Infrastructure/
COPY BillScanner/*.csproj BillScanner/

ENV NUGET_PACKAGES=${NUGET_CACHE_PATH}

RUN --mount=type=cache,id=nuget,target=${NUGET_CACHE_PATH} \
    dotnet restore BillScanner.deploy.slnx

COPY . .

RUN --mount=type=cache,id=nuget,target=${NUGET_CACHE_PATH} \
    dotnet build BillScanner/BillScanner.csproj \
    --configuration Release \
    --property:WarningLevel=0 \
    --no-restore

RUN --mount=type=cache,id=nuget,target=${NUGET_CACHE_PATH} \
    dotnet publish BillScanner/BillScanner.csproj \
    --configuration Release \
    --output /src/publish \
    --no-restore \
    --no-build

FROM mcr.microsoft.com/dotnet/aspnet:${DOTNET_VERSION} AS final
RUN apt-get update && apt-get install -y --no-install-recommends curl \
    && rm -rf /var/lib/apt/lists/*

WORKDIR /app
COPY --from=build /src/publish .

EXPOSE 8080
ENTRYPOINT ["dotnet", "BillScanner.dll"]