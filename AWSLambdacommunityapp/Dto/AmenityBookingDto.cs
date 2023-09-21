using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AWSLambdacommunityapp.Dto
{
    public class AmenityBookingDto
    {
        public string AmenityTypeId { get; set; }
        public string AttendeeId { get; set; }
        //public string NIC { get; set; }
        public int NumberofBookingByTheUser { get; set; }

        public int FromTime { get; set; }
        //public int ToTime { get; set; }

    }
}
