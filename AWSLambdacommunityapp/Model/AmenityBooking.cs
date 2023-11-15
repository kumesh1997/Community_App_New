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
        public int Booking_Date { get; set; }
        public string Requested_Time_From { get; set; }
        public  string Booking_Status { get; set; }
        public string UserId { get; set; }
        public int Updated_Time { get; set; }
        public string Tower { get; set; }
        public string Floor { get; set; }
        public string House_Number { get; set; }

    }
}
