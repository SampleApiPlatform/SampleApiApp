# Build stage
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release

WORKDIR /src

# Copy csproj
COPY SampleAppApi/SampleAppApi.csproj SampleAppApi/

# Restore
RUN dotnet restore "SampleAppApi/SampleAppApi.csproj"

# Copy the rest of the source code
COPY SampleAppApi/ SampleAppApi/

# Build
RUN dotnet build "SampleAppApi/SampleAppApi.csproj" -c $BUILD_CONFIGURATION -o /app/build

# Publish
RUN dotnet publish "SampleAppApi/SampleAppApi.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app

COPY --from=build /app/publish .

ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080
ENTRYPOINT ["dotnet", "SampleAppApi.dll"]