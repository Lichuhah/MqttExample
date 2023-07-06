using System;
using System.Text;
using MQTTnet;
using MQTTnet.Packets;

namespace MqttServerWeb.Mqtt;

public static class MqttHelper
{
    public static List<MqttClient> GetCurrentClients()
    {
        MyMqttServer server = MyMqttServer.GetServer();
        var clients = server.mqttServer.GetClientsAsync().Result;
        List<MqttClient> mqttClients = clients.Select(c => new MqttClient()
        {
            name = c.Id
        }).ToList();
        return mqttClients;
    }
    
    public static void SubscribeOnTopic(string name)
    {
        MyMqttServer server = MyMqttServer.GetServer();
        var mqttFilter = new MqttTopicFilterBuilder().WithTopic(name).Build();
        server.Topics.Add(name);
        List<MqttTopicFilter> filters = server.Topics.Select(x=>
            new MqttTopicFilterBuilder().WithTopic(x).Build()).ToList();
        server.mqttClientPublisher.SubscribeAsync(filters);
        
    } 
    
    public static List<string> GetTopics()
    {
        MyMqttServer server = MyMqttServer.GetServer();
        return server.Topics;
    } 
    
    public static List<MqttMessage> GetLastMessages()
    {
        MyMqttServer server = MyMqttServer.GetServer();
        return server.Messages;
    }

    public static void SendMessage(string topic, string message)
    {
        MyMqttServer server = MyMqttServer.GetServer();
        server.SendMessage(topic, message);
    }
}