FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY StudyENEM.API/StudyENEM.API.csproj StudyENEM.API/
RUN dotnet restore StudyENEM.API/StudyENEM.API.csproj
COPY . .
RUN dotnet publish StudyENEM.API/StudyENEM.API.csproj -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app
RUN apt-get update \
    && apt-get install -y --no-install-recommends curl \
    && rm -rf /var/lib/apt/lists/*
RUN mkdir -p /app/data
COPY --from=build /app/publish .
EXPOSE 8080
ENV ASPNETCORE_URLS=http://+:8080
ENTRYPOINT ["dotnet", "StudyENEM.API.dll"]
