using Amazon.IotData;
using Amazon.Lambda.APIGatewayEvents;
using Amazon.Lambda.Core;
using MQTTnet;
using MQTTnet.Client;
using Amazon.IotData.Model;


namespace AWSLambdacommunityapp.Service
{
    public class MQTTService
    {
        private IMqttClient _mqttClient;
        //private readonly ILogger _logger;

       /* public MQTTService()
        {
            // Create an MQTT client.
            var factory = new MqttFactory();
            _mqttClient = factory.CreateMqttClient();

            // Configure MQTT client options.
            var options = new MqttClientOptionsBuilder()
                .WithTcpServer("220.247.224.226", 1883) // Replace with your MQTT broker's address and port
                .WithClientId(Guid.NewGuid().ToString())
                .Build();

            _mqttClient.ConnectAsync(options).Wait(); // Wait for the connection to be established.
        }*/

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

            if (httpMethod == "GET" && request.Body == null && request.PathParameters != null)
            {
                return await HandleGetListRequest(request);
            }
            // Handle unsupported or unrecognized HTTP methods
            return new APIGatewayHttpApiV2ProxyResponse { StatusCode = 400 };
        }

        private async Task<APIGatewayHttpApiV2ProxyResponse> HandleGetListRequest(
           APIGatewayHttpApiV2ProxyRequest request)
        {
            request.PathParameters.TryGetValue("Id", out var Id);
            try
            {
                // MQTT topic and message to publish.
                string topic = "sos/alarm";
                string message = Id.ToString();
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
