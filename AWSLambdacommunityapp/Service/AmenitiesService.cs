using Amazon.DynamoDBv2.DataModel;
using Amazon.DynamoDBv2.Model;
using Amazon.Lambda.APIGatewayEvents;
using Amazon.Lambda.Core;
using AWSLambdacommunityapp.Dto;
using AWSLambdacommunityapp.Model;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace AWSLambdacommunityapp.Service
{
    public class AmenitiesService
    {
        // Reference to DynamoDBContext
        private readonly DynamoDBContext _dynamoDbContext;
        // Reference to S3Bucket
        private readonly S3BucketService _bucketService;

        public AmenitiesService()
        {
            // Instance of ConnectToBynamoDB 
            DynamoDB connectToDynamoDB = new DynamoDB();
            _dynamoDbContext = connectToDynamoDB.DBAccessFunction();

            // Instance of S3BucketService
            _bucketService = new S3BucketService();

        }


        public async Task<APIGatewayHttpApiV2ProxyResponse> AmenitiesFunctionHandler(APIGatewayHttpApiV2ProxyRequest request, ILambdaContext context)
        {
            string httpMethod = request.RequestContext.Http.Method.ToUpper();
            Uri requestUri = new Uri(request.RequestContext.Http.Path);

            
            if (httpMethod == "POST" && request.Body != null && request.PathParameters != null)
            {
                /*if (requestUri.AbsolutePath == "/amenity/booking")
                {
                    return await HandleBookingRequest(request); 
                }*/
                return await HandleBookingRequest(request);
            }
            else if (httpMethod == "POST" && request.Body != null && request.PathParameters == null)
            {
                //return await HandlePostRequest(request);
                return await HandleBookingRequest(request);
            }
            else if (httpMethod == "POST" && request.Body == null && request.PathParameters == null)
            {
                return await HandleDailyInitialBookingRequest(request);
            }

            // Handle unsupported or unrecognized HTTP methods
            return new APIGatewayHttpApiV2ProxyResponse { StatusCode = 400 };
        }

        // Add New Amenities
        private async Task<APIGatewayHttpApiV2ProxyResponse> HandlePostRequest(
           APIGatewayHttpApiV2ProxyRequest request)
        {
            var amenity = System.Text.Json.JsonSerializer.Deserialize<AmenitiesDto>(request.Body);
            Amenities newAmenity = new Amenities();
            // Auto Generate ID
            newAmenity.Id = GenerateId();
            newAmenity.AmenityType = amenity.AmenityType;
            newAmenity.MaximumCapacityCount = amenity.MaximumCapacityCount;
            newAmenity.AmenityLocation = amenity.AmenityLocation;
            newAmenity.MultimediaInfomation = _bucketService.UploadImageAndGetUrl(amenity.Image64Base, amenity.ImageName);
            await _dynamoDbContext.SaveAsync(newAmenity);
            return OkResponse();
        }


        // Initial Booking for every day
         private async Task<APIGatewayHttpApiV2ProxyResponse> HandleDailyInitialBookingRequest(
           APIGatewayHttpApiV2ProxyRequest request)
         {
            //var amenityBooking = JsonSerializer.Deserialize<AmenityBooking>(request.Body);
            // Get all Amenity Types
            var amenityTypesList = await _dynamoDbContext.ScanAsync<Amenities>(default).GetRemainingAsync();
            foreach (var item in amenityTypesList)
            {
                AmenityBooking amenityBooking = new AmenityBooking();
                amenityBooking.Id = GenerateId();
                amenityBooking.BookingCount = 0;
                amenityBooking.AttendeesList = null;
                amenityBooking.IsFull = false;
                amenityBooking.AmenityTypeId = item.Id;
                amenityBooking.Time = GetCurrentEpochValue();
                await _dynamoDbContext.SaveAsync(amenityBooking);
            }
             return OkResponse();
         }


        // New Booking for a selected date
        private async Task<APIGatewayHttpApiV2ProxyResponse> HandleBookingRequest(
           APIGatewayHttpApiV2ProxyRequest request)
        {
            // Get the Data Comming from Request
            var amenityBookingDto = System.Text.Json.JsonSerializer.Deserialize<AmenityBookingDto>(request.Body);
            // Get a List of Amenities available
            var amenityList = await _dynamoDbContext.ScanAsync<Amenities>(default).GetRemainingAsync();
            // Get a List of Booking
            var amenityBookingList = await _dynamoDbContext.ScanAsync<AmenityBooking>(default).GetRemainingAsync();
            // Check the availability of Booking
            var availability_response = CheckAvailabilityRequest(amenityBookingDto.FromTime, amenityBookingDto.AmenityTypeId, amenityBookingDto.NumberofBookingByTheUser, amenityList, amenityBookingList);
            if (availability_response.amenitybooking != null)
            {
                // add new amenity use count
                availability_response.amenitybooking.BookingCount = availability_response.amenitybooking.BookingCount + amenityBookingDto.NumberofBookingByTheUser;
                if (availability_response.capacity == availability_response.amenitybooking.BookingCount)
                {
                    // make the isfull flag true
                    availability_response.amenitybooking.IsFull = true;
                    await _dynamoDbContext.SaveAsync(availability_response.amenitybooking);
                    return new APIGatewayHttpApiV2ProxyResponse()
                    {
                        Body = JsonConvert.SerializeObject(availability_response.amenitybooking),
                        StatusCode = 200
                    };
                }else if (availability_response.capacity < availability_response.amenitybooking.BookingCount)
                {
                    // revert the amenity user count 
                    availability_response.amenitybooking.BookingCount = availability_response.amenitybooking.BookingCount - amenityBookingDto.NumberofBookingByTheUser;
                    return new APIGatewayHttpApiV2ProxyResponse()
                    {
                        Body = "The Amenity is Full !!!",
                        StatusCode = 503
                    };
                }
                
            }
            else
            {
                return new APIGatewayHttpApiV2ProxyResponse()
                {
                    Body = "The Amenity is Full !!!",
                    StatusCode = 503
                };
            }
            return new APIGatewayHttpApiV2ProxyResponse()
            {
                Body = "Amenity not Found !!!",
                StatusCode = 503
            };
        }


            // Check Availability
            public AvailabilityResponse CheckAvailabilityRequest(int time, string amenityTypeId, int count, List<Amenities> amenityList, List<AmenityBooking> amenityBookingList)
        {
            //var amenityList = await _dynamoDbContext.ScanAsync<Amenities>(default).GetRemainingAsync();
            int amenityCapacity = 0;
            foreach (var item in amenityList)
            {
                if (item.Id == amenityTypeId)
                {
                    amenityCapacity = item.MaximumCapacityCount;
                }
            }
            // Get the Specific Time slot comming from the user
            var fromTime = time;
            var toTime = CalculateNextEpochAfter30Minutes(time);
            //  Get the Amenity Type ID
            // Find the specific booking details of that amenity type in that time slot
            //var amenityBookingList = await _dynamoDbContext.ScanAsync<AmenityBooking>(default).GetRemainingAsync();
            // Find the booking
            var booking = FindBookingByTimeRange(amenityBookingList, fromTime, toTime, amenityTypeId);
            if (booking != null && booking.IsFull != true)
            {
                if (amenityCapacity > booking.BookingCount)
                {
                    return new AvailabilityResponse() {
                        amenitybooking = booking,
                        capacity = amenityCapacity
                    };
                }
                else
                {

                    return new AvailabilityResponse()
                    {
                        amenitybooking = null,
                        capacity = amenityCapacity
                    };
                }
            }
            // return the response
            return new AvailabilityResponse()
            {
                amenitybooking = null,
                capacity = amenityCapacity
            };
        }

        public AmenityBooking FindBookingByTimeRange(List<AmenityBooking> bookings, int startTime, int endTime, string amenityTypeId)
        {
            // Use LINQ to find the first booking that falls within the time range
            AmenityBooking matchingBooking = bookings
                .FirstOrDefault(booking => (booking.Time <= startTime || booking.Time == startTime ) && booking.AmenityTypeId == amenityTypeId);
            return matchingBooking;
        }


        public static DateTime ConvertEpochToDateTime(int epochValue)
        {
            // Unix epoch starts from January 1, 1970 (UTC)
            DateTime epochStart = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);

            // Add the epoch value in seconds to the epoch start time
            DateTime result = epochStart.AddSeconds(epochValue);

            return result;
        }


        public static int CalculateNextEpochAfter30Minutes(int currentEpoch)
        {
            // Convert the current epoch value to a DateTime object
            DateTime currentDateTime = DateTimeOffset.FromUnixTimeSeconds(currentEpoch).DateTime;

            // Add 30 minutes to the current time
            DateTime nextTime = currentDateTime.AddMinutes(30);

            // Convert the next time to Unix timestamp (epoch)
            int nextEpoch = (int)(nextTime - new DateTime(1970, 1, 1)).TotalSeconds;

            return nextEpoch;
        }

        private int GetCurrentEpochValue()
        {
            // Calculate the epoch value for the current date
            return (int)new DateTimeOffset(DateTime.UtcNow).ToUnixTimeSeconds();
        }


        // Autogenerate ID
        public string GenerateId()
        {
            Guid guid = Guid.NewGuid();
            string id = guid.ToString();
            return id;
        }

        // OK Response
        private static APIGatewayHttpApiV2ProxyResponse OkResponse() =>
            new APIGatewayHttpApiV2ProxyResponse()
            {
                StatusCode = 200
            };

        // Bad Response
        private static APIGatewayHttpApiV2ProxyResponse BadResponse(string message)
        {
            return new APIGatewayHttpApiV2ProxyResponse()
            {
                Body = message,
                StatusCode = 404
            };
        }
    }
}
