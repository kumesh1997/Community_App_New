using AWSLambdacommunityapp.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AWSLambdacommunityapp.Dto
{
    public class AvailabilityResponse
    {
        public AmenityBooking amenitybooking { get; set; }
        public int capacity { get; set; }
    }
}
