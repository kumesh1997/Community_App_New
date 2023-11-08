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
        public int NumberofBookingByTheUser { get; set; }

        public int FromTime { get; set; }
        public int ToTime { get; set; }
        public string Tower { get; set; }
        public string Floor { get; set; }
        public string House_Number { get; set; }

    }
}
