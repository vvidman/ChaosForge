# Stage 1: build frontend
FROM node:22-alpine AS fe-build
WORKDIR /app/web
COPY web/package*.json ./
RUN npm ci
COPY web/ .
RUN npm run build

# Stage 2: build backend
FROM mcr.microsoft.com/dotnet/sdk:10.0-alpine AS be-build
WORKDIR /app
COPY ChaosForge.slnx ./
COPY src/ src/
RUN dotnet publish src/ChaosForge.API/ChaosForge.API.csproj \
    -c Release -o /publish

# Stage 3: runtime
FROM mcr.microsoft.com/dotnet/aspnet:10.0-alpine AS runtime
WORKDIR /app
COPY --from=be-build /publish ./
COPY --from=fe-build /app/web/dist ./wwwroot
EXPOSE 8080
ENV ASPNETCORE_URLS=http://+:8080
ENTRYPOINT ["dotnet", "ChaosForge.API.dll"]
