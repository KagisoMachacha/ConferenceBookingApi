# Conference Booking System - Entity Relationship Diagram

## Database Schema: `booking`

```mermaid
erDiagram
    users {
        int id PK "Primary Key"
        varchar(100) name "User full name"
    }

    rooms {
        int id PK "Primary Key"
        varchar(100) name "Room name (indexed)"
        int capacity "Maximum occupancy (1-500)"
        varchar(200) location "Room location/building (nullable)"
    }

    amenities {
        int id PK "Primary Key"
        varchar(50) name UK "Amenity name (Projector, Whiteboard, Video Conference, Phone, TV Screen)"
    }

    room_amenities {
        int id PK "Primary Key"
        int room_id FK "References rooms(id)"
        int amenity_id FK "References amenities(id)"
    }

    bookings {
        int id PK "Primary Key"
        int room_id FK "References rooms(id)"
        int user_id FK "References users(id)"
        varchar(100) title "Booking title"
        timestamptz start_time "Booking start (UTC)"
        timestamptz end_time "Booking end (UTC)"
        varchar(20) status "confirmed, cancelled, rescheduled, booking updated"
        timestamptz created_at "Record creation timestamp"
        timestamptz updated_at "Last update timestamp"
    }

    users ||--o{ bookings : "creates"
    rooms ||--o{ bookings : "has"
    rooms ||--o{ room_amenities : "has"
    amenities ||--o{ room_amenities : "available_in"
```

## Business Rules

- **Users**: Simple name-only profile 
    
- **Rooms**: 
  - Can have multiple amenities (many-to-many via room_amenities)
  - Capacity range: 1-500 people
  - Location is optional
- **Amenities**: 
  - Unique amenity names
  - Shared across multiple rooms
  - System includes: Projector, Whiteboard, Video Conference, Phone, TV Screen
- **Bookings**: 
  - Duration: 30 minutes to 4 hours
  - Time: Between 9:00 AM and 5:00 PM SAST
  - No overlapping bookings per room (conflict prevention)
  - All times stored in UTC, displayed/validated in SAST
  - Status values: `confirmed`, `cancelled`, `rescheduled`, `booking updated`
  - Restrict delete on Room/User (preserve booking history)
- **Room Amenities**: Junction table for many-to-many relationship
  - Unique constraint on (room_id, amenity_id) pair
  - Cascade delete when room or amenity is deleted

## Database Indexes

Configured in BookingDbContext.OnModelCreating:

- `rooms.name` - For room searches
- `amenities.name` - Unique index
- `room_amenities(room_id, amenity_id)` - Unique composite index
- `bookings(room_id, start_time, end_time)` - For availability queries
- `bookings.user_id` - For user booking history
- `bookings.status` - For filtering active bookings

## Seed Data

**Amenities:**
- Projector, Whiteboard, Video Conference, Phone, TV Screen

**Rooms:**
1. Board Room - 12 capacity, 3rd Floor (all amenities)
2. Small Meeting Room A - 4 capacity, 2nd Floor (Whiteboard, TV Screen)
3. Small Meeting Room B - 4 capacity, 2nd Floor (Whiteboard, Video Conference)
4. Large Conference Room - 20 capacity, 1st Floor (Projector, Whiteboard, Video Conference, Phone)

**Users:**
1. John Doe
2. Jane Smith
3. Bob Wilson
