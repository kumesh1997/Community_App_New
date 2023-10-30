using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AWSLambdacommunityapp.Dto
{
    public class UpdateEventDto
    {
        public string EventId { get; set; }
        public bool Event_Status { get; set; }
    }
}
