FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
COPY src/Azul.Api/Azul.Api.csproj src/Azul.Api/
RUN dotnet restore src/Azul.Api/Azul.Api.csproj
COPY src/Azul.Api src/Azul.Api
RUN dotnet publish src/Azul.Api/Azul.Api.csproj -c Release -o /app --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:10.0
WORKDIR /app
COPY --from=build /app .
ENV ASPNETCORE_HTTP_PORTS=8080
ENV ASPNETCORE_FORWARDEDHEADERS_ENABLED=true
EXPOSE 8080
ENTRYPOINT ["dotnet", "Azul.Api.dll"]
