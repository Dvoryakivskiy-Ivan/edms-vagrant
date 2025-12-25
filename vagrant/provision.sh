#!/bin/bash

# Update system
apt-get update -y

# Install Docker
apt-get install -y docker.io
systemctl enable docker
systemctl start docker

# Run BaGet container
docker run -d \
  --name baget \
  -p 5555:80 \
  -e ApiKey=12345 \
  loicsharma/baget

# Install .NET SDK
wget https://packages.microsoft.com/config/ubuntu/20.04/packages-microsoft-prod.deb
dpkg -i packages-microsoft-prod.deb
apt-get update -y
apt-get install -y dotnet-sdk-8.0
