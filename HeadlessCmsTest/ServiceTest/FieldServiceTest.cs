using HeadlessCms.Interfaces;
using HeadlessCms.Services;
using FluentAssertions;
using HeadlessCms.Dtos.Field;
using HeadlessCms.Exceptions;
using HeadlessCms.Models;
using Moq;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace HeadlessCmsTest.ServiceTest
{
    public class FieldServiceTest
    {
        private readonly Mock<IFieldRepository> _mockRepo;

        public FieldServiceTest()
        {

            _mockRepo = new Mock<IFieldRepository>();

        }

        [Fact]
        public async Task FieldService_GetFields_ReturnListOfFieldDto()
        {
            //Arrange
            var expectedFields = new List<FieldDto>
            {
                new FieldDto { Id = 1, Name = "TestField1", Type = FieldType.String },
                new FieldDto { Id = 2, Name = "TestField2", Type = FieldType.Integer }
            };

            _mockRepo.Setup(f => f.GetAllField())
                .ReturnsAsync(expectedFields);

            var service = new FieldService(_mockRepo.Object);

            //Act
            var result = await service.GetFields();

            //Assert
            result.Should().NotBeNull();
            result.Should().BeOfType<List<FieldDto>>();
            result.Should().HaveCount(expectedFields.Count);
        }

        [Fact]
        public async Task FieldService_GetFields_ThrowNotFound()
        {
            //Arrange
            _mockRepo.Setup(f => f.GetAllField())
                .ThrowsAsync(new NotFoundException("No fields found."));

            var service = new FieldService(_mockRepo.Object);

            //Act
            Func<Task> result = () => service.GetFields();

            //Assert
            await result.Should().ThrowAsync<NotFoundException>()
                .WithMessage("No fields found.");
        }

        [Theory]
        [InlineData(1)]
        public async Task FieldService_GetFieldById_ReturnFieldDto(int id)
        {
            //Arrange
            var expectedField = new FieldDto { Id = 1, Name = "TestField1", Type = FieldType.String };

            _mockRepo.Setup(f => f.GetFieldById(id))
                .ReturnsAsync(expectedField);

            var service = new FieldService(_mockRepo.Object);

            //Act
            var result = await service.GetUniqueField(id);

            //Assert
            result.Should().NotBeNull();
            result.Should().BeOfType<FieldDto>();
            result.Id.Should().Be(expectedField.Id);
            result.Name.Should().Be(expectedField.Name);
            result.Type.Should().Be(expectedField.Type);

        }

        [Theory]
        [InlineData(1)]
        public async Task FieldService_GetFieldById_ThrowNotFoundException(int id)
        {
            //Arrange
            _mockRepo.Setup(f => f.GetFieldById(id))
                .ThrowsAsync(new NotFoundException($"Field with id {id} not found."));

            var service = new FieldService(_mockRepo.Object);

            //Act
            Func<Task> result = () => service.GetUniqueField(id);

            //Assert
            await result.Should().ThrowAsync<NotFoundException>()
                .WithMessage($"Field with id {id} not found.");
        }

        [Theory]
        [InlineData(1)]
        public async Task FieldService_GetExistingFieldForContentType_ReturnListOfFieldDto(int id)
        {
            //Arrange
            var expectedFields = new List<FieldDto>
            {
                new FieldDto { Id = 1, Name = "TestField1", Type = FieldType.String },
                new FieldDto { Id = 2, Name = "TestField2", Type = FieldType.Integer }
            };

            _mockRepo.Setup(f => f.GetFieldsForContentType(id))
                .ReturnsAsync(expectedFields);

            var service = new FieldService(_mockRepo.Object);

            //Act
            var result = await service.GetExistingFieldsForContentType(id);

            //Assert
            result.Should().NotBeNull();
            result.Should().BeOfType<List<FieldDto>>();
            result.Should().HaveCount(expectedFields.Count);
        }
        [Theory]
        [InlineData(1)]
        public async Task FieldService_GetExistingFieldForContentType_ThrowNotFoundException(int id)
        {
            //Arrange
            _mockRepo.Setup(f => f.GetFieldsForContentType(id))
                .ThrowsAsync(new NotFoundException($"No fields found for content type with id {id}."));

            var service = new FieldService(_mockRepo.Object);

            //Act
            Func<Task> result = () => service.GetExistingFieldsForContentType(id);

            //assert
            await result.Should().ThrowAsync<NotFoundException>()
                .WithMessage($"No fields found for content type with id {id}.");
        }

        [Fact]
        public async Task FieldService_CreateNewField_ReturnFieldDto()
        {
            //Arrange
            var createFieldDto = new CreateFieldRequestDto { Name = "NewField", Type = FieldType.String };

            var expectedField = new FieldDto { Id = 1, Name = "NewField", Type = FieldType.String };

            _mockRepo.Setup(f => f.CreateField(createFieldDto))
                .ReturnsAsync(expectedField);

            var service = new FieldService(_mockRepo.Object);

            //Act
            var result = await service.CreateNewField(createFieldDto);

            //Assert
            result.Should().NotBeNull();
            result.Should().BeOfType<FieldDto>();
            result.Id.Should().Be(expectedField.Id);
            result.Name.Should().Be(expectedField.Name);
        }

        [Fact]
        public async Task FieldService_CreateNewField_ThrowException()
        {
            //Arrange
            var createFieldDto = new CreateFieldRequestDto { Name = "NewField", Type = FieldType.String };

            _mockRepo.Setup(f => f.CreateField(createFieldDto))
                .ThrowsAsync(new Exception("Failed to create field."));

            var service = new FieldService(_mockRepo.Object);

            //Act
            Func<Task> result = () => service.CreateNewField(createFieldDto);

            //Assert
            await result.Should().ThrowAsync<Exception>()
                .WithMessage("Failed to create field.");
        }

        [Theory]
        [InlineData(1)]
        public async Task FieldService_DeleteExistingField_ReturnsVoid(int id)
        {
            //Arrange
            _mockRepo.Setup(f => f.DeleteField(id));

            var service = new FieldService(_mockRepo.Object);

            //Act
            var result = service.DeleteExistingField(id);

            //Assert
            result.Should().NotBeNull();
        }
    }
}
