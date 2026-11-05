using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace CodexFairy {
public static class ScreenChecks {
    public static void LiveCapture(){var preferences=Preferences.Load();var screens=System.Windows.Forms.Screen.AllScreens;int index=preferences.ScreenMonitor;var bounds=index>=0&&index<screens.Length?screens[index].Bounds:System.Windows.Forms.SystemInformation.VirtualScreen;using(var first=DesktopCapture.Capture(bounds,new Rectangle[0],1600)){System.Threading.Thread.Sleep(300);using(var second=DesktopCapture.Capture(bounds,new Rectangle[0],1600))using(var image=ScreenAttachment.Compose(new[]{second})){var folder=Path.Combine(Program.Data,"screen-cache");Directory.CreateDirectory(folder);File.WriteAllBytes(Path.Combine(folder,"live-game-check.png"),image.Png);double light=0;int lit=0;for(int i=2;i<second.Grid.Length;i+=3){light+=second.Grid[i];if(second.Grid[i]>8)lit++;}File.WriteAllText(Path.Combine(folder,"live-game-check.json"),Json.Stringify(new{realScreen=true,uploaded=false,width=second.Pixels.Width,height=second.Pixels.Height,change=ScreenFrames.Difference(first.Grid,second.Grid),meanBrightness=light/(second.Grid.Length/3),nonBlackFraction=lit/(second.Grid.Length/3.0)}));}}}
    public static Bitmap Fixture(Color color,string text="Synthetic desktop fixture"){var bitmap=new Bitmap(960,540);using(var g=Graphics.FromImage(bitmap))using(var font=new Font("Arial",22)){g.Clear(color);g.DrawString(text,font,Brushes.White,35,45);}return bitmap;}
    public static async Task Run(){
        if(ScreenPolicy.OwnMouseInteraction(true,false)||!ScreenPolicy.OwnMouseInteraction(true,true)||ScreenPolicy.InteractionBlocks(false,false,false,ScreenPolicy.OwnMouseInteraction(true,false)))throw new Exception("Game shooting incorrectly blocks analysis");
        var scene=DateTime.UtcNow;float[] grid;using(var fixture=Fixture(Color.DarkBlue))grid=ScreenFrames.Fingerprint(fixture);var small=(float[])grid.Clone();for(int i=2;i<small.Length;i+=3)small[i]+=8;
        using(var office=new ScreenBuffer())using(var game=new ScreenBuffer {GameMode=true}){office.Push(new ScreenFrame(Fixture(Color.DarkBlue),scene,grid));game.Push(new ScreenFrame(Fixture(Color.DarkBlue),scene,grid));if(office.Push(new ScreenFrame(Fixture(Color.DarkBlue),scene.AddMilliseconds(250),small))||!game.Push(new ScreenFrame(Fixture(Color.DarkBlue),scene.AddMilliseconds(250),small)))throw new Exception("Game sensitivity failed");if(!game.Push(new ScreenFrame(Fixture(Color.DarkBlue),scene.AddSeconds(2),small))||game.Count!=2)throw new Exception("Game heartbeat or frame bound failed");}
        using (var observation = new GameObservation()) {
            for (int i = 0; i < 20; i++) {
                using (var frame = new ScreenFrame(Fixture(i % 2 == 0 ? Color.DarkBlue : Color.Orange), scene.AddMilliseconds(i * 250)))
                    observation.Observe(frame);
            }
            if (!observation.IsBusy(scene.AddSeconds(5)) || observation.Event != 0)
                throw new Exception("Sustained motion incorrectly triggered speech");
            if (observation.ShouldReviewActivePlay(scene.AddSeconds(5),scene,5) ||
                !observation.ShouldReviewActivePlay(scene.AddSeconds(5),scene.AddSeconds(-3),5))
                throw new Exception("Continuous gameplay did not respect the automatic review interval");
            for (int i = 20; i < 48; i++) {
                using (var frame = new ScreenFrame(Fixture(Color.Orange), scene.AddMilliseconds(i * 250)))
                    observation.Observe(frame);
            }
            if (observation.IsBusy(scene.AddSeconds(12)) || observation.Event != 1)
                throw new Exception("Stable scene did not settle");
            if (observation.ShouldReviewActivePlay(scene.AddSeconds(12),scene,5))
                throw new Exception("Static gameplay should not trigger a periodic review");
            using (var latest = new ScreenFrame(Fixture(Color.DarkBlue), scene.AddSeconds(12)))
            using (var attachment = observation.Attach(latest))
                if (!attachment.Description.Contains("共 2 帧") || attachment.LatestAt != latest.At)
                    throw new Exception("Game recap lost its preceding scene");
            using (var image = Fixture(Color.Orange)) {
                var exploration = ScreenFrames.Fingerprint(image);
                for (int i = 2; i < exploration.Length; i += 3) exploration[i] -= 12;
                using (var changed = new ScreenFrame(new Bitmap(image), scene.AddSeconds(25), exploration))
                    observation.Observe(changed);
                if (observation.Event != 1 || observation.Opportunity != 2)
                    throw new Exception("Exploration should prompt review without resetting the scene");
                using (var unchanged = new ScreenFrame(new Bitmap(image), scene.AddSeconds(40), exploration))
                    observation.Observe(unchanged);
                if (observation.Opportunity != 2)
                    throw new Exception("Static gameplay should not prompt repeated reviews");
                var detail = (float[])exploration.Clone();
                for (int y=0;y<6;y++)for(int x=0;x<8;x++)detail[(y*64+x)*3+2]-=60;
                for (int i=0;i<=3;i++) {
                    using (var frame = new ScreenFrame(new Bitmap(image),scene.AddSeconds(41).AddMilliseconds(i*250),detail))
                        observation.Observe(frame);
                    if(i<3 && observation.Opportunity!=2)
                        throw new Exception("Single-frame HUD changes should not trigger analysis");
                }
                if(observation.Opportunity!=3||observation.Event!=1)
                    throw new Exception("Persistent HUD change was lost in the full-frame average");
            }
        }
        ScreenMemoryChecks.Run();GameGuideChecks.Run();
        using(var raw=Fixture(Color.White))using(var prepared=DesktopCapture.Prepare(raw,new Rectangle(0,0,960,540),new[]{new Rectangle(0,0,960,540)},960)){if(prepared.Pixels.GetPixel(500,300).ToArgb()!=Color.White.ToArgb()||prepared.Grid.Any(x=>x!=0))throw new Exception("Full-screen pixels were masked, or self-animation was not excluded from change detection");}
        if(!CodexWebOutput.ImageFailure("Codex could not read the local image at -")||!CodexWebOutput.ImageFailure("unable to attach local image")||CodexWebOutput.ImageFailure("temporary telemetry unavailable"))throw new Exception("Codex image diagnostic detection failed");
        var failedImage=new CodexWebOutput();failedImage.Accept("{\"type\":\"error\",\"message\":\"unable to attach local image\"}",null);if(!failedImage.ImageReadFailed)throw new Exception("Partial image failure ignored");
        if(ScreenPolicy.CanSample(false,true,true,false)||ScreenPolicy.CanSample(true,false,true,false)||ScreenPolicy.CanSample(true,true,false,false)||ScreenPolicy.CanSample(true,true,true,true))throw new Exception("Screen consent / lock gate failed");
        if(!ScreenPolicy.CanSample(true,true,true,false))throw new Exception("Screen sampling gate blocked valid capture");
        if(ScreenPolicy.CanPublish(false,true,true,true,false)||ScreenPolicy.CanPublish(true,false,true,true,false)||ScreenPolicy.CanPublish(true,true,true,false,false)||ScreenPolicy.CanPublish(true,true,true,true,true)||!ScreenPolicy.CanPublish(true,true,true,true,false))throw new Exception("Late visual output is not suppressed");
        if(Json.Stringify(new {image=new string('A',2300000)}).Length<2300000)throw new Exception("Image JSON truncated");
        using(var masked=Fixture(Color.White)){DesktopCapture.ApplyMasks(masked,new Rectangle(-1920,0,960,540),new[]{new Rectangle(-1900,20,100,100)});if(masked.GetPixel(25,25).ToArgb()!=Color.Black.ToArgb()||masked.GetPixel(5,5).ToArgb()!=Color.White.ToArgb())throw new Exception("Negative-monitor privacy mask failed");}
        var now=DateTime.UtcNow;if(ScreenPolicy.ShouldAnalyze(false,now,now.AddHours(-1),60)||ScreenPolicy.ShouldAnalyze(true,now,now.AddSeconds(-10),60)||!ScreenPolicy.ShouldAnalyze(true,now,now.AddSeconds(-61),60))throw new Exception("Screen cooldown failed");
        using(var buffer=new ScreenBuffer()){
            if(!buffer.Push(new ScreenFrame(Fixture(Color.DarkBlue),now)))throw new Exception("Initial scene missing");
            for(int i=1;i<=20;i++)if(buffer.Push(new ScreenFrame(Fixture(Color.DarkBlue),now.AddSeconds(i*2))))throw new Exception("Static scene was duplicated");
            buffer.Push(new ScreenFrame(Fixture(Color.DarkRed),now.AddSeconds(42)));buffer.Push(new ScreenFrame(Fixture(Color.DarkGreen),now.AddSeconds(44)));if(buffer.Count!=2)throw new Exception("Unbounded screen history");
            using(var image=buffer.Take(false)){
                using(var stream=new MemoryStream(image.Png))using(var decoded=new Bitmap(stream))if(decoded.Width!=960||decoded.Height!=1132)throw new Exception("Chronological montage failed");
                int calls=0;var turns=new List<Dictionary<string,string>> {new Dictionary<string,string>{{"role","user"},{"content","Explain this synthetic screenshot"}}};
                var answer=await DeepSeekWebChat.Run("deepseek-flash",Brain.Persona("DeepSeek","deepseek-flash",false,true),turns,true,(path,body,ct)=>{
                    calls++;var request=(Dictionary<string,object>)body;if(path!="/chat/completions"||request.ContainsKey("tools"))throw new Exception("Screen data can reach web tools");
                    var messages=(List<Dictionary<string,object>>)request["messages"];var content=(object[])messages.Last()["content"];var parsed=Json.Parse<Dictionary<string,object>>(Json.Stringify(content[1]));var url=(Dictionary<string,object>)parsed["image_url"];if(!url["url"].ToString().StartsWith("data:image/png;base64,"))throw new Exception("Image request missing");
                    return Task.FromResult("{\"choices\":[{\"message\":{\"content\":\"Synthetic screen analyzed.\"}}]}");
                },null,CancellationToken.None,image);
                if(calls!=1||answer.Answer!="Synthetic screen analyzed."||turns[0]["content"]!="Explain this synthetic screenshot")throw new Exception("Screenshot retained in text history");
            }
            buffer.Clear();if(buffer.Count!=0)throw new Exception("Screen buffer not cleared");
        }
        if(!Brain.Persona("Codex","",false,true).Contains("本轮附有用户授权")||Brain.Persona("Codex","",false,false).Contains("本轮附有用户授权"))throw new Exception("Visual capability claim mismatch");
    }
    public static async Task TestCodex(){
        using(var frame=new ScreenFrame(Fixture(Color.FromArgb(25,35,55),"SYNTHETIC TEST\nBuild status: FAILED\nError: missing return statement"),DateTime.UtcNow)){
            var image=ScreenAttachment.Compose(new[]{frame});
            // This synthetic fixture is deliberately retained locally; no real screen is captured.
            var answer=await new Brain().Ask("Codex","","",new List<Dictionary<string,string>>{new Dictionary<string,string>{{"role","user"},{"content","Only inspect the attached synthetic test image. State its build status and error in one short Chinese sentence. Do not call tools."}}},CancellationToken.None,false,null,image);
            if(CodexWebOutput.ImageFailure(answer)||answer.IndexOf("读取失败",StringComparison.Ordinal)>=0)throw new Exception("Synthetic image returned a partial-read warning");
            File.WriteAllText(Path.Combine(Program.Root,"preview","screen-codex-check.json"),Json.Stringify(new{syntheticImage=true,realDesktopCaptured=false,imageReadFailure=false,answer=answer}));Array.Clear(image.Png,0,image.Png.Length);
        }
    }
}
public sealed partial class Pet {
    public void TestScreenControls(){
        if(screenEnabled)throw new Exception("Screen access restored on startup");prefs.ScreenGame=true;prefs.ScreenWork=true;prefs.ScreenSeconds=5;OpenPanel();var dir=Path.Combine(Program.Root,"preview");Directory.CreateDirectory(dir);Render((System.Windows.FrameworkElement)panel.Content,Path.Combine(dir,"screen-settings.png"),540,740);showScreenSettings();
        screenEnabled=true;screenFrames.Push(new ScreenFrame(ScreenChecks.Fixture(Color.DarkBlue),DateTime.UtcNow));screenCancel=new CancellationTokenSource();var token=screenCancel.Token;gameGuideCancel=new CancellationTokenSource();var guideToken=gameGuideCancel.Token;int previous=screenEpoch;
        SetScreenEnabled(false);if(screenEnabled||!token.IsCancellationRequested||!guideToken.IsCancellationRequested||screenEpoch==previous||screenFrames.Count!=0||fairyFrame.ScreenWatching||screenSwitch.IsChecked==true)throw new Exception("Screen stop did not cancel screen / guide and clear data");gameGuideCancel.Dispose();gameGuideCancel=null;
        screenEnabled=true;screenFrames.Push(new ScreenFrame(ScreenChecks.Fixture(Color.DarkGreen),DateTime.UtcNow));screenCancel=new CancellationTokenSource();token=screenCancel.Token;PauseScreenOnLock();if(!token.IsCancellationRequested||screenFrames.Count!=0||!screenEnabled)throw new Exception("Lock pause did not clear and cancel");screenCancel.Dispose();screenCancel=null;SetScreenEnabled(false);
        Render((System.Windows.FrameworkElement)panel.Content,Path.Combine(dir,"screen-settings.png"),540,740);
        showGameSettings();
        Render((System.Windows.FrameworkElement)panel.Content,Path.Combine(dir,"game-companion.png"),540,740);
        if(DesktopCapture.CaptureAttempts!=0)throw new Exception("Synthetic test unexpectedly invoked native screen capture");
        File.WriteAllText(Path.Combine(dir,"screen-check.json"),Json.Stringify(new{syntheticOnly=true,realDesktopCaptured=false,nativeCaptureAttempts=DesktopCapture.CaptureAttempts,defaultOff=true,stopCancels=true,clearsFrames=true,indicatorOff=true,providerImageContract=true,staticSceneDeduplication=true,boundedFrames=2,cooldown=true,negativeMonitorMask=true,lockStopsOutput=true}));
        tray.Dispose();
    }
}
}
