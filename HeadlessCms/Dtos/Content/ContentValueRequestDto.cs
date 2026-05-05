using System.ComponentModel.DataAnnotations;

namespace HeadlessCms.Dtos.Content
{
    public class ContentValueRequestDto
    {
        [Required]
        public Dictionary<string, string> Data { get; set; } = new Dictionary<string, string>();
    
    }
}
