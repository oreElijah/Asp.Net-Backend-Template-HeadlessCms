using HeadlessCms.Dtos.Content;

namespace HeadlessCms.Interfaces
{
    public interface IContentValueService
    {
        public Task<List<ContentValueResponseDto>> GetContentValues(int ContentEntryId);
        public Task<ContentValueResponseDto> GetExistingContentValue(int id);
        public Task<List<ContentValueResponseDto>> CreateNewContentValue(int ContentTypeId, ContentValueRequestDto createContentValueDto, string userId);
        public Task<List<ContentValueResponseDto>> UpdateExistingContentValue(int ContentTypeId, ContentValueRequestDto cDto, string userId);
        public Task DeleteExistingContentValue(int ContentEntryId, string userId);
    }
}