using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using HeadlessCms.Interfaces;

namespace HeadlessCms.Services
{
    //Strategy Design Pattern for handling different field types
    public class StringHandler : IFieldHandler
    {
        public void Parse(string value)
        {
            // String values don't need parsing, so this method can be left empty
        }
    }

    public class IntHandler : IFieldHandler
    {
        public void Parse(string value)
        {
            if (!int.TryParse(value, out _))
                      
            throw new FormatException($"Unable to parse '{value}' as an integer.");
        }
    }

    public class BoolHandler : IFieldHandler
    {
        public void Parse(string value)
        {
            if (!bool.TryParse(value, out _))
          
            throw new FormatException($"Unable to parse '{value}' as a boolean.");
        }
    }

    public class DateHandler : IFieldHandler
    {
        public void Parse(string value)
        {
            if (!DateTime.TryParse(value, out _))
            
            throw new FormatException($"Unable to parse '{value}' as a date.");
        }
    }

}