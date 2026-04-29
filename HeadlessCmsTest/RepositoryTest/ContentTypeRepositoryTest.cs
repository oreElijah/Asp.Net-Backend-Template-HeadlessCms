using FluentAssertions;
using HeadlessCms.Data;
using HeadlessCms.Models;
using HeadlessCms.Exceptions;
using HeadlessCms.Dtos.Content;
using HeadlessCms.Repositories;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace HeadlessCmsTest.RepositoryTest
{
    public class ContentTypeRepositoryTest
    {
        private readonly ApplicationDBContext _context;
        private readonly ContentTypeRepository repo;

        public ContentTypeRepositoryTest()
        {
            _context = GetDbContext().GetAwaiter().GetResult();

            //SUT (Software Under Test)
            repo = new ContentTypeRepository(_context);
        }
        private async Task<ApplicationDBContext> GetDbContext()
        {
            var options = new DbContextOptionsBuilder<ApplicationDBContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            var context = new ApplicationDBContext(options);
            context.Database.EnsureCreated();

            if (await context.ContentType.CountAsync() == 0)
            {
                var contentType = new ContentType
                {
                    Id = 1,
                    Name = "TestContentType",
                    Fields = new List<Field>
                    {
                        new Field { Name = "TestField1", Type = FieldType.String },
                        new Field { Name = "TestField2", Type = FieldType.Integer }
                    }
                };

                await context.ContentType.AddAsync(contentType);

                await context.SaveChangesAsync();
            }

            return context;
        }

        [Fact]
        public async Task ContentTypeRepoSitory_GetAllContentType_ReturnContentTypeDtos()
        {
            //Act
            var result = await repo.GetAllContentType();

            //Assert
            result.Should().NotBeNull();
            result.Should().HaveCount(1);
            result.Should().BeOfType<List<ContentTypeDto>>();
        }

        [Theory]
        [InlineData(1)]
        public async Task ContentTypeRepository_GetContentTypeById_ReturnContentTypeDto(int id)
        {
            //Act
            var result = await repo.GetContentTypeById(id);

            //Assert
            result.Should().NotBeNull();
            result.Id.Should().Be(id);
            result.Should().BeOfType<ContentTypeDto>();

        }

        [Fact]
        public async Task ContentTypeRepository_CreateContentType_ReturnContentTypeDto()
        {
            //Arrange
            var createContentTypeDto = new CreateContentTypeRequestDto
            {
                Name = "NewTestContentType"
            };

            //Act
            var result = await repo.CreateContentType(createContentTypeDto);

            //Assert
            result.Should().NotBeNull();
            result.Name.Should().Be(createContentTypeDto.Name);
            result.Should().BeOfType<ContentTypeDto>();
        }

        [Fact]
        public async Task ContentTypeRepository_CreateContentType_ReturnAlreadyExistsException()
        {
            //Arrange
            var createContentTypeDto = new CreateContentTypeRequestDto
            {
                Name = "TestContentType"
            };
            //Act
            Func<Task> act = async () => await repo.CreateContentType(createContentTypeDto);

            //Assert
            await act.Should().ThrowAsync<AlreadyExistsException>()
                .WithMessage("Content type with the same name already exists.");
        }

        [Theory]
        [InlineData(999)]
        public async Task ContentTypeRepository_GetContentTypeById_ReturnNotFoundException(int nonExistentId)
        {
            //Act
            Func<Task> act = async () => await repo.GetContentTypeById(nonExistentId);

            //Assert
            await act.Should().ThrowAsync<NotFoundException>()
                .WithMessage($"Content type with id {nonExistentId} not found.");
        }

        [Theory]
        [InlineData(999)]
        public async Task ContentTypeRepository_DeleteContentType_ReturnNotFoundException(int nonExistentId)
        {
            //Act
            Func<Task> act = async () => await repo.DeleteContentType(nonExistentId);

            //Assert
            await act.Should().ThrowAsync<NotFoundException>()
                .WithMessage($"Content type with id {nonExistentId} not found.");
        }

        [Fact]
        public async Task ContentTypeRepository_DeleteContentType_ReturnSuccess()
        {
            //Arrange
            var contentType = new ContentType
            {
                Id = 2,
                Name = "ContentTypeToDelete",
            };

            await _context.ContentType.AddAsync(contentType);
            await _context.SaveChangesAsync();

            //Act
            await repo.DeleteContentType(contentType.Id);

            //Assert
            var deletedContentType = await _context.ContentType.FindAsync(contentType.Id);
            deletedContentType.Should().BeNull();
        }

        [Fact]
        public async Task ContentTypeRepository_ContentTypeExists_ReturnsTrue()
        {
            //Arrange
            var contentTypeName = "TestContentType";
            //Act
            var result = await repo.ContentTypeExists(contentTypeName);
            //Assert
            result.Should().BeTrue();
        }
    }
}
