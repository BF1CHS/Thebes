using System.Net;
using System.Net.Sockets;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Unicode;

namespace EchoService
{
    class Program
    {
        static bool IsPortAvailable(int port)
        {
            try
            {
                using (var tcpClient = new TcpClient())
                {
                    tcpClient.Connect("localhost", port);
                    return false;
                }
            }
            catch (SocketException)
            {
                return true;
            }
        }

        static void PrettyPrint(string message, ConsoleColor level = ConsoleColor.White, bool newline = true)
        {
            Console.ForegroundColor = level;
            Console.Write(message);
            if (newline)
            {
                Console.WriteLine();
            }
            Console.ResetColor();
        }

        static void Main(string[] args)
        {
            Console.OutputEncoding = System.Text.Encoding.Default;

            PrettyPrint("正在启动 BF1CHS 本地回显服务器...\n", ConsoleColor.Yellow);

            if (!IsPortAvailable(5000))
            {
                PrettyPrint("端口 5000 已被占用，请关闭占用该端口的应用程序。", ConsoleColor.Red);
                return;
            }

            using (var listener = new HttpListener())
            {
                listener.Prefixes.Add("http://localhost:5000/");
                listener.Start();

                PrettyPrint("服务器已启动，监听地址：http://localhost:5000/ \n", ConsoleColor.Green);

                while (true)
                {
                    try
                    {
                        var context = listener.GetContext();
                        var request = context.Request;
                        var response = context.Response;

                        if (request.HttpMethod == "POST" && request.Url.AbsolutePath == "/echo")
                        {
                            using (var reader = new StreamReader(request.InputStream, request.ContentEncoding))
                            {
                                // String dataStr = reader.ReadToEnd().Replace("\\\\", "\\").Replace("\\\"", "\"");
                                String dataStr = reader.ReadToEnd();
                                PrettyPrint("接收到的数据: ", ConsoleColor.White, false);
                                PrettyPrint(dataStr, ConsoleColor.Yellow);
                                PrettyPrint("长度: ", ConsoleColor.White, false);
                                PrettyPrint(dataStr.Length.ToString(), ConsoleColor.Yellow);

                                var data = JsonSerializer.Deserialize<object>(dataStr);
                                var dt = DateTime.Now.ToString("yyyy-MM-dd-HH-mm-ss");
                                var fileName = $"BF1CHS-{dt}.json";
                                var formattedJson = JsonSerializer.Serialize(data, new JsonSerializerOptions
                                {
                                    Encoder = JavaScriptEncoder.Create(UnicodeRanges.All),
                                    WriteIndented = true
                                });
                                File.WriteAllText(fileName, formattedJson);

                                PrettyPrint($"数据已保存至 {fileName}", ConsoleColor.Green);
                            }
                            response.StatusCode = 200;
                        }
                        else
                        {
                            response.StatusCode = 404;
                        }

                        response.Close();
                    }
                    catch (Exception ex)
                    {
                        PrettyPrint("发生异常: ", ConsoleColor.Red, false);
                        PrettyPrint(ex.Message, ConsoleColor.Yellow);
                    }
                    PrettyPrint("");
                }
            }
        }
    }
}
