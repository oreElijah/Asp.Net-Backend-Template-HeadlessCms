using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace HeadlessCms.Interfaces
{
    public interface IFieldHandler
    {
        public void Parse(string value); 
    }
}