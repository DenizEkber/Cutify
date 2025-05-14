using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cutify.Services.Helper
{
    public static class VerificationHelper
    {
        public static string GenerateCode()
        {
            var random = new Random();
            return random.Next(100000, 999999).ToString(); // 6 rəqəmli kod
        }
    }

}
