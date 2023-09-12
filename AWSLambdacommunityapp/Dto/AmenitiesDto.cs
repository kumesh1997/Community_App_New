using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AWSLambdacommunityapp.Dto
{
    public class AmenitiesDto
    {
        public string AmenityType { get; set; }
        //Club House /Swimming Pool /Gymnasium /Event /Party Space
        public int MaximumCapacityCount { get; set; }
        public string AmenityLocation { get; set; }
        public string Image64Base { get; set; }
        public string ImageName { get; set; }
    }
}
