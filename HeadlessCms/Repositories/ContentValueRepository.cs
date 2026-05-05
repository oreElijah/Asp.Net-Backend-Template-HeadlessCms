using HeadlessCms.Data;
using HeadlessCms.Dtos.Content;
using HeadlessCms.Exceptions;
using HeadlessCms.Interfaces;
using HeadlessCms.Mappers;
using HeadlessCms.Models;
using HeadlessCms.Services;
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

        public async Task<List<ContentValueResponseDto>> CreateContentValue(int ContentTypeId, ContentValueRequestDto cDto, string userId)
        {
            var contentEntry = await _ceRepo.AddContentEntry(ContentTypeId, userId);

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

        public async Task<List<ContentValueResponseDto>> UpdateContentValue(int ContentTypeId, ContentValueRequestDto cDto, string userId)
        {
            var contentValues = await _context.contentValue
                .Where(cv => cv.ContentEntry.ContentTypeId == ContentTypeId)
                .Include(cv => cv.Field)
                .Include(cv => cv.ContentEntry)
                .ToListAsync();

            if (contentValues == null || !contentValues.Any())
            {
                throw new NotFoundException("No content values found for the specified content entry.");
            }

            var contentEntryModel = contentValues.First().ContentEntry;
            if (contentEntryModel.AppUserId != userId)
            {
                throw new UnauthorizedAccessException("You are not allowed to update this content.");
            }

            var contentEntry = await _ceRepo.GetContentEntry(contentValues.First().ContentEntryId);
            foreach (var contentValue in contentValues)
            {
                if (cDto.Data.ContainsKey(contentValue.Field.Name))
                {
                    var value = cDto.Data[contentValue.Field.Name];
                    
                    await ValidateFieldType(contentValue.Field.Type, value);
                    contentValue.Value = value;
                }
            }
            await _context.SaveChangesAsync();
            return await GetAllContentValue(contentEntry.Id);
        }

        public async Task DeleteContentValue(int ContentEntryId, string userId)
        {
            var contentValues = await _context.contentValue
                .Where(cv => cv.ContentEntryId == ContentEntryId)
                .Include(cv => cv.ContentEntry)
                .ToListAsync();

            if (contentValues == null || !contentValues.Any())
            {
                throw new NotFoundException("No content values found for the specified content entry.");
            }

            if (contentValues.First().ContentEntry.AppUserId != userId)
            {
                throw new UnauthorizedAccessException("You are not allowed to delete this content.");
            }

            _context.contentValue.RemoveRange(contentValues);
            await _context.SaveChangesAsync();
        }

        public async Task ValidateFieldType(FieldType type, string value)
        {
            var handler = FieldHandlerFactory.Create(type);

            handler.Parse(value);
        }


    }
}
