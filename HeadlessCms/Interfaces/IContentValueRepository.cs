using HeadlessCms.Dtos.Content;
using HeadlessCms.Models;

namespace HeadlessCms.Interfaces
{
    public interface IContentValueRepository
    {
        public Task<List<ContentValueResponseDto>> GetAllContentValue(int ContentEntryId);
        public Task<ContentValueResponseDto> GetContentValueById(int id);
        public Task<List<ContentValueResponseDto>> CreateContentValue(int ContentTypeId, ContentValueRequestDto cDto);
        public Task ValidateFieldType(FieldType type, string value);
    }
}
