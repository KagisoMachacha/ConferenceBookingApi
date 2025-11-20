using ConferenceBookingSystem.Controllers;
using ConferenceBookingSystem.DTOs;
using ConferenceBookingSystem.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Xunit;

namespace ConferenceBookingSystem.Tests.Controllers
{
    public class UsersControllerTests
    {
        private readonly Mock<IBookingService> _bookingServiceMock;
        private readonly UsersController _controller;

        public UsersControllerTests()
        {
            _bookingServiceMock = new Mock<IBookingService>();
            _controller = new UsersController(_bookingServiceMock.Object);
        }

        [Fact]
        public async Task GetUserBookings_ShouldReturnOk_WhenSuccess()
        {
            // Arrange
            var bookings = new List<BookingDto> { new BookingDto { Id = 1, Title = "Test" } };
            _bookingServiceMock.Setup(s => s.GetUserBookingsAsync(1))
                .ReturnsAsync(bookings);

            // Act
            var result = await _controller.GetUserBookings(1);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            Assert.Equal(bookings, okResult.Value);
        }
    }
}
