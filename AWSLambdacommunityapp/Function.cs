using Amazon.DynamoDBv2.DataModel;
using Amazon.DynamoDBv2;
using Amazon.Lambda.APIGatewayEvents;
using Amazon.Lambda.Core;
using AWSLambdacommunityapp.Model;
using AWSLambdacommunityapp.Service;

// Assembly attribute to enable the Lambda function's JSON input to be converted into a .NET class.
[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.SystemTextJson.DefaultLambdaJsonSerializer))]

namespace AWSLambdacommunityapp;

public class Function
{
    // Reference for Help Desk Service
    private readonly HelpDeskService helpDeskService;
    // Reference for Visitor Service
    private readonly VisitorService visitorService;
    // Reference for Amenities Service
    private readonly AmenitiesService amenitiesService;
    // Reference for User Management Service
    private readonly UserManagementService userManagementService;
    // Reference for Condominium Service
    private readonly CondominiumService condominiumService;
    // Reference for Module Service
    private readonly ModuleService moduleService;
    // Reference for Admin Service
    private readonly AdminSevice adminSevice;
    // Reference for MQTT Service
    private readonly MQTTService mqttService;
    // Referece For User Authentication
    private readonly UserAuthentication authentication;
    // Reference For User Sign In Service
    private readonly SignInService signInService;
    // Reference For Event Service
    private readonly EventServices eventServices;
    // Reference For Amenity Type Service
    private readonly AmenityTypeService amenityTypeService;
    public Function()
    {
        // New Help Desk Service Instance
        helpDeskService = new HelpDeskService();

        // New Visitor Service Instance
        visitorService = new VisitorService();

        // New Amenities Service Instance
        amenitiesService = new AmenitiesService();

        // New User Management Service Instance
        userManagementService = new UserManagementService();

        // New Condominium Service Instance
        condominiumService = new CondominiumService();

        // New Module Service Instance
        moduleService = new ModuleService();

        // New Admin Service Instance
        adminSevice = new AdminSevice();

        // New MQTT Service Instance
        mqttService = new MQTTService();

        // New User Authentication Service Instance
        authentication = new UserAuthentication();

        // New User Sign In Service Instnace
        signInService = new SignInService();

        // New Event Service Instance
        eventServices = new EventServices();

        // New Amenity Type Service Instance
        amenityTypeService = new AmenityTypeService();
    }

   public async Task<APIGatewayHttpApiV2ProxyResponse> HelpDeskHandler(APIGatewayHttpApiV2ProxyRequest request, ILambdaContext context)
    {
        return await helpDeskService.HelpDeskFunctionHandler(request, context);
    }

    public async Task<APIGatewayHttpApiV2ProxyResponse> VisitorHandler(APIGatewayHttpApiV2ProxyRequest request, ILambdaContext context)
    {
        return await visitorService.VisitorFunctionHandler(request, context);
    }

    public async Task<APIGatewayHttpApiV2ProxyResponse> AmenitiesHandler(APIGatewayHttpApiV2ProxyRequest request, ILambdaContext context)
    {
        return await amenitiesService.AmenitiesFunctionHandler(request, context);
    }


    public async Task<APIGatewayHttpApiV2ProxyResponse> UserHandler(APIGatewayHttpApiV2ProxyRequest request, ILambdaContext context)
    {
        return await userManagementService.VisitorFunctionHandler(request, context);
    }

    public async Task<APIGatewayHttpApiV2ProxyResponse> CondoHandler(APIGatewayHttpApiV2ProxyRequest request, ILambdaContext context)
    {
        return await condominiumService.CondominiumFunctionHandler(request, context);
    }

    public async Task<APIGatewayHttpApiV2ProxyResponse> ModuleHandler(APIGatewayHttpApiV2ProxyRequest request, ILambdaContext context)
    {
        return await moduleService.ModuleFunctionHandler(request, context);
    }

    public async Task<APIGatewayHttpApiV2ProxyResponse> AdminHandler(APIGatewayHttpApiV2ProxyRequest request, ILambdaContext context)
    {
        return await adminSevice.AdminFunctionHandler(request, context);
    }

    public async Task<APIGatewayHttpApiV2ProxyResponse> MQQTHandler(APIGatewayHttpApiV2ProxyRequest request, ILambdaContext context)
    {
        return await mqttService.MQTTFunctionHandler(request, context);
    }

    public async Task<APIGatewayHttpApiV2ProxyResponse> AuthHandler(APIGatewayHttpApiV2ProxyRequest request, ILambdaContext context)
    {
        return await authentication.AuthenticationFunctionHandler(request, context);
    }

    public async Task<APIGatewayHttpApiV2ProxyResponse> SignInHandler(APIGatewayHttpApiV2ProxyRequest request, ILambdaContext context)
    {
        return await signInService.SignInFunctionHandler(request, context);
    }

    public async Task<APIGatewayHttpApiV2ProxyResponse> EventHandler(APIGatewayHttpApiV2ProxyRequest request, ILambdaContext context)
    {
        return await eventServices.EventFunctionHandler(request, context);
    }

    public async Task<APIGatewayHttpApiV2ProxyResponse> AmenityTypeHandler(APIGatewayHttpApiV2ProxyRequest request, ILambdaContext context)
    {
        return await amenityTypeService.AmenityTypeFunctionHandler(request, context);
    }
}
