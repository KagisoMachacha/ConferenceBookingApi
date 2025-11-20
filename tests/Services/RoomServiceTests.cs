using ConferenceBookingSystem.DTOs;
using ConferenceBookingSystem.Models;
using ConferenceBookingSystem.Services;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace ConferenceBookingSystem.Tests.Services
{
    public class RoomServiceTests
    {
        private readonly DbContextOptions<BookingDbContext> _dbContextOptions;

        public RoomServiceTests()
        {
            _dbContextOptions = new DbContextOptionsBuilder<BookingDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;
        }

        private BookingDbContext CreateContext()
        {
            var context = new BookingDbContext(_dbContextOptions);
            context.Database.EnsureCreated();
            return context;
        }

        [Fact]
        public async Task GetAllRoomsAsync_ShouldReturnRooms()
        {
            // Arrange
            using var context = CreateContext();
            var service = new RoomService(context);

            context.Rooms.AddRange(
                new Room { Id = 1, Name = "Room 1", Capacity = 10, Location = "L1" },
                new Room { Id = 2, Name = "Room 2", Capacity = 20, Location = "L2" }
            );
            await context.SaveChangesAsync();

            // Act
            var result = await service.GetAllRoomsAsync();

            // Assert
            Assert.True(result.Success);
            Assert.Equal(2, result.Rooms?.Count);
        }

        [Fact]
        public async Task GetRoomAvailabilityAsync_ShouldReturnAvailability()
        {
            // Arrange
            using var context = CreateContext();
            var service = new RoomService(context);

            var room = new Room { Id = 1, Name = "Room 1", Capacity = 10, Location = "L1" };
            context.Rooms.Add(room);
            
            // Add a booking
            var date = DateTime.UtcNow.AddDays(1).Date; // Tomorrow
            // 10:00 - 11:00 SAST (UTC+2) -> 08:00 - 09:00 UTC
            var booking = new Booking
            {
                RoomId = 1,
                UserId = 1,
                Title = "Meeting",
                StartTime = date.AddHours(8),
                EndTime = date.AddHours(9),
                Status = BookingStatus.Confirmed
            };
            context.Bookings.Add(booking);
            await context.SaveChangesAsync();

            // Act
            var result = await service.GetRoomAvailabilityAsync(1, date);

            // Assert
            Assert.True(result.Success);
            Assert.NotNull(result.Availability);
            Assert.True(result.Availability.HasAnyBookings);
            Assert.Single(result.Availability.Bookings);
            
            // Check if it correctly identifies availability (should be true as there are gaps)
            Assert.True(result.Availability.IsAvailable);
        }

        [Fact]
        public async Task GetRoomAvailabilityAsync_ShouldReturnFalse_WhenRoomNotFound()
        {
            // Arrange
            using var context = CreateContext();
            var service = new RoomService(context);

            // Act
            var result = await service.GetRoomAvailabilityAsync(999, DateTime.UtcNow);

            // Assert
            Assert.False(result.Success);
            Assert.Equal("RoomNotFound", result.Error?.Error);
        }
    }
}
