using Common;
using Microsoft.Agents.AI;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.Configuration;
using OpenAI;
using OpenAI.Chat;
using System.ClientModel;

IConfiguration configuration = new ConfigurationBuilder().AddAppSettings();

var llm = configuration.GetSection(ModelSettings.SectionName).Get<ModelSettings>()
    ?? throw new InvalidOperationException("缺少配置节 Llm");

if (string.IsNullOrWhiteSpace(llm.ApiKey))
{
    Console.WriteLine("请配置 ApiKey");
    return;
}

var agent = CreateAgent(llm);

/* 
 * 非流式输出示例
 */
//var response = await agent.RunAsync(new Microsoft.Extensions.AI.ChatMessage(ChatRole.User, "你好"));
//Console.WriteLine(response.Text);

/* 
 * 流式输出示例
 */
var streamPrompt = new Microsoft.Extensions.AI.ChatMessage(ChatRole.User, "你好，请介绍一下你自己");
using var cts = new CancellationTokenSource();

await foreach (AgentResponseUpdate update in agent.RunStreamingAsync(streamPrompt, cancellationToken: cts.Token))
{
    if (!string.IsNullOrEmpty(update.Text))
    {
        Console.Write(update.Text);  // 不换行，模拟打字效果
    }
}
Console.WriteLine();


static AIAgent CreateAgent(ModelSettings settings)
{
    var options = new OpenAIClientOptions
    {
        Endpoint = new Uri(settings.Endpoint!.TrimEnd('/') + "/")
    };

    var client = new OpenAIClient(new ApiKeyCredential(settings.ApiKey), options);
    return client.GetChatClient(settings.Model).AsAIAgent();
}
