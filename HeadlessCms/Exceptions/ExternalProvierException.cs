using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace HeadlessCms.Exceptions
{
    public class ExternalProvierException : Exception
    {
        public ExternalProvierException(string provider, string message) 
        : base($"External login provider: {provider} error occured: {message}")
        {
            
        }
    }
}