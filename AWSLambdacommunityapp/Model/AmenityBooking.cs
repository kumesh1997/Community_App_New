using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AWSLambdacommunityapp.Model
{
    public class AmenityBooking
    {
        public string Id { get; set; }
        public string AmenityTypeId { get; set; }
        public int BookingCount { get; set; }
        public int Time { get; set; }

        public int Requested_Time { get; set; }
        public  string Booking_Status { get; set; }
        public string UserId { get; set; }

        public int Updated_Time { get; set; }
    }
}
