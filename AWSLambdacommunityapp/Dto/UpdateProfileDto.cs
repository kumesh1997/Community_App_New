using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AWSLambdacommunityapp.Dto
{
    public class UpdateProfileDto
    {
        public string Id { get; set; }
        public string FullName { get; set; }
        public string Phone_Number { get; set; }
    }
}
