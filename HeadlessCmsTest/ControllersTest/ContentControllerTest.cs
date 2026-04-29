using FluentAssertions;
using HeadlessCms.Controllers;
using HeadlessCms.Dtos.Content;
using HeadlessCms.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using Xunit;

namespace HeadlessCmsTest.ControllersTest
{
    public class ContentControllerTest
    {
        private readonly Mock<IContentValueService> _cvMockService;
        private const string TestUserId = "test-user-id";

        public ContentControllerTest()
        {
            _cvMockService = new Mock<IContentValueService>();
        }

        private ContentController CreateControllerWithUser(string userId = TestUserId)
        {
            var controller = new ContentController(_cvMockService.Object);
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, userId)
            };
            var identity = new ClaimsIdentity(claims, "TestAuth");
            var principal = new ClaimsPrincipal(identity);

            var httpContext = new DefaultHttpContext { User = principal };
            controller.ControllerContext = new ControllerContext
            {
                HttpContext = httpContext
            };
            return controller;
        }

        private ContentController CreateController()
        {
            return new ContentController(_cvMockService.Object);
        }

        [Theory]
        [InlineData(1)]
        public async Task ContentController_GetAllContentValues_ReturnsOk(int entryId)
        {
            // Arrange
            var controller = CreateController();
            var expectedValues = new List<ContentValueResponseDto>
            {
                new ContentValueResponseDto { Id = 1, Value = "Test", ContentEntryId = entryId }
            };

            _cvMockService.Setup(s => s.GetContentValues(entryId))
                .ReturnsAsync(expectedValues);

            // Act
            var result = await controller.GetAllContentValues(entryId);
            var okResult = result as OkObjectResult;
            var returnedValues = okResult.Value as List<ContentValueResponseDto>;

            // Assert
            okResult.Should().NotBeNull();
            okResult.StatusCode.Should().Be(200);
            returnedValues.Should().NotBeNull();
            returnedValues.Should().HaveCount(1);
            returnedValues[0].Id.Should().Be(1);
        }

        [Fact]
        public async Task ContentController_GetAllContentValues_ReturnsNotFound_WhenIdTooLarge()
        {
            // Arrange
            var controller = CreateController();

            // Act
            var result = await controller.GetAllContentValues(long.MaxValue);
            var notFoundResult = result as NotFoundObjectResult;

            // Assert
            notFoundResult.Should().NotBeNull();
            notFoundResult.StatusCode.Should().Be(404);
        }

        [Theory]
        [InlineData(1)]
        public async Task ContentController_GetContentValueById_ReturnsOk(long id)
        {
            // Arrange
            var controller = CreateController();
            var expectedValue = new ContentValueResponseDto { Id = (int)id, Value = "Test" };

            _cvMockService.Setup(s => s.GetExistingContentValue((int)id))
                .ReturnsAsync(expectedValue);

            // Act
            var result = await controller.GetContentValueById(id);
            var okResult = result as OkObjectResult;
            var returnedValue = okResult?.Value as ContentValueResponseDto;

            // Assert
            okResult.Should().NotBeNull();
            okResult.StatusCode.Should().Be(200);
            returnedValue.Should().NotBeNull();
            returnedValue.Id.Should().Be((int)id);
        }

        [Fact]
        public async Task ContentController_CreateContentValue_ReturnsCreatedAtAction()
        {
            // Arrange
            var controller = CreateControllerWithUser();
            var createDto = new ContentValueRequestDto();
            var createdValues = new List<ContentValueResponseDto>
            {
                new ContentValueResponseDto { Id = 1, ContentEntryId = 10, Value = "NewValue" }
            };

            _cvMockService.Setup(s => s.CreateNewContentValue(1, createDto, TestUserId))
                .ReturnsAsync(createdValues);

            // Act
            var result = await controller.CreateContentValue(1, createDto);
            var createdAtActionResult = result as CreatedAtActionResult;
            var returnedValues = createdAtActionResult?.Value as List<ContentValueResponseDto>;

            // Assert
            createdAtActionResult.Should().NotBeNull();
            createdAtActionResult.StatusCode.Should().Be(201);
            createdAtActionResult.ActionName.Should().Be(nameof(ContentController.GetContentValueById));
            returnedValues.Should().NotBeNull();
            returnedValues.Should().HaveCount(1);
        }

        [Fact]
        public async Task ContentController_UpdateContentValue_ReturnsOk()
        {
            // Arrange
            var controller = CreateControllerWithUser();
            var updateDto = new ContentValueRequestDto();
            var updatedValues = new List<ContentValueResponseDto>
            {
                new ContentValueResponseDto { Id = 1, ContentEntryId = 10, Value = "UpdatedValue" }
            };

            _cvMockService.Setup(s => s.UpdateExistingContentValue(1, updateDto, TestUserId))
                .ReturnsAsync(updatedValues);

            // Act
            var result = await controller.UpdateContentValue(1, updateDto);
            var okResult = result as OkObjectResult;
            var returnedValues = okResult?.Value as List<ContentValueResponseDto>;

            // Assert
            okResult.Should().NotBeNull();
            okResult.StatusCode.Should().Be(200);
            returnedValues.Should().NotBeNull();
            returnedValues.Should().HaveCount(1);
        }

        [Theory]
        [InlineData(10)]
        public async Task ContentController_DeleteContentValue_ReturnsNoContent(long entryId)
        {
            // Arrange
            var controller = CreateControllerWithUser();
            _cvMockService.Setup(s => s.DeleteExistingContentValue((int)entryId, TestUserId))
                .Returns(Task.CompletedTask);

            // Act
            var result = await controller.DeleteContentValue(entryId);
            var noContentResult = result as NoContentResult;

            // Assert
            noContentResult.Should().NotBeNull();
            noContentResult.StatusCode.Should().Be(204);
        }

        [Fact]
        public async Task ContentController_DeleteContentValue_ReturnsNoContent_WhenIdTooLarge()
        {
            // Arrange
            var controller = CreateControllerWithUser();

            // Act
            var result = await controller.DeleteContentValue(long.MaxValue);
            var noContentResult = result as NoContentResult;

            // Assert
            noContentResult.Should().NotBeNull();
            noContentResult.StatusCode.Should().Be(204);
        }
    }
}
