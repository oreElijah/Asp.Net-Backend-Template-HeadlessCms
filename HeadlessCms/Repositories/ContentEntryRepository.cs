using HeadlessCms.Data;
using HeadlessCms.Dtos.Content;
using HeadlessCms.Interfaces;
using HeadlessCms.Exceptions;
using Microsoft.EntityFrameworkCore;
using HeadlessCms.Models;
using HeadlessCms.Mappers;

namespace HeadlessCms.Repositories
{
    public class ContentEntryRepository : IContentEntryRepository
    {
        private readonly ApplicationDBContext _context;

        public ContentEntryRepository(ApplicationDBContext context)
        {
            _context = context;
        }

        public async Task<ContentEntryDto> AddContentEntry(int ContentTypeId)
        {
            var contentType = await _context.ContentType
                .Include(ct => ct.Fields)
                .FirstOrDefaultAsync(ct => ct.Id == ContentTypeId);

            if (contentType == null)
            {
                throw new NotFoundException("Content Type does not exist");
            }

            var Entry = new ContentEntry
            {
                ContentTypeId = ContentTypeId
            };

            await _context.ContentEntry.AddAsync(Entry);
            await _context.SaveChangesAsync();

            return new ContentEntryDto
            {
                Id = Entry.Id,
                ContentTypeId = Entry.ContentTypeId,
                ContentType = Entry.ContentType.ToContentTypeDto()
            };
        }

        public async Task<ContentEntryDto> GetContentEntry(int EntryId)
        {
            var entry = await _context.ContentEntry
                .Include(e => e.ContentType)
                .ThenInclude(ct => ct.Fields)
                .FirstOrDefaultAsync(e => e.Id == EntryId);

            if (entry == null)
            {
                throw new NotFoundException("Content Entry not found");
            }

            return new ContentEntryDto
            {
                Id = entry.Id,
                ContentTypeId = entry.ContentTypeId,
                ContentType = entry.ContentType.ToContentTypeDto()
            };
        }
    }
}
