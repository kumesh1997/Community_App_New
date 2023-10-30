using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AWSLambdacommunityapp.Dto
{
    public class MQTTRequest
    {
        public string UserID { get; set; }
        public string Topic { get; set; }
    }
}
