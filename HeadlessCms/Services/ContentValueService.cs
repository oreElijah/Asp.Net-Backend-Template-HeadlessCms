using HeadlessCms.Dtos.Content;
using HeadlessCms.Interfaces;
using HeadlessCms.Exceptions;

namespace HeadlessCms.Services
{
    public class ContentValueService : IContentValueService
    {
        private readonly IContentValueRepository _cvRepo;

        public ContentValueService(IContentValueRepository cvRepo)
        {
            _cvRepo = cvRepo;
        }

        public async Task<List<ContentValueResponseDto>> GetContentValues(int ContentEntryId)
        {
            var contentValues = await _cvRepo.GetAllContentValue(ContentEntryId);
            if (contentValues == null || contentValues.Count == 0)
            {
                throw new NotFoundException("No content values found for the specified content entry.");
            }

            return contentValues;
        }

        public async Task<ContentValueResponseDto> GetExistingContentValue(int id)
        {
            var contentValue = await _cvRepo.GetContentValueById(id);
            if (contentValue == null)
            {
                throw new NotFoundException($"Content value with id {id} not found.");
            }
            return contentValue;
        }

        public async Task<List<ContentValueResponseDto>> CreateNewContentValue(int ContentTypeId, ContentValueRequestDto createContentValueDto, string userId)
        {
            var contentValues = await _cvRepo.CreateContentValue(ContentTypeId, createContentValueDto, userId);

            if (contentValues == null || !contentValues.Any())
            {
                throw new Exception("Content value could not be created.");
            }

            return contentValues;
        }

        public async Task<List<ContentValueResponseDto>> UpdateExistingContentValue(int ContentTypeId, ContentValueRequestDto cDto, string userId)
        {
            var contentValues = await _cvRepo.UpdateContentValue(ContentTypeId, cDto, userId);
            if (contentValues == null || !contentValues.Any())
            {
                throw new Exception("Content value could not be updated.");
            }
            return contentValues;
        }

        public async Task DeleteExistingContentValue(int ContentEntryId, string userId)
        {
            await _cvRepo.DeleteContentValue(ContentEntryId, userId);
        }
    }
}
