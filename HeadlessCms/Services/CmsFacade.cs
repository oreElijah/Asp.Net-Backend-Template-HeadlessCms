using HeadlessCms.Dtos.Content;
using HeadlessCms.Dtos.Field;
using HeadlessCms.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace HeadlessCms.Services
{
    //Facade Design Pattern to simplify the interface for creating content types with fields
    public class CmsFacade : ICmsFacade
    {
        private readonly IContentTypeRepository _ctRepo;
        private readonly IFieldRepository _fRepo;

        public CmsFacade(IContentTypeRepository ctRepo, IFieldRepository fRepo)
        {
            _ctRepo = ctRepo;
            _fRepo = fRepo;
        }

        public async Task CreateContentTypeWithFields(CreateContentTypeWithFieldsDto dto)
        {
            var createdContentTypesampleDto = new CreateContentTypeRequestDto
            {
                Name = dto.Name
            };

            var contenttype = await _ctRepo.CreateContentType(createdContentTypesampleDto);

            foreach (var field in dto.Fields)
            {
                var fieldSampledto = new CreateFieldRequestDto
                {
                    Name = field.Name,
                    Type = field.Type,
                    ContentTypeId = contenttype.Id
                };
                await _fRepo.CreateField(fieldSampledto);
            }
        }
    }
}