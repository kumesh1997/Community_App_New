using Amazon.DynamoDBv2.DataModel;
using Amazon.Lambda.APIGatewayEvents;
using Amazon.Lambda.Core;
using AWSLambdacommunityapp.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace AWSLambdacommunityapp.Service
{
    public class ModuleService
    {
        private readonly DynamoDBContext _dynamoDbContext;

        public ModuleService()
        {
            // Instance of ConnectToBynamoDB 
            DynamoDB connectToDynamoDB = new DynamoDB();
            _dynamoDbContext = connectToDynamoDB.DBAccessFunction();
        }

        public async Task<APIGatewayHttpApiV2ProxyResponse> ModuleFunctionHandler(APIGatewayHttpApiV2ProxyRequest request, ILambdaContext context)
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


            if (httpMethod == "POST" && request.Body != null && request.PathParameters == null)
            {
               return await HandleAddRequest(request);
            }
            else if (httpMethod == "GET" && request.Body == null && request.PathParameters == null)
            {
               return await HandleGetRequest(request);
            }
            // Handle unsupported or unrecognized HTTP methods
            return new APIGatewayHttpApiV2ProxyResponse { StatusCode = 400 };
        }


        // Add Modules
        private async Task<APIGatewayHttpApiV2ProxyResponse> HandleAddRequest(
          APIGatewayHttpApiV2ProxyRequest request)
        {
            var module = JsonSerializer.Deserialize<Module>(request.Body);
            try
            {
                module.Module_Id = GenerateCondoId();
                await _dynamoDbContext.SaveAsync(module);
                return OkResponse();
            }
            catch (Exception ex)
            {
                return BadResponse("Module was not Add !!! " + ex.Message);
            }

        }

        // Get Modules
        private async Task<APIGatewayHttpApiV2ProxyResponse> HandleGetRequest(APIGatewayHttpApiV2ProxyRequest request)
        {
            try
            {
                // Get Modules
                var moduleList = await _dynamoDbContext.ScanAsync<Module>(default).GetRemainingAsync();
                return new APIGatewayHttpApiV2ProxyResponse
                {
                    Body = JsonSerializer.Serialize(moduleList),
                    StatusCode = 200
                };
            }
            catch (Exception ex)
            {
                return BadResponse(" Exception " + ex.Message);
            }
        }

        private int nextId = 1;

        public string GenerateCondoId()
        {
            // Format the integer part as a three-digit number with leading zeros
            string formattedId = nextId.ToString("D3");

            // Combine "CONDO" with the formatted integer
            string condoId = "MOD" + formattedId;

            // Increment the nextId for the next ID
            nextId++;

            return condoId;
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
