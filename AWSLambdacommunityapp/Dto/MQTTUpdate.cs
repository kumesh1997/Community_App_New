using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AWSLambdacommunityapp.Dto
{
    public class MQTTUpdate
    {
        public string Id { get; set; }
        public string Status { get; set; }
        public string Comment { get; set; }
    }
}
