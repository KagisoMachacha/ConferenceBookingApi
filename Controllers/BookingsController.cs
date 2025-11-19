using Microsoft.AspNetCore.Mvc;
using ConferenceBookingSystem.Services;
using ConferenceBookingSystem.DTOs;


namespace ConferenceBookingSystem.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class BookingsController : ControllerBase
    {
        private readonly IBookingService _bookingService;
        private readonly ILogger<BookingsController> _logger;
        
        public BookingsController(IBookingService bookingService, ILogger<BookingsController> logger)
        {
            _bookingService = bookingService;
            _logger = logger;
        }
        
        
        /// POST /api/bookings - Create a new booking
        [HttpPost]
        [ProducesResponseType(typeof(BookingDto), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status409Conflict)]
        public async Task<ActionResult<BookingDto>> CreateBooking([FromBody] CreateBookingRequest request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new ErrorResponse
                {
                    Error = "Validation failed",
                    ValidationErrors = ModelState
                        .Where(x => x.Value?.Errors.Count > 0)
                        .ToDictionary(
                            kvp => kvp.Key,
                            kvp => kvp.Value!.Errors.Select(e => e.ErrorMessage).ToArray()
                        )
                });
            }
            
            var (success, booking, error) = await _bookingService.CreateBookingAsync(request);
            if (!success)
            {
                var statusCode = error!.Error switch
                {
                    "RoomNotFound" => StatusCodes.Status404NotFound,
                    "UserNotFound" => StatusCodes.Status404NotFound,
                    "TimeSlotConflict" => StatusCodes.Status409Conflict,
                    "ValidationError" => StatusCodes.Status400BadRequest,
                    "OutsideBusinessHours" => StatusCodes.Status400BadRequest,
                    "PastTimeNotAllowed" => StatusCodes.Status400BadRequest,
                    _ => StatusCodes.Status400BadRequest
                };
                return StatusCode(statusCode, error);
            }
            
            // Return 201 Created with Location header
            return CreatedAtAction(
                nameof(GetBooking),
                new { id = booking!.Id },
                booking);
        }
        
        
        /// GET /api/bookings/5 - Get booking details
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(BookingDto), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
        public async Task<ActionResult<BookingDto>> GetBooking(int id)
        {
            var booking = await _bookingService.GetBookingByIdAsync(id);
            
            if (booking == null)
            {
                return NotFound(new ErrorResponse
                {
                    Error = "Booking not found",
                    Message = $"Booking with ID {id} does not exist"
                });
            }
            
            return Ok(booking);
        }
        
        
        /// PATCH /api/bookings/5 - Update a booking (reschedule)
        [HttpPatch("{id}")]
        [ProducesResponseType(typeof(BookingDto), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status409Conflict)]
        public async Task<ActionResult<BookingDto>> UpdateBooking(
            int id,
            [FromBody] UpdateBookingRequest request)
        {
            var (success, booking, error) = await _bookingService.UpdateBookingAsync(id, request);
            if (!success)
            {
                var statusCode = error!.Error switch
                {
                    "BookingNotFound" => StatusCodes.Status404NotFound,
                    "TimeSlotConflict" => StatusCodes.Status409Conflict,
                    "ValidationError" => StatusCodes.Status400BadRequest,
                    "OutsideBusinessHours" => StatusCodes.Status400BadRequest,
                    "PastTimeNotAllowed" => StatusCodes.Status400BadRequest,
                    "BookingCancelled" => StatusCodes.Status400BadRequest,
                    _ => StatusCodes.Status400BadRequest
                };
                return StatusCode(statusCode, error);
            }
            
            return Ok(booking);
        }
        
        
        /// DELETE /api/bookings/5 - Cancel a booking
        [HttpDelete("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> CancelBooking(int id)
        {
            var (success, error) = await _bookingService.CancelBookingAsync(id);
            if (!success)
            {
                var statusCode = error!.Error switch
                {
                    "BookingNotFound" => StatusCodes.Status404NotFound,
                    "AlreadyCancelled" => StatusCodes.Status400BadRequest,
                    _ => StatusCodes.Status400BadRequest
                };
                return StatusCode(statusCode, error);
            }
            
            return NoContent(); // 204 - Success, no content to return
        }
    }

}