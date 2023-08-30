using Microsoft.Web.Administration;
using System.IO.Compression;
using System.Net;
using Masterdev.Updater.Client;
using Newtonsoft.Json;
using System.ServiceProcess;
using System;
using System.Net.Mime;
using static System.Runtime.InteropServices.JavaScript.JSType;


var wc = new WebClient();
var serverManager = new ServerManager();
var config = getconfig();

/*Root getupdateversion()
{
    try
    {
        wc.DownloadFile(new Uri(config.updateFilePath), pathWithFileName);
        wc.QueryString.Add("updatename", config.applicationName);
        wc.QueryString.Add("version", );
        string jsonValue = File.ReadAllText(pathWithFileName);
        Root myDeserializedClass = JsonConvert.DeserializeObject<Root>(jsonValue);
        return myDeserializedClass;
    }
    catch (Exception e)
    {
        Console.WriteLine(e + "\n");
    }
    return null;
}*/

Root getappversion()
{
    try
    {
        string pathWithFileName = Path.Combine(System.IO.Directory.GetCurrentDirectory(), config.directoryName, config.serverFileName);
        string jsonValue = File.ReadAllText(pathWithFileName);
        Root myDeserializedClass = JsonConvert.DeserializeObject<Root>(jsonValue);
        return myDeserializedClass;
    }
    catch (Exception e)
    {
        Console.WriteLine(e + "\n");
    }
    return null;
}

config getconfig()
{
    try
    {
        string pathWithFileName = Path.Combine(System.IO.Directory.GetCurrentDirectory(), "config.txt");
        string jsonValue = File.ReadAllText(pathWithFileName);
        config myDeserializedClass = JsonConvert.DeserializeObject<config>(jsonValue);
        return myDeserializedClass;
    }
    catch (Exception e)
    {
        Console.WriteLine(e + "\n");
    }
    return null;
}

void downloadUpdate(Root result)
{
    try
    {
        string pathWithFileName = Path.Combine(System.IO.Directory.GetCurrentDirectory(), "update.zip");
        if (File.Exists(pathWithFileName))
        {
            File.Delete(pathWithFileName);
        }
        
        wc.QueryString.Add("file_name", result.file_name);
        wc.QueryString.Add("version", result.version);
        wc.DownloadFile(new Uri(config.updateZipFilePath), pathWithFileName);
    }
    catch (WebException wex)
    {
        Console.WriteLine(wex + "\n");
    }
}

bool validate(string update, string uploaded)
{
    int updatenum = int.Parse(update.Replace(".", ""));
    int uploadednum = int.Parse(uploaded.Replace(".", ""));
    //Console.WriteLine(uploadednum + "<" + updatenum);
    if (uploadednum < updatenum)
    {
        return true;
    }
    else return false;
}

bool createbackup()
{
    try
    { 
        string startPath = Path.Combine(System.IO.Directory.GetCurrentDirectory(), config.directoryName);
        string zipPath = Path.Combine(System.IO.Directory.GetCurrentDirectory(), "backup.zip");
        if (File.Exists(zipPath))
        {
            File.Delete(zipPath);
        }
        ZipFile.CreateFromDirectory(startPath, zipPath);
    return true;
    }
    catch (Exception e)
    {
        Console.WriteLine(e + "\n");
    }
    return false;
}

bool uploadupdate()
{
    try
    {
        string extractPath = Path.Combine(System.IO.Directory.GetCurrentDirectory(), config.directoryName);
        string zipPath = Path.Combine(System.IO.Directory.GetCurrentDirectory(), "update.zip");
        ZipFile.ExtractToDirectory(zipPath, extractPath, true);
        return true;
    }
    catch (Exception e)
    {
        Console.WriteLine(e + "\n");
    }
    return false;
}

bool turnoffapp()
{
    try
    {
        if (serverManager.ApplicationPools[config.applicationName] != null)
        {
            if ((int)serverManager.ApplicationPools.First(ap => ap.Name.Equals(config.applicationName)).State == 1)
            {
                //Console.WriteLine(serverManager.ApplicationPools.First(ap => ap.Name.Equals(config.applicationName)).State.ToString());
                serverManager.ApplicationPools.First(ap => ap.Name.Equals(config.applicationName)).Stop();
                //Console.WriteLine(serverManager.ApplicationPools.First(ap => ap.Name.Equals(config.applicationName)).State.ToString());
                if ((int)serverManager.ApplicationPools.First(ap => ap.Name.Equals(config.applicationName)).State != 1)
                {
                    Console.Write("Status puli:" + serverManager.ApplicationPools.First(ap => ap.Name.Equals(config.applicationName)).State + "\n");
                    return true;
                }
                else
                {
                    Console.Write("Error przy zatrzymywaniu puli" + "\n");
                    return false;
                }
            }
            else if((int)serverManager.ApplicationPools.First(ap => ap.Name.Equals(config.applicationName)).State == 3)
            {
                Console.Write("Status puli:" + serverManager.ApplicationPools.First(ap => ap.Name.Equals(config.applicationName)).State + "\n");
                return true;
            }
            else
            {
                Console.Write("Błąd Status puli:" + serverManager.ApplicationPools.First(ap => ap.Name.Equals(config.applicationName)).State + " - Próba zatrzymania puli" + "\n");
                serverManager.ApplicationPools.First(ap => ap.Name.Equals(config.applicationName)).Stop();
                
                if ((int)serverManager.ApplicationPools.First(ap => ap.Name.Equals(config.applicationName)).State == 3)
                {
                    Console.Write("Status puli:" + serverManager.ApplicationPools.First(ap => ap.Name.Equals(config.applicationName)).State + "\n");
                    return true;
                }
                else
                {
                    Console.Write("Błąd Status puli:" + serverManager.ApplicationPools.First(ap => ap.Name.Equals(config.applicationName)).State + "\n");
                    return false;
                }
            }
        }
        else
        {
            Console.WriteLine("Nima taki appki" + "\n");
            return false;
        }
    }
    catch (Exception e)
    {
        Console.WriteLine(e + "\n");
        return false;
    }
}

bool turnonapp()
{
    try
    {
        if (serverManager.ApplicationPools[config.applicationName] != null)
        {
            if ((int)serverManager.ApplicationPools.First(ap => ap.Name.Equals(config.applicationName)).State == 3)
            {
                serverManager.ApplicationPools.First(ap => ap.Name.Equals(config.applicationName)).Start();
                if ((int)serverManager.ApplicationPools.First(ap => ap.Name.Equals(config.applicationName)).State != 3)
                {
                    Console.Write("Pula wystartowała"+ "\n");
                    return true;
                }
                else
                {
                    Console.Write("Status puli:" + serverManager.ApplicationPools.First(ap => ap.Name.Equals(config.applicationName)).State+" Error przy starcie");
                    return false;
                }
            }
            else
            {
                Console.Write("Status puli:" + serverManager.ApplicationPools.First(ap => ap.Name.Equals(config.applicationName)).State+ "Próba startu ");
                serverManager.ApplicationPools.First(ap => ap.Name.Equals(config.applicationName)).Start();
                if ((int)serverManager.ApplicationPools.First(ap => ap.Name.Equals(config.applicationName)).State == 1)
                {
                    Console.Write("Pula wystartowała" + "\n");
                    return true;
                }
                else
                {
                    Console.Write("Status puli:" + serverManager.ApplicationPools.First(ap => ap.Name.Equals(config.applicationName)).State + " Error przy starcie");
                    return false;
                }
            }
        }
        else
        {
            Console.WriteLine("Nima taki appki");
            return false;
        }
    }
    catch (Exception e)
    {
        Console.WriteLine(e + "\n");
        return false;
    }
}

var app = getappversion();

    wc.Headers.Add("x-api-key", "866fccaf6c5b4efda50505ecbf838f55");
    wc.QueryString.Add("appname", config.applicationName);
    wc.QueryString.Add("version", app.version);
    string result = wc.DownloadString(config.updateFilePath);
    var resultx = JsonConvert.DeserializeObject<Root>(result);

    if (resultx.file_name != null && resultx.version != null)
    {
        wc.QueryString.Clear();
        downloadUpdate(resultx);
    }
    else
    {
        Console.WriteLine("Masz aktualną wersję oprogramowania lub odpowiedz z bazy danych byłą pusta" + "\n");
        Environment.Exit(0);
}

if (createbackup() == true)
{
    if (turnoffapp() == true)
    {
        if (uploadupdate() == true)
        {
            File.WriteAllText(Path.Combine(System.IO.Directory.GetCurrentDirectory(), config.directoryName , config.serverFileName), JsonConvert.SerializeObject(resultx));
            Console.WriteLine("Update Wgrany" + resultx.file_name + "\n");
            if (turnonapp() == true)
            {
                Console.WriteLine("Applikacja Wystartowana" + "\n");
                Environment.Exit(0);
            }
        }
        else
        {
            Console.WriteLine("Error while turning off apppool" + "\n");
            Environment.Exit(0);
        }
    }
    else
    {
        Console.WriteLine("Upload error" + "\n");
        Environment.Exit(0);
    }
}
