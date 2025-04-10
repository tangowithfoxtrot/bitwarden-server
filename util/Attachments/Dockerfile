###############################################
#                 Build stage                 #
###############################################
FROM --platform=$BUILDPLATFORM mcr.microsoft.com/dotnet/sdk:8.0 AS build

# Docker buildx supplies the value for this arg
ARG TARGETPLATFORM

# Determine proper runtime value for .NET
# We put the value in a file to be read by later layers.
RUN if [ "$TARGETPLATFORM" = "linux/amd64" ]; then \
      RID=linux-x64 ; \
    elif [ "$TARGETPLATFORM" = "linux/arm64" ]; then \
      RID=linux-arm64 ; \
    elif [ "$TARGETPLATFORM" = "linux/arm/v7" ]; then \
      RID=linux-arm ; \
    fi \
    && echo "RID=$RID" > /tmp/rid.txt

# Copy csproj files as distinct layers
WORKDIR /source
COPY util/Server/*.csproj ./util/Server/
COPY Directory.Build.props .
COPY .editorconfig .

# Restore project dependencies and tools
WORKDIR /source/util/Server
RUN . /tmp/rid.txt && dotnet restore -r $RID

# Copy required project files
WORKDIR /source
COPY util/Server/. ./util/Server/

# Build project
WORKDIR /source/util/Server
RUN . /tmp/rid.txt && dotnet publish -c release -o /app/Server --no-restore --no-self-contained -r $RID

WORKDIR /app

###############################################
#                  App stage                  #
###############################################
FROM mcr.microsoft.com/dotnet/aspnet:8.0

ARG TARGETPLATFORM
LABEL com.bitwarden.product="bitwarden"
ENV ASPNETCORE_ENVIRONMENT=Production
ENV ASPNETCORE_URLS=http://+:5000
EXPOSE 5000

RUN apt-get update \
    && apt-get install -y --no-install-recommends \
        gosu \
        curl \
    && rm -rf /var/lib/apt/lists/*

# Copy app from the build stage
WORKDIR /bitwarden_server
COPY --from=build /app/Server ./
COPY util/Attachments/entrypoint.sh /
RUN chmod +x /entrypoint.sh

HEALTHCHECK CMD curl -f http://localhost:5000/alive || exit 1

ENTRYPOINT ["/entrypoint.sh"]
