using FluentAssertions;
using HeadlessCms.Data;
using HeadlessCms.Dtos.Field;
using HeadlessCms.Exceptions;
using HeadlessCms.Models;
using HeadlessCms.Repositories;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace HeadlessCmsTest.RepositoryTest
{
    public class FieldRepositoryTest
    {
        private readonly ApplicationDBContext _context;
        private readonly FieldRepository repo;

        public FieldRepositoryTest()
        {
            _context = GetDbContext().GetAwaiter().GetResult();

            //SUT (Software Under Test)
            repo = new FieldRepository(_context);
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
                context.ContentType.Add(new ContentType { Id = 1, Name = "TestContentType"});
            }

            if (await context.Field.CountAsync() == 0)
            {
                context.Field.Add(
                    new Field { Id = 1, Name = "TestField", Type = FieldType.String, ContentTypeId = 1 }
                );
                context.Field.Add(
                    new Field { Id = 2, Name = "TestField2", Type = FieldType.Integer, ContentTypeId = 1 }
                    );
                await context.SaveChangesAsync();
            }

            return context;
        }

        [Fact]
        public async Task FieldRepository_GetAllField_ReturnsFieldDtos()
        {
            // Act
            var result = await repo.GetAllField();

            // Assert
            result.Should().NotBeNull();
            result.Should().BeOfType<List<FieldDto>>();
            result.Should().HaveCount(2);
        }

        [Theory]
        [InlineData(1)]
        public async Task FieldRepository_GetFieldById_ReturnsFieldDto(int id)
        {
            // Act
            var result = await repo.GetFieldById(id);

            // Assert
            result.Should().NotBeNull();
            result.Should().BeOfType<FieldDto>();
            result.Id.Should().Be(1);
            result.Name.Should().Be("TestField");
            result.Type.Should().Be(FieldType.String);
        }

        [Fact]
        public async Task FieldRepository_GetFieldById_ThrowsNotFoundException_WhenIdDoesNotExist()
        {
            // Act
            Func<Task> action = async () => await repo.GetFieldById(999);

            // Assert
            await action.Should().ThrowAsync<NotFoundException>().WithMessage("Field with id 999 not found.");
        }

        [Fact]
        public async Task FieldRepository_GetFieldsForContentType_ReturnsFieldDtos()
        {
            // Act
            var result = await repo.GetFieldsForContentType(1);

            // Assert
            result.Should().NotBeNull();
            result.Should().NotBeEmpty();
            result.Should().BeOfType<List<FieldDto>>();
            result.Count.Should().Be(2);
        }

        [Fact]
        public async Task FieldRepository_GetFieldsForContentType_ThrowsNotFoundException_WhenContentTypeHasNoFields()
        {
            // Act
            Func<Task> action = async () => await repo.GetFieldsForContentType(999);

            // Assert
            await action.Should().ThrowAsync<NotFoundException>().WithMessage("No fields found for content type with id 999.");
        }

        [Fact]
        public async Task FieldRepository_CreateField_ReturnsCreatedFieldDto()
        {
            var newFieldDto = new CreateFieldRequestDto
            {
                Name = "NewField",
                Type = FieldType.String,
                ContentTypeId = 1
            };

            // Act
            var result = await repo.CreateField(newFieldDto);

            // Assert
            result.Should().NotBeNull();
            result.Should().BeOfType<FieldDto>();
            result.Name.Should().Be("NewField");
        }

        [Fact]
        public async Task FieldRepository_CreateField_ThrowsAlreadyExistsException_WhenFieldExistsForContentType()
        {
            var existingFieldDto = new CreateFieldRequestDto
            {
                Name = "TestField", // Exists in DB seeded by GetDbContext
                Type = FieldType.String,
                ContentTypeId = 1
            };

            // Act
            Func<Task> action = async () => await repo.CreateField(existingFieldDto);

            // Assert
            await action.Should().ThrowAsync<AlreadyExistsException>().WithMessage("A field with the name 'TestField' already exists for this content type.");
        }

        [Fact]
        public async Task FieldRepository_DeleteField_RemovesFieldFromDatabase()
        {
            // Act
            await repo.DeleteField(1);

            // Assert
            var deletedField = await _context.Field.FindAsync(1);
            deletedField.Should().BeNull();
        }

        [Fact]
        public async Task FieldRepository_DeleteField_ThrowsNotFoundException_WhenFieldDoesNotExist()
        {
            // Act
            Func<Task> action = async () => await repo.DeleteField(999);

            // Assert
            await action.Should().ThrowAsync<NotFoundException>().WithMessage("Field with id 999 not found.");
        }

        [Fact]
        public async Task FieldRepository_FieldExists_ReturnsTrue_WhenExists()
        {
            // Act
            var result = await repo.FieldExists("TestField", 1);

            // Assert
            result.Should().BeTrue();
        }

        [Fact]
        public async Task FieldRepository_FieldExists_ReturnsFalse_WhenDoesNotExist()
        {
            // Act
            var result = await repo.FieldExists("NonExistentField", 1);

            // Assert
            result.Should().BeFalse();
        }
    }
}
