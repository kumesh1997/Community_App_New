using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AWSLambdacommunityapp.Model
{
    public class HelpDesk
    {
        public string Id { get; set; }
        public string UserId { get; set; }
        public string Fault { get; set; }
        public string Description { get; set; }
        public string FaultLocation { get; set; }
        // Unattended, Open, Hold, In Progress, Closed
        public string Status { get; set; }
        public int LastUpdate { get; set; }
        public int CreatedDate { get; set; }
        public string Image { get; set; }
    }
}
