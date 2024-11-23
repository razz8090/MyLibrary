# Use the official .NET SDK image for building the application
FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build

# Set the working directory inside the container
WORKDIR /app

# Copy the .csproj file(s) to the working directory
COPY *.csproj ./

# Restore the project dependencies
RUN dotnet restore

# Copy the rest of the application source code
COPY . ./

# Build the application in Release mode
RUN dotnet publish -c Release -o /publish

# Use the official .NET runtime image for running the application
FROM mcr.microsoft.com/dotnet/aspnet:7.0 AS runtime

# Set the working directory inside the container
WORKDIR /app

# Copy the published application files from the build stage
COPY --from=build /publish .

# Expose the port your application listens on (default: 5000 for HTTP, 5001 for HTTPS)
EXPOSE 5000
EXPOSE 5001

# Set the command to run your application
ENTRYPOINT ["dotnet", "mylibrary.dll"]
