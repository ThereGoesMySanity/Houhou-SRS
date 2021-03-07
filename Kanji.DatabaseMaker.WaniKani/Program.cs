using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Net.Http;
using Newtonsoft.Json.Linq;
using WaniKaniApi;
using WaniKaniApi.Models;

namespace Kanji.DatabaseMaker.WaniKani
{
    /// <summary>
    /// Given a WaniKani API key, produces wanikani data files usable by Houhou.Etl.
    /// </summary>
    class Program
    {
        static void Main(string[] args)
        {
            string key = ConfigurationManager.AppSettings["ApiKey"];
            Console.WriteLine("To pull data from WaniKani, the application needs an API key from a subscribed user account.");
            if (string.IsNullOrWhiteSpace(key))
            {
                Console.WriteLine("No key could be read from the App.config file.");
                Console.WriteLine("Press any key to exit.");
                Console.ReadKey(true);
                return;
            }
            using (HttpClient client = new HttpClient())
            {
                Console.WriteLine($"The key \"{key}\" has been read from the App.config file. Initializing the client.");
                var baseUrl = "https://api.wanikani.com/v2/";
                client.DefaultRequestHeaders.Add("Authorization", $"Bearer {key}");
                Console.WriteLine("Retrieving kanji...");

                List<JObject> kanji = new List<JObject>();
                List<JObject> vocab = new List<JObject>();
                JObject subRes = JObject.Parse(client.GetStringAsync(baseUrl + $"subjects?types=kanji,vocabulary").Result);
                kanji.AddRange(subRes["data"].Where(s => (string)s["object"] == "kanji").Select(s => (JObject)s["data"]));
                vocab.AddRange(subRes["data"].Where(s => (string)s["object"] == "vocabulary").Select(s => (JObject)s["data"]));
                while (!String.IsNullOrEmpty((string)subRes["pages"]["next_url"]) && kanji.Count + vocab.Count < (int)subRes["total_count"])
                {
                    subRes = JObject.Parse(client.GetStringAsync((string)subRes["pages"]["next_url"]).Result);
                    kanji.AddRange(subRes["data"].Where(s => (string)s["object"] == "kanji").Select(s => (JObject)s["data"]));
                    vocab.AddRange(subRes["data"].Where(s => (string)s["object"] == "vocabulary").Select(s => (JObject)s["data"]));
                }
                Console.WriteLine("Retrieving vocab...");

                Console.WriteLine("Writing files...");

                File.WriteAllLines("WaniKaniKanjiList.txt", kanji.Select(k => $"{k["characters"]}|{k["level"]}"));

                List<string> vocabLines = new List<string>();
                foreach (var v in vocab)
                {
                    // For each vocab, get each different kana reading.
                    foreach (string reading in v["readings"].Select(r => (string)r["reading"]))
                    {
                        string text = (string)v["characters"];
                        // For each reading, write a line.
                        vocabLines.Add($"{text}|{reading}|{(string)v["level"]}");

                        // Handle the する verb case: WaniKani sometimes teaches only the する verb version of a noun
                        // and it isn't necessarily in the dictionary, so we add another line without the する.
                        if (text.EndsWith("する") && reading.EndsWith("する"))
                            vocabLines.Add($"{text.Substring(0, text.Length - 2)}|{reading.Substring(0, reading.Length - 2)}|{(string)v["level"]}");
                    }
                }
                File.WriteAllLines("WaniKaniVocabList.txt", vocabLines);

                Console.WriteLine("Done.");
            }
        }
    }
}
