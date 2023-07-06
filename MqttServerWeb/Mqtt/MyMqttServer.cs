using System.Text;
using MQTTnet;
using MQTTnet.Client;
using MQTTnet.Extensions.ManagedClient;
using MQTTnet.Formatter;
using MQTTnet.Packets;
using MQTTnet.Server;

namespace MqttServerWeb.Mqtt;

public class MyMqttServer
{
    private static MyMqttServer instance;
    /// <summary>
    /// The managed publisher client.
    /// </summary>
    public IManagedMqttClient? mqttClientPublisher;

    /// <summary>
    /// The managed subscriber client.
    /// </summary>
    public IManagedMqttClient? mqttClientSubscriber;

    /// <summary>
    /// The MQTT server.
    /// </summary>
    public MqttServer? mqttServer;

    /// <summary>
    /// The port.
    /// </summary>
    private int port = 707;

    /// <summary>
    /// Log.
    /// </summary>
    public List<MqttMessage> Messages = new List<MqttMessage>();
    
    /// <summary>
    /// Topics.
    /// </summary>
    public List<string> Topics = new List<string>(){"test_topic"};

    private MyMqttServer()
    {
        CreateServer();
        CreateSubscriber();
        CreatePublisher();
    }
    
    /// <summary>
    /// Настройка сервера
    /// </summary>
    private void CreateServer()
    {
        if (mqttServer != null)
        {
            return;
        }

        var options = new MqttServerOptionsBuilder()
            .WithDefaultEndpoint()
            .WithDefaultEndpointPort(port)
            .Build();

        mqttServer = new MqttFactory().CreateMqttServer(options);
        mqttServer.ClientConnectedAsync += OnClientConnected;

        try
        {
            mqttServer.StartAsync().GetAwaiter().GetResult();
        }
        catch (Exception ex)
        {
            mqttServer.StopAsync();
            mqttServer = null;
        }
    }
    
    public Task OnClientConnected(ClientConnectedEventArgs eventArgs)
    {
        Console.WriteLine($"Client '{eventArgs.ClientId}' connected.");
        return Task.CompletedTask;
    }
    
    /// <summary>
    /// Настройка тестового клиента-подписчика
    /// </summary>
    public void CreateSubscriber()
    {
        var options = new ManagedMqttClientOptionsBuilder().WithAutoReconnectDelay(TimeSpan.FromSeconds(5))
       .WithClientOptions(new MqttClientOptionsBuilder().WithClientId("TestSubscriber").WithTcpServer("localhost", port).Build()).Build();

        mqttClientPublisher = new MqttFactory().CreateManagedMqttClient();
        mqttClientPublisher.ApplicationMessageReceivedAsync += HandleReceivedApplicationMessage;
        var mqttFilter = new MqttTopicFilterBuilder().WithTopic("test_topic").Build();
        mqttClientPublisher.SubscribeAsync(new List<MqttTopicFilter> { mqttFilter });
        mqttClientPublisher.StartAsync(options);
    }
    
    private Task HandleReceivedApplicationMessage(MqttApplicationMessageReceivedEventArgs e)
    {
       Messages.Add(new MqttMessage()
           {
               Client = e.ClientId,
               Topic = e.ApplicationMessage.Topic,
               Payload = Encoding.UTF8.GetString(e.ApplicationMessage.Payload)
           });
       return Task.CompletedTask;
    }

    
    /// <summary>
    /// Настройка тестового клиента-публициста
    /// </summary>
    public void CreatePublisher()
    {
        var mqttFactory = new MqttFactory();

        var tlsOptions = new MqttClientTlsOptions
        {
            UseTls = false,
            IgnoreCertificateChainErrors = true,
            IgnoreCertificateRevocationErrors = true,
            AllowUntrustedCertificates = true
        };

        var options = new MqttClientOptions
        {
            ClientId = "DispatcherPublisher",
            ProtocolVersion = MqttProtocolVersion.V500,
            ChannelOptions = new MqttClientTcpOptions
            {
                Server = "localhost",
                Port = port,
                TlsOptions = tlsOptions
            }
        };

        if (options.ChannelOptions == null)
        {
            throw new InvalidOperationException();
        }

        options.CleanSession = true;
        options.KeepAlivePeriod = TimeSpan.FromSeconds(30);

        mqttClientSubscriber = mqttFactory.CreateManagedMqttClient();
        mqttClientSubscriber.ApplicationMessageReceivedAsync += HandleReceivedApplicationMessage;

        mqttClientSubscriber.StartAsync(
            new ManagedMqttClientOptions
            {
                ClientOptions = options
            });
    }
    
    public void SendMessage(string topic, string payload)
    {
        try
        {
            var message = new MqttApplicationMessageBuilder()
                .WithTopic(topic).WithPayload(payload).Build();

            if (mqttClientSubscriber != null)
            {
                mqttClientSubscriber.EnqueueAsync(message);
            }
        }
        catch (Exception ex)
        {
            var a = 5;
        }
    }
    public static MyMqttServer GetServer()
    {
        if (instance == null)
            instance = new MyMqttServer();
        return instance;
    }
}