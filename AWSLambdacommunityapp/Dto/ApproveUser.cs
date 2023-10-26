using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AWSLambdacommunityapp.Dto
{
    public class ApproveUser
    {
        public string UserId { get; set; }
        public string Tower { get; set; }
        public string Floor { get; set; }
        public string House_Number { get; set; }
    }
}
