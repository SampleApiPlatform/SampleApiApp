## Build stage
#FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
#ARG BUILD_CONFIGURATION=Release
#WORKDIR /src
#
## Copy everything
#COPY . .
#
## Restore
#RUN dotnet restore "./SampleAppApi.csproj"
#
## Build
#RUN dotnet build "./SampleAppApi.csproj" -c $BUILD_CONFIGURATION -o /app/build
#
## Publish
#RUN dotnet publish "./SampleAppApi.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false
#
## Runtime stage
#FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
#WORKDIR /app
#COPY --from=build /app/publish .
#
#ENV ASPNETCORE_URLS=http://+:8080
#EXPOSE 8080
#
#ENTRYPOINT ["dotnet", "SampleAppApi.dll"]
# Build stage
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release

# Set working directory inside the container
WORKDIR /src

# Copy only the csproj first (better caching)
COPY SampleAppApi/SampleAppApi.csproj SampleAppApi/

# Restore dependencies
# RUN dotnet restore "SampleAppApi/SampleAppApi.csproj" 
# Get all the restore packages from the nuget cache in my local machine
# this solution is only when running in public networks (hotspots)
# also disable running jobs in parallel
#COPY nuget.config .
#RUN dotnet restore "SampleAppApi/SampleAppApi.csproj" --disable-parallel
RUN --mount=type=cache,target=/root/.nuget/packages \
    dotnet restore "SampleAppApi/SampleAppApi.csproj" --disable-parallel



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
