using HeadlessCms.Dtos.Field;
using HeadlessCms.Interfaces;
using HeadlessCms.Exceptions;

namespace HeadlessCms.Services
{
    public class FieldService : IFieldService
    {
        private readonly IFieldRepository _fRepo;
        public FieldService(IFieldRepository fRepo)
        {
            _fRepo = fRepo;
        }

        public async Task<List<FieldDto>> GetFields()
        {
            var fields = await _fRepo.GetAllField();
            if (fields == null)
            {
                throw new NotFoundException("No fields found.");
            }
            return fields;
        }

        public async Task<FieldDto> GetUniqueField(int id)
        {
            var field = await _fRepo.GetFieldById(id);
            if (field == null)
            {
                throw new NotFoundException($"Field with id {id} not found.");
            }
            return field;
        }

        public async Task<List<FieldDto>> GetExistingFieldsForContentType(int contentTypeId)
        {
            var fields = await _fRepo.GetFieldsForContentType(contentTypeId);
            if (fields == null || fields.Count == 0)
            {
                throw new NotFoundException($"No fields found for content type with id {contentTypeId}.");
            }
            return fields;
        }

        public async Task<FieldDto> CreateNewField(CreateFieldRequestDto createFieldDto)
        {
            var field = await _fRepo.CreateField(createFieldDto);
            if (field == null)
            {
                throw new Exception("Failed to create field.");
            }
            return field;
        }

        public async Task DeleteExistingField(int id)
        { 
            await _fRepo.DeleteField(id);
        }
    }
}
