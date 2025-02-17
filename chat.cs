using System.IO;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

class Chat
{
    static async Task Main()
    {
        var client = new HttpClient();
        var apiUrl = "https://127.0.0.1:11434/api/chat";

        var requestPayload = new
        {
            model = "your-model-name", // 替换为实际的模型名称
            question = "你好，Ollama!"
        };

        var jsonPayload = JsonConvert.SerializeObject(requestPayload);
        var content = new StringContent(jsonPayload, Encoding.UTF8, "application/json");

        var response = await client.PostAsync(apiUrl, content);
        var responseString = await response.Content.ReadAsStringAsync();

        System.Console.WriteLine(responseString);
    }
}