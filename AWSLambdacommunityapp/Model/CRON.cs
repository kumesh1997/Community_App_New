using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AWSLambdacommunityapp.Model
{
    public class CRON
    {
        public string Id { get; set; }
        public string AmenityId { get; set; }
        public string AmenityName { get; set; }
        public string Date { get; set; }
        public string TimeSlot { get; set; }
        public int Capacity { get; set; }
        public int NumberOfUsers { get; set; }
        public string CondoId { get; set; }

    }
}
