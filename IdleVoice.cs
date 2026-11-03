using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;

namespace CodexFairy {
public sealed class VoiceCue { public double Start; public string Text; }
public sealed class IdleClip {
    public string Id,Text,Sha256; public double Duration; public List<VoiceCue> Cues;
    public byte[] Read(){return File.ReadAllBytes(Path.Combine(Program.Root,"assets","idle-original",Id+".pcm"));}
}
public sealed class IdleCatalog {
    public string Source; public List<IdleClip> Clips=new List<IdleClip>();
    public static IdleCatalog Load(){var file=Path.Combine(Program.Root,"assets","idle-original","catalog.json");if(!File.Exists(file))return new IdleCatalog();var catalog=Json.Parse<IdleCatalog>(File.ReadAllText(file));if(catalog.Clips==null)throw new Exception("Invalid idle voice catalog");foreach(var clip in catalog.Clips)if(!System.Text.RegularExpressions.Regex.IsMatch(clip.Id??"",@"^[a-z0-9][a-z0-9-]*$"))throw new Exception("Invalid idle clip path");catalog.Clips=catalog.Clips.Where(c=>File.Exists(Path.Combine(Program.Root,"assets","idle-original",c.Id+".pcm"))).ToList();return catalog;}
    public static int Minutes(int value){return Math.Max(1,Math.Min(60,value));}
    public static void PlaybackCheck(){
        var app=new System.Windows.Application();var player=new System.Windows.Media.MediaPlayer {Volume=0};var clips=Load().Clips;int index=0;var results=new List<object>();
        var timer=new System.Windows.Threading.DispatcherTimer {Interval=TimeSpan.FromSeconds(15)};
        Action next=()=>{if(index>=clips.Count){timer.Stop();player.Close();File.WriteAllText(Path.Combine(Program.Data,"idle-playback-check.json"),Json.Stringify(results));app.Shutdown();return;}var file=Path.Combine(Program.Data,"idle-playback-"+System.Diagnostics.Process.GetCurrentProcess().Id+"-"+clips[index].Id+".wav");player.Close();File.WriteAllBytes(file,Voice.Wave(clips[index].Read()));player.Open(new Uri(file));timer.Stop();timer.Start();};
        timer.Tick+=(s,e)=>{Environment.ExitCode=1;File.WriteAllText(Path.Combine(Program.Data,"idle-playback-check.json"),"{\"error\":\"playback timed out\"}");player.Close();app.Shutdown();};
        player.MediaOpened+=(s,e)=>{var duration=player.NaturalDuration.HasTimeSpan?player.NaturalDuration.TimeSpan.TotalSeconds:0;results.Add(new {id=clips[index].Id,seconds=duration});if(Math.Abs(duration-clips[index].Duration)>.1){Environment.ExitCode=1;app.Shutdown();return;}player.Play();};
        player.MediaEnded+=(s,e)=>{index++;next();};player.MediaFailed+=(s,e)=>{Environment.ExitCode=1;File.WriteAllText(Path.Combine(Program.Data,"idle-playback-check.json"),"{\"error\":\"media decoding failed\"}");app.Shutdown();};
        app.Startup+=(s,e)=>next();app.Run();
    }
    public static void Verify(){
        var catalog=Load();if(catalog.Clips.Select(c=>c.Id).Distinct().Count()!=catalog.Clips.Count)throw new Exception("Idle groups duplicated");
        foreach(var c in catalog.Clips){
            if(!System.Text.RegularExpressions.Regex.IsMatch(c.Id,@"^[a-z0-9][a-z0-9-]*$"))throw new Exception("Invalid idle clip path");
            var pcm=c.Read();if(pcm.Length%2!=0||Math.Abs(pcm.Length/64000.0-c.Duration)>.001||c.Duration<3)throw new Exception("Idle audio duration failed");
            using(var sha=SHA256.Create()){var hash=BitConverter.ToString(sha.ComputeHash(pcm)).Replace("-","").ToLowerInvariant();if(hash!=c.Sha256)throw new Exception("Idle audio checksum failed");}
            if(c.Cues==null||c.Cues.Count==0||string.Concat(c.Cues.Select(q=>q.Text))!=c.Text)throw new Exception("Idle caption incomplete");
            double previous=-1;foreach(var cue in c.Cues){if(cue.Start<0||cue.Start<=previous||cue.Start>=c.Duration||string.IsNullOrWhiteSpace(cue.Text))throw new Exception("Idle cue timing failed");previous=cue.Start;}
        }
        if(Minutes(0)!=1||Minutes(1)!=1||Minutes(60)!=60||Minutes(100)!=60)throw new Exception("Idle interval bounds failed");
    }
}
public sealed partial class Pet {
    readonly IdleCatalog idleCatalog=IdleCatalog.Load();
    Action refreshIdle=()=>{};
    readonly List<System.Windows.Controls.MenuItem> idleMenuItems=new List<System.Windows.Controls.MenuItem>();
    void SetIdleMinutes(int value,bool save){prefs.IdleMinutes=IdleCatalog.Minutes(value);Touch();refreshIdle();foreach(var item in idleMenuItems)item.IsChecked=(int)item.Tag==prefs.IdleMinutes;if(save&&!Program.Preview)prefs.Save();}
}
}
