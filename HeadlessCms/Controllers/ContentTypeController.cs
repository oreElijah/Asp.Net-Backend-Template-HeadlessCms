using HeadlessCms.Interfaces;
using HeadlessCms.Dtos.Content;
using HeadlessCms.Dtos.Field;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using HeadlessCms.Models;

namespace HeadlessCms.Controllers
{
    [Route("api/{version:apiVersion}/")]  
    [ApiController]

    public class ContentTypeController : ControllerBase
    {
        private readonly IContentTypeService _ctService;
        private readonly IFieldService _fService;

        public ContentTypeController(IContentTypeService ctService, IFieldService fService)
        {
            _ctService = ctService;
            _fService = fService;
        }

        [ApiVersion("1.0")]
        [HttpGet("content-types")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetAllContentType()
        {
            var contentTypes = await _ctService.GetContentTypes();
            if (contentTypes == null || !contentTypes.Any())
            {
                return NotFound("No content types found.");
            }
            return Ok(contentTypes);
        }

        [ApiVersion("1.0")]
        [HttpGet("content-types/{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetContentTypeById(int id)
        {
            var contentType = await _ctService.GetUniqueContentType(id);
            if (contentType == null)
            {
                return NotFound($"Content type with id {id} not found.");
            }

            return Ok(contentType);
        }

        [ApiVersion("1.0")]
        [HttpPost("content-types")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> CreateContentType([FromBody] CreateContentTypeRequestDto createContentTypeDto)
        {
            var createdContentType = await _ctService.CreateNewContentType(createContentTypeDto);
            if (createdContentType == null)
            {
                return NotFound("Failed to create content type.");
            }
            return CreatedAtAction(nameof(GetContentTypeById), new { id = createdContentType.Id, version = "1.0" }, createdContentType);
        }

        [ApiVersion("1.0")]
        [HttpDelete("content-types/{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteContentType(int id)
        {
            await _ctService.DeleteExistingContentType(id);
            return NoContent();
        }

        [ApiVersion("1.0")]
        [HttpGet("fields")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetAllFields()
        {
            var fields = await _fService.GetFields();
            if (fields == null || !fields.Any())
            {
                return NotFound("No fields found.");
            }
            return Ok(fields);
        }

        [ApiVersion("1.0")]
        [HttpGet("fields/{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetFieldById(int id)
        {
            var field = await _fService.GetUniqueField(id);
            if (field == null)
            {
                return NotFound($"Field with id {id} not found.");
            }
            return Ok(field);
        }

        [ApiVersion("1.0")]
        [HttpPost("{content_Id}/fields")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> CreateField([FromRoute] int content_Id, [FromBody] CreateFieldRequestDto createFieldDto)
        {
            createFieldDto.ContentTypeId = content_Id;

            var createdField = await _fService.CreateNewField(createFieldDto);
            if (createdField == null)
            {
                return NotFound("Failed to create field.");
            }
            return CreatedAtAction(nameof(GetFieldById), new { id = createdField.Id, version = "1.0" }, createdField);
        }

        [ApiVersion("1.0")]
        [HttpDelete("fields/{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteField(int id)
        {
            await _fService.DeleteExistingField(id);
            return NoContent();
        }
    }
}
