using Microsoft.AspNetCore.SignalR.Client;
using System;
using System.Threading.Tasks;
using UnityEngine;

public class WebServerConnection
{
    public Action<Message> OnMessageReceived;
    private HubConnection connection;
    public HubConnection Connection => connection;

    public async Task InitAsync<T>(string url, string handlerMethod) where T : Message
    {
        connection = new HubConnectionBuilder()
                .WithUrl(url)
                .Build();

        connection.On<T>(handlerMethod, (message) =>
        {
            OnMessageReceived?.Invoke(message);
        });

        connection.Closed += async (exception) =>
        {
            if (exception == null)
            {
                Message msg = new Message();
                msg.Type = "CloseConnection";
                msg.Content = "";

                await SendMessageAsync(msg);
            } else
            {
                Message msg = new Message();
                msg.Type = "CloseConnection";
                msg.Content = exception.Message;
            }
        };

        await StartConnectionAsync();
    }

    public async Task SendMessageAsync<T>(T message) where T : Message
    {
        try
        {
            await connection.InvokeAsync(message.Type, message.Content);
        }
        catch (Exception ex)
        {
            Debug.LogError($"Error {ex.Message}");
        }
    }

    public async Task CreateLobbyAsync()
    {
        try
        {
            await connection.InvokeAsync("CreateRoom");
        }
        catch (Exception ex)
        {
            Debug.LogError($"Error {ex.Message}");
        }
    }

    public async Task AddPlayerAsync(string connectionID, string lobbyCode, string name, bool success)
    {
        try
        {
            await connection.InvokeAsync("AddPlayer", connectionID, lobbyCode, name, success);
        }
        catch (Exception ex)
        {
            Debug.LogError($"Error {ex.Message}");
        }
    }

    private async Task StartConnectionAsync()
    {
        try
        {
            await connection.StartAsync();
        }
        catch (Exception ex)
        {
            Debug.LogError($"Error: {ex.Message}");
        }
    }
}

public class Message
{
    private string type = "";
    private string content = "";

    public string Type { get => type; set => type = value; }
    public string Content { get => content; set => content = value; }
}