using HeadlessCms.Dtos.Content;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace HeadlessCms.Interfaces
{
    public interface ICmsFacade
    {

       public Task CreateContentTypeWithFields(CreateContentTypeWithFieldsDto dto);

    }
}