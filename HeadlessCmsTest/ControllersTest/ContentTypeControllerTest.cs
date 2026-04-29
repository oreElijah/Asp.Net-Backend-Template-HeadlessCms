using HeadlessCms.Interfaces;
using HeadlessCms.Dtos.Content;
using HeadlessCms.Dtos.Field;
using HeadlessCms.Controllers;
using HeadlessCms.Models;
using FluentAssertions;
using Moq;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

namespace HeadlessCmsTest.ControllersTest
{
    public class ContentTypeControllerTest
    {
        private readonly Mock<IContentTypeService> _ctMockService;
        private readonly Mock<IFieldService> _fMockService;

        public ContentTypeControllerTest()
        {
            _ctMockService = new Mock<IContentTypeService>();
            _fMockService = new Mock<IFieldService>();
        }

        [Fact]
        public async Task ContentTypeController_GetAllContentType_ReturnContentTypes()
        {
            //Arrange
            var expectedContentTypes = new List<ContentTypeDto>
            {
                new ContentTypeDto { Id = 1, Name = "TestContentType1" },
                new ContentTypeDto { Id = 2, Name = "TestContentType2" }
            };

            _ctMockService.Setup(_ctMockService => _ctMockService.GetContentTypes())
                .ReturnsAsync(expectedContentTypes);

            var controller = new ContentTypeController(_ctMockService.Object, _fMockService.Object);

            //Act
            var result = await controller.GetAllContentType();
            var okResult = result as OkObjectResult;
            var returnedContentTypes = okResult.Value as List<ContentTypeDto>;

            //Assert
            okResult.Should().NotBeNull();
            okResult.StatusCode.Should().Be(200);
            returnedContentTypes.Should().NotBeNull();
            returnedContentTypes.Count.Should().Be(2);
        }

        [Fact]
        public async Task ContentTypeController_GetAllContentType_ReturnNotFound()
        {
            //Arrange
            _ctMockService.Setup(_ctMockService => _ctMockService.GetContentTypes())
                .ReturnsAsync(new List<ContentTypeDto>());
            var controller = new ContentTypeController(_ctMockService.Object, _fMockService.Object);
            //Act
            var result = await controller.GetAllContentType();
            var notFoundResult = result as NotFoundObjectResult;
            //Assert
            notFoundResult.Should().NotBeNull();
            notFoundResult.StatusCode.Should().Be(404);
            notFoundResult.Value.Should().Be("No content types found.");
        }

        [Theory]
        [InlineData(1)]
        public async Task ContentTypeController_GetContentTypeById_ReturnContentTypes(int id)
        {
            //Arrange
            var expectedContentType = new ContentTypeDto { Id = id, Name = $"TestContentType{id}" };

            _ctMockService.Setup(ct => ct.GetUniqueContentType(id))
                .ReturnsAsync(expectedContentType);

            var controller = new ContentTypeController(_ctMockService.Object, _fMockService.Object);

            //Act
            var result = await controller.GetContentTypeById(id);
            var okResult = result as OkObjectResult;
            var returnedContentType = okResult.Value as ContentTypeDto;

            //Assert
            okResult.Should().NotBeNull();
            okResult.StatusCode.Should().Be(200);
            returnedContentType.Should().NotBeNull();
            returnedContentType.Id.Should().Be(id);
            returnedContentType.Name.Should().Be($"TestContentType{id}");
        }

        [Theory]
        [InlineData(1)]
        public async Task ContentTypeController_GetContentTypeById_ReturnNotFound(int id)
        {
            //Arrange
            _ctMockService.Setup(ct => ct.GetUniqueContentType(id))
                .ReturnsAsync((ContentTypeDto)null);

            var controller = new ContentTypeController(_ctMockService.Object, _fMockService.Object);

            //Act
            var result = await controller.GetContentTypeById(id);
            var notFoundResult = result as NotFoundObjectResult;

            //Assert
            notFoundResult.Should().NotBeNull();
            notFoundResult.StatusCode.Should().Be(404);
            notFoundResult.Value.Should().Be($"Content type with id {id} not found.");
        }

        [Fact]
        public async Task ContentTypeController_CreateContentType_ReturnCreatedAtAction()
        {
            //Arrange
            var contentTypeCreateDto = new CreateContentTypeRequestDto { Name = "NewContentType" };

            var createdContentType = new ContentTypeDto { Id = 1, Name = "NewContentType" };

            _ctMockService.Setup(ct => ct.CreateNewContentType(contentTypeCreateDto))
                .ReturnsAsync(createdContentType);

            var controller = new ContentTypeController(_ctMockService.Object, _fMockService.Object);

            //Act
            var result = await controller.CreateContentType(contentTypeCreateDto);
            var createdAtActionResult = result as CreatedAtActionResult;
            var returnedContentType = createdAtActionResult.Value as ContentTypeDto;

            //Assert
            createdAtActionResult.Should().NotBeNull();
            createdAtActionResult.StatusCode.Should().Be(201);
            createdAtActionResult.ActionName.Should().Be(nameof(controller.GetContentTypeById));
            createdAtActionResult.RouteValues["id"].Should().Be(createdContentType.Id);
            returnedContentType.Should().NotBeNull();
            returnedContentType.Id.Should().Be(createdContentType.Id);
            returnedContentType.Name.Should().Be(createdContentType.Name);
        }

        [Fact]
        public async Task ContentTypeController_CreateContentType_ReturnNotFound()
        {
            //Arrange
            var contentTypeCreateDto = new CreateContentTypeRequestDto { Name = "NewContentType" };
          

            _ctMockService.Setup(ct => ct.CreateNewContentType(contentTypeCreateDto))
                .ReturnsAsync((ContentTypeDto)null);

            var controller = new ContentTypeController(_ctMockService.Object, _fMockService.Object);

            //Act
            var result = await controller.CreateContentType(contentTypeCreateDto);
            var notFoundResult = result as NotFoundObjectResult;

            //Assert
            notFoundResult.Should().NotBeNull();
            notFoundResult.StatusCode.Should().Be(404);
            notFoundResult.Value.Should().Be("Failed to create content type.");
        }

        [Theory]
        [InlineData(1)]
        public async Task ContentTypeController_DeleteContentType_ReturnNoContent(int id)
        {
            //Arrange
            _ctMockService.Setup(ct => ct.DeleteExistingContentType(id));

            var controller = new ContentTypeController(_ctMockService.Object, _fMockService.Object);
            
            //Act
            var result = await controller.DeleteContentType(id);
            var noContentResult = result as NoContentResult;
           
            //Assert
            noContentResult.Should().NotBeNull();
            noContentResult.StatusCode.Should().Be(204);
        }

        [Fact]
        public async Task ContentType_GetAllFields_ReturnOkFieldDtos()
        {
            //Arrange
            var expectedFields = new List<FieldDto>
            {
                new FieldDto { Id = 1, Name = "TestField1", Type = FieldType.String },
                new FieldDto { Id = 2, Name = "TestField2", Type = FieldType.Integer }
            };

            _fMockService.Setup(f => f.GetFields())
                .ReturnsAsync(expectedFields);

            var controller = new ContentTypeController(_ctMockService.Object, _fMockService.Object);

            //Act
            var result = await controller.GetAllFields();
            var okResult = result as OkObjectResult;
            var returnedFields = okResult.Value as List<FieldDto>;

            //Assert
            okResult.Should().NotBeNull();
            okResult.StatusCode.Should().Be(200);
            okResult.Value.Should().NotBeNull();
            returnedFields.Should().NotBeNull();
            returnedFields.Should().HaveCount(expectedFields.Count);
        }

        [Fact]
        public async Task ContentType_GetAllFields_ReturnNotFound()
        {
            //Arrange
            _fMockService.Setup(f => f.GetFields())
                .ReturnsAsync(new List<FieldDto>());
            
            var controller = new ContentTypeController(_ctMockService.Object, _fMockService.Object);
            
            //Act
            var result = await controller.GetAllFields();
            var notFoundResult = result as NotFoundObjectResult;
            
            //Assert
            notFoundResult.Should().NotBeNull();
            notFoundResult.StatusCode.Should().Be(404);
            notFoundResult.Value.Should().Be("No fields found .");
        }

        [Theory]
        [InlineData(1)]
        [InlineData(2)]
        public async Task ContentTypeController_GetFieldById_ReturnOkFieldDto(int id)
        {
            //Arrange
            var expectedField = new FieldDto { Id = id, Name = $"TestField{id}", Type = FieldType.String };

            _fMockService.Setup(f => f.GetUniqueField(id))
                .ReturnsAsync(expectedField);

            var controller = new ContentTypeController(_ctMockService.Object, _fMockService.Object);

                        //Act
            var result = await controller.GetFieldById(id);
            var okResult = result as OkObjectResult;
            var returnedField = okResult.Value as FieldDto;

            //Assert
            okResult.Should().NotBeNull();
            okResult.StatusCode.Should().Be(200);
            returnedField.Should().NotBeNull();
            returnedField.Id.Should().Be(expectedField.Id);
            returnedField.Name.Should().Be(expectedField.Name);
            returnedField.Type.Should().Be(expectedField.Type);
        }

        [Theory]
        [InlineData(1)]
        [InlineData(2)]
        public async Task ContentTypeController_GetFieldById_ReturnNotFound(int id)
        {
            //Arrange
            _fMockService.Setup(f => f.GetUniqueField(id))
                .ReturnsAsync((FieldDto)null);

            var controller = new ContentTypeController(_ctMockService.Object, _fMockService.Object);

            //Act
            var result = await controller.GetFieldById(id);
            var notFoundResult = result as NotFoundObjectResult;

            //Assert
            notFoundResult.Should().NotBeNull();
            notFoundResult.StatusCode.Should().Be(404);
            notFoundResult.Value.Should().Be("Field not found");
        }

        [Theory]
        [InlineData(1)]
        public async Task ContentTypeController_CreateField_ReturnCreatedAtAction(int contentTypeId)
        {
            //Arrange
            var createFieldDto = new CreateFieldRequestDto { Name = "NewField", Type = FieldType.String };
            var createdField = new FieldDto { Id = 1, Name = "NewField", Type = FieldType.String };
            
            _fMockService.Setup(f => f.CreateNewField(createFieldDto))
                .ReturnsAsync(createdField);
            
            var controller = new ContentTypeController(_ctMockService.Object, _fMockService.Object);
            
            //Act
            var result = await controller.CreateField(contentTypeId, createFieldDto);
            var createdAtActionResult = result as CreatedAtActionResult;
            var returnedField = createdAtActionResult.Value as FieldDto;
            
            //Assert
            createdAtActionResult.Should().NotBeNull();
            createdAtActionResult.StatusCode.Should().Be(201);           
            returnedField.Should().NotBeNull();
            returnedField.Id.Should().Be(createdField.Id);
            returnedField.Name.Should().Be(createdField.Name);
            returnedField.Type.Should().Be(createdField.Type);
        }
        [Theory]
        [InlineData(1)]
        public async Task ContentTypeController_CreateField_ReturnNotFound(int contentTypeId)
        {
            //Arrange
            var createFieldDto = new CreateFieldRequestDto { Name = "NewField", Type = FieldType.String };
           
            _fMockService.Setup(f => f.CreateNewField(createFieldDto))
                .ReturnsAsync((FieldDto)null);
           
            var controller = new ContentTypeController(_ctMockService.Object, _fMockService.Object);
            
            //Act
            var result = await controller.CreateField(contentTypeId, createFieldDto);
            var notFoundResult = result as NotFoundObjectResult;
            
            //Assert
            notFoundResult.Should().NotBeNull();
            notFoundResult.StatusCode.Should().Be(404);
            notFoundResult.Value.Should().Be("Failed to create field.");
        }


        [Theory]
        [InlineData(1)]
        public async Task ContentTypeController_DeleteField_ReturnNoContent(int Id)
        {
            //Arrange
            _fMockService.Setup(f => f.DeleteExistingField(Id));
           
            var controller = new ContentTypeController(_ctMockService.Object, _fMockService.Object);
           
            //Act
            var result = await controller.DeleteField(Id);
            var noContentResult = result as NoContentResult;
            
            //Assert
            noContentResult.Should().NotBeNull();
            noContentResult.StatusCode.Should().Be(204);
        }
    }
}
