using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

string uri = "null";
string filePath = default;
Uri targetUri = default;

Options options = ParseOptions(args);
filePath = options.WordlistPath;
targetUri = options.TargetUrl;

if (!File.Exists(filePath))
{
    Console.WriteLine($"File Path does not exist @{filePath}");
    DisplayHelp();
    Environment.Exit(0);
}
if (targetUri == null)
{
    Console.WriteLine("[ERROR]: Url not Provided:");
    DisplayHelp();
    Environment.Exit(0);
}

static void DisplayHelp() => Console.WriteLine(@"
Available Commands:
  dir         Supply URI to Enumerate

Flags:
  -w, --wordlist string   Path to the wordlist
");

Console.WriteLine(@"

 _____     ______           _            
/  __ \    | ___ \         | |           
| /  \/___ | |_/ /_   _ ___| |_ ___ _ __ 
| |   / __|| ___ \ | | / __| __/ _ \ '__|
| \__/\__ \| |_/ / |_| \__ \ ||  __/ |   
 \____/___/\____/ \__,_|___/\__\___|_|
___________
GoBusterSharp
------------
By: t0g3ly
");

static Options ParseOptions(string[] args)
{
    Options options = new();
    for (int i = 0; i < args.Length; i++)
    {
        if (args[i] == "-w" && i + 1 < args.Length)
        {
            options.WordlistPath = args[i + 1];
            i++;
        }
        if (args[i] == "dir" && i < args.Length)
        {
            options.TargetUrl = new Uri(args[i + 1]);
        }
    }
    return options;
}

GoSharp goSharp = new();
goSharp.goSharpUri = targetUri;
goSharp.goSharpFilePath = filePath;

HttpClient client = new();
HttpResponseMessage responseMessage = await client.GetAsync(targetUri);
Console.WriteLine(responseMessage.StatusCode);

List<string> wordList = new List<string>();
using StreamReader reader = new(filePath);
string line;
while ((line = reader.ReadLine()) != null)
{
    wordList.Add(line);
}

int numThreads = 10;
List<Task> tasks = new List<Task>();


for (int i = 0; i < numThreads; i++)
{
    int threadId = i;
    tasks.Add(Task.Run(() =>
    {
        int numRequests = 0;

        for (int j = threadId; j < wordList.Count; j += numThreads)
        {
            string w = wordList[j];
            Uri goSharpTarget = new(targetUri+ w);

            try
            {
                responseMessage = client.GetAsync(goSharpTarget).GetAwaiter().GetResult();
                responseMessage.EnsureSuccessStatusCode();
                Console.WriteLine($"\n{responseMessage.StatusCode} Found at {goSharpTarget}");
            }
            catch (HttpRequestException ex)
            {
                if (responseMessage.StatusCode != HttpStatusCode.NotFound)
                    Console.WriteLine($"\n{responseMessage.StatusCode} Found at {goSharpTarget}");
            }

            Console.SetCursorPosition(0, Console.CursorTop);
            numRequests++;
            //Console.Write($"\tRequests Sent: {numRequests}/{wordList.Count} \ton Thread {threadId + 1}/{numThreads}");
        }
    }));
}

Task.WaitAll(tasks.ToArray());

Console.WriteLine($"\nDone. Total requests sent: {wordList.Count}");

public class GoSharp
{
    public Uri goSharpUri;
    public string goSharpFilePath;
}

public class Options
{
    public string WordlistPath { get; set; }
    public Uri TargetUrl { get; set; }
    public int NumThreads { get; set; } = 10;
}