using HeadlessCms.Dtos.Field;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace HeadlessCms.Dtos.Content
{
    public class CreateContentTypeWithFieldsDto
    {
        public string Name { get; set; } = string.Empty;
        public List<CreateFieldWithContentTypeDto> Fields { get; set; } = new List<CreateFieldWithContentTypeDto>();
    }
}