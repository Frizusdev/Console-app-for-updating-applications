using System;
namespace Masterdev.Updater.Client
{
    // Root myDeserializedClass = JsonConvert.DeserializeObject<Root>(myJsonResponse);
    public class Root
    {
        public string file_name { get; set; }
        public string version { get; set; }
    }
}

