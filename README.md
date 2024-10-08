# Car Stocks API

This project is part of the `OracleCMS.CarStocks` solution and represents the API for managing car stocks.

### Running in Docker via Docker Compose

### Step 1: Run docker compose
```bash
docker-compose up --build
```

### Step 2: Access the Web and API
API Base URL: http://localhost:48019
Swagger UI: http://localhost:48019/index.html

Web UI: https://localhost:48021

Username : system@admin
Password: Admin123!@#

### Running in Docker via Docker File
Before running this application in Docker, make sure you have the following installed:

- **Docker**: [Install Docker](https://www.docker.com/get-started) if you don't have it already.

## Running the API with Docker

### Step 1: Clone the Repository

If you haven't already, clone the repository to your local machine:

```bash
git clone https://github.com/supersamamor/oraclecmsexam.git
cd oraclecmsexam
```

### Step 2: Build the Docker Image
1. Navigate to the root directory of the solution, where the .sln file is located.
2. Run the following command to build the Docker image:

```bash
docker build -t supersamamor/oraclecms-carstocks-api:latest -f Dockerfile-API .
```
### Step 3: Run the Docker Container
Once the Docker image is built, run the container using the following command:

```bash
docker run -d -p 48019:8080 --name oraclecms-carstocks-api supersamamor/oraclecms-carstocks-api:latest
```

### Step 4: Access the API
API Base URL: http://localhost:48019
Swagger UI: http://localhost:48019/swagger


### Docker Image Url

https://hub.docker.com/repository/docker/supersamamor/oraclecms-carstocks-api

### Pull docker image
```bash
docker pull supersamamor/oraclecms-carstocks-api
```

### 

### Steps for running the app using UseInMemoryDatabase in Visual Studio
1. Ensure that the 'UseInMemoryDatabase' option from app settings of OracleCMS.CarStocks.API project is set to true.
    ```
      {
         "UseInMemoryDatabase": true,
          "Version": {
            "ReleaseName": "1.0.0.0",
            "BuildNumber": "19000101.0"
          }
      }    
    ```
2. Make sure that the start-up project is 'OracleCMS.CarStocks.API' then Run the application.


### Steps for running the app using MS SQL Database in Visual Studio
1. Ensure that the 'UseInMemoryDatabase' option from app settings of OracleCMS.CarStocks.API project is set to false.
    ```
      {
         "UseInMemoryDatabase": false,
          "Version": {
            "ReleaseName": "1.0.0.0",
            "BuildNumber": "19000101.0"
          }
      }    
    ```

2. Open the Package Manager Console and apply the EF Core migrations

    ```powershell
	Add-Migration -Context ApplicationContext InitialDatabaseStructure
    Update-Database -Context IdentityContext
    Update-Database -Context ApplicationContext
    ```

3. Out of the box, the application assumes that the following URLs are configured
    - Web: https://localhost:56469
    - Web API: https://localhost:56471

      To configure Visual Studio to use the above URLs, edit *launchSettings.json* for the Web API and Web projects.

      Web API
      ```
      {
        "profiles": {
          "MyApp.API": {
            "commandName": "Project",
            "launchBrowser": true,
            "launchUrl": "swagger",
            "environmentVariables": {
              "ASPNETCORE_ENVIRONMENT": "Development"
            },
            "applicationUrl": "https://localhost:56471;http://localhost:44378"
          }
        }
      }    
      ```

      Web
      ```
      {
        "profiles": {
          "MyApp.Web": {
            "commandName": "Project",
            "launchBrowser": true,
            "environmentVariables": {
              "ASPNETCORE_ENVIRONMENT": "Development"
            },
            "applicationUrl": "https://localhost:56469;http://localhost:5000"
          }
        }
      }
      ```
