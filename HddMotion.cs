using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Threading;

namespace CodexFairy {
// One reversible timeline: frame first, then extension. Reversing never jumps to a new start value.
public sealed class HddMotion {
    readonly DispatcherTimer timer=new DispatcherTimer {Interval=TimeSpan.FromMilliseconds(16)};
    readonly Stopwatch clock=new Stopwatch();readonly Action<double,double> apply;readonly Action<bool> completed;
    double progress,target,last;readonly double duration;
    public bool Opening {get{return target==1;}}
    public double Progress {get{return progress;}}
    public static double Ease(double x){x=Math.Max(0,Math.Min(1,x));return x*x*(3-2*x);}
    public static double Frame(double p){return Ease(p/.30);}
    public static double Extension(double p){return Ease((p-.30)/.70);}
    public HddMotion(Action<double,double> apply,Action<bool> completed,double duration=.44){this.apply=apply;this.completed=completed;this.duration=duration;timer.Tick+=(s,e)=>{var now=clock.Elapsed.TotalSeconds;var elapsed=Math.Max(0,now-last);last=now;Advance(elapsed);};}
    public void Play(bool open){target=open?1:0;if(!SystemParameters.ClientAreaAnimation||progress==target){Snap(target);completed(open);return;}last=0;clock.Restart();timer.Start();}
    void Advance(double seconds){var step=seconds/duration;progress=target>progress?Math.Min(target,progress+step):Math.Max(target,progress-step);apply(Frame(progress),Extension(progress));if(progress==target){timer.Stop();clock.Stop();completed(target==1);}}
    public void Stop(){timer.Stop();clock.Stop();}
    public void Snap(double value){Stop();progress=target=Math.Max(0,Math.Min(1,value));apply(Frame(progress),Extension(progress));}
    public void Preview(double value){Snap(value);}
    public static void Verify(){
        int opened=0,closed=0;double f=0,r=0;var motion=new HddMotion((frame,extension)=>{f=frame;r=extension;if(extension>0&&frame<.999999)throw new Exception("Caption extends before frame is ready");},open=>{if(open)opened++;else closed++;});
        motion.target=1;motion.Advance(.10);if(f<=0||r!=0)throw new Exception("Frame-first opening failed");
        motion.Advance(.10);var before=motion.progress;if(f!=1||r<=0)throw new Exception("Extension stage failed");
        motion.target=0;motion.Advance(.04);if(motion.progress>=before||f!=1)throw new Exception("Interrupted closing jumped");
        motion.target=1;motion.Advance(.04);if(Math.Abs(motion.progress-before)>.000001)throw new Exception("Interrupted reopening jumped");
        motion.Advance(1);if(opened!=1||r!=1)throw new Exception("Opening completion failed");
        motion.target=0;motion.Advance(.33);if(r!=0||f>=1||f<=0)throw new Exception("Frame closes before extension is retracted");
        motion.Advance(1);if(closed!=1||f!=0||r!=0)throw new Exception("Closing completion failed");
        foreach(var right in new[]{false,true}){var clip=FairyHint.RevealRect(400,76,.4,right);if(Math.Abs((right?clip.Left:clip.Right)-(right?0:400))>.01)throw new Exception("Caption reveal origin failed");}
        foreach(var down in new[]{false,true}){var clip=FairyComposer.RevealRect(470,220,.4,down);if(Math.Abs((down?clip.Top:clip.Bottom)-(down?0:220))>.01)throw new Exception("Input reveal origin failed");}
    }
}
// Drawn behind the existing eye, never a second avatar.
public sealed class FairyFrame : FrameworkElement {
    double progress;
    public double Progress {get{return progress;}set{progress=value;InvalidateVisual();}}
    public bool ToRight;
    public bool ScreenWatching;
    protected override void OnRender(DrawingContext dc){DrawRing(dc,new Point(ActualWidth/2,ActualHeight/2),Math.Min(ActualWidth,ActualHeight)/2-3,progress);if(ScreenWatching)dc.DrawEllipse(Ui.Brush("#FFB545"),new Pen(Ui.Brush("#171717"),2),new Point(ActualWidth-10,10),5,5);}
    public static void DrawRing(DrawingContext dc,Point center,double radius,double progress){if(progress<=0||radius<1)return;var scale=.92+.08*progress;
        dc.PushOpacity(progress);dc.PushTransform(new ScaleTransform(scale,scale,center.X,center.Y));
        // The sprite has a translucent glow. A solid backing prevents a growing
        // desktop-colored annulus between its opaque pixels and the outer rim.
        var ring=new EllipseGeometry(center,radius,radius);
        dc.DrawGeometry(Ui.Brush("#101113"),null,ring);dc.DrawEllipse(null,new Pen(Ui.Brush("#090A0B"),5),center,radius-2,radius-2);dc.DrawEllipse(null,new Pen(Ui.Brush("#4B4C50"),2.5),center,radius-5,radius-5);dc.DrawEllipse(null,new Pen(Ui.Brush("#232426"),3),center,radius-10,radius-10);dc.Pop();dc.Pop();
    }
}
public sealed class HddInputSurface : Border {
    public bool TerminalShell;
    protected override void OnRender(DrawingContext dc){var w=ActualWidth;var h=ActualHeight;if(w<120||h<80)return;
        var shell=new StreamGeometry();using(var p=shell.Open()){p.BeginFigure(new Point(7,47),true,true);p.BezierTo(new Point(7,17),new Point(12,5),new Point(39,5),true,false);p.LineTo(new Point(149,5),true,false);p.LineTo(new Point(165,21),true,false);p.LineTo(new Point(w-32,21),true,false);p.BezierTo(new Point(w-9,21),new Point(w-7,34),new Point(w-7,47),true,false);p.LineTo(new Point(w-7,h-30),true,false);p.BezierTo(new Point(w-7,h-9),new Point(w-20,h-7),new Point(w-32,h-7),true,false);p.LineTo(new Point(32,h-7),true,false);p.BezierTo(new Point(9,h-7),new Point(7,h-20),new Point(7,h-30),true,false);}
        if(!TerminalShell)dc.DrawGeometry(Ui.Brush("#090909"),new Pen(Ui.Brush("#080909"),6),shell);
        dc.DrawGeometry(new LinearGradientBrush(Color.FromRgb(48,48,48),Color.FromRgb(28,28,28),90),new Pen(Ui.Brush("#555654"),2),shell);
        if(TerminalShell)return;
        dc.DrawRoundedRectangle(Ui.Brush("#111111"),new Pen(Ui.Brush("#080808"),4),new Rect(16,56,w-32,h-70),17,17);
        dc.PushClip(new RectangleGeometry(new Rect(19,60,w-38,h-77),13,13));
        dc.DrawRectangle(new LinearGradientBrush(Color.FromRgb(29,29,29),Color.FromRgb(24,24,24),90),null,new Rect(19,60,w-38,h-106));
        dc.DrawRectangle(Ui.Brush("#101010"),null,new Rect(19,h-47,w-38,30));dc.DrawLine(new Pen(Ui.Brush("#060606"),3),new Point(20,h-48),new Point(w-20,h-48));dc.Pop();
        dc.DrawLine(new Pen(Ui.Brush("#252625"),1),new Point(24,49),new Point(w-24,49));
    }
}
public sealed partial class Pet {
    readonly FairyFrame fairyFrame=new FairyFrame {IsHitTestVisible=false};double captionFrame,inputFrame;
    HddMotion petMotion;
    double entranceFrame;
    bool exitAfterMotion;
    void RefreshFairyFrame(){fairyFrame.Progress=Math.Max(entranceFrame,Math.Max(captionFrame,inputFrame));}
    void SetFrame(bool caption,double value){if(caption)captionFrame=value;else inputFrame=value;RefreshFairyFrame();}
    void InitializePetMotion(Grid root){
        var aperture=new ScaleTransform(1,1);
        eye.RenderTransformOrigin=new Point(.5,.5);
        eye.RenderTransform=aperture;
        petMotion=new HddMotion((rim,opening)=>{
            entranceFrame=rim*(1-opening);
            RefreshFairyFrame();
            aperture.ScaleX=.76+.24*opening;
            aperture.ScaleY=.06+.94*opening;
            eye.Opacity=opening;
            root.IsHitTestVisible=petMotion.Opening&&opening>=.98;
        },open=>{
            if(open){Touch();return;}
            if(exitAfterMotion)Close();
            else Hide();
        },.52);
        petMotion.Snap(Program.Preview?1:0);
    }
    void ShowPet(){
        if(exitAfterMotion)return;
        if(!IsVisible)Show();
        petMotion.Play(true);
    }
    void RetractPet(bool exit){
        if(exitAfterMotion)return;
        exitAfterMotion=exit;
        if(!IsVisible){if(exit)Close();return;}
        SetScreenEnabled(false);
        if(hint!=null)hint.Dismiss();
        if(composer!=null)composer.Dismiss();
        petMotion.Play(false);
    }
    void RenderHddMotion(string dir){
        HddMotion.Verify();var stages=new[]{0.0,.12,.28,.40,.60,.80,1.0,.80,.60,.40,.28,.12,0.0};
        foreach(var right in new[]{false,true})for(int i=0;i<stages.Length;i++){
            var p=stages[i];var scene=new Canvas {Background=Ui.Brush("#151619")};double px=right?90:800,py=55;var pet=new Rect(px,py,204,204);
            var rim=new FairyFrame {Progress=HddMotion.Frame(p),Width=204,Height=204};Canvas.SetLeft(rim,px);Canvas.SetTop(rim,py);scene.Children.Add(rim);
            var image=new Image {Source=frames[0],Width=180,Height=180};Canvas.SetLeft(image,px+12);Canvas.SetTop(image,py+12);scene.Children.Add(image);
            var notice=new FairyHint();var bounds=notice.PreviewAttached("主人，我在。请下达指令。",pet,new Rect(0,0,1100,490));notice.PreviewMotion(p,right);var caption=(FrameworkElement)notice.Content;notice.Content=null;caption.Width=notice.Width;caption.Height=76;Canvas.SetLeft(caption,bounds.Left);Canvas.SetTop(caption,bounds.Top);scene.Children.Add(caption);
            var entry=new FairyComposer(text=>{},text=>{},()=>{},()=>{},()=>{});entry.Preview();entry.PreviewMotion(p,true);var input=(FrameworkElement)entry.Content;entry.Content=null;input.Width=470;input.Height=220;Canvas.SetLeft(input,Math.Max(12,Math.Min(px+102-235,618)));Canvas.SetTop(input,pet.Bottom+8);scene.Children.Add(input);
            Render(scene,System.IO.Path.Combine(dir,"hdd-"+(right?"right":"left")+"-"+i.ToString("00")+".png"),1100,490);
            if(!right&&p==1)Render(scene,System.IO.Path.Combine(dir,"hdd-scene.png"),1100,490);
        }
        var above=new FairyComposer(text=>{},text=>{},()=>{},()=>{},()=>{});above.Preview();above.PreviewMotion(.6,false);Render((FrameworkElement)above.Content,System.IO.Path.Combine(dir,"hdd-input-above.png"),470,220);
        System.IO.File.WriteAllText(System.IO.Path.Combine(dir,"motion-check.json"),Json.Stringify(new{frameBeforeExtension=true,extensionBeforeFrameOnClose=true,reversibleInterruption=true,leftAndRightOrigins=true,inputAboveAndBelowOrigins=true,stages=stages}));
    }
    void RenderJoinScenes(string dir){
        int verified=0;
        foreach(var size in new[]{32,48,64,96,180,360})foreach(var right in new[]{false,true})foreach(var bottom in new[]{false,true}){
            var area=new Rect(0,0,1280,600);double side=size+24;var pet=new Rect(right?8:1280-side-8,bottom?600-side-8:8,side,side);double radius=side/2-3,cx=pet.Left+side/2,cy=pet.Top+side/2;
            var scene=new Canvas {Background=Brushes.White};var rim=new FairyFrame {Width=side,Height=side,Progress=1};Canvas.SetLeft(rim,pet.Left);Canvas.SetTop(rim,pet.Top);scene.Children.Add(rim);
            var image=new Image {Source=frames[0],Width=size,Height=size};Canvas.SetLeft(image,pet.Left+12);Canvas.SetTop(image,pet.Top+12);scene.Children.Add(image);
            var notice=new FairyHint();var bounds=notice.PreviewAttached("主人，我在。请下达指令。",pet,area);notice.PreviewMotion(1,right);if(!notice.TextFits)throw new Exception("Attached caption text overflow");
            var caption=(FrameworkElement)notice.Content;notice.Content=null;caption.Width=notice.Width;caption.Height=76;Canvas.SetLeft(caption,bounds.Left);Canvas.SetTop(caption,bounds.Top);scene.Children.Add(caption);
            var bitmap=Render(scene,System.IO.Path.Combine(dir,"join-"+size+"-"+(right?"right":"left")+"-"+(bottom?"bottom":"top")+".png"),1280,600);var pixels=new byte[1280*600*4];bitmap.CopyPixels(pixels,1280*4,0);double half=Math.Min(56,Math.Max(32,2*(radius-6)))/2;
            if(size>=180){double checkRadius=(size*.45+radius-12)/2;int gapX=(int)Math.Round(cx+(right?-checkRadius:checkRadius)),gapY=(int)Math.Round(cy),gapIndex=(gapY*1280+gapX)*4;if(pixels[gapIndex]>240&&pixels[gapIndex+1]>240&&pixels[gapIndex+2]>240)throw new Exception("Transparent annulus at large pet size: "+size);}
            for(int y=(int)Math.Ceiling(cy-half+5);y<cy+half-5;y+=2)for(int offset=2;offset<=radius+10;offset+=2){int x=(int)Math.Round(cx+(right?offset:-offset));double distance=Math.Sqrt((x-cx)*(x-cx)+(y-cy)*(y-cy));if(distance<radius-5)continue;int i=(y*1280+x)*4;if(pixels[i]>240&&pixels[i+1]>240&&pixels[i+2]>240)throw new Exception("White gap at caption join: "+size+" / "+right+" / "+bottom+" / "+x+","+y);}
            verified++;
        }
        System.IO.File.WriteAllText(System.IO.Path.Combine(dir,"join-check.json"),Json.Stringify(new{whiteBackground=true,scenes=verified,sizes=new[]{32,48,64,96,180,360},leftAndRight=true,topAndBottomEdges=true,noWhiteJoinPixels=true,solidBackingAtLargeSizes=true}));
    }
}
}
