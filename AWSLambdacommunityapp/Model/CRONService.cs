using Amazon.DynamoDBv2.DataModel;
using Amazon.Lambda.APIGatewayEvents;
using Amazon.Lambda.Core;
using AWSLambdacommunityapp.Dto;
using AWSLambdacommunityapp.Service;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AWSLambdacommunityapp.Model
{
    public class CRONService
    {
        // Reference to DynamoDBContext
        private readonly DynamoDBContext _dynamoDbContext;


        public CRONService()
        {
            // Instance of ConnectToBynamoDB 
            DynamoDB connectToDynamoDB = new DynamoDB();
            _dynamoDbContext = connectToDynamoDB.DBAccessFunction();
        }


        public async Task<APIGatewayHttpApiV2ProxyResponse> CRONServiceFunctionHandler(APIGatewayHttpApiV2ProxyRequest request, ILambdaContext context)
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


            if (httpMethod == "GET" && request.Body == null && request.PathParameters != null)
            {
                return await HandleGetBookingRequest(request);
            }
            else if (httpMethod == "PUT" && request.Body != null && request.PathParameters == null)
            {
                return await HandleUpdateBookingRequest(request);
            }
            // Handle unsupported or unrecognized HTTP methods
            return new APIGatewayHttpApiV2ProxyResponse { StatusCode = 400 };
        }

        // Get Booking List
        private async Task<APIGatewayHttpApiV2ProxyResponse> HandleGetBookingRequest(
           APIGatewayHttpApiV2ProxyRequest request)
        {
            try
            {
                // get the parameter value
                request.PathParameters.TryGetValue("Id", out var Id);
                var booking_List = await _dynamoDbContext.ScanAsync<CRON>(default).GetRemainingAsync();
                var filteredList = booking_List.Where(v => v.CondoId == Id).ToList();

                return new APIGatewayHttpApiV2ProxyResponse()
                {
                    Body = System.Text.Json.JsonSerializer.Serialize(filteredList),
                    StatusCode = 200
                };
            }
            catch (Exception ex)
            {
                return new APIGatewayHttpApiV2ProxyResponse()
                {
                    Body = ex.Message,
                    StatusCode = 404
                };
            }
        }

        // Upadate Booking
        private async Task<APIGatewayHttpApiV2ProxyResponse> HandleUpdateBookingRequest(
           APIGatewayHttpApiV2ProxyRequest request)
        {
            try
            {
                // Get the Data Comming from Request
                var UpdateCronDto = System.Text.Json.JsonSerializer.Deserialize<UpdateCRONDto>(request.Body);
                var cron = await _dynamoDbContext.LoadAsync<CRON>(UpdateCronDto.Id.ToString());

                cron.NumberOfUsers = cron.NumberOfUsers + UpdateCronDto.NumberOfUsers;
                if (cron.NumberOfUsers <= cron.Capacity)
                {
                    await _dynamoDbContext.SaveAsync(cron);

                    return OkResponse();
                }
                else
                {
                    return new APIGatewayHttpApiV2ProxyResponse()
                    {
                        Body = "Out of Capacity",
                        StatusCode = 404
                    };
                }
                
            }
            catch (Exception ex)
            {
                return new APIGatewayHttpApiV2ProxyResponse()
                {
                    Body = ex.Message,
                    StatusCode = 404
                };
            }
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
