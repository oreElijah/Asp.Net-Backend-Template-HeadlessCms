using HeadlessCms.Data;
using HeadlessCms.Dtos.Field;
using HeadlessCms.Interfaces;
using HeadlessCms.Mappers;
using HeadlessCms.Exceptions;
using Microsoft.EntityFrameworkCore;

namespace HeadlessCms.Repositories
{
    public class FieldRepository : IFieldRepository
    {
        private readonly ApplicationDBContext _context;

        public FieldRepository(ApplicationDBContext context)
        {
            _context = context;
        }

        public async Task<List<FieldDto>> GetAllField()
        {
            var fields = await _context.Field
                .Include(f => f.ContentType)
                .ToListAsync();

            if(fields == null)
            {
                throw new NotFoundException("No fields found.");
            }

            return fields.Select(f => f.ToFieldDto()).ToList();
        }

        public async Task<FieldDto> GetFieldById(int id)
        {
            var field = await _context.Field
                .Include(f => f.ContentType)
                .FirstOrDefaultAsync(f => f.Id == id);
            
            if(field == null)
            {
                throw new NotFoundException($"Field with id {id} not found.");
            }

            return field.ToFieldDto();
        }

        public async Task<List<FieldDto>> GetFieldsForContentType(int contentTypeId)
        {
            var fields = await _context.Field
                .Where(f => f.ContentTypeId == contentTypeId)
                .Include(f => f.ContentType)
                .ToListAsync();

            if (fields == null || fields.Count == 0)
            {
                throw new NotFoundException($"No fields found for content type with id {contentTypeId}.");
            }

            return fields.Select(f => f.ToFieldDto()).ToList();
        }

        public async Task<FieldDto> CreateField(CreateFieldRequestDto createFieldDto)
        {
            var field = createFieldDto.ToCreateField();
            
            if (await FieldExists(field.Name, field.ContentTypeId))
            {
                throw new AlreadyExistsException($"A field with the name '{field.Name}' already exists for this content type.");
            }

            await _context.AddAsync(field);
            await _context.SaveChangesAsync();

            return field.ToFieldDto();
        }

        public async Task DeleteField(int id)
        {
            var field = await _context.Field.FindAsync(id);
            if (field == null)
            {
                throw new NotFoundException($"Field with id {id} not found.");
            }
            _context.Field.Remove(field);
            await _context.SaveChangesAsync();
        }

        public async Task<bool> FieldExists(string Name, int CategoryId)
        {
            return await _context.Field.AnyAsync(f => f.Name == Name && f.ContentTypeId == CategoryId);
        }
    }
}