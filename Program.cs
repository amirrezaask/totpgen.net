using OtpNet;
using System.Text.Json;


switch (args.Length)
{
    case 0:
        PrintCodes();
        break;
    case 1:
        HandleCommand(args[0]);
        break;
    default:
        Console.WriteLine("Invalid arguments.");
        break;
}


static void HandleCommand(string cmd)
{
    switch (cmd)
    {
        case "edit":
            EditConfig();
            break;
        default:
            Console.WriteLine("Unknown command.");
            break;
    }
}

static void EditConfig()
{
    var homeDirectory = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
    var configFilePath = Path.Combine(homeDirectory, "totp.json");

    var editor = Environment.GetEnvironmentVariable("EDITOR") ?? "notepad";
    var process = new System.Diagnostics.Process
    {
        StartInfo = new System.Diagnostics.ProcessStartInfo
        {
            FileName = editor,
            Arguments = configFilePath,
            RedirectStandardOutput = false,
            UseShellExecute = true,
            CreateNoWindow = false,
        }
    };
    process.Start();
    process.WaitForExit();
}


static void PrintCodes()
{
    // read configuration from json config in home directory
    var homeDirectory = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
    var configFilePath = Path.Combine(homeDirectory, "totp.json");

    if (!File.Exists(configFilePath))
    {
        Console.WriteLine("Configuration file not found.");
        var defaultConfig = new Dictionary<string, string>
    {
        { "Github", "LHTKLTVRBECV4DVH" }
    };

        string defaultJsonString = JsonSerializer.Serialize(defaultConfig, new JsonSerializerOptions { WriteIndented = true });
        File.WriteAllText(configFilePath, defaultJsonString);
        Console.WriteLine("Default configuration file created.");
        return;
    }
    string jsonString = File.ReadAllText(configFilePath);
    var config = JsonSerializer.Deserialize<Dictionary<string, string>>(jsonString);

    foreach (var kvp in config!)
    {
        byte[] secretKey = Base32Encoding.ToBytes(kvp.Value);
        Totp totp = new Totp(secretKey);
        Console.WriteLine($"{kvp.Key} => {totp.ComputeTotp()}");
    }

}
