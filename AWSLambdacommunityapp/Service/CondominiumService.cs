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
    public class CondominiumService
    {
        private readonly DynamoDBContext _dynamoDbContext;

        public CondominiumService()
        {
            // Instance of ConnectToBynamoDB 
            DynamoDB connectToDynamoDB = new DynamoDB();
            _dynamoDbContext = connectToDynamoDB.DBAccessFunction();
        }

        public async Task<APIGatewayHttpApiV2ProxyResponse> CondominiumFunctionHandler(APIGatewayHttpApiV2ProxyRequest request, ILambdaContext context)
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
                return await HandleAddCondominiumRequest(request);
            }
            else if (httpMethod == "GET" && request.Body == null && request.PathParameters == null)
            {
                return await HandleGetRequest(request);
            }
            else if (httpMethod == "DELETE" && request.Body == null && request.PathParameters != null)
            {
                return await HandleDeleteRequest(request);
            }
            // Handle unsupported or unrecognized HTTP methods
            return new APIGatewayHttpApiV2ProxyResponse { StatusCode = 400 };
        }

        // Add Conodomenium
        private async Task<APIGatewayHttpApiV2ProxyResponse> HandleAddCondominiumRequest(
           APIGatewayHttpApiV2ProxyRequest request)
        {
            var condo = JsonSerializer.Deserialize<Condominium>(request.Body);
            try
            {
                var condo_List = await _dynamoDbContext.ScanAsync<Condominium>(default).GetRemainingAsync();
                if (condo_List.Count == 0 || condo_List.Count > 0 || condo_List == null)
                {
                    foreach(var i in condo_List)
                    {
                        if (i.Condominium_Description.ToLower() != condo.Condominium_Description.ToLower())
                        {
                            condo.Condominium_Id = GenerateUniqueCondoId();
                            condo.Is_Delete = false;
                            await _dynamoDbContext.SaveAsync(condo);
                            return OkResponse();
                        }
                        else
                        {
                            return BadResponse("Condo Name Can't Duplicated !!! ");
                        }
                    }
                    
                }
                condo.Condominium_Id = GenerateUniqueCondoId();
                condo.Is_Delete = false;
                await _dynamoDbContext.SaveAsync(condo);
                return OkResponse();
            }
            catch (Exception ex)
            {
                return BadResponse("Condo was not Add !!! " + ex.Message);
            }

        }

        // Get Condomenium
        private async Task<APIGatewayHttpApiV2ProxyResponse> HandleGetRequest(APIGatewayHttpApiV2ProxyRequest request)
        {
            try
            {
                // Get Users
                var condoList = await _dynamoDbContext.ScanAsync<Condominium>(default).GetRemainingAsync();
                return new APIGatewayHttpApiV2ProxyResponse
                {
                    Body = JsonSerializer.Serialize(condoList),
                    StatusCode = 200
                };
            }
            catch (Exception ex)
            {
                return BadResponse(" Exception " + ex.Message);
            }
        }


        // Delete Condomenium
        private async Task<APIGatewayHttpApiV2ProxyResponse> HandleDeleteRequest(APIGatewayHttpApiV2ProxyRequest request)
        {
            try
            {
                // get the parameter value
                request.PathParameters.TryGetValue("Id", out var Id);
                // Get Users
                var condoList = await _dynamoDbContext.ScanAsync<Condominium>(default).GetRemainingAsync();
                var selectedCondo = condoList.FirstOrDefault(v => v.Condominium_Id.Equals(Id));

                if (selectedCondo != null)
                {
                    selectedCondo.Is_Delete = true;
                    await _dynamoDbContext.SaveAsync(selectedCondo);
                    return OkResponse();
                }else
                {
                    return BadResponse(" Condominium was not Deleted !!!!");
                }
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
            string condoId = "CONDO" + formattedId;

            // Increment the nextId for the next ID
            nextId++;

            return condoId;
        }

        public string GenerateUniqueCondoId()
        {
            // Use a timestamp as a base for uniqueness
            long timestamp = DateTime.UtcNow.Ticks;

            // Generate a random portion (you can use a random number generator)
            Random random = new Random();
            int randomPart = random.Next(10000); // Adjust the range as needed

            // Combine timestamp and random portion to create the condoId
            string condoId = "CONDO" + timestamp.ToString() + randomPart.ToString("D4");

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
