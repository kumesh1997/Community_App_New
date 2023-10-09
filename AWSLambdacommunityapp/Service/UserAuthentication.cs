using Amazon;
using Amazon.CognitoIdentityProvider;
using Amazon.CognitoIdentityProvider.Model;
using Amazon.DynamoDBv2;
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
    public class UserAuthentication
    {
        private readonly DynamoDBContext _dynamoDbContext;
        private readonly AmazonDynamoDBClient _amazonDynamoDBClient;

        private readonly IAmazonCognitoIdentityProvider _cognitoClient;
        private readonly string _userPoolId = "us-east-1_dRHFkZCMr";
        private readonly string _clientId = "7drftsbsv2tm316d72o6ek6422";
        private readonly string _clientSecret = "1q807qber9cpm1blu782jndeo6fg7mck6ncrpdbblq9c31bib6jk";
        private readonly RegionEndpoint _region = RegionEndpoint.USEast1;

        public UserAuthentication()
        {
            // Instance of ConnectToBynamoDB 
            DynamoDB connectToDynamoDB = new DynamoDB();
            _dynamoDbContext = connectToDynamoDB.DBAccessFunction();

            _amazonDynamoDBClient = connectToDynamoDB.AmazonDynamoDBClient();

            var region = RegionEndpoint.USEast1; // e.g., RegionEndpoint.USWest2
            _cognitoClient = new AmazonCognitoIdentityProviderClient(region);
        }


        public async Task<APIGatewayHttpApiV2ProxyResponse> AuthenticationFunctionHandler(APIGatewayHttpApiV2ProxyRequest request, ILambdaContext context)
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
                return await VerifyUser(request);
            }
            else if (httpMethod == "GET" && request.Body == null && request.PathParameters == null)
            {
                return await GetNewAccessToken(request);
            }
            // Handle unsupported or unrecognized HTTP methods
            return new APIGatewayHttpApiV2ProxyResponse { StatusCode = 400 };
        }

        private async Task<APIGatewayHttpApiV2ProxyResponse> VerifyUser(
         APIGatewayHttpApiV2ProxyRequest request)
        {
            try
            {
                var userDto = JsonSerializer.Deserialize<ConfirmEmailDto>(request.Body);
                var provider = new AmazonCognitoIdentityProviderClient(new AmazonCognitoIdentityProviderConfig
                {
                    RegionEndpoint = _region
                });

                var confirmRequest = new ConfirmSignUpRequest
                {
                    ClientId = _clientId,
                    Username = userDto.Email,
                    ConfirmationCode = userDto.Code
                };

                var confirmResponse = await provider.ConfirmSignUpAsync(confirmRequest);

                // Check if confirmation was successful
                if (confirmResponse.HttpStatusCode == System.Net.HttpStatusCode.OK)
                {
                    // User registration confirmed successfully. Now, obtain tokens.
                    var authenticationRequest = new InitiateAuthRequest
                    {
                        AuthFlow = AuthFlowType.USER_PASSWORD_AUTH,
                        ClientId = _clientId,
                        AuthParameters = new Dictionary<string, string>
                    {
                        { "USERNAME", userDto.Email },
                        { "PASSWORD", userDto.Password }, // Replace with the user's password
                    }
                    };



                    var authenticationResponse = await provider.InitiateAuthAsync(authenticationRequest);

                    // Check if authentication was successful
                    if (authenticationResponse.HttpStatusCode == System.Net.HttpStatusCode.OK)
                    {
                        var accessToken = authenticationResponse.AuthenticationResult.AccessToken;
                        var refreshToken = authenticationResponse.AuthenticationResult.RefreshToken;

                        return new APIGatewayHttpApiV2ProxyResponse
                        {
                            StatusCode = 200,
                            Body = $"User registration confirmed successfully. Access Token: {accessToken}, Refresh Token: {refreshToken}",
                        };
                    }
                    else
                    {
                        return new APIGatewayHttpApiV2ProxyResponse
                        {
                            StatusCode = 400,
                            Body = "User authentication failed.",
                        };
                    }
                }
                else
                {
                    return new APIGatewayHttpApiV2ProxyResponse
                    {
                        StatusCode = 400,
                        Body = "User registration confirmation failed.",
                    };
                }
            }
            catch (Exception ex)
            {
                return new APIGatewayHttpApiV2ProxyResponse
                {
                    Body = ex.Message,
                    StatusCode = 500
                };
            }
        }

        // Refresh Token
        private async Task<APIGatewayHttpApiV2ProxyResponse> GetNewAccessToken(
         APIGatewayHttpApiV2ProxyRequest request)
        {
            try
            {
                // Extract the refresh token from the "Authorization" header
                var authorizationHeader = request.Headers["refresh_token"];
                var refreshToken = authorizationHeader?.Replace("Bearer ", ""); // Assuming "Bearer" prefix is used

                if (string.IsNullOrEmpty(refreshToken))
                {
                    return new APIGatewayHttpApiV2ProxyResponse
                    {
                        StatusCode = 400,
                        Body = "Refresh token not provided in the Authorization header.",
                    };
                }

                var provider = new AmazonCognitoIdentityProviderClient(new AmazonCognitoIdentityProviderConfig
                {
                    RegionEndpoint = _region
                });

                var tokenRequest = new InitiateAuthRequest
                {
                    AuthFlow = AuthFlowType.REFRESH_TOKEN_AUTH,
                    ClientId = _clientId,
                    AuthParameters = new Dictionary<string, string>
                {
                    { "REFRESH_TOKEN", refreshToken }
                }
                };

                var tokenResponse = await provider.InitiateAuthAsync(tokenRequest);

                if (tokenResponse.HttpStatusCode == System.Net.HttpStatusCode.OK)
                {
                    var accessToken = tokenResponse.AuthenticationResult.AccessToken;
                    var refresh_Token = tokenResponse.AuthenticationResult.RefreshToken;

                    return new APIGatewayHttpApiV2ProxyResponse
                    {
                        StatusCode = 200,
                        Body = $"New access token and refresh token generated successfully. Access Token: {accessToken}, Refresh Token: {refreshToken}",
                    };
                }
                else
                {
                    return new APIGatewayHttpApiV2ProxyResponse
                    {
                        StatusCode = 400,
                        Body = "Failed to generate new tokens using the provided refresh token.",
                    };
                }
            }
            catch (Exception ex)
            {
                return new APIGatewayHttpApiV2ProxyResponse
                {
                    StatusCode = 500,
                    Body = "Error generating new tokens: " + ex.Message,
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
