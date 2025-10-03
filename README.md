# Enterprise Web Application

## Overview
This project is a full-stack enterprise web application built using ASP.NET Core for the backend and Angular for the frontend. It follows the Clean Architecture principles, ensuring a clear separation of concerns and maintainability.

## Project Structure
The project is organized into two main parts: the backend and the frontend.

### Backend
- **src**
  - **WebApi**
    - **Controllers**: Contains API controllers for handling HTTP requests.
    - **Program.cs**: Entry point of the ASP.NET Core application.
    - **appsettings.json**: Configuration settings for the application.
  - **Application**
    - **Interfaces**: Defines interfaces for repositories.
    - **Services**: Contains business logic services.
    - **DTOs**: Data Transfer Objects for communication between layers.
  - **Domain**
    - **Entities**: Contains domain entities.
    - **Interfaces**: Defines domain-specific operations.
  - **Infrastructure**
    - **Data**: Contains the database context and repositories.
    - **Services**: Handles external operations.

- **tests**
  - **UnitTests**: Contains unit tests for application services.
  - **IntegrationTests**: Contains integration tests for API controllers.

- **enterprise-web-app.sln**: Solution file for the backend project.

### Frontend
- **src**
  - **app**
    - **core**: Core services and interfaces.
    - **features**: Feature-specific components and services.
    - **shared**: Shared components and guards.
    - **app.component.ts**: Root component of the Angular application.
    - **app.module.ts**: Root module of the Angular application.
  - **assets**: Static assets like images and styles.
  - **environments**: Environment-specific settings.
  - **main.ts**: Main entry point for the Angular application.

- **angular.json**: Configuration settings for the Angular project.
- **package.json**: Lists dependencies and scripts for the Angular project.
- **tsconfig.json**: TypeScript configuration file.

## Getting Started
To get started with the application, follow these steps:

### Prerequisites
- .NET SDK
- Node.js and npm

### Backend Setup
1. Navigate to the `backend` directory.
2. Restore the NuGet packages:
   ```
   dotnet restore
   ```
3. Run the application:
   ```
   dotnet run
   ```

### Frontend Setup
1. Navigate to the `frontend` directory.
2. Install the dependencies:
   ```
   npm install
   ```
3. Run the Angular application:
   ```
   ng serve
   ```

## Testing
- Unit tests can be run from the `backend` directory using:
  ```
  dotnet test
  ```
- Integration tests can also be executed similarly.

## Contributing
Contributions are welcome! Please submit a pull request or open an issue for any suggestions or improvements.

## License
This project is licensed under the MIT License. See the LICENSE file for details.