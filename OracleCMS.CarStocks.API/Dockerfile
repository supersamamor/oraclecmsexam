# Stage 1: Build the application
FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /app

# Copy the csproj and restore as distinct layers
COPY *.csproj ./
RUN dotnet restore

# Copy everything else and build the app
COPY . ./
RUN dotnet publish -c Release -o /out

# Stage 2: Serve the application using a smaller runtime image
FROM mcr.microsoft.com/dotnet/aspnet:6.0 AS runtime
WORKDIR /app
COPY --from=build /out .

# Expose port 80 for HTTP traffic
EXPOSE 80

# Set the entry point for the container
ENTRYPOINT ["dotnet", "OracleCMS.CarStocks.API.dll"]
