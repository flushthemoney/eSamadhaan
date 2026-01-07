# eSamadhaan

A grievance portal for government usage built with ASP.NET & Angular

## Prerequisites

### Backend

- .NET 10.0 SDK
- SQL Server (local or remote)

### Frontend

- Node.js (v18 or higher)
- npm (v11.6.2 or compatible)

## Backend Setup

1. Navigate to the backend API directory:

   ```bash
   cd backend/src/eSamadhaan.API
   ```

2. Configure the database connection string in `appsettings.json` or `appsettings.Development.json`:

   ```json
   {
     "ConnectionStrings": {
       "DefaultConnection": "Server=localhost;Database=esamadhaan;Trusted_Connection=True;TrustServerCertificate=True;"
     }
   }
   ```

3. Configure JWT settings in `appsettings.json`:

   ```json
   {
     "JwtSettings": {
       "SecretKey": "YOUR_SECURE_KEY_AT_LEAST_32_CHARACTERS_LONG",
       "Issuer": "eSamadhaan",
       "Audience": "eSamadhaan-Users",
       "ExpirationMinutes": 120
     }
   }
   ```

4. Run database migrations:

   ```bash
   dotnet ef database update
   ```

   (Run this from the `eSamadhaan.API` directory)

5. Seed the database with initial data (optional):

   ```bash
   cd ../eSamadhaan.DatabaseSeeder
   dotnet run
   ```

6. Run the API:
   ```bash
   cd ../eSamadhaan.API
   dotnet run
   ```

The API will be available at `http://localhost:5124` (or the port configured in `launchSettings.json`).

## Frontend Setup

1. Navigate to the frontend directory:

   ```bash
   cd frontend/esamadhaan-ui
   ```

2. Install dependencies:

   ```bash
   npm install
   ```

3. Start the development server:
   ```bash
   npm start
   ```

The frontend will be available at `http://localhost:4200`.

## Default Login Credentials

After running the database seeder, you can use these credentials:

- **Admin**: `admin@esamadhaan.test` / `Password123!`
- **Supervisor**: `supervisor.pwd@esamadhaan.test` / `Password123!`
- **Officer**: `officer.pwd1@esamadhaan.test` / `Password123!`
- **Citizen**: `citizen.ramesh@test.com` / `Password123!`
