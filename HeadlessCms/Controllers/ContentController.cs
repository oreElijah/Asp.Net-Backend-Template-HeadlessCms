using HeadlessCms.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using HeadlessCms.Dtos.Content;

namespace HeadlessCms.Controllers
{
    [Route("api/{version:apiVersion}/content/")]
    [ApiController]
    public class ContentController : ControllerBase
    {
        private readonly IContentValueService _cvService;
        public ContentController(IContentValueService cvService)
        {
            _cvService = cvService;
        }

        [ApiVersion("1.0")]
        [HttpGet("Entry/{EntryId}")]
        public async Task<IActionResult> GetAllContentValues(int EntryId)
        {
            var contentValues = await _cvService.GetContentValues(EntryId);

            if(contentValues == null)
            {
                return NotFound($"No content values found for entry with id.");
            }

            return Ok(contentValues);
        }

        [ApiVersion("1.0")]
        [HttpGet("{id}")]
        public async Task<IActionResult> GetContentValueById(int id)
        {
            var contentValue = await _cvService.GetExistingContentValue(id);
            if (contentValue == null)
            {
                return NotFound($"Content value with id {id} not found.");
            }
            return Ok(contentValue);
        }

        [ApiVersion("1.0")]
        [HttpPost("{ContentTypeId}")]
        public async Task<IActionResult> CreateContentValue(int ContentTypeId, [FromBody] ContentValueRequestDto contentValueDto)
        {
            var contentValue = await _cvService.CreateNewContentValue(ContentTypeId, contentValueDto);
            if (contentValue == null)
            {
                return BadRequest("Failed to create content value.");
            }
            return Ok(contentValue);
        }
    }
}
