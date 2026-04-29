using FluentAssertions;
using HeadlessCms.Dtos.Content;
using HeadlessCms.Dtos.Field;
using HeadlessCms.Exceptions;
using HeadlessCms.Interfaces;
using HeadlessCms.Models;
using HeadlessCms.Services;
using Moq;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace HeadlessCmsTest.ServiceTest
{
    public class ContentValueServiceTest
    {
        private readonly Mock<IContentValueRepository> _mockRepo;

        public ContentValueServiceTest()
        {
            _mockRepo = new Mock<IContentValueRepository>();
        }

        [Theory]
        [InlineData(1)]
        public async Task ContentValueService_GetContentValues_ReturnListOfContentValueResponseDto(int id)
        {
            //Arrange
            var expectedContentValues = new List<ContentValueResponseDto>
            {
                new ContentValueResponseDto{
                    Id = 1,
                    Value = "TestValue",
                    FieldId = 1,
                    Field = new FieldDto { Id = 1, Name = "TestField", Type = FieldType.String},
                    ContentEntryId = 1,
                    ContentEntry = new ContentEntryDto
                    {
                        Id = 1,
                        ContentTypeId = 1,
                        ContentType = new ContentTypeDto { Id = 1, Name = "TestContentType", Fields = new List<FieldDto> { } }
                    }
                },
                new ContentValueResponseDto { Id = 2, Value = "TestValue2" }
            };

            _mockRepo.Setup(cv => cv.GetAllContentValue(id))
                .ReturnsAsync(expectedContentValues);

            var service = new ContentValueService(_mockRepo.Object);

            //Act
            var result = await service.GetContentValues(1);

            //Assert
            result.Should().NotBeNull();
            result.Should().BeOfType<List<ContentValueResponseDto>>();
            result.Should().HaveCount(expectedContentValues.Count);
            result.Should().BeEquivalentTo(expectedContentValues);
        }

        [Theory]
        [InlineData(1)]
        public async Task ContentValueService_GetContentValues_ThrowNotFoundException(int id)
        {
            //Arrange
            _mockRepo.Setup(cv => cv.GetAllContentValue(id))
                .ThrowsAsync(new NotFoundException("No content values found for the specified content entry."));
            var service = new ContentValueService(_mockRepo.Object);

            //Act
            Func<Task> result = () => service.GetContentValues(id);

            //Assert
            await result.Should().ThrowAsync<NotFoundException>()
                .WithMessage("No content values found for the specified content entry.");
        }

        [Theory]
        [InlineData(1)]
        public async Task ContentValueService_GetExistingContentValue_ReturnContentValueResponseDto(int id)
        {
            //Arrange
            var expectedContentValue = new ContentValueResponseDto
            {
                Id = 1,
                Value = "TestValue",
                FieldId = 1,
                Field = new FieldDto { Id = 1, Name = "TestField", Type = FieldType.String },
                ContentEntryId = 1,
                ContentEntry = new ContentEntryDto
                {
                    Id = 1,
                    ContentTypeId = 1,
                    ContentType = new ContentTypeDto { Id = 1, Name = "TestContentType", Fields = new List<FieldDto> { } }
                }
            };

            _mockRepo.Setup(cv => cv.GetContentValueById(id))
                .ReturnsAsync(expectedContentValue);

            var service = new ContentValueService(_mockRepo.Object);

            //Act
            var result = await service.GetExistingContentValue(id);

            //Assert
            result.Should().NotBeNull();
            result.Should().BeOfType<ContentValueResponseDto>();
            result.Id.Should().Be(expectedContentValue.Id);
        }

        [Theory]
        [InlineData(1)]
        public async Task ContentValueService_GetExistingContentValue_ThrowsNotFoundException(int id)
        {
            //Arrange
            _mockRepo.Setup(cv => cv.GetContentValueById(id))
                .ThrowsAsync(new NotFoundException($"Content value with {id} not found."));

            var service = new ContentValueService(_mockRepo.Object);

            //Act
            Func<Task> result = () => service.GetExistingContentValue(id);

            //Assert
            await result.Should().ThrowAsync<NotFoundException>()
                .WithMessage($"Content value with {id} not found.");
        }

        [Theory]
        [InlineData(1)]
        public async Task ContentService_CreateNewContentValue_ReturnListOfContentValueResponseDto(int id)
        {
            //Arrange
            var createContentValueDto = new ContentValueRequestDto
            {
                Data = new Dictionary<string, string>
                {
                    { "TestField", "TestValue" }
                }
            };

            var expectedContentValues = new List<ContentValueResponseDto>
            {
                new ContentValueResponseDto { Id = 1, Value = "TestValue" }
            };

            _mockRepo.Setup(cv => cv.CreateContentValue(id, createContentValueDto, "test-user-id"))
                .ReturnsAsync(expectedContentValues);

            var service = new ContentValueService(_mockRepo.Object);

            //Act
            var result = await service.CreateNewContentValue(id, createContentValueDto, "test-user-id");

            //Assert
            result.Should().NotBeNull();
            result.Should().BeOfType<List<ContentValueResponseDto>>();
            result.Should().HaveCount(expectedContentValues.Count);
            result[0].Id.Should().Be(expectedContentValues[0].Id);
        }

        [Theory]
        [InlineData(1)]
        public async Task ContentService_CreateNewContentValue_ThrowException(int id)
        {
            //Arrange
            var createContentValueDto = new ContentValueRequestDto
            {
                Data = new Dictionary<string, string>
                {
                    { "TestField", "TestValue" }
                }
            };
           
            _mockRepo.Setup(cv => cv.CreateContentValue(id, createContentValueDto, "test-user-id"))
                .ThrowsAsync(new Exception("Content value could not be created."));
            
            var service = new ContentValueService(_mockRepo.Object);
            
            //Act
            Func<Task> result = () => service.CreateNewContentValue(id, createContentValueDto, "test-user-id");
            
            //Assert
            await result.Should().ThrowAsync<Exception>()
                .WithMessage("Content value could not be created.");
        }

        [Theory]
        [InlineData(1)]
        public async Task ContentService_UpdateNewContentValue_ReturnListOfContentValueResponseDto(int id)
        {
            //Arrange
            var UpdateContentValueDto = new ContentValueRequestDto
            {
                Data = new Dictionary<string, string>
                {
                    { "TestField", "UpdatedTestValue" }
                }
            };

            var expectedContentValues = new List<ContentValueResponseDto>
            {
                new ContentValueResponseDto { Id = 1, Value = "UpdatedTestValue" }
            };

            _mockRepo.Setup(cv => cv.CreateContentValue(id, UpdateContentValueDto, "test-user-id"))
                .ReturnsAsync(expectedContentValues);

            var service = new ContentValueService(_mockRepo.Object);

            //Act
            var result = await service.CreateNewContentValue(id, UpdateContentValueDto, "test-user-id");

            //Assert
            result.Should().NotBeNull();
            result.Should().BeOfType<List<ContentValueResponseDto>>();
            result.Should().HaveCount(expectedContentValues.Count);
            result[0].Id.Should().Be(expectedContentValues[0].Id);
        }

        [Theory]
        [InlineData(1)]
        public async Task ContentService_UpdateNewContentValue_ThrowException(int id)
        {
            //Arrange
            var UpdateContentValueDto = new ContentValueRequestDto
            {
                Data = new Dictionary<string, string>
                {
                    { "TestField", "UpdatedTestValue" }
                }
            };

            _mockRepo.Setup(cv => cv.CreateContentValue(id, UpdateContentValueDto, "test-user-id"))
                .ThrowsAsync(new Exception("Content value could not be updated."));

            var service = new ContentValueService(_mockRepo.Object);

            //Act
            Func<Task> result = () => service.CreateNewContentValue(id, UpdateContentValueDto, "test-user-id");

            //Assert
            await result.Should().ThrowAsync<Exception>()
                .WithMessage("Content value could not be updated.");
        }

        [Theory]
        [InlineData(1)]
        public async Task ContentService_DeleteExistingContentValue_ReturnsTask(int id)
        {
            //Arrange
            _mockRepo.Setup(cv => cv.DeleteContentValue(id, "test-user-id"));

            var service = new ContentValueService(_mockRepo.Object);

            //Act
            var result = service.DeleteExistingContentValue(id, "test-user-id");
            
            //Assert
            result.Should().NotBeNull();
        }
    }
}
