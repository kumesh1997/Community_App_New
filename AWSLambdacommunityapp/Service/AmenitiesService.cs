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
                return await HandleBookingRequest(request);
            }
            else if (httpMethod == "GET" && request.Body == null && request.PathParameters == null)
            {
               return await HandleGetBookingListRequest(request);
            }
            else if (httpMethod == "PUT" && request.Body != null && request.PathParameters == null)
            {
                return await HandleUpdateBookingRequest(request);
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


        // New Booking for a selected date
        private async Task<APIGatewayHttpApiV2ProxyResponse> HandleBookingRequest(
           APIGatewayHttpApiV2ProxyRequest request)
        {
            try
            {
                // Get the Data Comming from Request
                var amenityBookingDto = System.Text.Json.JsonSerializer.Deserialize<AmenityBookingDto>(request.Body);
                AmenityBooking amenityBooking = new AmenityBooking();
                amenityBooking.Id = GenerateId();
                amenityBooking.AmenityTypeId = amenityBookingDto.AmenityTypeId;
                amenityBooking.Time = GetCurrentEpoch();
                amenityBooking.Requested_Time = amenityBookingDto.FromTime;
                amenityBooking.BookingCount = amenityBookingDto.NumberofBookingByTheUser;
                amenityBooking.UserId = amenityBookingDto.AttendeeId;
                amenityBooking.Booking_Status = "opened";

                await _dynamoDbContext.SaveAsync(amenityBooking);
                return new APIGatewayHttpApiV2ProxyResponse()
                {
                    Body = "Booking Completed !!!",
                    StatusCode = 200
                };
            }
            catch (Exception ex)
            {
                return new APIGatewayHttpApiV2ProxyResponse()
                {
                    Body = ex.Message,
                    StatusCode = 503
                };
            }
            
            return new APIGatewayHttpApiV2ProxyResponse()
            {
                Body = "Booking Failed !!!",
                StatusCode = 503
            };
        }

        // Get A List of Booking
        private async Task<APIGatewayHttpApiV2ProxyResponse> HandleGetBookingListRequest(
           APIGatewayHttpApiV2ProxyRequest request)
        {
            try
            {
                var booking_List = await _dynamoDbContext.ScanAsync<AmenityBooking>(default).GetRemainingAsync();
                // Filter Booking Where Status is not Accepted
                var filteredList = booking_List.Where(v => v.Booking_Status.ToLower() != "accepted" || v.Booking_Status.ToLower() != "rejected").ToList();
                if (filteredList != null && filteredList.Count > 0)
                {
                    return new APIGatewayHttpApiV2ProxyResponse()
                    {
                        Body = System.Text.Json.JsonSerializer.Serialize(filteredList),
                        StatusCode = 200
                    };
                }
            }
            catch (Exception ex)
            {
                return new APIGatewayHttpApiV2ProxyResponse()
                {
                    Body = ex.Message,
                    StatusCode = 503
                };

            }
            return new APIGatewayHttpApiV2ProxyResponse()
            {
                Body = "Booking Not Found !!!",
                StatusCode = 503
            };
        }

        // Update Booking
        private async Task<APIGatewayHttpApiV2ProxyResponse> HandleUpdateBookingRequest(
       APIGatewayHttpApiV2ProxyRequest request)
        {
            try
            {
                // Get the Data Comming from Request
                var updatedBooking = System.Text.Json.JsonSerializer.Deserialize<UpdateBooking>(request.Body);
                try
                {
                    // Find Booking List 
                    var booking = await _dynamoDbContext.LoadAsync<AmenityBooking>(updatedBooking.Booking_Id);
                    // Update Status
                    booking.Booking_Status = updatedBooking.Status.ToLower();
                    booking.Updated_Time = GetCurrentEpoch();

                    await _dynamoDbContext.SaveAsync(booking);
                    return new APIGatewayHttpApiV2ProxyResponse()
                    {
                        Body = "Booking Updated !!! ",
                        StatusCode = 200
                    };
                }
                catch(Exception ex)
                {
                    return new APIGatewayHttpApiV2ProxyResponse()
                    {
                        Body = "Booking was not Found !!! ",
                        StatusCode = 503
                    };
                }
                

            }
            catch (Exception ex)
            {
                return new APIGatewayHttpApiV2ProxyResponse()
                {
                    Body = ex.Message,
                    StatusCode = 503
                };
            }
            return new APIGatewayHttpApiV2ProxyResponse()
            {
                Body = "Booking Was Not Updated !!!",
                StatusCode = 503
            };
        }

        // Get Current Epoch
        public static int GetCurrentEpoch()
        {
            DateTimeOffset epochStart = new DateTimeOffset(1970, 1, 1, 0, 0, 0, TimeSpan.Zero);
            TimeSpan currentTime = DateTimeOffset.UtcNow - epochStart;
            return (int)currentTime.TotalSeconds;
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
