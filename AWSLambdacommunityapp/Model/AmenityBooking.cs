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
        public int DailyBookingCount { get; set; }
        public bool IsFull { get; set; } = false;
        public int Date { get; set; }
    }
}
