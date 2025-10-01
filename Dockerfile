# -------------------------------
# Stage 1: Build backend
# -------------------------------
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS backend-build
WORKDIR /src

# Copy csproj files and restore as distinct layers for better caching
COPY ["backend/src/PetCare.Api/PetCare.Api.csproj", "PetCare.Api/"]
COPY ["backend/src/PetCare.Application/PetCare.Application.csproj", "PetCare.Application/"]
COPY ["backend/src/PetCare.Domain/PetCare.Domain.csproj", "PetCare.Domain/"]
COPY ["backend/src/PetCare.Infrastructure/PetCare.Infrastructure.csproj", "PetCare.Infrastructure/"]
COPY ["backend/Directory.Build.props", "./"]

# Restore dependencies (cached layer)
RUN dotnet restore "PetCare.Api/PetCare.Api.csproj"

# Copy source code and build
COPY backend/src/ .
RUN dotnet publish "PetCare.Api/PetCare.Api.csproj" -c Release -o /app/publish /p:UseAppHost=false

# -------------------------------
# Stage 2: Build frontend
# -------------------------------
FROM node:20-alpine AS frontend-build
WORKDIR /src/frontend

# Copy package files
COPY frontend/package*.json ./

# Install dependencies
RUN npm ci

# Copy frontend source
COPY frontend/ ./

# Set production environment variables for React build
RUN echo "VITE_API_BASE_URL=/api" > .env.production
RUN echo "VITE_FEATURE_AUTH_ROUTING=true" >> .env.production

# Build frontend for production
RUN npm run build

# -------------------------------
# Stage 3: Final image
# -------------------------------
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS final
WORKDIR /app

# Copy backend published output
COPY --from=backend-build /app/publish .

# Copy frontend build output into ASP.NET wwwroot
COPY --from=frontend-build /src/frontend/dist ./wwwroot

# Expose port for Azure App Service
EXPOSE 80

# Set environment variables for production
ENV ASPNETCORE_ENVIRONMENT=Production
ENV ASPNETCORE_URLS=http://+:80

# Start backend (serves frontend from wwwroot)
ENTRYPOINT ["dotnet", "PetCare.Api.dll"]