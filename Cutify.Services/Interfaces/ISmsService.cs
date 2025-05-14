using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cutify.Services.Interfaces
{
    public interface ISmsService
    {
        Task SendVerificationCodeAsync(string phoneNumber, string code);
    }

}
