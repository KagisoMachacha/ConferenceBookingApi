# Conference Room Booking API

A RESTful API for managing conference room bookings in a co-working space, built with ASP.NET Core and PostgreSQL. 


## AiUsageReport and DesignDocument are in the AssessmentFiles folder

## Prerequisites

- .NET 8.0 SDK 
- PostgreSQL 8 or later
- Your favorite IDE (Visual Studio, VS Code, Rider)

## Getting Started

### 1. Clone the Project

```bash
git clone https://github.com/KagisoMachacha/ConferenceBookingApi.git
cd ConferenceBookingApi
```

### 2. Install Required Packages

```bash
# Install EF Core Tools globally
dotnet tool install --global dotnet-ef
# Or update: dotnet tool install --global dotnet-ef --version 8.0.8

# Restore project packages
dotnet restore

# Or manually install packages if needed:
dotnet add package Npgsql.EntityFrameworkCore.PostgreSQL --version 9.0.2
dotnet add package Microsoft.EntityFrameworkCore.Design --version 9.0.7
dotnet add package Swashbuckle.AspNetCore --version 6.5.0
```

### 3. Configure Database Connection

Edit `appsettings.json` with your PostgreSQL credentials:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Database=ConferenceBooking;Username=postgres;Password=yourpassword"
  }
}
```

### 4. Apply Database Migrations

```bash
# Apply migrations to create database and schema
dotnet ef database update
```
This will:
- Create the `ConferenceBooking` database
- Create the `booking` schema
- Create all tables (users, rooms, amenities, room_amenities, bookings)
- Seed initial data (4 rooms, 5 amenities, 3 users)

**Other migration commands:**
```bash
# View migration history
dotnet ef migrations list

# Rollback to specific migration
dotnet ef database update PreviousMigrationName

# Remove last migration (only if not applied to database)
dotnet ef migrations remove

# Drop database completely
dotnet ef database drop
```

### 5. Build and Run the Application

```bash
# Clean build artifacts
dotnet clean

# Restore NuGet packages
dotnet restore

# Build project
dotnet build

# Run application
dotnet run
```
The API will be available at:

1. Navigate to http://localhost:5087
2. You'll see interactive API documentation
3. Click "Try it out" on any endpoint to test

**Running your tests:**

```bash
# Run tests
dotnet test
```

## 🧪 Testing the API Using cURL

**Get all rooms:** `/api/rooms`
```bash
curl http://localhost:5087/api/rooms
```

**Check availability:** `/api/rooms/{id}/availability?date=YYYY-MM-DD`
```bash
curl "http://localhost:5087/api/rooms/1/availability?date=2025-11-20"
```

**Create a booking:** `/api/bookings`
```bash
curl -X POST http://localhost:5087/api/bookings \
  -H "Content-Type: application/json" \
  -d '{
    "roomId": 1,
    "userId": 1,
    "startTime": "2025-11-20T10:00:00",
    "endTime": "2025-11-20T11:00:00",
    "title": "Team Standup"
  }'
```

**Get user bookings:** `/api/users/{id}/bookings`
```bash
curl http://localhost:5087/api/users/1/bookings
```

**Update a booking:** `/api/bookings/{id}`
```bash
curl -X PATCH http://localhost:5087/api/bookings/1 \
  -H "Content-Type: application/json" \
  -d '{
    "startTime": "2025-11-20T14:00:00",
    "endTime": "2025-11-20T15:00:00"
  }'
```

**Cancel a booking:** `/api/bookings/{id}`
```bash
curl -X DELETE http://localhost:5087/api/bookings/1
```

**Get a booking details:** `/api/bookings/{id}`
```bash
curl -X GET http://localhost:5087/api/bookings/1
```

## 📊 Database Schema

**Schema Name:** `booking`

### Tables

- **users**: User accounts (id, name)
- **rooms**: Conference room details (id, name, capacity, location)
- **amenities**: Available amenities (id, name) - Projector, Whiteboard, Video Conference, Phone, TV Screen
- **room_amenities**: Many-to-many junction table (id, room_id, amenity_id)
- **bookings**: Room reservations (id, room_id, user_id, title, start_time, end_time, status, created_at, updated_at)

See [ERD.markdown](ERD.markdown) for detailed entity relationship diagram.

### Seed Data

The database is automatically seeded with:

**Rooms:**
- Board Room (12 capacity, 3rd Floor) - All amenities
- Small Meeting Room A (4 capacity, 2nd Floor) - Whiteboard, TV Screen
- Small Meeting Room B (4 capacity, 2nd Floor) - Whiteboard, Video Conference
- Large Conference Room (20 capacity, 1st Floor) - Projector, Whiteboard, Video Conference, Phone

**Users:**
- John Doe (ID: 1)
- Jane Smith (ID: 2)
- Bob Wilson (ID: 3)


## 🏗️ Architecture

```
┌─────────────────────────────────────┐
│   Controllers (API Endpoints)       │ ← HTTP Requests
├─────────────────────────────────────┤
│   Services (Business Logic)         │ ← Validation, Rules
├─────────────────────────────────────┤
│   DbContext (Data Access)           │ ← Database Operations
├─────────────────────────────────────┤
│   Models (Entities)                 │ ← Database Tables
└─────────────────────────────────────┘
         ↕ DTOs ↕                       ← Data Transformation
```

## 📝 Business Rules

- **Booking Duration**: Minimum 30 minutes, maximum 4 hours
- **Business Hours**: 9:00 AM - 5:00 PM SAST (South Africa Standard Time, UTC+2)
- **Time Handling**: 
  - Input times treated as SAST (sent as UTC from Swagger: `2025-12-03T09:30:00.852Z`)
  - Stored in UTC+2 in database (PostgreSQL timestamptz: `2025-12-03 09:30:00.852+02`)
  - Displayed in SAST (UTC+2) in API responses
- **Conflict Prevention**: No overlapping bookings for the same room
- **Time Validation**: Cannot book in the past
- **Status Values**: `confirmed`, `cancelled`, `rescheduled`, `booking updated`


### HTTP Status Codes

- `200 OK`: Successful GET request
- `201 Created`: Resource created successfully
- `204 No Content`: Successful DELETE
- `400 Bad Request`: Validation errors, invalid input, business rule violations
- `404 Not Found`: Room, user, or booking not found
- `409 Conflict`: Time slot conflict (double booking)
- `500 Internal Server Error`: Unexpected server errors

## 👤 Author

**Kagiso Machacha**
- GitHub: [@KagisoMachacha](https://github.com/KagisoMachacha)
- Repository: [ConferenceBookingApi](https://github.com/KagisoMachacha/ConferenceBookingApi)




