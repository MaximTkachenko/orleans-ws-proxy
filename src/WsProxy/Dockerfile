FROM mcr.microsoft.com/dotnet/sdk:8.0-alpine AS build
WORKDIR /source

# copy csproj and restore as distinct layers
COPY *.csproj .
RUN dotnet restore

# copy and publish app and libraries
COPY . .
RUN dotnet publish --no-restore -o /app


# Enable globalization and time zones:
# https://github.com/dotnet/dotnet-docker/blob/main/samples/enable-globalization.md
# final stage/image
FROM mcr.microsoft.com/dotnet/aspnet:8.0-alpine
# for silo-to-silo
EXPOSE 11111
# for client-to-silo
EXPOSE 30000
# for http api
EXPOSE 8080
# for orleans dashboard
EXPOSE 5223
WORKDIR /app
COPY --from=build /app .
ENTRYPOINT ["./WsProxy"]