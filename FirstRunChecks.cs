using System;
using System.IO;
using System.Windows;
namespace CodexFairy {
public sealed partial class Pet {
    public void TestFirstRun(){
        if(RuntimePaths.HasVoiceConfiguration()||File.Exists(Path.Combine(Program.Root,"runtime","private","fairy-persona.txt"))||idleCatalog.Clips.Count!=0)throw new Exception("First-run check requires a public-only source copy");
        if(prefs.ReadAnswers||prefs.Idle||File.Exists(Path.Combine(Program.Data,"preferences.json")))throw new Exception("First-run preferences depend on local data");
        InitializeVoice().GetAwaiter().GetResult();if(!lastStatus.Contains("文字模式"))throw new Exception("Unconfigured voice blocks first run");
        WindowState=WindowState.Minimized;Topmost=false;Show();
        var frame=new System.Windows.Threading.DispatcherFrame();var timer=new System.Windows.Threading.DispatcherTimer {Interval=TimeSpan.FromMilliseconds(200)};timer.Tick+=(s,e)=>frame.Continue=false;timer.Start();System.Windows.Threading.Dispatcher.PushFrame(frame);timer.Stop();
        if(!IsLoaded||new System.Windows.Interop.WindowInteropHelper(this).Handle==IntPtr.Zero)throw new Exception("First-run native window failed to initialize");
        OpenPanel();CreateComposer();if(panel==null||input==null||providers==null||composer==null)throw new Exception("First-run UI creation failed");
        providers.SelectedItem="DeepSeek";showConnection();if(apiKey.Password!=""||sessionKey!="")throw new Exception("Credential inherited during isolated test");
        providers.SelectedItem="Codex";if(RuntimePaths.PersonaFile()!=Path.Combine(Program.Root,"assets","fairy-persona.txt"))throw new Exception("Private persona inherited");
        string path=Path.Combine(Program.Data,"path-fixture");Directory.CreateDirectory(path);File.WriteAllText(Path.Combine(path,"codex.exe"),"Synthetic path-resolution fixture; never executed");
        if(Brain.FindCodexInPath("\""+path+"\"")!=Path.Combine(path,"codex.exe")||Brain.FindCodexInPath(Path.Combine(path,"missing"))!=null)throw new Exception("Codex PATH lookup failed");
        Checks.Run(false);
        File.WriteAllText(Path.Combine(Program.Root,"preview","first-run-check.json"),Json.Stringify(new{emptyPreferences=true,nativeWindowLoaded=true,voiceDisabled=true,noTtsRequired=true,noCodexRequiredForSelfTest=true,emptyCredentials=true,publicPersona=true,chatWindowCreated=true,quickInputCreated=true,bothProvidersSelectable=true,codexPathResolution=true,chatRequestSent=false,ttsStarted=false}));
        Close();
    }
}
}
