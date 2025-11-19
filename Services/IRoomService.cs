using ConferenceBookingSystem.DTOs;

namespace ConferenceBookingSystem.Services
{
    public interface IRoomService
    {
        Task<(bool Success, List<RoomDto>? Rooms, ErrorResponse? Error)> GetAllRoomsAsync();
        Task<(bool Success, AvailabilityResponse? Availability, ErrorResponse? Error)> GetRoomAvailabilityAsync(int roomId, DateTime date);
    }
}
