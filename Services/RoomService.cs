using Microsoft.EntityFrameworkCore;
using ConferenceBookingSystem.DTOs;
using ConferenceBookingSystem.Models;

namespace ConferenceBookingSystem.Services
{
    public class RoomService : IRoomService
    {
        private readonly BookingDbContext _context;
        private static readonly TimeZoneInfo SastTimeZone = TimeZoneInfo.FindSystemTimeZoneById("South Africa Standard Time");
        
        public RoomService(BookingDbContext context) => _context = context;

        private ErrorResponse BuildError(string code, string message) => new() { Error = code, Message = message };

        public async Task<(bool Success, List<RoomDto>? Rooms, ErrorResponse? Error)> GetAllRoomsAsync()
        {
            try
            {
                var rooms = await _context.Rooms
                    .Include(r => r.RoomAmenities)
                        .ThenInclude(ra => ra.Amenity)
                    .ToListAsync();
                var result = rooms.Select(room => new RoomDto
                {
                    Id = room.Id,
                    Name = room.Name,
                    Capacity = room.Capacity,
                    Location = room.Location,
                    Amenities = room.RoomAmenities
                        .Select(ra => ra.Amenity?.Name ?? "")
                        .Where(name => !string.IsNullOrEmpty(name))
                        .ToList()
                }).ToList();
                return (true, result, null);
            }
            catch (Exception ex)
            {
                return (false, null, BuildError("RoomsFetchFailed", ex.Message));
            }
        }

        public async Task<(bool Success, AvailabilityResponse? Availability, ErrorResponse? Error)> GetRoomAvailabilityAsync(int roomId, DateTime date)
        {
            try
            {
                var room = await _context.Rooms.FindAsync(roomId);
                if (room == null)
                    return (false, null, BuildError("RoomNotFound", $"Room with ID {roomId} does not exist"));
                // Normalize date to UTC midnight
                var dayUtc = DateTime.SpecifyKind(date.Date, DateTimeKind.Utc);
                var startOfDay = new DateTimeOffset(dayUtc);
                var endOfDay = startOfDay.AddDays(1);

                var bookingsUtc = await _context.Bookings
                    .Where(b => b.RoomId == roomId
                                && b.Status == BookingStatus.Confirmed
                                && b.StartTime >= startOfDay
                                && b.StartTime < endOfDay)
                    .OrderBy(b => b.StartTime)
                    .ToListAsync();

                // Convert booking times to SAST for display
                var bookings = bookingsUtc.Select(b => new BookingSlotDto
                {
                    StartTime = TimeZoneInfo.ConvertTime(b.StartTime, SastTimeZone),
                    EndTime = TimeZoneInfo.ConvertTime(b.EndTime, SastTimeZone),
                    Title = b.Title ?? ""
                }).ToList();

                // Business hours in SAST (09:00-17:00 SAST)
                var businessStartSast = TimeZoneInfo.ConvertTime(startOfDay, SastTimeZone).AddHours(9);
                var businessEndSast = TimeZoneInfo.ConvertTime(startOfDay, SastTimeZone).AddHours(17);
                bool hasAvailability = bookings.Count == 0 || HasGaps(bookings, businessStartSast, businessEndSast);

                var availability = new AvailabilityResponse
                {
                    RoomId = roomId,
                    RoomName = room.Name,
                    Date = TimeZoneInfo.ConvertTime(startOfDay, SastTimeZone),
                    Bookings = bookings,
                    IsAvailable = hasAvailability,
                    HasAnyBookings = bookings.Count > 0
                };
                return (true, availability, null);
            }
            catch (Exception ex)
            {
                return (false, null, BuildError("RoomAvailabilityFailed", ex.Message));
            }
        }
        
        private bool HasGaps(List<BookingSlotDto> bookings, DateTimeOffset  businessStart, DateTimeOffset  businessEnd)
        {
            if (bookings.First().StartTime > businessStart)
                return true;
            
            for (int i = 0; i < bookings.Count - 1; i++)
            {
                if (bookings[i].EndTime < bookings[i + 1].StartTime)
                    return true;
            }
            
            if (bookings.Last().EndTime < businessEnd)
                return true;
            
            return false;
        }
    }
}