using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AWSLambdacommunityapp.Model
{
    public class Amenities
    {
        public string Id { get; set; }
        public string AmenityType { get; set; }
        //Club House /Swimming Pool /Gymnasium /Event /Party Space

        public string MultimediaInfomation { get; set; }
        //Text /Photos /Videos

        public int MaximumCapacityCount { get; set; }
        public string AmenityLocation { get; set; }
    }
}
