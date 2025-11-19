using ConferenceBookingSystem.Models;  
using Microsoft.EntityFrameworkCore;

namespace ConferenceBookingSystem;

public class BookingDbContext : DbContext
{
    public BookingDbContext(DbContextOptions<BookingDbContext> options) : base(options) { }

    public DbSet<Room> Rooms { get; set; }
    public DbSet<User> Users { get; set; }
    public DbSet<Booking> Bookings { get; set; }
    public DbSet<Amenity> Amenities { get; set; }
    public DbSet<RoomAmenity> RoomAmenities { get; set; }
    
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema("booking");
        base.OnModelCreating(modelBuilder);

        // ===== CONFIGURE TABLES =====
        
        // Configure Room
        modelBuilder.Entity<Room>(entity =>
        {
            entity.HasKey(r => r.Id);
            entity.HasIndex(r => r.Name);
            entity.Property(r => r.Name).IsRequired();
        });
        
        // Configure Amenity
        modelBuilder.Entity<Amenity>(entity =>
        {
            entity.HasKey(a => a.Id);
            entity.HasIndex(a => a.Name).IsUnique();
        });
        
        // Configure RoomAmenity (Many-to-Many)
        modelBuilder.Entity<RoomAmenity>(entity =>
        {
            entity.HasKey(ra => ra.Id);
            entity.HasIndex(ra => new { ra.RoomId, ra.AmenityId }).IsUnique();
            
            entity.HasOne(ra => ra.Room)
                .WithMany(r => r.RoomAmenities)
                .HasForeignKey(ra => ra.RoomId)
                .OnDelete(DeleteBehavior.Cascade);
            
            entity.HasOne(ra => ra.Amenity)
                .WithMany(a => a.RoomAmenities)
                .HasForeignKey(ra => ra.AmenityId)
                .OnDelete(DeleteBehavior.Cascade);
        });
        
        // Configure Booking
        modelBuilder.Entity<Booking>(entity =>
        {
            entity.HasKey(b => b.Id);
            entity.HasIndex(b => new { b.RoomId, b.StartTime, b.EndTime });
            entity.HasIndex(b => b.UserId);
            entity.HasIndex(b => b.Status);
            
            entity.Property(b => b.Title).IsRequired();
            entity.Property(b => b.Status).IsRequired();
            
            entity.HasOne(b => b.Room)
                .WithMany(r => r.Bookings)
                .HasForeignKey(b => b.RoomId)
                .OnDelete(DeleteBehavior.Restrict);
            
            entity.HasOne(b => b.User)
                .WithMany(u => u.Bookings)
                .HasForeignKey(b => b.UserId)
                .OnDelete(DeleteBehavior.Restrict);
        });
        
        // Configure User
        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(u => u.Id);
            entity.Property(u => u.Name).IsRequired();
        });
        
        // ===== SEED DATA =====
        SeedData(modelBuilder);
    }
    
    private void SeedData(ModelBuilder modelBuilder)
    {
        // Seed Amenities
        modelBuilder.Entity<Amenity>().HasData(
            new Amenity { Id = 1, Name = "Projector"},
            new Amenity { Id = 2, Name = "Whiteboard"},
            new Amenity { Id = 3, Name = "Video Conference"},
            new Amenity { Id = 4, Name = "Phone"},
            new Amenity { Id = 5, Name = "TV Screen"}
        );
        
        // Seed Rooms
        modelBuilder.Entity<Room>().HasData(
            new Room { Id = 1, Name = "Board Room", Capacity = 12, Location = "3rd Floor" },
            new Room { Id = 2, Name = "Small Meeting Room A", Capacity = 4, Location = "2nd Floor" },
            new Room { Id = 3, Name = "Small Meeting Room B", Capacity = 4, Location = "2nd Floor" },
            new Room { Id = 4, Name = "Large Conference Room", Capacity = 20, Location = "1st Floor" }
        );
        
        // Seed RoomAmenities (which rooms have which amenities)
        modelBuilder.Entity<RoomAmenity>().HasData(
            // Board Room: All amenities
            new RoomAmenity { Id = 1, RoomId = 1, AmenityId = 1 },
            new RoomAmenity { Id = 2, RoomId = 1, AmenityId = 2 },
            new RoomAmenity { Id = 3, RoomId = 1, AmenityId = 3 },
            new RoomAmenity { Id = 4, RoomId = 1, AmenityId = 4 },
            new RoomAmenity { Id = 5, RoomId = 1, AmenityId = 5 },
            
            // Small Meeting Room A: Basic amenities
            new RoomAmenity { Id = 6, RoomId = 2, AmenityId = 2 },
            new RoomAmenity { Id = 7, RoomId = 2, AmenityId = 5 },
            
            // Small Meeting Room B: Basic amenities
            new RoomAmenity { Id = 8, RoomId = 3, AmenityId = 2 },
            new RoomAmenity { Id = 9, RoomId = 3, AmenityId = 3 },
            
            // Large Conference Room: Full setup
            new RoomAmenity { Id = 10, RoomId = 4, AmenityId = 1 },
            new RoomAmenity { Id = 11, RoomId = 4, AmenityId = 2 },
            new RoomAmenity { Id = 12, RoomId = 4, AmenityId = 3 },
            new RoomAmenity { Id = 13, RoomId = 4, AmenityId = 4 }
        );
        
        // Seed Users
        modelBuilder.Entity<User>().HasData(
            new User { Id = 1, Name = "John Doe"},
            new User { Id = 2, Name = "Jane Smith"},
            new User { Id = 3, Name = "Bob Wilson"}
        );
    }
}