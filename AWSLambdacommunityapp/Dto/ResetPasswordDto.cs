using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AWSLambdacommunityapp.Dto
{
    public class ResetPasswordDto
    {
        public string Code { get; set; }
        public string Password { get; set; }
    }
}
