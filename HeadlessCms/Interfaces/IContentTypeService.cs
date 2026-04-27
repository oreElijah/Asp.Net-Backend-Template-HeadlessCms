using HeadlessCms.Dtos.Content;

namespace HeadlessCms.Interfaces
{
    public interface IContentTypeService
    {
        public Task<List<ContentTypeDto>> GetContentTypes();
        public Task<ContentTypeDto> GetUniqueContentType(int id);
        public Task<ContentTypeDto> CreateNewContentType(CreateContentTypeRequestDto createContentTypeDto);
        public Task DeleteExistingContentType(int id);
    }
}
