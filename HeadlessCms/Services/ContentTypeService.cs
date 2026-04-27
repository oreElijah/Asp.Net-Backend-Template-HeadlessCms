using HeadlessCms.Dtos.Content;
using HeadlessCms.Interfaces;
using HeadlessCms.Exceptions;

namespace HeadlessCms.Services
{
    public class ContentTypeService : IContentTypeService
    {
        private readonly IContentTypeRepository _ctRepo;
        public ContentTypeService(IContentTypeRepository ctRepo)
        {
            _ctRepo = ctRepo;
        }

        public async Task<List<ContentTypeDto>> GetContentTypes()
        {
            var contentTypes = await _ctRepo.GetAllContentType();
            if (contentTypes == null)
            {
                throw new NotFoundException("No content types found.");
            }
            return contentTypes;
        }

        public async Task<ContentTypeDto> GetUniqueContentType(int id)
        {
            var contentType = await _ctRepo.GetContentTypeById(id);
            if (contentType == null)
            {
                throw new NotFoundException($"Content type with id {id} not found.");
            }
            return contentType;
        }

        public async Task<ContentTypeDto> CreateNewContentType(CreateContentTypeRequestDto createContentTypeDto)
        {
            var contentType = await _ctRepo.CreateContentType(createContentTypeDto);

            if (contentType == null)
            {
                throw new Exception("Content type data is required.");
            }
            return contentType;
        }

        public async Task DeleteExistingContentType(int id)
        {
            await _ctRepo.DeleteContentType(id);
        }
    }
}
