################################
# 1) BUILD STAGE
################################
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# 1.1) Kopiraj solution
COPY PFM.sln .

# 1.2) Kopiraj cijelu src/ strukturu
COPY src/ src/

# 1.3) Restore NuGet pakete za cijelo rješenje
RUN dotnet restore PFM.sln

# 1.4) Publish API projekt
RUN dotnet publish src/PersonalFinance.API/PersonalFinance.API.csproj \
    -c Release \
    -o /app/publish

################################
# 2) RUNTIME STAGE
################################
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS runtime
WORKDIR /app

COPY --from=build /app/publish .

EXPOSE 80

# obezbedi da Kestrel sluša na svim adresama na portu 80
ENV ASPNETCORE_URLS=http://+:80

ENTRYPOINT ["dotnet", "PersonalFinance.API.dll"]
