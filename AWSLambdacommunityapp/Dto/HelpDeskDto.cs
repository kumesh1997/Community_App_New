using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AWSLambdacommunityapp.Dto
{
    public class HelpDeskDto
    {
        public string UserId { get; set; }
        public string Fault { get; set; }
        public string Description { get; set; }
        public string FaultLocation { get; set; }
        // Unattended, Open, Hold, In Progress, Closed
        public string Status { get; set; }
        public string Image64Base { get; set; }
        public string ImageName { get; set; }
    }
}
