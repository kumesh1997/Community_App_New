using Amazon.DynamoDBv2.DataModel;
using Amazon.Lambda.APIGatewayEvents;
using Amazon.Lambda.Core;
using AWSLambdacommunityapp.Dto;
using AWSLambdacommunityapp.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace AWSLambdacommunityapp.Service
{
    public class HelpDeskService
    {
        private readonly DynamoDBContext _dynamoDbContext;
        private readonly S3BucketService _bucketService;

        public HelpDeskService()
        {
            // Instance of ConnectToBynamoDB 
            DynamoDB connectToDynamoDB = new DynamoDB();
            _dynamoDbContext = connectToDynamoDB.DBAccessFunction();

            _bucketService = new S3BucketService();
        }


        public async Task<APIGatewayHttpApiV2ProxyResponse> HelpDeskFunctionHandler(APIGatewayHttpApiV2ProxyRequest request, ILambdaContext context)
        {
            string httpMethod = request.RequestContext.Http.Method.ToUpper();

            if (httpMethod == "OPTIONS")
            {
                return new APIGatewayHttpApiV2ProxyResponse
                {
                    StatusCode = 200,
                    Headers = new Dictionary<string, string>
                {
                    { "Access-Control-Allow-Origin", "http://localhost:3000" },
                    { "Access-Control-Allow-Headers", "Content-Type" },
                    { "Access-Control-Allow-Methods", "OPTIONS,POST,GET" },
                    { "Access-Control-Allow-Credentials", "true" },
                },
                };
            }


            if (httpMethod == "POST")
            {
                return await HandlePostRequest(request);
            }
            else if (httpMethod == "PUT")
            {
                return await HandleUpdateRequest(request);
            }
            else if (httpMethod == "GET" && request.Body == null && request.PathParameters == null)
            {
                return await HandleGetListRequest(request);
            }
            else if (httpMethod == "GET" && request.Body == null && request.PathParameters != null)
            {
                return await HandleGetSpecificHelpdeskRequest(request);
            }


            // Handle unsupported or unrecognized HTTP methods
            return new APIGatewayHttpApiV2ProxyResponse { StatusCode = 400 };
        }

        // Save a Help Desk
        private async Task<APIGatewayHttpApiV2ProxyResponse> HandlePostRequest(APIGatewayHttpApiV2ProxyRequest request)
        {
            // Get the Array of Help Desk Requests
            var helpDesks = JsonSerializer.Deserialize<List<HelpDeskDto>>(request.Body);
            if (helpDesks == null)
            {
                return BadResponse("Invalid Visitor details");
            }
            // Iterate each Request
            foreach (var helpDeskDTO in helpDesks)
            {
                HelpDesk newHD = new HelpDesk();
                newHD.UserId = helpDeskDTO.UserId;
                newHD.Description = helpDeskDTO.Description;
                newHD.Status = helpDeskDTO.Status;
                newHD.Fault = helpDeskDTO.Fault;
                newHD.FaultLocation = helpDeskDTO.FaultLocation;
                // Autogenerate an ID
                newHD.Id = GenerateId();
                // Set the Current Time Stamp
                newHD.LastUpdate = GetCurrentEpochValue();
                // Set the Created date of the Helpdesk Request
                newHD.CreatedDate = GetCurrentEpochValue();
                newHD.Image = _bucketService.UploadImageAndGetUrl(helpDeskDTO.Image64Base, helpDeskDTO.ImageName);
                if (newHD == null)
                {
                    return BadResponse("Invalid Request !!!");
                }
                else if (newHD.Image == null)
                {
                    return BadResponse("Image Size is Too Large !!!");
                }
                await _dynamoDbContext.SaveAsync(newHD);
            }
            return OkResponse();
        }


        // Update Help Desk
        private async Task<APIGatewayHttpApiV2ProxyResponse> HandleUpdateRequest(APIGatewayHttpApiV2ProxyRequest request)
        {
            var helpdesk = JsonSerializer.Deserialize<HelpDesk>(request.Body);
            if (helpdesk != null)
            {
                var existingHelpDeskRequest = await _dynamoDbContext.LoadAsync<HelpDesk>(helpdesk.Id);
                if (existingHelpDeskRequest != null)
                {
                    // Update the Status
                    existingHelpDeskRequest.Status = helpdesk.Status;
                    // Update the Last Updated Date
                    existingHelpDeskRequest.LastUpdate = GetCurrentEpochValue();
                    // Save the Updated Helpdesk Request
                    await _dynamoDbContext.SaveAsync(existingHelpDeskRequest);
                    return OkResponse();
                }
            }
            return BadResponse("Helpdesk Request was not Updated !!!!");
        }

        public async Task<APIGatewayHttpApiV2ProxyResponse> HandleGetListRequest(APIGatewayHttpApiV2ProxyRequest request)
        {
            // List of Help Desk Requests
            var requestsList = await _dynamoDbContext.ScanAsync<HelpDesk>(default).GetRemainingAsync();
            if (requestsList != null && requestsList.Count > 0)
            {
                return new APIGatewayHttpApiV2ProxyResponse()
                {
                    // The counted value of each category is included in the response body 
                    Body = JsonSerializer.Serialize(CountRequestsByStatus(requestsList)),
                    StatusCode = 200
                };
            }
            return BadResponse("No Help Desk Requests Found !!!!");
        }


        // Get HelpDesk of a Specific User
        private async Task<APIGatewayHttpApiV2ProxyResponse> HandleGetSpecificHelpdeskRequest(
           APIGatewayHttpApiV2ProxyRequest request)
        {
            request.PathParameters.TryGetValue("Id", out var Id);
            if (Id != null)

            {
                //var visitor = await _dynamoDbContext.LoadAsync<Visitor>(Id);
                // Filter the Visitors List Based on User Id
                var helpdesk = await _dynamoDbContext.ScanAsync<HelpDesk>(default).GetRemainingAsync();
                var filteredHelpDesk = helpdesk.Where(v => v.UserId == Id).ToList();
                if (filteredHelpDesk != null)
                {
                    return new APIGatewayHttpApiV2ProxyResponse()
                    {
                        Body = JsonSerializer.Serialize(filteredHelpDesk),
                        StatusCode = 200
                    };
                }
            }
            return BadResponse("Visitor Not Found !!!");
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


        // Autogenerate ID
        public string GenerateId()
        {
            Guid guid = Guid.NewGuid();
            string id = guid.ToString();
            return id;
        }

        // Count By Status Category
        private Dictionary<string, int> CountRequestsByStatus(List<HelpDesk> helpDesks)
        {
            var statusCounts = helpDesks
                .Where(h => h.Status != null) // Filter out records with null Status
                .GroupBy(h => h.Status)
                .ToDictionary(g => g.Key, g => g.Count());

            return statusCounts;
        }

        private int GetCurrentEpochValue()
        {
            // Calculate the epoch value for the current date
            return (int)new DateTimeOffset(DateTime.UtcNow).ToUnixTimeSeconds();
        }
    }
}
