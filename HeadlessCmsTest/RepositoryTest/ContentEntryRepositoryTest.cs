using FluentAssertions;
using HeadlessCms.Data;
using HeadlessCms.Dtos.Content;
using HeadlessCms.Models;
using HeadlessCms.Repositories;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace HeadlessCmsTest.RepositoryTest
{
    public  class ContentEntryRepositoryTest
    {
        private readonly ApplicationDBContext _context;
        private readonly ContentEntryRepository repo;

        public ContentEntryRepositoryTest()
        {
            _context = GetDbContext().GetAwaiter().GetResult();

            //SUT (Software Under Test)
            repo = new ContentEntryRepository(_context);
        }
        private async Task<ApplicationDBContext> GetDbContext()
        {
            var options = new DbContextOptionsBuilder<ApplicationDBContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            var context = new ApplicationDBContext(options);
            context.Database.EnsureCreated();

            if (await context.ContentEntry.CountAsync() == 0)
            {
                context.ContentEntry.Add(new ContentEntry 
                { 
                    Id = 1, 
                    AppUserId = "test-user-id", 
                    ContentTypeId = 1, 
                    AppUser = new AppUser { Id = "test-user-id", UserName = "testuser", Email = "test@example.com" },
                    ContentType = new ContentType { Id = 1, Name = "TestContentType", Fields = new List<Field> { } }
                });   
                await context.SaveChangesAsync();
            }

            return context;
        }

        [Fact]
        public async Task ContentEntryRepository_AddContentEntry_returnContentEntryDto()
        {
            //Act
            var result = await repo.AddContentEntry(1, "test-user-id");

            //Assert
            result.Should().NotBeNull();
            result.Should().BeOfType<ContentEntryDto>();
            result.Id.Should().BeGreaterThan(0);
        }

        [Theory]
        [InlineData(1)]
        public async Task ContentEntryRepository_GetContentEntry_ReturnContentEntryDto(int id)
        {
            //Act
            var result = await repo.GetContentEntry(id);
            //Assert
            result.Should().NotBeNull();
            result.Should().BeOfType<ContentEntryDto>();
            result.Id.Should().Be(1);
        }


    }
}
