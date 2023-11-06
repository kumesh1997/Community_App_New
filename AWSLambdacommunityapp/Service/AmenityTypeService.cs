using Amazon.DynamoDBv2.DataModel;
using Amazon.Lambda.APIGatewayEvents;
using Amazon.Lambda.Core;
using AWSLambdacommunityapp.Dto;
using AWSLambdacommunityapp.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AWSLambdacommunityapp.Service
{
    public class AmenityTypeService
    {
        // Reference to DynamoDBContext
        private readonly DynamoDBContext _dynamoDbContext;
        // Reference to S3Bucket
        private readonly S3BucketService _bucketService;

        public AmenityTypeService() {
            // Instance of ConnectToBynamoDB 
            DynamoDB connectToDynamoDB = new DynamoDB();
            _dynamoDbContext = connectToDynamoDB.DBAccessFunction();

            // Instance of S3BucketService
            _bucketService = new S3BucketService();
        }

        public async Task<APIGatewayHttpApiV2ProxyResponse> AmenityTypeFunctionHandler(APIGatewayHttpApiV2ProxyRequest request, ILambdaContext context)
        {
            string httpMethod = request.RequestContext.Http.Method.ToUpper();
            Uri requestUri = new Uri(request.RequestContext.Http.Path);


            if (httpMethod == "POST" && request.Body != null && request.PathParameters == null)
            {        
                return await HandlePostRequest(request);
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
            newAmenity.Condo_ID = amenity.Condo_Id;
            newAmenity.MultimediaInfomation = _bucketService.UploadImageAndGetUrl(amenity.Image64Base, amenity.ImageName);
            await _dynamoDbContext.SaveAsync(newAmenity);
            return OkResponse();
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
