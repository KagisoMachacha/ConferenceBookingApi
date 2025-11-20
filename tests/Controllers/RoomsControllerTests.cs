using ConferenceBookingSystem.Controllers;
using ConferenceBookingSystem.DTOs;
using ConferenceBookingSystem.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Xunit;

namespace ConferenceBookingSystem.Tests.Controllers
{
    public class RoomsControllerTests
    {
        private readonly Mock<IRoomService> _roomServiceMock;
        private readonly RoomsController _controller;

        public RoomsControllerTests()
        {
            _roomServiceMock = new Mock<IRoomService>();
            _controller = new RoomsController(_roomServiceMock.Object);
        }

        [Fact]
        public async Task GetRooms_ShouldReturnOk_WhenSuccess()
        {
            // Arrange
            var rooms = new List<RoomDto> { new RoomDto { Id = 1, Name = "Room 1" } };
            _roomServiceMock.Setup(s => s.GetAllRoomsAsync())
                .ReturnsAsync((true, rooms, null));

            // Act
            var result = await _controller.GetRooms();

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            Assert.Equal(rooms, okResult.Value);
        }

        [Fact]
        public async Task GetRoomAvailability_ShouldReturnOk_WhenSuccess()
        {
            // Arrange
            var availability = new AvailabilityResponse { RoomId = 1, IsAvailable = true };
            var dateStr = "2025-11-20";
            var date = DateTime.Parse(dateStr);
            
            _roomServiceMock.Setup(s => s.GetRoomAvailabilityAsync(1, date))
                .ReturnsAsync((true, availability, null));

            // Act
            var result = await _controller.GetRoomAvailability(1, dateStr);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            Assert.Equal(availability, okResult.Value);
        }

        [Fact]
        public async Task GetRoomAvailability_ShouldReturnBadRequest_WhenInvalidDate()
        {
            // Act
            var result = await _controller.GetRoomAvailability(1, "invalid-date");

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result.Result);
            var error = Assert.IsType<ErrorResponse>(badRequestResult.Value);
            Assert.Equal("InvalidDate", error.Error);
        }
    }
}
