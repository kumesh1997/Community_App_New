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
    public class VisitorService
    {
        // Reference to DynamoDBContext
        private readonly DynamoDBContext _dynamoDbContext;

        public VisitorService()
        {
            // Instance of ConnectToBynamoDB 
            DynamoDB connectToDynamoDB = new DynamoDB();
            _dynamoDbContext = connectToDynamoDB.DBAccessFunction();

        }

        public async Task<APIGatewayHttpApiV2ProxyResponse> VisitorFunctionHandler(APIGatewayHttpApiV2ProxyRequest request, ILambdaContext context)
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




            if (httpMethod == "GET" && request.Body == null && request.PathParameters == null)
            {
                return await HandleGetListRequest(request);
            }
            else if (httpMethod == "GET" && request.PathParameters != null)
            {
                return await HandleGetRequest(request);
            }
            else if (httpMethod == "GET" && request.Body != null && request.PathParameters == null)
            {
                return await HandleGetVisitorListBetweenTwoDaysRequest(request);
            }
            else if (httpMethod == "POST")
            {
                return await HandlePostRequest(request);
            }
            else if (httpMethod == "DELETE")
            {
                return await HandleDeleteRequest(request);
            }
            else if (httpMethod == "PUT")
            {
                return await HandleUpdateRequest(request);
            }

            // Handle unsupported or unrecognized HTTP methods
            return new APIGatewayHttpApiV2ProxyResponse { StatusCode = 400 };
        }

        // Insert Visitor
        private async Task<APIGatewayHttpApiV2ProxyResponse> HandlePostRequest(
           APIGatewayHttpApiV2ProxyRequest request)
        {
            var visitor = JsonSerializer.Deserialize<Visitor>(request.Body);
            // Auto Generate ID
            visitor.Id = GenerateId();
            await _dynamoDbContext.SaveAsync(visitor);
            return OkResponse();
        }



        // Get Visitor of a Specific User
        private async Task<APIGatewayHttpApiV2ProxyResponse> HandleGetRequest(
           APIGatewayHttpApiV2ProxyRequest request)
        {
            request.PathParameters.TryGetValue("Id", out var Id);
            if (Id != null)

            {
                //var visitor = await _dynamoDbContext.LoadAsync<Visitor>(Id);
                // Filter the Visitors List Based on User Id
                var visitor = await _dynamoDbContext.ScanAsync<Visitor>(default).GetRemainingAsync();
                var filteredVisitors = visitor.Where(v => v.UserId == Id).ToList();
                if (visitor != null)
                {
                    return new APIGatewayHttpApiV2ProxyResponse()
                    {
                        Body = JsonSerializer.Serialize(filteredVisitors),
                        StatusCode = 200
                    };
                }
            }
            return BadResponse("Visitor Not Found !!!");
        }



        // Update Visitor
        private async Task<APIGatewayHttpApiV2ProxyResponse> HandleUpdateRequest(APIGatewayHttpApiV2ProxyRequest request)
        {
            var user = JsonSerializer.Deserialize<Visitor>(request.Body);
            if (user != null)
            {
                var existingUser = await _dynamoDbContext.LoadAsync<Visitor>(user.Id);
                if (existingUser != null)
                {
                    existingUser.Name = user.Name;
                    await _dynamoDbContext.SaveAsync(user);
                    return OkResponse();
                }
            }
            return BadResponse("Visitor was not Updated !!!!");
        }




        // Delete Visitor
        private async Task<APIGatewayHttpApiV2ProxyResponse> HandleDeleteRequest(
            APIGatewayHttpApiV2ProxyRequest request)
        {
            request.PathParameters.TryGetValue("Id", out var Id);
            if (Id != null)
            {
                await _dynamoDbContext.DeleteAsync<Visitor>(Id);
                return OkResponse();
            }
            return BadResponse("Error !!!");
        }


        // Get Visitors List in Last Six Month
        private async Task<APIGatewayHttpApiV2ProxyResponse> HandleGetListRequest(APIGatewayHttpApiV2ProxyRequest request)
        {
            // Calculate the epoch value for the start date (6 months ago)
            var sixMonthsAgo = DateTime.UtcNow.AddMonths(-6);
            var startEpoch = new DateTimeOffset(sixMonthsAgo).ToUnixTimeSeconds();

            // Calculate the epoch value for the current date
            var currentEpoch = new DateTimeOffset(DateTime.UtcNow).ToUnixTimeSeconds();

            // Filter Visitors
            var visitor = await _dynamoDbContext.ScanAsync<Visitor>(default).GetRemainingAsync();
            var filteredVisitors = visitor.Where(v => v.From >= startEpoch && v.From <= currentEpoch).ToList();

            if (visitor != null && visitor.Count > 0)
            {
                return new APIGatewayHttpApiV2ProxyResponse()
                {
                    Body = JsonSerializer.Serialize(filteredVisitors),
                    StatusCode = 200
                };
            }
            return BadResponse("No Visitors Found !!!!");
        }


        // Get Visitors List Between Two Days
        private async Task<APIGatewayHttpApiV2ProxyResponse> HandleGetVisitorListBetweenTwoDaysRequest(APIGatewayHttpApiV2ProxyRequest request)
        {

            var visitorDateDetails = JsonSerializer.Deserialize<VisitorDateDto>(request.Body);
            // Filter Visitors whare the From epoch value between the specified two epoch values
            var visitor = await _dynamoDbContext.ScanAsync<Visitor>(default).GetRemainingAsync();
            var filteredVisitors = visitor.Where(v => v.From >= visitorDateDetails.StartingDate && v.From <= visitorDateDetails.EndingDate).ToList();

            if (visitor != null && visitor.Count > 0)
            {
                return new APIGatewayHttpApiV2ProxyResponse()
                {
                    Body = JsonSerializer.Serialize(filteredVisitors),
                    StatusCode = 200
                };
            }
            return BadResponse("No Visitors Found !!!!");
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
