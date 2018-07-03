FROM microsoft/aspnetcore-build:latest AS build-env
WORKDIR /app

# Copy everything else, restore and build
COPY . ./

RUN dotnet restore Sample.WebApi.csproj
RUN dotnet publish Sample.WebApi.csproj -c Release -o /app/out

# Build runtime image
FROM microsoft/aspnetcore:latest
WORKDIR /app
COPY --from=build-env /app/out .
ENTRYPOINT ["dotnet", "Sample.WebApi.dll"]