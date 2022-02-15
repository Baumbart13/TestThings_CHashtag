namespace NetMQ_DesktopClient;

public class DirectoryConfguration
{
    public string InputDirectory { get; private set; }

    public string OutputDirectory { get; private set; }

    private DirectoryConfguration(string input, string output)
    {
        InputDirectory = input;
        OutputDirectory = output;
    }

    public DirectoryConfguration CreateFromConfig(string filepath = "dirs.config")
    {
        if (!File.Exists(filepath))
        {
            throw new FileNotFoundException("Config-file for Server not found!");
        }

        var lines = File.ReadAllLines(filepath);
        foreach (var line in lines)
        {
            
        }
    }
}