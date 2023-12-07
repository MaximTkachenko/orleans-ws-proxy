FROM mcr.microsoft.com/dotnet/sdk:8.0-alpine AS build
WORKDIR /source

# copy csproj and restore as distinct layers
COPY WsProxy/*.csproj .
RUN dotnet restore

# copy and publish app and libraries
COPY WsProxy/. .
RUN dotnet publish --no-restore -o /app


# Enable globalization and time zones:
# https://github.com/dotnet/dotnet-docker/blob/main/samples/enable-globalization.md
# final stage/image
FROM mcr.microsoft.com/dotnet/aspnet:8.0-alpine
WORKDIR /app
EXPOSE 11111
EXPOSE 30000
EXPOSE 8080
EXPOSE 5223
COPY --from=build /app .
ENTRYPOINT ["./WsProxy"]