using FluentAssertions;
using HeadlessCms.Data;
using HeadlessCms.Dtos.Content;
using HeadlessCms.Models;
using HeadlessCms.Repositories;
using HeadlessCms.Exceptions;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace HeadlessCmsTest.RepositoryTest
{
    public class ContentValueRepositoryTest
    {
        private readonly ApplicationDBContext _context;
        private readonly ContentValueRepository repo;
        private readonly ContentEntryRepository cerepo;

        public ContentValueRepositoryTest()
        {
            _context = GetDbContext().GetAwaiter().GetResult();

            //SUT (Software Under Test)
            cerepo = new ContentEntryRepository(_context);
            repo = new ContentValueRepository(_context, cerepo);
        }
        private async Task<ApplicationDBContext> GetDbContext()
        {
            var options = new DbContextOptionsBuilder<ApplicationDBContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            var context = new ApplicationDBContext(options);
            context.Database.EnsureCreated();

            if (await context.contentValue.CountAsync() == 0)
            {

                context.contentValue.Add(new ContentValue()
                {
                    Id = 1,
                    Value = "TestValue",
                    FieldId = 1,
                    Field = new Field { Id = 1, Name = "TestField", Type = FieldType.String, ContentTypeId = 1 },
                    ContentEntryId = 1,
                    ContentEntry = new ContentEntry
                    {
                        Id = 1,
                        AppUserId = "test-user-id2",
                        ContentTypeId = 1,
                        AppUser = new AppUser { Id = "test-user-id2", UserName = "testuser", Email = "test@example.com" },
                        ContentType = new ContentType { Id = 1, Name = "TestContentType", Fields = new List<Field> { } }
                    }
                });

                context.contentValue.Add(new ContentValue()
                {
                    Id = 2,
                    Value = "TestValue2",
                    FieldId = 2,
                    Field = new Field { Id = 2, Name = "TestField2", Type = FieldType.String, ContentTypeId = 2 },
                    ContentEntryId = 2,
                    ContentEntry = new ContentEntry
                    {
                        Id = 2,
                        AppUserId = "test-user-id3",
                        ContentTypeId = 2,
                        AppUser = new AppUser { Id = "test-user-id3", UserName = "testuser", Email = "test@example.com" },
                        ContentType = new ContentType { Id = 2, Name = "TestContentType2", Fields = new List<Field> { } }
                    }
                });
                await context.SaveChangesAsync();
            }

            return context;
        }

        [Fact]
        public async Task ContentValueRepository_GetAllContentValue_ReturnContentValueResponseDtos()
        {
            //Act
            var result = await repo.GetAllContentValue(1);

            //Assert
            result.Should().NotBeNull();
            result.Should().HaveCount(1);
            result.Should().BeOfType<List<ContentValueResponseDto>>();
        }

        [Theory]
        [InlineData(1)]
        public async Task ContentValueRepository_GetContentValueById_ReturnContentValueResponseDto(int id)
        {
            //Act
            var result = await repo.GetContentValueById(id);

            //Assert
            result.Should().NotBeNull();
            result.Should().BeOfType<ContentValueResponseDto>();
            result.Id.Should().Be(1);
            result.Value.Should().Be("TestValue");
            result.FieldId.Should().Be(1);
            result.ContentEntryId.Should().Be(1);
        }

        [Theory]
        [InlineData(999)]
        public async Task ContentValueRepository_GetContentValueById_ThrowNotFoundException(int id)
        {
            //act
            Func<Task> result = () => repo.GetContentValueById(id);

            //assert
            await result.Should().ThrowAsync<NotFoundException>()
                .WithMessage($"Content value with id {id} not found.");
        }

        [Fact]
        public async Task ContentValueRepository_CreateContentValue_ReturnContentValueResponseDto()
        {
            //Arrange
            var createDto = new ContentValueRequestDto
            {
                Data = new Dictionary<string, string>
               {
                   { "TestField", "NewTestValue" }
               }
            };

            //Act
            var result = await repo.CreateContentValue(1, createDto, "test-user-id");

            //Assert
            result.Should().NotBeNull();
            result.Should().HaveCount(1);
            result[0].Should().BeOfType<ContentValueResponseDto>();
            result[0].Value.Should().Be("NewTestValue");
        }

        [Fact]
        public async Task ContentValueRepository_CreateContentValue_ThrowNotFoundException()
        {
            //Arrange
            var createDto = new ContentValueRequestDto
            {
                Data = new Dictionary<string, string>
                {
                    { "TestField", "NewTestValue" }
                }
            };
            //Act
            Func<Task> result = () => repo.CreateContentValue(999, createDto, "test-user-id");
            //Assert
            await result.Should().ThrowAsync<NotFoundException>()
                .WithMessage($"Content Type does not exist");
        }

        [Fact]
        public async Task ContentValueRepository_UpdateContentValue_ReturnContentValueResponseDto()
        {
            //Arrange
            var updateDto = new ContentValueRequestDto
            {
                Data = new Dictionary<string, string>
               {
                   { "TestField", "NewTestValue" }
               }
            };

            //Act
            var result = await repo.UpdateContentValue(1, updateDto, "test-user-id2");

            //Assert
            result.Should().NotBeNull();
            result.Should().HaveCount(1);
            result[0].Should().BeOfType<ContentValueResponseDto>();
            result[0].Value.Should().Be("NewTestValue");
        }

        [Fact]
        public async Task ContentValueRepository_UpdateContentValue_ThrowNotFoundException()
        {
            //Arrange
            var updateDto = new ContentValueRequestDto
            {
                Data = new Dictionary<string, string>
                {
                    { "TestField", "NewTestValue" }
                }
            };

            //Act
            Func<Task> result = () => repo.UpdateContentValue(999, updateDto, "test-user-id2");
            
            //Assert
            await result.Should().ThrowAsync<NotFoundException>()
                .WithMessage($"No content values found for the specified content entry.");
        }

        [Theory]
        [InlineData(1)]
        public async Task ContentValueRepository_DeleteContentValue_ReturnsVoid(int contentEntryId)
        {
            //Act
            Func<Task> result = () => repo.DeleteContentValue(contentEntryId, "test-user-id2");
            
            //Assert
            await result.Should().NotThrowAsync();
        }

        [Theory]
        [InlineData(1)]
        public async Task ContentValueRepository_DeleteContentValue_ThrowUnAuthorizedAccessException(int contentEntryId)
        {
            //Act
            Func<Task> result = () => repo.DeleteContentValue(contentEntryId, "test-user-id");

            //Assert
            await result.Should().ThrowAsync<UnauthorizedAccessException>()
                .WithMessage("You are not allowed to delete this content.");
        }

        [Theory]
        [InlineData(999)]
        public async Task ContentValueRepository_DeleteContentValue_ThrownNotFoundException(int contentEntryId)
        {
            //Act
            Func<Task> result = () => repo.DeleteContentValue(contentEntryId, "test-user-id");

            //Assert
            await result.Should().ThrowAsync<NotFoundException>()
                .WithMessage("No content values found for the specified content entry.");
        }
    }
}
