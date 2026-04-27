using HeadlessCms.Dtos.Content;

namespace HeadlessCms.Interfaces
{
    public interface IContentTypeRepository
    {
        public Task<List<ContentTypeDto>> GetAllContentType();
        public Task<ContentTypeDto> GetContentTypeById(int id);
        public Task<ContentTypeDto> CreateContentType(CreateContentTypeRequestDto createContentTypeDto);
        public Task<bool> ContentTypeExists(string Name);
        public Task DeleteContentType(int id);
    }
}
