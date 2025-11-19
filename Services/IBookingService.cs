using ConferenceBookingSystem.DTOs;

namespace ConferenceBookingSystem.Services
{
    public interface IBookingService
    {
        Task<(bool Success, BookingDto? Booking, ErrorResponse? Error)> CreateBookingAsync(CreateBookingRequest request);
        Task<(bool Success, BookingDto? Booking, ErrorResponse? Error)> UpdateBookingAsync(int bookingId, UpdateBookingRequest request);
        Task<(bool Success, ErrorResponse? Error)> CancelBookingAsync(int bookingId);
        Task<BookingDto?> GetBookingByIdAsync(int bookingId);
        Task<List<BookingDto>> GetUserBookingsAsync(int userId);
    }
}