namespace ReoLinkApiSharp.Handlers;

public static class RestHandler
{
    public static void Post(string Url, List<Dictionary<string, string>> data,
        Dictionary<string, string> parameters = null)
    {
        try
        {
            var header = new Dictionary<string, string>();
            header.Add("content-type","application/json");
            var r = // TODO: https://github.com/ReolinkCameraAPI/reolinkapipy/blob/8b6049bb2617e0efdc74e72a1f39789de1aaa48d/reolinkapi/handlers/rest_handler.py#L20
        }
    }
}