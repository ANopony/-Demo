using System;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Newtonsoft.Json;

public partial class Form1 : Form
{
    private TextBox inputTextBox;
    private Button sendButton;
    private TextBox outputTextBox;
    private Button uploadButton;
    private OpenFileDialog openFileDialog;

    public Form1()
    {
        InitializeChatUI();
    }

    private void InitializeChatUI()
    {
        this.Size = new System.Drawing.Size(400, 400); // 设置窗体大小

        this.outputTextBox = new TextBox
        {
            Location = new System.Drawing.Point(12, 12),
            Width = 360,
            Height = 250,
            Multiline = true,
            ReadOnly = true,
            ScrollBars = ScrollBars.Vertical
        };

        this.inputTextBox = new TextBox
        {
            Location = new System.Drawing.Point(12, 270),
            Width = 260
        };

        this.sendButton = new Button
        {
            Text = "发送",
            Location = new System.Drawing.Point(278, 270)
        };

        this.uploadButton = new Button
        {
            Text = "上传文件",
            Location = new System.Drawing.Point(12, 310)
        };

        this.openFileDialog = new OpenFileDialog();

        this.sendButton.Click += new EventHandler(this.SendButton_Click);
        this.uploadButton.Click += new EventHandler(this.UploadButton_Click);

        this.Controls.Add(this.outputTextBox);
        this.Controls.Add(this.inputTextBox);
        this.Controls.Add(this.sendButton);
        this.Controls.Add(this.uploadButton);
    }

    private async void SendButton_Click(object sender, EventArgs e)
    {
        var userInput = this.inputTextBox.Text;
        if (string.IsNullOrWhiteSpace(userInput))
        {
            MessageBox.Show("请输入你的问题。");
            return;
        }

        var response = await GetAIResponse(userInput);
        this.outputTextBox.AppendText($"你：{userInput}\r\n");
        this.outputTextBox.AppendText($"AI：{response}\r\n");
        this.inputTextBox.Clear();
    }

    private async void UploadButton_Click(object sender, EventArgs e)
    {
        if (openFileDialog.ShowDialog() == DialogResult.OK)
        {
            var filePath = openFileDialog.FileName;
            var fileContent = await File.ReadAllTextAsync(filePath);
            var embeddingResponse = await GetEmbedding(fileContent);
            this.outputTextBox.AppendText($"文件已上传并处理：{filePath}\r\n");
            this.outputTextBox.AppendText($"Embedding结果：{embeddingResponse}\r\n");
        }
    }

    private async Task<string> GetAIResponse(string question)
    {
        var client = new HttpClient();
        var apiUrl = "https://localhost:11434/api/chat"; // 替换为实际的API URL
        var apiKey = "your-api-key"; // 替换为实际的API密钥

        var requestPayload = new
        {
            question = question
        };

        var jsonPayload = JsonConvert.SerializeObject(requestPayload);
        var content = new StringContent(jsonPayload, Encoding.UTF8, "application/json");

        client.DefaultRequestHeaders.Add("Authorization", $"Bearer {apiKey}");

        var response = await client.PostAsync(apiUrl, content);
        var responseString = await response.Content.ReadAsStringAsync();

        return responseString;
    }

    private async Task<string> GetEmbedding(string text)
    {
        var client = new HttpClient();
        var apiUrl = "https://api.example.com/embedding"; // 替换为实际的API URL
        var apiKey = "your-api-key"; // 替换为实际的API密钥

        var requestPayload = new
        {
            text = text
        };

        var jsonPayload = JsonConvert.SerializeObject(requestPayload);
        var content = new StringContent(jsonPayload, Encoding.UTF8, "application/json");

        client.DefaultRequestHeaders.Add("Authorization", $"Bearer {apiKey}");

        var response = await client.PostAsync(apiUrl, content);
        var responseString = await response.Content.ReadAsStringAsync();

        return responseString;
    }
}