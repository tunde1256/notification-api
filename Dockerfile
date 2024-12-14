# Use the .NET 8.0 SDK image
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build

# Set the working directory inside the container
WORKDIR /app

# Copy everything to the working directory
COPY . ./

# Restore dependencies for the project
RUN dotnet restore

# Build and publish the app
RUN dotnet publish -c Release -o out

# Use the .NET runtime for the final image
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime

WORKDIR /app
COPY --from=build /app/out .

# Expose the application's default port
EXPOSE 5145

# Start the application
ENTRYPOINT ["dotnet", "NotificationApi.dll"]
