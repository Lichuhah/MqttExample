using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using MqttServerWeb.Mqtt;

namespace MqttServerWeb.Pages;

public class Publisher : PageModel
{
    public void OnGet()
    {
        
    }

    public ActionResult OnPostSend(string topic, string body)
    {
        MqttHelper.SendMessage(topic, body);
        return Redirect("/Publisher");
    }
}