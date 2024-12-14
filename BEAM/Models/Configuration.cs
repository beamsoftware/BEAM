using System;
using System.IO;
using System.Text.Json;

namespace BEAM.Models;
public class Configuration(string version, Language language, string file, string open, 
    string exit, string edit, string paste, string copy, string help)
{ 
    public string Version { get; set; } = version;
    public Language Language { get; set; } = language;
    public string FileMenu { get; set; } = file;
    public string Open {get; set;} = open;
    public string Exit {get; set;} = exit;
    public string Edit {get; set;} = edit;
    public string Paste {get; set;} = paste;
    public string Copy {get; set;} = copy;
    public string Help {get; set;} = help;

    public static Configuration AttemptLoad(string path)
    {
        try
        {
            var json = File.ReadAllText(path);
            var config = JsonSerializer.Deserialize<Configuration>(json);
            return config ?? StandardEnglish();
        }
        catch (Exception e)
        {
            return StandardEnglish();
        }
    }
    
    public static Configuration StandardEnglish()
    {
        return new Configuration("1.0", Language.English, "_File", "_Open...", "_Exit", "_Edit", "Paste", "Copy", "Help");
    }
}