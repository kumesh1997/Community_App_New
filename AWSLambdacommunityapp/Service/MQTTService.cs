using Amazon.IotData;
using Amazon.Lambda.APIGatewayEvents;
using Amazon.Lambda.Core;
using MQTTnet;
using MQTTnet.Client;
using Amazon.IotData.Model;
using Amazon.DynamoDBv2.DataModel;
using Amazon.DynamoDBv2;
using Amazon.CognitoIdentityProvider;
using AWSLambdacommunityapp.Model;
using AWSLambdacommunityapp.Dto;

namespace AWSLambdacommunityapp.Service
{
    public class MQTTService
    {
        private IMqttClient _mqttClient;
        private readonly DynamoDBContext _dynamoDbContext;
        private readonly AmazonDynamoDBClient _amazonDynamoDBClient;

        public MQTTService()
        {
            // Instance of ConnectToBynamoDB 
            DynamoDB connectToDynamoDB = new DynamoDB();
            _dynamoDbContext = connectToDynamoDB.DBAccessFunction();
            _amazonDynamoDBClient = connectToDynamoDB.AmazonDynamoDBClient();
        }

        public async Task<APIGatewayHttpApiV2ProxyResponse> MQTTFunctionHandler(APIGatewayHttpApiV2ProxyRequest request, ILambdaContext context)
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

            if (httpMethod == "GET" && request.Body != null && request.PathParameters == null)
            {
                return await HandleGetListRequest(request);
            }
            else if (httpMethod == "PUT" && request.Body != null && request.PathParameters == null)
            {
                return await HandleUpdateRequest(request);
            }
            else if (httpMethod == "GET" && request.Body == null && request.PathParameters == null)
            {
                return await HandleGetRequest(request);
            }
            // Handle unsupported or unrecognized HTTP methods
            return new APIGatewayHttpApiV2ProxyResponse { StatusCode = 400 };
        }

        private async Task<APIGatewayHttpApiV2ProxyResponse> HandleGetListRequest(
           APIGatewayHttpApiV2ProxyRequest request)
        {
            //request.PathParameters.TryGetValue("Id", out var Id);
            var REQ = System.Text.Json.JsonSerializer.Deserialize<MQTTRequest>(request.Body);
            try
            {
                // MQTT topic and message to publish.
                string topic = "sos/alarm";
                string message = REQ.Topic.ToString();
                message = RemoveSpaces(message);

                // Initialize MQTT client if not already initialized.
                if (_mqttClient == null || !_mqttClient.IsConnected)
                {
                    InitializeMqttClient();
                }

                // Create an MQTT message.
                var mqttMessage = new MqttApplicationMessageBuilder()
                    .WithTopic(topic)
                    .WithPayload(message)
                    .WithRetainFlag()
                    .Build();

                // Publish the MQTT message.
                var res = await _mqttClient.PublishAsync(mqttMessage);

               
                if (res.IsSuccess)
                {
                    SOS sOS = new SOS();
                    sOS.Id = GenerateId();
                    sOS.UserId = REQ.UserID;
                    sOS.SOSTime = GetCurrentEpoch();
                    sOS.Status = "opened";

                    await _dynamoDbContext.SaveAsync(sOS);
                    return OkResponse();
                    // Disconnect the MQTT client after publishing.
                    //await _mqttClient.DisconnectAsync();
                }
                else
                {
                    return BadResponse(res.ToString());
                }

            }
            catch (Exception ex)
            {
                return BadResponse(ex.Message);
            }
            //return null;
        }

        // Get the SOS List
        private async Task<APIGatewayHttpApiV2ProxyResponse> HandleGetRequest(
         APIGatewayHttpApiV2ProxyRequest request)
        {
            try
            {
                var sos_List = await _dynamoDbContext.ScanAsync<SOS>(default).GetRemainingAsync();
                // Filter Booking Where Status is not Accepted
                var filteredSOSList = sos_List.Where(v => v.Status.ToLower() != "closed" || v.Status.ToLower() != "inprogress").ToList();
                if (filteredSOSList != null && filteredSOSList.Count > 0)
                {
                    return new APIGatewayHttpApiV2ProxyResponse()
                    {
                        Body = System.Text.Json.JsonSerializer.Serialize(filteredSOSList),
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
                Body = "SOS List not Found !!! ",
                StatusCode = 503
            };
        }
            // Update SOS
            private async Task<APIGatewayHttpApiV2ProxyResponse> HandleUpdateRequest(
          APIGatewayHttpApiV2ProxyRequest request)
        {
            try
            {
                var update = System.Text.Json.JsonSerializer.Deserialize<MQTTUpdate>(request.Body);
                try
                {
                    // Find Booking List 
                    var sos = await _dynamoDbContext.LoadAsync<SOS>(update.Id);
                    // Update Status
                    sos.Status = update.Status;
                    sos.Comment = update.Comment;
                    sos.Updated_Time = GetCurrentEpoch();

                    await _dynamoDbContext.SaveAsync(sos);
                    return new APIGatewayHttpApiV2ProxyResponse()
                    {
                        Body = "SOS Updated !!! ",
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

            }
            catch (Exception ex)
            {
                return BadResponse(ex.Message);
            }
            return new APIGatewayHttpApiV2ProxyResponse()
            {
                Body = "Error",
                StatusCode = 500
            };
        }

            private void InitializeMqttClient()
        {
            // MQTT broker address and port.
            string brokerAddress = "220.247.224.226";
            int brokerPort = 1883;

            // Create an MQTT client.
            var factory = new MqttFactory();
            _mqttClient = factory.CreateMqttClient();

            // Configure MQTT client options.
            var options = new MqttClientOptionsBuilder()
                .WithTcpServer(brokerAddress, brokerPort)
                .WithClientId(Guid.NewGuid().ToString())
                .Build();

            // Connect to the MQTT broker.
            _mqttClient.ConnectAsync(options).Wait(); // Wait for the connection to be established.
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

        // Remove 
        public string RemoveSpaces(string input)
        {
            if (input == null)
            {
                return null;
            }

            return input.Replace(" ", "");
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
