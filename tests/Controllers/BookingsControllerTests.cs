using ConferenceBookingSystem.Controllers;
using ConferenceBookingSystem.DTOs;
using ConferenceBookingSystem.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace ConferenceBookingSystem.Tests.Controllers
{
    public class BookingsControllerTests
    {
        private readonly Mock<IBookingService> _bookingServiceMock;
        private readonly Mock<ILogger<BookingsController>> _loggerMock;
        private readonly BookingsController _controller;

        public BookingsControllerTests()
        {
            _bookingServiceMock = new Mock<IBookingService>();
            _loggerMock = new Mock<ILogger<BookingsController>>();
            _controller = new BookingsController(_bookingServiceMock.Object, _loggerMock.Object);
        }

        [Fact]
        public async Task CreateBooking_ShouldReturnCreated_WhenSuccess()
        {
            // Arrange
            var request = new CreateBookingRequest { Title = "Test" };
            var bookingDto = new BookingDto { Id = 1, Title = "Test" };
            
            _bookingServiceMock.Setup(s => s.CreateBookingAsync(request))
                .ReturnsAsync((true, bookingDto, null));

            // Act
            var result = await _controller.CreateBooking(request);

            // Assert
            var createdAtActionResult = Assert.IsType<CreatedAtActionResult>(result.Result);
            Assert.Equal(StatusCodes.Status201Created, createdAtActionResult.StatusCode);
            Assert.Equal(bookingDto, createdAtActionResult.Value);
        }

        [Fact]
        public async Task CreateBooking_ShouldReturnNotFound_WhenRoomNotFound()
        {
            // Arrange
            var request = new CreateBookingRequest { Title = "Test" };
            var error = new ErrorResponse { Error = "RoomNotFound" };
            
            _bookingServiceMock.Setup(s => s.CreateBookingAsync(request))
                .ReturnsAsync((false, null, error));

            // Act
            var result = await _controller.CreateBooking(request);

            // Assert
            var objectResult = Assert.IsType<ObjectResult>(result.Result);
            Assert.Equal(StatusCodes.Status404NotFound, objectResult.StatusCode);
            Assert.Equal(error, objectResult.Value);
        }

        [Fact]
        public async Task GetBooking_ShouldReturnOk_WhenFound()
        {
            // Arrange
            var bookingDto = new BookingDto { Id = 1, Title = "Test" };
            _bookingServiceMock.Setup(s => s.GetBookingByIdAsync(1))
                .ReturnsAsync(bookingDto);

            // Act
            var result = await _controller.GetBooking(1);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            Assert.Equal(bookingDto, okResult.Value);
        }

        [Fact]
        public async Task GetBooking_ShouldReturnNotFound_WhenNotFound()
        {
            // Arrange
            _bookingServiceMock.Setup(s => s.GetBookingByIdAsync(1))
                .ReturnsAsync((BookingDto?)null);

            // Act
            var result = await _controller.GetBooking(1);

            // Assert
            var notFoundResult = Assert.IsType<NotFoundObjectResult>(result.Result);
            Assert.Equal(StatusCodes.Status404NotFound, notFoundResult.StatusCode);
        }

        [Fact]
        public async Task UpdateBooking_ShouldReturnOk_WhenSuccess()
        {
            // Arrange
            var request = new UpdateBookingRequest { Title = "Updated" };
            var bookingDto = new BookingDto { Id = 1, Title = "Updated" };
            
            _bookingServiceMock.Setup(s => s.UpdateBookingAsync(1, request))
                .ReturnsAsync((true, bookingDto, null));

            // Act
            var result = await _controller.UpdateBooking(1, request);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            Assert.Equal(bookingDto, okResult.Value);
        }

        [Fact]
        public async Task CancelBooking_ShouldReturnNoContent_WhenSuccess()
        {
            // Arrange
            _bookingServiceMock.Setup(s => s.CancelBookingAsync(1))
                .ReturnsAsync((true, null));

            // Act
            var result = await _controller.CancelBooking(1);

            // Assert
            Assert.IsType<NoContentResult>(result);
        }
    }
}
