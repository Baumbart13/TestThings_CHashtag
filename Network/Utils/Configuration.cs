using System.Net;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Network.Utils;

public class Configuration
{
    [JsonIgnore]
    public static IPAddress DefaultIpAddress => new IPAddress(new byte[] { 142, 132, 224, 12 });
    [JsonIgnore]
    public const uint DefaultPort = 42555;
    [JsonIgnore]
    public static string DefaultInputPath => "";
    [JsonIgnore]
    public static string DefaultOutputPath => "";
    [JsonIgnore]
    public const string ConfigFileName = "artemis.config";

    private Configuration()
    {
    }

    [JsonIgnore(Condition = JsonIgnoreCondition.Always)]
    public IPAddress IpAddress { get; private set; } = DefaultIpAddress;

    [JsonPropertyName("IPAddress")]
    [JsonInclude]
    private string _ipAddressAsString
    {
        get { return this.IpAddress.ToString(); }
    }

    public uint Port { get; private set; } = DefaultPort;

    public static Configuration ReadFromFile(string directoryPath = "./")
    {
        foreach (var file in Directory.GetFiles(directoryPath))
        {
            Console.WriteLine(file);
        }

        return new Configuration();
    }

    public string ToJsonString()
    {
        var opts = new JsonSerializerOptions
        {
            MaxDepth = 1,
            WriteIndented = true,
            AllowTrailingCommas = false,
            IncludeFields = true
        };
        return JsonSerializer.Serialize( this, opts);
    }

    public static Configuration ParseJson(string json)
    {
        return JsonSerializer.Deserialize<Configuration>(json) ?? new Configuration();
    }
}