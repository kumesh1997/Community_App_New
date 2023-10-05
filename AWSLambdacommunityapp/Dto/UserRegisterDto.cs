using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AWSLambdacommunityapp.Dto
{
    internal class UserRegisterDto
    {
        public string UserId { get; set; }
        public string Password { get; set; }
        public string FullName { get; set; }
        public string NIC { get; set; }
        public string Tower { get; set; }
        public string Floor { get; set; }
        public string House_Number { get; set; }
        public string Phone_Number { get; set; }
    }
}
