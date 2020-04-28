FROM microsoft/dotnet:2.2-sdk AS builder
WORKDIR /source

COPY . .

RUN dotnet publish "./webapi/webapi.csproj" --configuration Release --framework netcoreapp2.2 --no-build --output /app/

FROM microsoft/dotnet:2.2-aspnetcore-runtime

WORKDIR /app

COPY --from=builder /app .

ENV ASPNETCORE_URLS http://*:80/
EXPOSE 80

ENTRYPOINT [ "dotnet", "webapi.dll" ]
STOPSIGNAL SIGINT
