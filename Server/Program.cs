using System;
using System.Net.Http;
using System.Text;
using ModifyColors;
using NetMQ;
using NetMQ.Sockets;
using Network;
using NetworkConstants;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

static class Program
{
    private const string FILENAME = "NetMQ.png";
    private const string SAVE_PATH = $"/root/diplomarbeit/foto_saves/{FILENAME}";

    public static void Main()
    {
        Console.WriteLine("Creating server instance");
        using var responder = new ResponseSocket();
        Constants.WriteLocalIpToFile();
        responder.Bind($"tcp://*:{Constants.ServerCredentials.Port}");
        Console.WriteLine($"Bound address to TCP on Port {Constants.ServerCredentials.Port}");

        while (true)
        {
            Console.WriteLine("Waiting for message");
            
            //VersionStrBuilder(responder);
            VersionArtemisMessage(responder);
        }
    }
    public static async void SendMessage(string title, string msg, string webhook)
    {
        try
        {
            DateTime now = DateTime.Now;
            string[] strArray = new string[20];
            strArray[0] = "{\"username\":\"Netzwerk-Server-Log\",\"avatar_url\":\"https://discord.com/channels/707008612207296562/717408941881163827/862005945549586512\",\"content\":\"\",\"embeds\":[{\"author\":{\"name\":\" \",\"url\":\"https://discord.gg/rvUxcnZEqz\",\"icon_url\":\"https://discord.com/channels/707008612207296562/717408941881163827/862005945549586512\"},\"title\":\"" + title + "\",\"thumbnail\":{\"url\":\"https://discord.com/channels/707008612207296562/717408941881163827/862005945549586512\"},\"url\":\"https://discord.gg/rvUxcnZEqz\",\"description\":\"**";
            int num = now.Day;
            strArray[1] = num.ToString();
            strArray[2] = ".";
            num = now.Month;
            strArray[3] = num.ToString();
            strArray[4] = ".";
            num = now.Year;
            strArray[5] = num.ToString();
            strArray[6] = " | ";
            num = now.Hour;
            strArray[7] = num.ToString();
            strArray[8] = ":";
            num = now.Minute;
            strArray[9] = num.ToString();
            strArray[10] = "**\",\"color\":1127128,\"fields\":[{\"name\":\"";
            strArray[11] = title;
            strArray[12] = "\",\"value\":\"";
            strArray[13] = msg;
            strArray[14] = "\",\"inline\":true}]}]}";
            string stringPayload = string.Concat(strArray);
            StringContent httpContent = new StringContent(stringPayload, Encoding.UTF8, "application/json");
            using (HttpClient httpClient = new HttpClient())
            {
                HttpResponseMessage httpResponseMessage = await httpClient.PostAsync(webhook, (HttpContent)httpContent);
            }
            stringPayload = (string)null;
            httpContent = (StringContent)null;
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
        }
    }


    private static void VersionArtemisMessage(ResponseSocket responder)
    {
        var netMqMsg = responder.ReceiveMultipartMessage(3);
        var artemisMsg = Network.Message.FromNetMqMessage(netMqMsg);
        // Decode the image and save it
        Console.WriteLine($"[{DateTime.Now} {DateTime.Now}]: Received message from Client");
        //  Decode it
        var responseMsg = $"Received Image at [{DateTime.Now} {DateTime.Now.Millisecond}]\n";

        var img = artemisMsg.ReadImage();
        var colFormat = ImageManipulator.GetColorFormat(img) == ColorFormat.Rgba32 ? "Rgba32" : "Grayscale";
        Console.WriteLine($"ColorFormat is {colFormat}");
        SendMessage("LOG",$"ColorFormat is {colFormat}","https://discord.com/api/webhooks/945307267874586714/sfprelH1jWXhYfdCNwUmjAh7g-SAHrRT3eJ4MUcOEVauV549QTWgb5BmIVfsALpHI-Tq");
        var Width = img.Width;
        var Height = img.Height;
        Console.WriteLine($"Image has a size of {Width}x{Height} pixels");
        SendMessage("LOG",$"Image has a size of {Width}x{Height} pixels","https://discord.com/api/webhooks/945307267874586714/sfprelH1jWXhYfdCNwUmjAh7g-SAHrRT3eJ4MUcOEVauV549QTWgb5BmIVfsALpHI-Tq");

        responseMsg += $"Decoded Image at [{DateTime.Now}]\n";
            
        // save it
        img.SaveAsPng(SAVE_PATH);
        responseMsg += $"Saved Image at [{DateTime.Now}] as \"{SAVE_PATH}\"";

        responder.SendFrame(responseMsg);
    }

    private static void VersionStrBuilder(ResponseSocket responder)
    {
        var str = responder.ReceiveFrameString();
        var strSeparated = str.Split(';');
        // Decode the image and save it
        Console.WriteLine($"[{DateTime.Now} {DateTime.Now}]: Received message from Client");
        SendMessage("LOG",$"[{DateTime.Now} {DateTime.Now}]: Received message from Client","https://discord.com/api/webhooks/945307267874586714/sfprelH1jWXhYfdCNwUmjAh7g-SAHrRT3eJ4MUcOEVauV549QTWgb5BmIVfsALpHI-Tq");

        //  Decode it
        var responseMsg = $"Received Image at [{DateTime.Now} {DateTime.Now.Millisecond}]\n";

        var currArgFromMsg = 0;
        var colFormat = (ColorFormat)Convert.ToInt32(strSeparated[currArgFromMsg++]);
        Console.WriteLine($"ColorFormat is {nameof(colFormat)}");
        SendMessage("LOG",$"ColorFormat is {nameof(colFormat)}","https://discord.com/api/webhooks/945307267874586714/sfprelH1jWXhYfdCNwUmjAh7g-SAHrRT3eJ4MUcOEVauV549QTWgb5BmIVfsALpHI-Tq");

        var Width = Convert.ToInt32(strSeparated[currArgFromMsg++]);
        var Height = Convert.ToInt32(strSeparated[currArgFromMsg++]);
        Console.WriteLine($"Image has a size of {Width}x{Height} pixels");
        SendMessage("LOG",$"Image has a size of {Width}x{Height} pixels","https://discord.com/api/webhooks/945307267874586714/sfprelH1jWXhYfdCNwUmjAh7g-SAHrRT3eJ4MUcOEVauV549QTWgb5BmIVfsALpHI-Tq");

        using (var img = new Image<Rgba32>(Width, Height))
        {
            try
            {
                for (var x = 0; x < Width; ++x)
                {
                    for (var y = 0; y < Height; ++y)
                    {
                        var r = Convert.ToByte(strSeparated[currArgFromMsg++]);
                        var g = Convert.ToByte(strSeparated[currArgFromMsg++]);
                        var b = Convert.ToByte(strSeparated[currArgFromMsg++]);
                        var newP = new Rgba32(r, g, b);
                        img[x, y] = newP;
                    }
                }

                responseMsg += $"Decoded Image at [{DateTime.Now}]\n";

                // save it
                img.SaveAsPng(SAVE_PATH);
                responseMsg += $"Saved Image at [{DateTime.Now}] as \"{FILENAME}\"";
            }
            catch (Exception e)
            {
                Console.Error.WriteLine(e.Message);
                SendMessage("ERROR",e.Message,"https://discord.com/api/webhooks/945307267874586714/sfprelH1jWXhYfdCNwUmjAh7g-SAHrRT3eJ4MUcOEVauV549QTWgb5BmIVfsALpHI-Tq");

                responder.SendFrame(e.Message);
                img.Dispose();
                return;
            }
        }

        responder.SendFrame(responseMsg);
    }
}