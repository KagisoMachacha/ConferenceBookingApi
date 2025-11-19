using Microsoft.EntityFrameworkCore;
using ConferenceBookingSystem.DTOs;
using ConferenceBookingSystem.Models;

namespace ConferenceBookingSystem.Services
{
    public class BookingService : IBookingService
    {
        private readonly BookingDbContext _context;
        private readonly ILogger<BookingService> _logger;
        private static readonly TimeZoneInfo SastTimeZone = TimeZoneInfo.FindSystemTimeZoneById("South Africa Standard Time");
        
        private const int MinBookingDurationMinutes = 30;
        private const int MaxBookingDurationHours = 4;
        private const int BusinessHourStart = 9;
        private const int BusinessHourEnd = 17;
        
        public BookingService(BookingDbContext context, ILogger<BookingService> logger)
        {
            _context = context;
            _logger = logger;
        }

        private ErrorResponse BuildError(string error, string message, Dictionary<string, string[]>? validationErrors = null)
        {
            return new ErrorResponse
            {
                Error = error,
                Message = message,
                ValidationErrors = validationErrors
            };
        }
        
        public async Task<(bool Success, BookingDto? Booking, ErrorResponse? Error)> CreateBookingAsync(CreateBookingRequest request)
        {
            // ALWAYS interpret incoming times as SAST (local business timezone), then convert to UTC for storage
            // Strip any timezone info and treat as SAST
            var startSast = DateTime.SpecifyKind(request.StartTime, DateTimeKind.Unspecified);
            var endSast = DateTime.SpecifyKind(request.EndTime, DateTimeKind.Unspecified);
            
            var startUtc = TimeZoneInfo.ConvertTimeToUtc(startSast, SastTimeZone);
            var endUtc = TimeZoneInfo.ConvertTimeToUtc(endSast, SastTimeZone);
            
            var validationError = ValidateBookingTimes(startUtc, endUtc);
            if (validationError != null)
                return (false, null, validationError);
            
            var roomExists = await _context.Rooms.AnyAsync(r => r.Id == request.RoomId);
            if (!roomExists)
                return (false, null, BuildError("RoomNotFound", "Room with the provided ID does not exist."));
            
            var userExists = await _context.Users.AnyAsync(u => u.Id == request.UserId);
            if (!userExists)
                return (false, null, BuildError("UserNotFound", "User with the provided ID does not exist."));
            
            using var transaction = await _context.Database.BeginTransactionAsync();
            
            try
            {
                var hasConflict = await _context.Bookings
                    .Where(b => b.RoomId == request.RoomId 
                                && b.Status == BookingStatus.Confirmed
                                && b.StartTime < endUtc 
                                && b.EndTime > startUtc)
                    .AnyAsync();
                
                if (hasConflict)
                    return (false, null, BuildError("TimeSlotConflict", "This room is already booked for the requested time slot"));
                
                var booking = new Booking
                {
                    RoomId = request.RoomId,
                    UserId = request.UserId,
                    StartTime = startUtc,
                    EndTime = endUtc,
                    Title = request.Title,
                    Status = BookingStatus.Confirmed,
                    CreatedAt = DateTimeOffset.UtcNow,
                    UpdatedAt = DateTimeOffset.UtcNow
                };
                
                _context.Bookings.Add(booking);
                await _context.SaveChangesAsync();
                await transaction.CommitAsync();
                
                _logger.LogInformation("Booking {BookingId} created for Room {RoomId} by User {UserId}", 
                    booking.Id, request.RoomId, request.UserId);
                
                var createdBooking = await GetBookingByIdAsync(booking.Id);
                return (true, createdBooking, null);
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError(ex, "Error creating booking");
                return (false, null, BuildError("CreateFailed", "An error occurred while creating the booking"));
            }
        }
        
        public async Task<(bool Success, BookingDto? Booking, ErrorResponse? Error)> UpdateBookingAsync(
            int bookingId, UpdateBookingRequest request)
        {
            var booking = await _context.Bookings
                .Include(b => b.Room)
                .Include(b => b.User)
                .FirstOrDefaultAsync(b => b.Id == bookingId);

            if (booking == null)
                return (false, null, BuildError("BookingNotFound", "The requested booking does not exist."));

            if (booking.Status == BookingStatus.Cancelled)
                return (false, null, BuildError("BookingCancelled", "Cannot update a cancelled booking"));

            // If times are provided, treat them as SAST and convert to UTC
            DateTimeOffset newStartTimeOffset;
            DateTimeOffset newEndTimeOffset;
            
            if (request.StartTime.HasValue)
            {
                var startSast = DateTime.SpecifyKind(request.StartTime.Value, DateTimeKind.Unspecified);
                newStartTimeOffset = new DateTimeOffset(TimeZoneInfo.ConvertTimeToUtc(startSast, SastTimeZone));
            }
            else
            {
                newStartTimeOffset = booking.StartTime;
            }
            
            if (request.EndTime.HasValue)
            {
                var endSast = DateTime.SpecifyKind(request.EndTime.Value, DateTimeKind.Unspecified);
                newEndTimeOffset = new DateTimeOffset(TimeZoneInfo.ConvertTimeToUtc(endSast, SastTimeZone));
            }
            else
            {
                newEndTimeOffset = booking.EndTime;
            }
            
            var newStartTime = newStartTimeOffset.UtcDateTime;
            var newEndTime = newEndTimeOffset.UtcDateTime;
            var timeChanged = newStartTimeOffset != booking.StartTime || newEndTimeOffset != booking.EndTime;

            if (request.StartTime.HasValue || request.EndTime.HasValue)
            {
                var validationError = ValidateBookingTimes(newStartTime, newEndTime);
                if (validationError != null)
                    return (false, null, validationError);

                var hasConflict = await _context.Bookings
                    .Where(b => b.RoomId == booking.RoomId
                                && b.Id != bookingId
                                && b.Status == BookingStatus.Confirmed
                                && b.StartTime < newEndTime
                                && b.EndTime > newStartTime)
                    .AnyAsync();

                if (hasConflict)
                    return (false, null, BuildError("TimeSlotConflict", "The new time slot conflicts with another booking"));

                booking.StartTime = newStartTime;
                booking.EndTime = newEndTime;
            }

            if (!string.IsNullOrWhiteSpace(request.Title))
            {
                booking.Title = request.Title;
            }

            booking.Status = timeChanged ? "Rescheduled" : "BookingUpdated";
            booking.UpdatedAt = DateTimeOffset.UtcNow;
            await _context.SaveChangesAsync();

            var updatedBooking = await GetBookingByIdAsync(bookingId);
            return (true, updatedBooking, null);
        }
        
        public async Task<(bool Success, ErrorResponse? Error)> CancelBookingAsync(int bookingId)
        {
            var booking = await _context.Bookings.FindAsync(bookingId);
            if (booking == null)
                return (false, BuildError("BookingNotFound", "Booking not found"));
            if (booking.Status == BookingStatus.Cancelled)
                return (false, BuildError("AlreadyCancelled", "Booking is already cancelled"));
            
            booking.Status = BookingStatus.Cancelled;
            booking.UpdatedAt = DateTimeOffset.UtcNow;
            await _context.SaveChangesAsync();
            
            _logger.LogInformation("Booking {BookingId} cancelled", bookingId);
            return (true, null);
        }
        
        public async Task<BookingDto?> GetBookingByIdAsync(int bookingId)
        {
            var booking = await _context.Bookings
                .Include(b => b.Room)
                .Include(b => b.User)
                .FirstOrDefaultAsync(b => b.Id == bookingId);
            
            if (booking == null) return null;
            
            return MapToDto(booking);
        }
        
        public async Task<List<BookingDto>> GetUserBookingsAsync(int userId)
        {
            var bookings = await _context.Bookings
                .Include(b => b.Room)
                .Include(b => b.User)
                .Where(b => b.UserId == userId)
                .OrderByDescending(b => b.StartTime)
                .ToListAsync();
            
            return bookings.Select(MapToDto).ToList();
        }
        
        private ErrorResponse? ValidateBookingTimes(DateTime startTime, DateTime endTime)
        {
            if (endTime <= startTime)
                return BuildError("ValidationError", "End time must be after start time");

            var duration = endTime - startTime;
            if (duration.TotalMinutes < MinBookingDurationMinutes)
                return BuildError("ValidationError", $"Booking must be at least {MinBookingDurationMinutes} minutes");

            if (duration.TotalHours > MaxBookingDurationHours)
                return BuildError("ValidationError", $"Booking cannot exceed {MaxBookingDurationHours} hours");

            // Convert UTC times to SAST for business hours validation
            var startSast = TimeZoneInfo.ConvertTimeFromUtc(startTime, SastTimeZone);
            var endSast = TimeZoneInfo.ConvertTimeFromUtc(endTime, SastTimeZone);
            
            if (startSast.Hour < BusinessHourStart || endSast.Hour > BusinessHourEnd)
                return BuildError("OutsideBusinessHours", $"Bookings must be between {BusinessHourStart}:00 and {BusinessHourEnd}:00 SAST");

            if (startTime < DateTime.UtcNow)
                return BuildError("PastTimeNotAllowed", "Cannot book times in the past");

            return null;
        }

        private BookingDto MapToDto(Booking booking)
        {
            // Convert UTC times to SAST for display
            var startSast = TimeZoneInfo.ConvertTime(booking.StartTime, SastTimeZone);
            var endSast = TimeZoneInfo.ConvertTime(booking.EndTime, SastTimeZone);
            
            return new BookingDto
            {
                Id = booking.Id,
                RoomId = booking.RoomId,
                RoomName = booking.Room?.Name ?? "Unknown",
                UserId = booking.UserId,
                UserName = booking.User?.Name ?? "Unknown",
                Title = booking.Title ?? string.Empty,
                StartTime = startSast,
                EndTime = endSast,
                Status = booking.Status,
                CreatedAt = TimeZoneInfo.ConvertTime(booking.CreatedAt, SastTimeZone)
            };
        }
    }
}