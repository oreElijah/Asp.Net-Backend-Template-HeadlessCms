using HeadlessCms.Dtos.Content;
using HeadlessCms.Models;

namespace HeadlessCms.Interfaces
{
    public interface IContentValueRepository
    {
        public Task<List<ContentValueResponseDto>> GetAllContentValue(int ContentEntryId);
        public Task<ContentValueResponseDto> GetContentValueById(int id);
        public Task<List<ContentValueResponseDto>> CreateContentValue(int ContentTypeId, ContentValueRequestDto cDto, string userId);
        public Task<List<ContentValueResponseDto>> UpdateContentValue(int ContentTypeId, ContentValueRequestDto cDto, string userId);

        public Task DeleteContentValue(int ContentEntryId, string userId);
        public Task ValidateFieldType(FieldType type, string value);
    }
}
