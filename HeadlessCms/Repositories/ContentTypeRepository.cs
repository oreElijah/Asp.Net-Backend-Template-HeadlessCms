using HeadlessCms.Data;
using HeadlessCms.Mappers;
using HeadlessCms.Dtos.Content;
using HeadlessCms.Exceptions;
using HeadlessCms.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace HeadlessCms.Repositories
{
    public class ContentTypeRepository : IContentTypeRepository
    {
        private readonly ApplicationDBContext _context;

        public ContentTypeRepository(ApplicationDBContext context)
        {
            _context = context;
        }

        public async Task<List<ContentTypeDto>> GetAllContentType()
        {
            var contentTypes = await _context.ContentType
                .Include(ct => ct.Fields)
                .ToListAsync();

            if (contentTypes == null)
            {
                throw new NotFoundException("No content types found.");
            }

            return contentTypes.Select(ct => ct.ToContentTypeDto()).ToList();
        }

        public async Task<ContentTypeDto> GetContentTypeById(int id)
        {
            var contentType = await _context.ContentType
                .Include(ct => ct.Fields)
                .FirstOrDefaultAsync(ct => ct.Id == id);

            if (contentType == null)
            {
                throw new NotFoundException($"Content type with id {id} not found.");
            }

            return contentType.ToContentTypeDto();
        }

        public async Task<ContentTypeDto> CreateContentType(CreateContentTypeRequestDto createContentTypeDto)
        {
            var contentType = createContentTypeDto.ToCreateContentType();

            if (await ContentTypeExists(contentType.Name))
            {
                throw new AlreadyExistsException("Content type with the same name already exists.");
            }

            await _context.AddAsync(contentType);
            await _context.SaveChangesAsync();

            return contentType.ToContentTypeDto();
        }

        public async Task<bool> ContentTypeExists(string Name)
        {
            return await _context.ContentType.AnyAsync(ct => ct.Name == Name);
        }

        public async Task DeleteContentType(int id)
        {
            var contentType = await _context.ContentType.FindAsync(id);
            if (contentType == null)
            {
                throw new NotFoundException($"Content type with id {id} not found.");
            }
            _context.ContentType.Remove(contentType);
            await _context.SaveChangesAsync();
        }
    }
}
