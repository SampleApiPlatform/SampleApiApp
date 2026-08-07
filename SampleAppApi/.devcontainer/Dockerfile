#Codespaces will use the Dockerfile automatically.
#When someone opens your repo in Codespaces, GitHub:
#Reads devcontainer.json
#Builds the Dockerfile
#Creates a container in the cloud
#Runs VS Code inside that container
FROM mcr.microsoft.com/devcontainers/base:ubuntu

# Install .NET 8 SDK
RUN apt-get update && \
    apt-get install -y dotnet-sdk-8.0
