using System;
using System.IO;
using System.Text.Json;

namespace BEAM.Models;
public class Configuration(string version, Language language, string file, string open, string openFolder,
    string exit, string edit, string paste, string copy, string help, string view, string viewOpenStatusWindow, 
    string synchro, string deactivateSynchro, string activateSynchro)
{ 
    public string Version { get; set; } = version;
    public Language Language { get; set; } = language;
    public string FileMenu { get; set; } = file;
    public string Open {get; set;} = open;
    public string OpenFolder {get; set;} = openFolder;
    public string Exit {get; set;} = exit;
    public string Edit {get; set;} = edit;
    public string Paste {get; set;} = paste;
    public string Copy {get; set;} = copy;
    public string Help {get; set;} = help;
    public string View {get; set;} = view;
    public string Synchro {get; set;} = synchro;
    public string DeactivateSynchro {get; set;} = deactivateSynchro;
    public string ActivateSynchro {get; set;} = activateSynchro;
    public string ViewOpenStatusWindow {get; set;} = viewOpenStatusWindow;

    public static Configuration AttemptLoad(string path)
    {
        try
        {
            var json = File.ReadAllText(path);
            var config = JsonSerializer.Deserialize<Configuration>(json);
            return config ?? StandardEnglish();
        }
        catch (Exception)
        {
            return StandardEnglish();
        }
    }
    
    public static Configuration StandardEnglish()
    {
        return new Configuration("1.0", Language.English, "_File", "_Open...",  "O_pen Folder...", "_Exit", "_Edit", "Paste", "Copy", "Help", "View", "Open Status Window", "Synchronization", "Deactivate Synchronization", "Activate Synchronization");
    }
}