// 访问API实现chat功能

using System.IO;
using System.Net.Http;
using System.Threading.Tasks;

class Chat
{
    static async Task Main()
    {
        var client = new HttpClient();
        var response = await client.GetAsync("https://api.github.com/repos/dotnet/corefx/issues");
        var stream = await response.Content.ReadAsStreamAsync();
        using (var reader = new StreamReader(stream))
        {
            System.Console.WriteLine(await reader.ReadToEndAsync());
        }
    }
}