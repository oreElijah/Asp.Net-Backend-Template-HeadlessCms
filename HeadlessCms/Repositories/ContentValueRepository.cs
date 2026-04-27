using HeadlessCms.Data;
using HeadlessCms.Dtos.Content;
using HeadlessCms.Exceptions;
using HeadlessCms.Interfaces;
using HeadlessCms.Mappers;
using HeadlessCms.Models;
using Humanizer;
using Microsoft.EntityFrameworkCore;

namespace HeadlessCms.Repositories
{
    public class ContentValueRepository : IContentValueRepository
    {
        private readonly ApplicationDBContext _context;
        private readonly IContentEntryRepository _ceRepo;

        public ContentValueRepository(ApplicationDBContext context, IContentEntryRepository ceRepo)
        {
            _ceRepo = ceRepo;
            _context = context;
        }

        public async Task<List<ContentValueResponseDto>> GetAllContentValue(int ContentEntryId)
        {
                var contentValues = await _context.contentValue
                .Where(cv => cv.ContentEntryId == ContentEntryId)
                .Include(cv => cv.ContentEntry)
                .Include(cv => cv.Field)
                .ToListAsync();

                if (contentValues == null || !contentValues.Any())
                {
                    throw new NotFoundException("No content values found for the specified content entry.");
                }

                return contentValues.Select(cv => cv.ToContentValueResponseDto()).ToList();
        }

        public async Task<ContentValueResponseDto> GetContentValueById(int id)
        {
            var contentValue = await _context.contentValue
                .Include(cv => cv.ContentEntry)
                .Include(cv => cv.Field)
                .FirstOrDefaultAsync(cv => cv.Id == id);

            if (contentValue == null)
            {
                throw new NotFoundException($"Content value with id {id} not found.");
            }

            return contentValue.ToContentValueResponseDto();
        }

        public async Task<List<ContentValueResponseDto>> CreateContentValue(int ContentTypeId, ContentValueRequestDto cDto)
        {
            var contentEntry = await _ceRepo.AddContentEntry(ContentTypeId);

            var contentType = await _context.ContentType
                .Include(ct => ct.Fields)
                .FirstOrDefaultAsync(ct => ct.Id == contentEntry.ContentTypeId);

            if (contentType == null)
            {
                throw new NotFoundException($"Content type with id {contentEntry.ContentTypeId} not found.");
            }

            foreach (var field in contentType.Fields)
            {
                if (cDto.Data.ContainsKey(field.Name))
                {
                    var value = cDto.Data[field.Name];
                   
                    await ValidateFieldType(field.Type, value);

                    var contentValue = new ContentValue
                    {
                        ContentEntryId = contentEntry.Id,
                        FieldId = field.Id,
                        Value = value
                    };

                    await _context.contentValue.AddAsync(contentValue);
                }
            }

            await _context.SaveChangesAsync();

            return await GetAllContentValue(contentEntry.Id);
        }

        public async Task ValidateFieldType(FieldType type, string value)
        {
            switch (type)
            {
                case FieldType.Integer:
                    if (!int.TryParse(value, out _))
                        throw new Exception("Invalid integer value");
                    break;

                case FieldType.Date:
                    if (!DateTime.TryParse(value, out _))
                        throw new Exception("Invalid date value");
                    break;

                case FieldType.Boolean:
                    if (!bool.TryParse(value, out _))
                        throw new Exception("Invalid boolean value");
                    break;

                case FieldType.String:
                default:
                    break;
            }
        }


    }
}
