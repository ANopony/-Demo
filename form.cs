using System;
using System.Data.SQLite;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

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
        InitializeDatabase();
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

    private void InitializeDatabase()
    {
        string connectionString = "Data Source=embedding.db;Version=3;";
        using (var connection = new SQLiteConnection(connectionString))
        {
            connection.Open();
            string createTableQuery = @"
                CREATE TABLE IF NOT EXISTS Embeddings (
                    Id INTEGER PRIMARY KEY AUTOINCREMENT,
                    FilePath TEXT,
                    Embedding TEXT
                )";
            using (var command = new SQLiteCommand(createTableQuery, connection))
            {
                command.ExecuteNonQuery();
            }
        }
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

            SaveEmbeddingToDatabase(filePath, embeddingResponse);
        }
    }

    private async Task<string> GetAIResponse(string question)
    {
        var client = new HttpClient();
        var apiUrl = "http://10.3.74.124:11434/api/chat"; // 使用HTTP而不是HTTPS

        var requestPayload = new
        {
            model = "qwen2:1.5b", // 替换为实际的模型名称
            messages = new[]
            {
                new { role = "user", content = question }
            },
            stream = false
        };

        var jsonPayload = JsonConvert.SerializeObject(requestPayload);
        var content = new StringContent(jsonPayload, Encoding.UTF8, "application/json");

        var response = await client.PostAsync(apiUrl, content);
        var responseString = await response.Content.ReadAsStringAsync();

        var responseJson = JObject.Parse(responseString);
        var contentField = responseJson["message"]?["content"]?.ToString();

        return contentField ?? "未能获取到AI的回答";
    }

    private async Task<string> GetEmbedding(string text)
    {
        var client = new HttpClient();
        var apiUrl = "http://10.3.74.124:11434/api/embed"; // 替换为实际的API URL

        var requestPayload = new
        {
            model = "quentinz/bge-embedding-768",
            input = text
        };

        var jsonPayload = JsonConvert.SerializeObject(requestPayload);
        var content = new StringContent(jsonPayload, Encoding.UTF8, "application/json");

        var response = await client.PostAsync(apiUrl, content);
        var responseString = await response.Content.ReadAsStringAsync();

        return responseString;
    }

    private void SaveEmbeddingToDatabase(string filePath, string embedding)
    {
        string connectionString = "Data Source=embedding.db;Version=3;";
        using (var connection = new SQLiteConnection(connectionString))
        {
            connection.Open();
            string insertQuery = "INSERT INTO Embeddings (FilePath, Embedding) VALUES (@FilePath, @Embedding)";
            using (var command = new SQLiteCommand(insertQuery, connection))
            {
                command.Parameters.AddWithValue("@FilePath", filePath);
                command.Parameters.AddWithValue("@Embedding", embedding);
                command.ExecuteNonQuery();
            }
        }
    }
}