namespace ConferenceBookingSystem.DTOs
{
    public class UserDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
    }
    
    public class UserBookingsDto
    {
        public UserDto User { get; set; } = new();
        public List<BookingDto> Bookings { get; set; } = new();
    }
}