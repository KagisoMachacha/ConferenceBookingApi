using ConferenceBookingSystem.DTOs;
using ConferenceBookingSystem.Models;
using ConferenceBookingSystem.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace ConferenceBookingSystem.Tests.Services
{
    public class BookingServiceTests
    {
        private readonly DbContextOptions<BookingDbContext> _dbContextOptions;
        private readonly Mock<ILogger<BookingService>> _loggerMock;

        public BookingServiceTests()
        {
            _dbContextOptions = new DbContextOptionsBuilder<BookingDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString()) // Unique DB for each test
                .Options;
            
            _loggerMock = new Mock<ILogger<BookingService>>();
        }

        private BookingDbContext CreateContext()
        {
            var context = new BookingDbContext(_dbContextOptions);
            context.Database.EnsureCreated();
            return context;
        }

        [Fact]
        public async Task CreateBookingAsync_ShouldCreateBooking_WhenValidRequest()
        {
            // Arrange
            using var context = CreateContext();
            var service = new BookingService(context, _loggerMock.Object);

            // Add required entities
            var room = new Room { Id = 1, Name = "Test Room", Capacity = 10, Location = "Test" };
            var user = new User { Id = 1, Name = "Test User" };
            context.Rooms.Add(room);
            context.Users.Add(user);
            await context.SaveChangesAsync();

            // Time needs to be in future and within business hours (9-17 SAST)
            // SAST is UTC+2. So 9:00 SAST is 7:00 UTC.
            // Let's pick a date in the future.
            var tomorrow = DateTime.UtcNow.AddDays(1).Date; 
            var startTime = tomorrow.AddHours(8); // 8:00 UTC = 10:00 SAST
            var endTime = tomorrow.AddHours(9);   // 9:00 UTC = 11:00 SAST

            var request = new CreateBookingRequest
            {
                RoomId = 1,
                UserId = 1,
                Title = "Test Meeting",
                StartTime = startTime,
                EndTime = endTime
            };

            // Act
            var result = await service.CreateBookingAsync(request);

            // Assert
            Assert.True(result.Success);
            Assert.NotNull(result.Booking);
            Assert.Null(result.Error);
            Assert.Equal("Test Meeting", result.Booking.Title);
            
            var bookingInDb = await context.Bookings.FirstOrDefaultAsync();
            Assert.NotNull(bookingInDb);
            Assert.Equal(BookingStatus.Confirmed, bookingInDb.Status);
        }

        [Fact]
        public async Task CreateBookingAsync_ShouldFail_WhenRoomNotFound()
        {
            // Arrange
            using var context = CreateContext();
            var service = new BookingService(context, _loggerMock.Object);
            
            // Add user but no room
            context.Users.Add(new User { Id = 1, Name = "Test User" });
            await context.SaveChangesAsync();

            var tomorrow = DateTime.UtcNow.AddDays(1).Date;
            var request = new CreateBookingRequest
            {
                RoomId = 999, // Non-existent
                UserId = 1,
                Title = "Test Meeting",
                StartTime = tomorrow.AddHours(8),
                EndTime = tomorrow.AddHours(9)
            };

            // Act
            var result = await service.CreateBookingAsync(request);

            // Assert
            Assert.False(result.Success);
            Assert.Equal("RoomNotFound", result.Error?.Error);
        }

        [Fact]
        public async Task CreateBookingAsync_ShouldFail_WhenTimeSlotConflict()
        {
            // Arrange
            using var context = CreateContext();
            var service = new BookingService(context, _loggerMock.Object);

            var room = new Room { Id = 1, Name = "Test Room", Capacity = 10, Location = "Test" };
            var user = new User { Id = 1, Name = "Test User" };
            context.Rooms.Add(room);
            context.Users.Add(user);
            
            var tomorrow = DateTime.UtcNow.AddDays(1).Date;
            var existingBooking = new Booking
            {
                RoomId = 1,
                UserId = 1,
                Title = "Existing Meeting",
                StartTime = tomorrow.AddHours(8), // 10:00 SAST
                EndTime = tomorrow.AddHours(9),   // 11:00 SAST
                Status = BookingStatus.Confirmed
            };
            context.Bookings.Add(existingBooking);
            await context.SaveChangesAsync();

            var request = new CreateBookingRequest
            {
                RoomId = 1,
                UserId = 1,
                Title = "New Meeting",
                StartTime = tomorrow.AddHours(8).AddMinutes(30), // Overlap
                EndTime = tomorrow.AddHours(9).AddMinutes(30)
            };

            // Act
            var result = await service.CreateBookingAsync(request);

            // Assert
            Assert.False(result.Success);
            Assert.Equal("TimeSlotConflict", result.Error?.Error);
        }

        [Fact]
        public async Task UpdateBookingAsync_ShouldUpdate_WhenValid()
        {
            // Arrange
            using var context = CreateContext();
            var service = new BookingService(context, _loggerMock.Object);

            var room = new Room { Id = 1, Name = "Test Room", Capacity = 10, Location = "Test" };
            var user = new User { Id = 1, Name = "Test User" };
            context.Rooms.Add(room);
            context.Users.Add(user);

            var tomorrow = DateTime.UtcNow.AddDays(1).Date;
            var booking = new Booking
            {
                RoomId = 1,
                UserId = 1,
                Title = "Original Title",
                StartTime = tomorrow.AddHours(8),
                EndTime = tomorrow.AddHours(9),
                Status = BookingStatus.Confirmed
            };
            context.Bookings.Add(booking);
            await context.SaveChangesAsync();

            var request = new UpdateBookingRequest
            {
                Title = "Updated Title"
            };

            // Act
            var result = await service.UpdateBookingAsync(booking.Id, request);

            // Assert
            Assert.True(result.Success);
            Assert.Equal("Updated Title", result.Booking?.Title);
            
            var updatedBooking = await context.Bookings.FindAsync(booking.Id);
            Assert.Equal("Updated Title", updatedBooking.Title);
            Assert.Equal("BookingUpdated", updatedBooking.Status);
        }

        [Fact]
        public async Task CancelBookingAsync_ShouldCancel_WhenExists()
        {
            // Arrange
            using var context = CreateContext();
            var service = new BookingService(context, _loggerMock.Object);

            var booking = new Booking
            {
                RoomId = 1,
                UserId = 1,
                Title = "To Cancel",
                StartTime = DateTime.UtcNow.AddDays(1),
                EndTime = DateTime.UtcNow.AddDays(1).AddHours(1),
                Status = BookingStatus.Confirmed
            };
            context.Bookings.Add(booking);
            await context.SaveChangesAsync();

            // Act
            var result = await service.CancelBookingAsync(booking.Id);

            // Assert
            Assert.True(result.Success);
            
            var cancelledBooking = await context.Bookings.FindAsync(booking.Id);
            Assert.Equal(BookingStatus.Cancelled, cancelledBooking.Status);
        }

        [Fact]
        public async Task GetUserBookingsAsync_ShouldReturnBookings()
        {
            // Arrange
            using var context = CreateContext();
            var service = new BookingService(context, _loggerMock.Object);

            var user = new User { Id = 1, Name = "Test User" };
            var room = new Room { Id = 1, Name = "Test Room", Capacity = 10, Location = "Test" };
            context.Users.Add(user);
            context.Rooms.Add(room);

            context.Bookings.Add(new Booking
            {
                UserId = 1,
                RoomId = 1,
                Title = "Booking 1",
                StartTime = DateTime.UtcNow.AddDays(1),
                EndTime = DateTime.UtcNow.AddDays(1).AddHours(1),
                Status = BookingStatus.Confirmed
            });
            context.Bookings.Add(new Booking
            {
                UserId = 1,
                RoomId = 1,
                Title = "Booking 2",
                StartTime = DateTime.UtcNow.AddDays(2),
                EndTime = DateTime.UtcNow.AddDays(2).AddHours(1),
                Status = BookingStatus.Confirmed
            });
            await context.SaveChangesAsync();

            // Act
            var result = await service.GetUserBookingsAsync(1);

            // Assert
            Assert.Equal(2, result.Count);
        }
    }
}
