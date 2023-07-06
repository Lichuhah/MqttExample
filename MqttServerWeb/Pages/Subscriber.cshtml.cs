using System.Security.Cryptography;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using MqttServerWeb.Mqtt;

namespace MqttServerWeb.Pages;

public class SubscriberModel : PageModel
{
    public void OnGet()
    {
        
    }
    public ActionResult OnPostTopic(string topic)
    {
        MqttHelper.SubscribeOnTopic(topic);
        return Redirect("/Subscriber");
    }
}