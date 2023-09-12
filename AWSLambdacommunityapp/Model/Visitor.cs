using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AWSLambdacommunityapp.Model
{
    public class Visitor
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string MobileNumber { get; set; }
        public int NumberOfVisitors { get; set; }
        public string VisitorVehicleNumber { get; set; }
        public int From { get; set; }
        public int To { get; set; }
        public string Purpose { get; set; }
        public string UserId { get; set; }
        public bool IsApproved { get; set; } = false;
    }
}
