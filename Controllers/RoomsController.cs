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
    public class RoomsController : ControllerBase
    {
        private readonly IRoomService _roomService;
        
        public RoomsController(IRoomService roomService)
        {
            _roomService = roomService;
        }
        
        
        /// GET /api/rooms - List all rooms with their amenities
        [HttpGet]
            /// GET /api/rooms/5/availability?date=2025-11-20 - Check availability for a room.
            /// Returns HasAnyBookings=false when the room has zero bookings that day (completely free).
        public async Task<ActionResult<List<RoomDto>>> GetRooms()
        {
            var (success, rooms, error) = await _roomService.GetAllRoomsAsync();
            if (!success)
            {
                var status = error!.Error == "RoomsFetchFailed" ? StatusCodes.Status500InternalServerError : StatusCodes.Status400BadRequest;
                return StatusCode(status, error);
            }
            return Ok(rooms);
        }
        
        
        /// GET /api/rooms/5/availability?date=2025-11-20 - Check availability for a room
        [HttpGet("{id}/availability")]
        [ProducesResponseType(typeof(AvailabilityResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<AvailabilityResponse>> GetRoomAvailability(int id, [FromQuery] string date)
        {
            if (!DateTime.TryParse(date, out var parsedDate))
                return BadRequest(new ErrorResponse { Error = "InvalidDate", Message = "Date must be YYYY-MM-DD" });

            var (success, availability, error) = await _roomService.GetRoomAvailabilityAsync(id, parsedDate);
            if (!success)
            {
                var status = error!.Error switch
                {
                    "RoomNotFound" => StatusCodes.Status404NotFound,
                    "RoomAvailabilityFailed" => StatusCodes.Status500InternalServerError,
                    _ => StatusCodes.Status400BadRequest
                };
                return StatusCode(status, error);
            }
            return Ok(availability);
        }
    }
}