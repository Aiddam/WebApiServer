using Server.Models.Enum;

public class Request
{
    public string Path { get; init; }
    public string ControllerName { get; init; }
    public HttpActionType HttpMethod { get; init; }
    public string MethodName { get; init; }
    public object[]? Data { get; init; }
    public string? DataFromPost { get; set; }
    public Request(string path, string controllerName, HttpActionType httpMethod, string methodName, object[]? data = null, string? dataFromPost = null)
    {
        Path = path;
        ControllerName = controllerName;
        HttpMethod = httpMethod;
        MethodName = methodName;
        Data = data;
        DataFromPost = dataFromPost;
    }
}