using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using HeadlessCms.Models;
using HeadlessCms.Interfaces;

namespace HeadlessCms.Services
{
    public static class FieldHandlerFactory
    {
        public static IFieldHandler Create(FieldType Type)
        {
            return Type switch
            {
                FieldType.String => new StringHandler(),
                FieldType.Integer => new IntHandler(),
                FieldType.Boolean => new BoolHandler(),
                FieldType.Date => new DateHandler(),
                _ => throw new NotSupportedException($"Field type '{Type}' is not supported.")
            };
        }
    }
}