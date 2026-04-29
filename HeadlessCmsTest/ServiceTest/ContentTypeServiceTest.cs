using HeadlessCms.Interfaces;
using HeadlessCms.Services;
using HeadlessCms.Exceptions;
using HeadlessCms.Dtos.Content;
using Moq;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using FluentAssertions;

namespace HeadlessCmsTest.ServiceTest
{
    public class ContentTypeServiceTest
    {
        private readonly Mock<IContentTypeRepository> _mockRepo;

        public ContentTypeServiceTest()
        {
            _mockRepo = new Mock<IContentTypeRepository>();
        }

        [Fact]
        public async Task ContentTypeService_GetContentTypes_ReturnListOfContentTypeDto()
        {
            //Arrange
            var expectedContentTypes = new List<ContentTypeDto>
            {
                new ContentTypeDto { Id = 1, Name = "TestContentType1" },
                new ContentTypeDto { Id = 2, Name = "TestContentType2" }
            };

            _mockRepo.Setup(ct => ct.GetAllContentType())
                .ReturnsAsync(expectedContentTypes);

            var service = new ContentTypeService(_mockRepo.Object);

            //Act
            var result = await service.GetContentTypes();

            //Assert
            result.Should().NotBeNull();
            result.Should().BeOfType<List<ContentTypeDto>>();
            result.Should().HaveCount(expectedContentTypes.Count);
            result.Should().BeEquivalentTo(expectedContentTypes);
        }

        [Fact]
        public async Task ContentTypeService_GetContentTypes_ThrowNotFoundException()
        {
            //Arrange
            _mockRepo.Setup(ct => ct.GetAllContentType())
                .ThrowsAsync(new NotFoundException("No content types found"));

            var services = new ContentTypeService(_mockRepo.Object);

            //Act
            Func<Task> result = () => services.GetContentTypes();

            //Assert
            await result.Should().ThrowAsync<NotFoundException>()
                .WithMessage("No content types found");
        }

        [Theory]
        [InlineData(1)]
        [InlineData(2)]
        public async Task ContentTypeService_GetUniqueContentType_ReturnContentTypeDto(int id)
        {
            //Arrange
            var expectedContentType = new ContentTypeDto { Id = id, Name = $"TestContentType{id}" };

            _mockRepo.Setup(ct => ct.GetContentTypeById(id))
                .ReturnsAsync(expectedContentType);

            var service = new ContentTypeService(_mockRepo.Object);

            //Act
            var result = await service.GetUniqueContentType(id);

            //Assert
            result.Should().NotBeNull();
            result.Should().BeOfType<ContentTypeDto>();
            result.Id.Should().Be(expectedContentType.Id);
            result.Name.Should().Be(expectedContentType.Name);
        }

        [Theory]
        [InlineData(1)]
        [InlineData(2)]
        public async Task ContentTypeService_GetUniqueContentType_ThrowNotFoundException(int id)
        {
            //Arrange
            _mockRepo.Setup(ct => ct.GetContentTypeById(id))
                .ThrowsAsync(new NotFoundException($"Content type with id {id} not found."));

            var services = new ContentTypeService(_mockRepo.Object);

            //Act
            Func<Task> result = () => services.GetUniqueContentType(id);

            //Assert
            await result.Should().ThrowAsync<NotFoundException>()
                .WithMessage($"Content type with id {id} not found.");
        }

        [Fact]
        public async Task ContentTypeService_CreateNewContentType_ReturnContentTypeDto()
        {
            //Arrange
            var createContentTypeDto = new CreateContentTypeRequestDto { Name = "NewContentType" };
            var expectedContentType = new ContentTypeDto { Id = 1, Name = "NewContentType" };

            _mockRepo.Setup(ct => ct.CreateContentType(createContentTypeDto))
                .ReturnsAsync(expectedContentType);

            var service = new ContentTypeService(_mockRepo.Object);

            //Act
            var result = await service.CreateNewContentType(createContentTypeDto);

            //Assert
            result.Should().NotBeNull();
            result.Should().BeOfType<ContentTypeDto>();
            result.Id.Should().Be(expectedContentType.Id);
            result.Name.Should().Be(expectedContentType.Name);
        }

        [Fact]
        public async Task ContentTypeService_CreateNewContentType_ThrowException()
        {
            //Arrange
            var createContentTypeDto = new CreateContentTypeRequestDto { Name = "NewContentType" };

            _mockRepo.Setup(ct => ct.CreateContentType(createContentTypeDto))
                .ThrowsAsync(new Exception("Content type data is required."));

            var service = new ContentTypeService(_mockRepo.Object);

            //Act
            Func<Task> result = () => service.CreateNewContentType(createContentTypeDto);

            //Assert
            await result.Should().ThrowAsync<Exception>()
                .WithMessage("Content type data is required.");
        }

        [Theory]
        [InlineData(1)]
        [InlineData(2)]
        public async Task ContentTypeService_DeleteContentType_ReturnsNull(int id)
        {
            //Arrange
            _mockRepo.Setup(ct => ct.DeleteContentType(id));             

            var service = new ContentTypeService(_mockRepo.Object);

            //Act
            var result = service.DeleteExistingContentType(id);

            //Assert
            result.Should().NotBeNull();
        }
    }
}