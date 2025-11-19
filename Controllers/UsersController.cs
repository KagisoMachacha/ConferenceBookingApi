using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using ConferenceBookingSystem.Services;
using ConferenceBookingSystem.DTOs;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ConferenceBookingSystem.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UsersController : ControllerBase
    {
        private readonly IBookingService _bookingService;
        
        public UsersController(IBookingService bookingService)
        {
            _bookingService = bookingService;
        }
        
        
        /// GET /api/users/5/bookings - Get all bookings for a user
        [HttpGet("{id}/bookings")]
        [ProducesResponseType(typeof(List<BookingDto>), StatusCodes.Status200OK)]
        public async Task<ActionResult<List<BookingDto>>> GetUserBookings(int id)
        {
            var bookings = await _bookingService.GetUserBookingsAsync(id);
            return Ok(bookings);
        }
    }
}