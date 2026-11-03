using System;
using System.Collections.Generic;
using System.IO;
namespace CodexFairy {
public static class RuntimePaths {
    public static bool HasVoiceConfiguration(){return File.Exists(Path.Combine(Program.Root,"runtime.local.json"));}
    static Dictionary<string,string> Settings(){var file=Path.Combine(Program.Root,"runtime.local.json");return File.Exists(file)?Json.Parse<Dictionary<string,string>>(File.ReadAllText(file)):new Dictionary<string,string>();}
    public static string ReferenceDirectory(){var settings=Settings();string path;if(settings.TryGetValue("ReferenceDir",out path)&&!string.IsNullOrWhiteSpace(path))return Path.GetFullPath(Path.IsPathRooted(path)?path:Path.Combine(Program.Root,path));var local=Path.Combine(Program.Root,"runtime","private","reference");if(File.Exists(Path.Combine(local,"fairy_ref.wav"))&&File.Exists(Path.Combine(local,"fairy_ref.txt")))return local;return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),".dsh","fairy-voice","runtime","reference");}
    public static string Launcher(){return Path.Combine(Program.Root,"runtime","start-fairy-tts.ps1");}
    public static string PersonaFile(){var local=Path.Combine(Program.Root,"runtime","private","fairy-persona.txt");return File.Exists(local)?local:Path.Combine(Program.Root,"assets","fairy-persona.txt");}
}
}
