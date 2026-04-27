using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using HeadlessCms.Models;

namespace HeadlessCms.Interfaces
{
    public interface IEmailService
    {
        public Task SendVerifyUserEmail(string userName, string email, string verify_token);
        public Task VerifyEmail(string email, string token);
    }
}