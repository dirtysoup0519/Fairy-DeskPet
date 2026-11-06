using System;
using System.Collections.Generic;
using System.IO;
using System.Globalization;
using System.Diagnostics;
using System.Windows.Media.Animation;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;

namespace CodexFairy {
// The clipped card silhouette and dialogue identifier are inspired by H.D.D.
public sealed class CutCard : Border {
    public Brush Fill=Ui.Brush("#111A24"), Stroke=Ui.Brush("#344658");
    protected override void OnRender(DrawingContext dc) {
        var w=ActualWidth;var h=ActualHeight;if(w<1||h<1)return;var n=Math.Min(12,Math.Min(w,h)/3);
        var shape=new StreamGeometry();using(var path=shape.Open()) {path.BeginFigure(new Point(.5,.5),true,true);path.LineTo(new Point(w-n,.5),true,false);path.LineTo(new Point(w-.5,n),true,false);path.LineTo(new Point(w-.5,h-.5),true,false);path.LineTo(new Point(n,h-.5),true,false);path.LineTo(new Point(.5,h-n),true,false);}
        dc.DrawGeometry(Fill,new Pen(Stroke,1),shape);
    }
}
public static class Ui {
    internal static TextBlock FairyLabel(string text,int size) {
        var label=Text(text,size,"#F4F3EE");
        label.FontFamily=new FontFamily("Segoe UI");
        label.FontWeight=FontWeights.Black;
        return label;
    }
    public static SolidColorBrush Brush(string hex){var b=new SolidColorBrush((Color)ColorConverter.ConvertFromString(hex));b.Freeze();return b;}
    public static TextBlock Text(string text,int size=13,string color="#DAE6ED") {return new TextBlock {Text=text,FontSize=size,Foreground=Brush(color),FontFamily=new FontFamily("Microsoft YaHei UI"),TextWrapping=TextWrapping.Wrap};}
    public static readonly Style ButtonStyle=(Style)XamlReader.Parse(@"<Style xmlns='http://schemas.microsoft.com/winfx/2006/xaml/presentation' xmlns:x='http://schemas.microsoft.com/winfx/2006/xaml' TargetType='Button'>
      <Setter Property='Foreground' Value='#DAE6ED'/><Setter Property='Background' Value='#182631'/><Setter Property='BorderBrush' Value='#3A4D5B'/><Setter Property='BorderThickness' Value='1'/><Setter Property='Padding' Value='13,9'/><Setter Property='FontSize' Value='13'/><Setter Property='Cursor' Value='Hand'/>
      <Setter Property='Template'><Setter.Value><ControlTemplate TargetType='Button'><Border x:Name='Plate' CornerRadius='3' Background='{TemplateBinding Background}' BorderBrush='{TemplateBinding BorderBrush}' BorderThickness='{TemplateBinding BorderThickness}' Padding='{TemplateBinding Padding}'><ContentPresenter HorizontalAlignment='Center' VerticalAlignment='Center'/></Border><ControlTemplate.Triggers>
      <Trigger Property='IsMouseOver' Value='True'><Setter TargetName='Plate' Property='BorderBrush' Value='#78D5F1'/></Trigger><Trigger Property='IsPressed' Value='True'><Setter TargetName='Plate' Property='Opacity' Value='.65'/></Trigger><Trigger Property='IsEnabled' Value='False'><Setter TargetName='Plate' Property='Opacity' Value='.4'/></Trigger><Trigger Property='IsKeyboardFocused' Value='True'><Setter TargetName='Plate' Property='BorderBrush' Value='#D2F566'/></Trigger></ControlTemplate.Triggers></ControlTemplate></Setter.Value></Setter></Style>");
    public static readonly ResourceDictionary Controls=(ResourceDictionary)XamlReader.Parse(@"<ResourceDictionary xmlns='http://schemas.microsoft.com/winfx/2006/xaml/presentation' xmlns:x='http://schemas.microsoft.com/winfx/2006/xaml'>
      <Style TargetType='Slider'><Setter Property='Height' Value='22'/><Setter Property='Template'><Setter.Value><ControlTemplate TargetType='Slider'><Grid Background='Transparent'><Track x:Name='PART_Track' Minimum='{TemplateBinding Minimum}' Maximum='{TemplateBinding Maximum}' Value='{Binding Value, RelativeSource={RelativeSource TemplatedParent}, Mode=TwoWay}'>
      <Track.DecreaseRepeatButton><RepeatButton Command='Slider.DecreaseLarge' Focusable='False'><RepeatButton.Template><ControlTemplate><Border Height='4' Background='#FFC844' CornerRadius='2'/></ControlTemplate></RepeatButton.Template></RepeatButton></Track.DecreaseRepeatButton>
      <Track.Thumb><Thumb Width='14' Height='14'><Thumb.Template><ControlTemplate><Border Background='#FFDC7E' BorderBrush='#A18136' BorderThickness='1' CornerRadius='2'/></ControlTemplate></Thumb.Template></Thumb></Track.Thumb>
      <Track.IncreaseRepeatButton><RepeatButton Command='Slider.IncreaseLarge' Focusable='False'><RepeatButton.Template><ControlTemplate><Border Height='4' Background='#454543' CornerRadius='2'/></ControlTemplate></RepeatButton.Template></RepeatButton></Track.IncreaseRepeatButton>
      </Track></Grid></ControlTemplate></Setter.Value></Setter></Style>
      <Style TargetType='ScrollBar'><Setter Property='Width' Value='7'/><Setter Property='Background' Value='Transparent'/><Setter Property='Template'><Setter.Value><ControlTemplate TargetType='ScrollBar'><Grid Background='{TemplateBinding Background}'><Track x:Name='PART_Track' Orientation='Vertical' IsDirectionReversed='True' Minimum='{TemplateBinding Minimum}' Maximum='{TemplateBinding Maximum}' ViewportSize='{TemplateBinding ViewportSize}' Value='{Binding Value, RelativeSource={RelativeSource TemplatedParent}, Mode=TwoWay}'>
      <Track.DecreaseRepeatButton><RepeatButton Command='ScrollBar.PageUpCommand' Opacity='0' Focusable='False'/></Track.DecreaseRepeatButton>
      <Track.Thumb><Thumb MinHeight='20'><Thumb.Template><ControlTemplate><Border Background='#62615C' CornerRadius='3' Margin='1'/></ControlTemplate></Thumb.Template></Thumb></Track.Thumb>
      <Track.IncreaseRepeatButton><RepeatButton Command='ScrollBar.PageDownCommand' Opacity='0' Focusable='False'/></Track.IncreaseRepeatButton>
      </Track></Grid></ControlTemplate></Setter.Value></Setter></Style>
      </ResourceDictionary>");
    public static Button Button(string text,Action action) {var b=new Button {Content=text,Style=ButtonStyle,Margin=new Thickness(0,0,8,0)};b.Click+=(s,e)=>action();return b;}
    public static TextBlock Caption(string text) {var cjk=text.Any(c=>c>127);var t=Text(text,cjk?11:10,"#8196A6");t.FontFamily=new FontFamily(cjk?"Microsoft YaHei UI":"Consolas");return t;}
    public static CheckBox Check(string text,bool value,Action<bool> action){var box=new CheckBox {Content=new TextBlock {Text=text,TextWrapping=TextWrapping.Wrap},IsChecked=value,Foreground=Brush("#D9E5EB"),FontSize=13,Margin=new Thickness(0,5,0,9),VerticalContentAlignment=VerticalAlignment.Center};System.Windows.Automation.AutomationProperties.SetName(box,text);box.Click+=(s,e)=>action(box.IsChecked==true);return box;}
    public static TextBox Box(string text,int height=0){return new TextBox {Text=text,Height=height==0?double.NaN:height,TextWrapping=TextWrapping.Wrap,AcceptsReturn=true,VerticalScrollBarVisibility=ScrollBarVisibility.Auto,HorizontalScrollBarVisibility=ScrollBarVisibility.Disabled,Padding=new Thickness(12),Background=Brush("#0D151E"),Foreground=Brush("#EAF2F5"),CaretBrush=Brush("#80DAF6"),BorderBrush=Brush("#364C60"),FontSize=14,FontFamily=new FontFamily("Microsoft YaHei UI")};}
    public static StackPanel Section(string title,string subtitle){
        var section=new StackPanel();
        var heading=Text(title,17,"#F4F3EE");
        heading.FontWeight=FontWeights.SemiBold;
        section.Children.Add(heading);
        var description=Text(subtitle,12,"#BEBDB6");
        description.LineHeight=19;
        description.Margin=new Thickness(0,4,0,12);
        section.Children.Add(description);
        return section;
    }
}
// The bar grows out of the existing Fairy frame; there is no duplicate portrait or bubble tail.
public sealed class HddSpeechSurface : Border {
    public bool OpensRight;public double FrameRadius,FrameCenterX,FrameCenterY;double reveal=1;
    public double Reveal {get{return reveal;}set{reveal=value;InvalidateVisual();}}
    protected override void OnRender(DrawingContext dc){var width=ActualWidth*reveal;var h=ActualHeight;if(width<1||h<30)return;var x=OpensRight?0:ActualWidth-width;double barHeight=FrameRadius>0?Math.Min(h-20,Math.Max(32,2*(FrameRadius-6))):h-20;var y=(FrameRadius>0?FrameCenterY:h/2)-barHeight/2;var r=Math.Min(27,width/2);var rect=new Rect(x,y,width,barHeight);
        // Extend under the rim, but keep the existing eye's pixels transparent.
        if(FrameRadius>0){double hole=Math.Max(1,FrameRadius-8);dc.PushClip(new CombinedGeometry(GeometryCombineMode.Exclude,new RectangleGeometry(new Rect(0,0,ActualWidth,h)),new EllipseGeometry(new Point(FrameCenterX,FrameCenterY),hole,hole)));}
        var shape=new StreamGeometry();using(var p=shape.Open()){
            if(OpensRight){p.BeginFigure(new Point(x,y),true,true);p.LineTo(new Point(x+width-r,y),true,false);p.ArcTo(new Point(x+width-r,y+rect.Height),new Size(r,rect.Height/2),0,false,SweepDirection.Clockwise,true,false);p.LineTo(new Point(x,y+rect.Height),true,false);}
            else {p.BeginFigure(new Point(x+width,y),true,true);p.LineTo(new Point(x+r,y),true,false);p.ArcTo(new Point(x+r,y+rect.Height),new Size(r,rect.Height/2),0,false,SweepDirection.Counterclockwise,true,false);p.LineTo(new Point(x+width,y+rect.Height),true,false);}}
        dc.DrawGeometry(Ui.Brush("#141416"),new Pen(Ui.Brush("#080809"),7),shape);dc.DrawGeometry(null,new Pen(Ui.Brush("#4A4A4D"),2.5),shape);
        dc.PushClip(shape);for(int col=0;col<width;col+=5)for(int row=0;row<barHeight;row+=5)if((col/5+row/5)%2==0)dc.DrawRectangle(Ui.Brush("#202023"),null,new Rect(x+col,y+row,2.5,2.5));dc.DrawLine(new Pen(Ui.Brush("#353538"),2),new Point(x+8,y+5),new Point(x+width-8,y+5));dc.Pop();
        // Repaint the exact same rim over the attached edge, hiding the rectangular cap.
        if(FrameRadius>0){FairyFrame.DrawRing(dc,new Point(FrameCenterX,FrameCenterY),FrameRadius,1);dc.Pop();}
    }
}
public sealed class FairyHint : Window {
    readonly TextBlock copy=Ui.Text("",21,"#F5F5F3");readonly HddSpeechSurface card=new HddSpeechSurface();readonly Grid body=new Grid();readonly HddMotion motion;
    readonly DispatcherTimer writer=new DispatcherTimer(),expiry=new DispatcherTimer(),advance=new DispatcherTimer();readonly Stopwatch clock=new Stopwatch();
    readonly List<string> pages=new List<string>();readonly List<double> cueStarts=new List<double>();readonly Stopwatch speechClock=new Stopwatch();int page,position;int[] ends=new int[0];string current="";bool followingSpeech,waitingForAudio;double speechSeconds,pageSeconds,maxWidth=650,joinInset;Window anchor;
    public FairyHint(Action<double,bool> frame=null){
        Title="Fairy · HDD 字幕";WindowStyle=WindowStyle.None;ResizeMode=ResizeMode.NoResize;AllowsTransparency=true;Background=Brushes.Transparent;ShowInTaskbar=Program.UiCheck;Topmost=true;ShowActivated=false;Width=490;Height=76;Content=card;card.Child=body;
        copy.FontStyle=FontStyles.Italic;copy.FontWeight=FontWeights.ExtraBold;copy.TextWrapping=TextWrapping.NoWrap;copy.VerticalAlignment=VerticalAlignment.Center;copy.Margin=new Thickness(27,0,27,0);body.Children.Add(copy);System.Windows.Automation.AutomationProperties.SetName(copy,"Fairy 字幕，点击展开或继续，右键收回");
        motion=new HddMotion((f,r)=>{card.Reveal=r;body.Clip=new RectangleGeometry(RevealRect(Width,Height,r,card.OpensRight));if(frame!=null)frame(f,card.OpensRight);},open=>{if(open)StartContent();else Hide();});
        writer.Interval=TimeSpan.FromMilliseconds(16);writer.Tick+=(s,e)=>UpdateTypewriter();advance.Tick+=(s,e)=>{advance.Stop();if(page+1<pages.Count){page++;SetPage();}};expiry.Tick+=(s,e)=>Dismiss();
        IsVisibleChanged+=(s,e)=>{if(!IsVisible){writer.Stop();expiry.Stop();advance.Stop();clock.Stop();followingSpeech=false;motion.Snap(0);}};
        MouseLeftButtonUp+=(s,e)=>{if(motion.Opening)Next();e.Handled=true;};MouseRightButtonUp+=(s,e)=>{Dismiss();e.Handled=true;};KeyDown+=(s,e)=>{if(e.Key==Key.Escape)Dismiss();};Closed+=(s,e)=>{motion.Snap(0);writer.Stop();expiry.Stop();advance.Stop();};
    }
    public static Rect RevealRect(double w,double h,double reveal,bool right){return new Rect(right?0:w*(1-reveal),0,Math.Max(0,w*reveal),h);}
    static int[] Elements(string text){return StringInfo.ParseCombiningCharacters(text);}
    public static List<string> Split(string text,int limit=26){var result=new List<string>();text=(text??"").Replace("\r","").Replace("\n"," ").Trim();while(text.Length>0){var ix=Elements(text);int count=Math.Min(limit,ix.Length),take=count<ix.Length?ix[count]:text.Length;if(count<ix.Length){int end=text.LastIndexOfAny(new[]{'。','！','？','；','，',',',';','!','?'},take-1,take);if(end>=take/3)take=end+1;}result.Add(text.Substring(0,take));text=text.Substring(take).TrimStart();}if(result.Count==0)result.Add("主人，我在。");return result;}
    int PageLimit(){return Math.Max(1,Math.Min(26,(int)((maxWidth-60)/16)));}
    double Measure(string text,double size){return new FormattedText(text,CultureInfo.CurrentUICulture,FlowDirection.LeftToRight,new Typeface(copy.FontFamily,copy.FontStyle,copy.FontWeight,copy.FontStretch),size,copy.Foreground,VisualTreeHelper.GetDpi(this).PixelsPerDip).WidthIncludingTrailingWhitespace;}
    List<string> LayoutPages(string text){var output=new List<string>();foreach(var item in Split(text,PageLimit())){var rest=item;while(Measure(rest,14)>maxWidth-60){var ix=Elements(rest);if(ix.Length<2)break;int n=ix.Length-1;while(n>1&&Measure(rest.Substring(0,ix[n]),14)>maxWidth-60)n--;output.Add(rest.Substring(0,ix[n]));rest=rest.Substring(ix[n]);}if(rest.Length>0)output.Add(rest);}return output;}
    void Fit(){if(anchor!=null)joinInset=Math.Max(0,anchor.Width/2-12);double size=21;while(size>14&&Measure(current,size)>maxWidth-60)size-=.5;copy.FontSize=size;Width=Math.Max(90,Math.Min(maxWidth,Math.Ceiling(Measure(current,size)+60)))+joinInset;Reposition();card.Reveal=HddMotion.Extension(motion.Progress);body.Clip=new RectangleGeometry(RevealRect(Width,Height,card.Reveal,card.OpensRight));}
    void Attach(Rect pet,Rect area,Rect bounds){card.OpensRight=DockLayout.OnRight(pet,area);card.FrameRadius=Math.Min(pet.Width,pet.Height)/2-3;card.FrameCenterX=pet.Left+pet.Width/2-bounds.Left;card.FrameCenterY=pet.Top+pet.Height/2-bounds.Top;copy.Margin=new Thickness(27+(card.OpensRight?joinInset:0),0,27+(card.OpensRight?0:joinInset),0);copy.RenderTransform=new TranslateTransform(0,card.FrameCenterY-Height/2);card.InvalidateVisual();}
    void Reposition(){if(anchor==null)return;var area=DockLayout.Area(anchor);var pet=DockLayout.Bounds(anchor);var bounds=DockLayout.Caption(pet,area,Width,Height);Attach(pet,area,bounds);Left=bounds.Left;Top=bounds.Top;}
    public void FollowAnchor(){if(anchor==null)return;maxWidth=DockLayout.CaptionWidth(DockLayout.Bounds(anchor),DockLayout.Area(anchor));if(Measure(current,14)<=maxWidth-60){Fit();return;}var pending=string.Concat(pages.Skip(page));pages.Clear();pages.AddRange(LayoutPages(pending));page=0;SetPage();writer.Stop();copy.Text=current;position=ends.Length;}
    public void Present(string text,Window owner,bool forSpeech){
        writer.Stop();advance.Stop();expiry.Stop();
        waitingForAudio=forSpeech;followingSpeech=false;speechClock.Stop();cueStarts.Clear();
        anchor=owner;maxWidth=DockLayout.CaptionWidth(DockLayout.Bounds(owner),DockLayout.Area(owner));
        pages.Clear();pages.AddRange(LayoutPages(text));page=0;SetPage();
        if(!IsVisible){motion.Snap(0);Show();}
        motion.Play(true);
    }
    public void Dismiss(){if(!IsVisible||!motion.Opening)return;writer.Stop();advance.Stop();expiry.Stop();motion.Play(false);}
    void SetPage(){current=pages[page];ends=Elements(current);position=0;copy.Text="";Fit();writer.Stop();advance.Stop();pageSeconds=followingSpeech?Math.Max(.65,speechSeconds*current.Length/Math.Max(1,pages.Sum(p=>p.Length))):Math.Max(2.8,ends.Length*.14);if(followingSpeech&&cueStarts.Count==pages.Count)pageSeconds=Math.Max(.05,(page+1<pages.Count?cueStarts[page+1]:speechSeconds)-speechClock.Elapsed.TotalSeconds);if(motion.Progress>=1&&motion.Opening)StartContent();}
    void StartContent(){
        clock.Restart();writer.Stop();
        if(SystemParameters.ClientAreaAnimation)writer.Start();else{copy.Text=current;position=ends.Length;}
        expiry.Stop();
        if(!waitingForAudio&&!followingSpeech){expiry.Interval=TimeSpan.FromSeconds(Math.Max(5,pageSeconds+1.8));expiry.Start();}
        advance.Stop();if(!waitingForAudio&&page+1<pages.Count){advance.Interval=TimeSpan.FromSeconds(pageSeconds);advance.Start();}
    }
    void UpdateTypewriter(){double budget=clock.Elapsed.TotalSeconds,used=0;int next=0;double step=Math.Min(.045,pageSeconds*.68/Math.Max(1,ends.Length));while(next<ends.Length){double cost=step*("，。！？；,.!?;".IndexOf(current[ends[next]])>=0?2.2:1);if(used+cost>budget)break;used+=cost;next++;}position=next;copy.Text=position==ends.Length?current:current.Substring(0,ends[position]);if(position==ends.Length)writer.Stop();}
    void Next(){if(position<ends.Length){position=ends.Length;copy.Text=current;writer.Stop();}else if(page+1<pages.Count){EndSpeech();page++;SetPage();}else Dismiss();}
    public void BeginSpeech(double seconds,List<VoiceCue> cues=null){if(!IsVisible||!motion.Opening||!waitingForAudio||pages.Count==0)return;waitingForAudio=false;speechSeconds=seconds;cueStarts.Clear();if(cues!=null&&cues.Count>0){pages.Clear();for(int i=0;i<cues.Count;i++){var pieces=LayoutPages(cues[i].Text);double start=i==0?0:cues[i].Start,end=i+1<cues.Count?cues[i+1].Start:seconds;int total=pieces.Sum(p=>p.Length),used=0;foreach(var piece in pieces){pages.Add(piece);cueStarts.Add(start+(end-start)*used/Math.Max(1,total));used+=piece.Length;}}}followingSpeech=true;speechClock.Restart();page=0;SetPage();}
    public void EndSpeech(){
        if(followingSpeech&&motion.Opening){writer.Stop();copy.Text=current;position=ends.Length;}
        waitingForAudio=false;followingSpeech=false;speechClock.Stop();cueStarts.Clear();advance.Stop();
        if(IsVisible&&motion.Opening){expiry.Stop();expiry.Interval=TimeSpan.FromSeconds(2.5);expiry.Start();}
    }
    public async System.Threading.Tasks.Task WaitForOpen(System.Threading.CancellationToken token){while(IsVisible&&motion.Opening&&motion.Progress<1){token.ThrowIfCancellationRequested();await System.Threading.Tasks.Task.Delay(16,token);}}
    public void Preview(string text=null,double availableWidth=650){anchor=null;joinInset=0;card.FrameRadius=0;copy.Margin=new Thickness(27,0,27,0);copy.RenderTransform=Transform.Identity;maxWidth=availableWidth;pages.Clear();pages.AddRange(LayoutPages(text??"主人，我在。请继续您的工作。"));page=0;SetPage();motion.Preview(1);writer.Stop();expiry.Stop();advance.Stop();copy.Text=current;}
    public Rect PreviewAttached(string text,Rect pet,Rect area){Preview(text,DockLayout.CaptionWidth(pet,area));joinInset=Math.Max(0,pet.Width/2-12);Width+=joinInset;var bounds=DockLayout.Caption(pet,area,Width,Height);Attach(pet,area,bounds);return bounds;}
    public void PreviewMotion(double progress,bool right){card.OpensRight=right;motion.Preview(progress);copy.Text=progress>=.8?current:"";}
    public void PreviewJoin(double radius,double centerX){card.FrameRadius=radius;card.FrameCenterX=centerX;card.FrameCenterY=Height/2;card.InvalidateVisual();}
    public double TextSize {get{return copy.FontSize;}}public bool TextFits {get{return Measure(current,copy.FontSize)<=Width-54-joinInset;}}
}
public sealed partial class Pet {
    FairyHint hint;Button connectionApply;ScrollViewer settingsScroll;Slider idleIntervalSlider;Action showSettings=()=>{},showConnection=()=>{},showAppearance=()=>{},showGameSettings=()=>{},refreshPetSize=()=>{};
    void SetPetSize(double value,bool save) {
        if(double.IsNaN(value)||double.IsInfinity(value))value=180;
        value=Math.Max(32,Math.Min(DockLayout.MaximumSize(DockLayout.Area(this)),Math.Round(value)));prefs.Scale=value;eye.Width=value;eye.Height=value;Width=value+24;Height=value+24;
        if(IsLoaded){Clamp();RepositionPopups();}refreshPetSize();if(save&&!Program.Preview)SavePosition();
    }
    static readonly Style TerminalButtonStyle=(Style)XamlReader.Parse(@"<Style xmlns='http://schemas.microsoft.com/winfx/2006/xaml/presentation' xmlns:x='http://schemas.microsoft.com/winfx/2006/xaml' TargetType='Button'>
      <Setter Property='Foreground' Value='#CDCDC7'/><Setter Property='Background' Value='#222222'/>
      <Setter Property='BorderBrush' Value='#555654'/><Setter Property='BorderThickness' Value='1'/>
      <Setter Property='Padding' Value='10,5'/><Setter Property='FontSize' Value='12'/><Setter Property='Cursor' Value='Hand'/>
      <Setter Property='Template'><Setter.Value><ControlTemplate TargetType='Button'>
        <Border x:Name='Plate' Background='{TemplateBinding Background}' BorderBrush='{TemplateBinding BorderBrush}' BorderThickness='{TemplateBinding BorderThickness}' Padding='{TemplateBinding Padding}' CornerRadius='3'>
          <ContentPresenter HorizontalAlignment='Center' VerticalAlignment='Center'/>
        </Border><ControlTemplate.Triggers>
          <Trigger Property='IsMouseOver' Value='True'><Setter TargetName='Plate' Property='Background' Value='#3A3937'/></Trigger>
          <Trigger Property='IsKeyboardFocused' Value='True'><Setter TargetName='Plate' Property='BorderBrush' Value='#FFC844'/></Trigger>
          <Trigger Property='IsPressed' Value='True'><Setter TargetName='Plate' Property='Opacity' Value='.68'/></Trigger>
          <Trigger Property='IsEnabled' Value='False'><Setter TargetName='Plate' Property='Opacity' Value='.45'/></Trigger>
        </ControlTemplate.Triggers></ControlTemplate></Setter.Value></Setter></Style>");
    static Button TerminalButton(string label,Action action) {
        var button=Ui.Button(label,action);button.Style=TerminalButtonStyle;return button;
    }
    static TextBlock TerminalCaption(string text) {
        var caption=Ui.Caption(text);caption.Foreground=Ui.Brush("#B8B7B2");return caption;
    }

    void OpenPanel(){
        Touch();if(panel!=null){if(!Program.Preview){panel.Show();panel.Activate();}return;}
        panel=new Window {Title="Fairy · H.D.D 对话终端",Width=540,Height=740,MinWidth=480,MinHeight=620,WindowStyle=WindowStyle.None,ResizeMode=ResizeMode.CanResize,AllowsTransparency=true,Background=Brushes.Transparent,Foreground=Brushes.White,WindowStartupLocation=WindowStartupLocation.CenterScreen,FontFamily=new FontFamily("Microsoft YaHei UI")};
        panel.Icon=Icon;
        System.Windows.Shell.WindowChrome.SetWindowChrome(panel,new System.Windows.Shell.WindowChrome {CaptionHeight=0,ResizeBorderThickness=new Thickness(13,10,13,13),GlassFrameThickness=new Thickness(0),UseAeroCaptionButtons=false});
        panel.Resources.MergedDictionaries.Add(Ui.Controls);
        var frame=new HddInputSurface {TerminalShell=true,Padding=new Thickness(17,5,17,11)};
        var layout=new Grid {UseLayoutRounding=true};frame.Child=layout;panel.Content=frame;
        foreach(var height in new[]{50.0,52,37,-1})layout.RowDefinitions.Add(new RowDefinition {Height=height<0?new GridLength(1,GridUnitType.Star):new GridLength(height)});
        var title=new DockPanel {LastChildFill=true};
        title.MouseLeftButtonDown+=(s,e)=>{if(e.ClickCount==1&&e.ButtonState==MouseButtonState.Pressed)panel.DragMove();};
        var close=TerminalButton("×",()=>{if(Program.UiCheck)Close();else panel.Close();});close.Width=36;close.Margin=new Thickness(0,20,5,4);close.Padding=new Thickness(0);close.BorderThickness=new Thickness(0);close.Background=Brushes.Transparent;DockPanel.SetDock(close,Dock.Right);title.Children.Add(close);
        var minimize=TerminalButton("—",()=>panel.WindowState=WindowState.Minimized);minimize.Width=36;minimize.Margin=new Thickness(0,20,3,4);minimize.Padding=new Thickness(0);minimize.BorderThickness=new Thickness(0);minimize.Background=Brushes.Transparent;DockPanel.SetDock(minimize,Dock.Right);title.Children.Add(minimize);
        var identity=Ui.FairyLabel("Fairy",22);
        identity.VerticalAlignment=VerticalAlignment.Center;
        identity.Margin=new Thickness(12,0,16,0);
        DockPanel.SetDock(identity,Dock.Left);title.Children.Add(identity);
        var titleDragArea=new Border {Background=Brushes.Transparent};
        title.Children.Add(titleDragArea);
        layout.Children.Add(title);
        providers=new ComboBox {ItemsSource=new[]{"Codex","DeepSeek"},SelectedItem=prefs.Provider};
        var switcher=new Grid {Margin=new Thickness(9,7,1,7)};switcher.ColumnDefinitions.Add(new ColumnDefinition());switcher.ColumnDefinitions.Add(new ColumnDefinition());switcher.ColumnDefinitions.Add(new ColumnDefinition{Width=GridLength.Auto});
        var codex=TerminalButton("CODEX / 订阅",()=>providers.SelectedItem="Codex");var deepseek=TerminalButton("DEEPSEEK / API",()=>{providers.SelectedItem="DeepSeek";if(apiKey!=null&&string.IsNullOrWhiteSpace(apiKey.Password))showConnection();});switcher.Children.Add(codex);Grid.SetColumn(deepseek,1);switcher.Children.Add(deepseek);var configureKey=TerminalButton("配置 Key",()=>{providers.SelectedItem="DeepSeek";showConnection();});configureKey.Padding=new Thickness(10,9,10,9);Grid.SetColumn(configureKey,2);switcher.Children.Add(configureKey);Grid.SetRow(switcher,1);layout.Children.Add(switcher);
        var tabs=new StackPanel {Orientation=Orientation.Horizontal,Margin=new Thickness(9,0,9,0)};Button chatTab=null,settingsTab=null;var chat=new Grid {Margin=new Thickness(9,8,9,3)};var settings=new ScrollViewer {Margin=new Thickness(11,0,0,0),VerticalScrollBarVisibility=ScrollBarVisibility.Auto};
        var settingsLayout=new Grid {Margin=new Thickness(9,8,6,8)};
        settingsLayout.ColumnDefinitions.Add(new ColumnDefinition {Width=new GridLength(105)});
        settingsLayout.ColumnDefinitions.Add(new ColumnDefinition());
        var navigation=new StackPanel();
        var navigationFrame=new Border {Child=navigation,Background=Ui.Brush("#191919"),
            BorderBrush=Ui.Brush("#454543"),BorderThickness=new Thickness(1),CornerRadius=new CornerRadius(7),
            Padding=new Thickness(6),VerticalAlignment=VerticalAlignment.Top};
        settingsLayout.Children.Add(navigationFrame);
        Grid.SetColumn(settings,1);settingsLayout.Children.Add(settings);
        Action<bool> selectTab=(isSettings)=>{chat.Visibility=isSettings?Visibility.Collapsed:Visibility.Visible;settingsLayout.Visibility=isSettings?Visibility.Visible:Visibility.Collapsed;chatTab.Foreground=Ui.Brush(isSettings?"#AAA9A3":"#FFC844");settingsTab.Foreground=Ui.Brush(isSettings?"#FFC844":"#AAA9A3");chatTab.BorderBrush=isSettings?Brushes.Transparent:Ui.Brush("#FFC844");settingsTab.BorderBrush=isSettings?Ui.Brush("#FFC844"):Brushes.Transparent;};
        chatTab=TerminalButton("对话",()=>selectTab(false));settingsTab=TerminalButton("设置",()=>selectTab(true));foreach(var tab in new[]{chatTab,settingsTab}){tab.Background=Brushes.Transparent;tab.Padding=new Thickness(14,5,14,8);tab.BorderThickness=new Thickness(0,0,0,2);tabs.Children.Add(tab);}Grid.SetRow(tabs,2);layout.Children.Add(tabs);Grid.SetRow(chat,3);Grid.SetRow(settingsLayout,3);layout.Children.Add(chat);layout.Children.Add(settingsLayout);showSettings=()=>selectTab(true);
        chat.RowDefinitions.Add(new RowDefinition {Height=GridLength.Auto});
        chat.RowDefinitions.Add(new RowDefinition());
        chat.RowDefinitions.Add(new RowDefinition {Height=GridLength.Auto});
        chat.RowDefinitions.Add(new RowDefinition {Height=GridLength.Auto});
        var providerNote=TerminalCaption("");providerNote.Margin=new Thickness(3,0,0,8);var taskHeader=new StackPanel();taskHeader.Children.Add(TaskBar());taskHeader.Children.Add(providerNote);chat.Children.Add(taskHeader);

        var reply=new CutCard {Fill=Ui.Brush("#151515"),Stroke=Ui.Brush("#555654"),Padding=new Thickness(14),Margin=new Thickness(0,0,0,10)};
        var replyBody=new DockPanel();
        var speaker=new DockPanel {Margin=new Thickness(0,0,0,9)};
        AddSourcesButton(speaker);sourceButton.Style=TerminalButtonStyle;
        var avatar=new Image {Source=frames[0],Width=25,Height=25,Margin=new Thickness(0,0,8,0)};
        DockPanel.SetDock(avatar,Dock.Left);speaker.Children.Add(avatar);
        var speakerName=Ui.FairyLabel("Fairy  /  回复",14);
        speakerName.VerticalAlignment=VerticalAlignment.Center;speaker.Children.Add(speakerName);
        DockPanel.SetDock(speaker,Dock.Top);replyBody.Children.Add(speaker);
        output=Ui.Box(lastAnswer.Length>0?lastAnswer:UserAddress+"，我在。输入消息即可开始对话。");
        output.IsReadOnly=true;output.Padding=new Thickness(0,3,4,0);output.BorderThickness=new Thickness(0);
        output.Background=Brushes.Transparent;output.Foreground=Ui.Brush("#F1F0EB");output.FontSize=15;output.MinHeight=80;
        replyBody.Children.Add(output);reply.Child=replyBody;Grid.SetRow(reply,1);chat.Children.Add(reply);

        var commandDeck=new Border {Background=Ui.Brush("#181818"),BorderBrush=Ui.Brush("#555654"),
            BorderThickness=new Thickness(1),CornerRadius=new CornerRadius(13),Padding=new Thickness(11,8,11,8)};
        var commandBody=new StackPanel();commandDeck.Child=commandBody;
        var commandHeading=new DockPanel {Margin=new Thickness(2,0,2,6)};
        var shortcut=TerminalCaption("Enter 换行 · Ctrl+Enter 发送");DockPanel.SetDock(shortcut,Dock.Right);commandHeading.Children.Add(shortcut);
        var commandTitle=Ui.FairyLabel("向 Fairy 发话",13);
        commandHeading.Children.Add(commandTitle);commandBody.Children.Add(commandHeading);

        var entry=new Grid();
        input=Ui.Box(CurrentTask.PanelDraft,76);input.MaxLength=4000;input.Background=Ui.Brush("#101010");input.Foreground=Ui.Brush("#F1F0EB");
        input.BorderBrush=Ui.Brush("#454543");input.CaretBrush=Ui.Brush("#FFC844");input.SelectionBrush=Ui.Brush("#666460");
        input.Padding=new Thickness(10,8,10,8);
        System.Windows.Automation.AutomationProperties.SetName(input,"向 Fairy 发话");
        input.PreviewKeyDown+=(s,e)=>{Touch();if(e.Key==Key.Enter&&Keyboard.Modifiers==ModifierKeys.Control){e.Handled=true;Ask();}};
        entry.Children.Add(input);
        var placeholder=Ui.Text("在这里输入你的指令…",14,"#AAA9A3");
        placeholder.FontStyle=FontStyles.Italic;placeholder.Margin=new Thickness(11,9,0,0);placeholder.IsHitTestVisible=false;
        entry.Children.Add(placeholder);placeholder.Visibility=input.Text.Length==0?Visibility.Visible:Visibility.Collapsed;
        input.TextChanged+=(s,e)=>placeholder.Visibility=input.Text.Length==0?Visibility.Visible:Visibility.Collapsed;
        commandBody.Children.Add(entry);

        var mainActions=new DockPanel {LastChildFill=false,Margin=new Thickness(0,7,0,0)};
        send=TerminalButton("发送 ▶",Ask);send.Foreground=Ui.Brush("#FFC844");send.FontWeight=FontWeights.Bold;
        send.FontStyle=FontStyles.Italic;send.MinWidth=82;send.Margin=new Thickness(6,0,0,0);
        DockPanel.SetDock(send,Dock.Right);mainActions.Children.Add(send);
        var stop=TerminalButton("停止",()=>{StopChat();StopSpeech();});DockPanel.SetDock(stop,Dock.Right);mainActions.Children.Add(stop);
        mainActions.Children.Add(TerminalButton("朗读输入",()=>Speak(input.Text,null)));
        mainActions.Children.Add(TerminalButton("重播回复",()=>{if(lastAnswer.Length>0)Speak(lastAnswer,null);}));
        commandBody.Children.Add(mainActions);Grid.SetRow(commandDeck,2);chat.Children.Add(commandDeck);
        readAnswers=Ui.Check("自动朗读回复",prefs.ReadAnswers,value=>{prefs.ReadAnswers=value;SaveUi();});
        readAnswers.Foreground=Ui.Brush("#CCCBC5");readAnswers.Margin=new Thickness(3,7,0,1);
        readAnswers.ToolTip="自动朗读回复前 600 字";Grid.SetRow(readAnswers,3);chat.Children.Add(readAnswers);
        settingsScroll=settings;
        var pageButtons=new List<Button>();
        Action<StackPanel,Button> showPage=(page,active)=>{
            selectTab(true);
            if(settings.Content!=page){settings.Content=page;settings.ScrollToTop();}
            foreach(var button in pageButtons){
                bool selected=button==active;
                button.Foreground=Ui.Brush(selected?"#FFC844":"#CDCDC7");
                button.Background=Ui.Brush(selected?"#383123":"#191919");
                button.BorderBrush=Ui.Brush(selected?"#FFC844":"#191919");
                button.FontWeight=selected?FontWeights.SemiBold:FontWeights.Normal;
            }
        };
        Func<string,UIElement[],Action> addPage=(caption,sections)=>{
            var body=new StackPanel {Margin=new Thickness(0,0,7,8)};
            foreach(var section in sections){
                var card=new Border {Child=section,Background=Ui.Brush("#202020"),BorderBrush=Ui.Brush("#454543"),
                    BorderThickness=new Thickness(1),CornerRadius=new CornerRadius(6),
                    Padding=new Thickness(12),Margin=new Thickness(0,0,0,10)};
                body.Children.Add(card);
            }
            Button button=null;
            Action open=()=>showPage(body,button);
            button=TerminalButton(caption,open);
            button.Margin=new Thickness(0,0,0,4);button.Padding=new Thickness(6,8,6,8);
            button.FontSize=13;
            navigation.Children.Add(button);pageButtons.Add(button);
            return open;
        };
        var sizeSection=Ui.Section("桌宠大小","APPEARANCE / 连续调节");var sizeLabel=Ui.Text(((int)prefs.Scale)+" px",20,"#FFC844");sizeSection.Children.Add(sizeLabel);
        var sizeSlider=new Slider {Minimum=32,Maximum=DockLayout.MaximumSize(DockLayout.Area(this)),Value=Math.Max(32,Math.Min(DockLayout.MaximumSize(DockLayout.Area(this)),prefs.Scale)),TickFrequency=1,IsSnapToTickEnabled=true,Margin=new Thickness(0,12,0,14)};System.Windows.Automation.AutomationProperties.SetName(sizeSlider,"桌宠尺寸，32 到 "+sizeSlider.Maximum+" 像素");sizeSlider.ValueChanged+=(s,e)=>{SetPetSize(sizeSlider.Value,false);sizeLabel.Text=((int)prefs.Scale)+" px";};sizeSlider.LostMouseCapture+=(s,e)=>{if(!Program.Preview)SavePosition();};sizeSlider.PreviewKeyUp+=(s,e)=>{if(!Program.Preview)SavePosition();};sizeSection.Children.Add(sizeSlider);
        refreshPetSize=()=>{sizeSlider.Maximum=DockLayout.MaximumSize(DockLayout.Area(this));if(sizeSlider.Value!=prefs.Scale)sizeSlider.Value=prefs.Scale;sizeLabel.Text=((int)prefs.Scale)+" px / 上限 "+sizeSlider.Maximum+" px";};refreshPetSize();
        var presets=new WrapPanel();foreach(var size in new[]{64,180,240}){var value=size;var preset=TerminalButton((size==64?"小":size==180?"中":"大")+" / "+size,()=>{sizeSlider.Value=value;SetPetSize(value,true);});preset.MinWidth=94;presets.Children.Add(preset);}sizeSection.Children.Add(presets);
        var voiceSection=Ui.Section("Fairy 语音","VOICE / 本机合成与原作片段");var volumeLabel=Ui.Text("音量 "+Math.Round(prefs.Volume*100)+"%",13);voiceSection.Children.Add(volumeLabel);var volume=new Slider {Minimum=0,Maximum=1,Value=prefs.Volume,Margin=new Thickness(0,12,0,16)};volume.ValueChanged+=(s,e)=>{prefs.Volume=volume.Value;if(player!=null)player.Volume=prefs.Volume;volumeLabel.Text="音量 "+Math.Round(prefs.Volume*100)+"%";};volume.LostMouseCapture+=(s,e)=>SaveUi();voiceSection.Children.Add(volume);
        var idleSection=Ui.Section("待机语音","开启屏幕观察并选择「待机陪伴」后，Fairy 会结合画面说一句。");
        idleSection.Children.Add(Ui.Check("空闲时播放待机语音",prefs.Idle,value=>{prefs.Idle=value;SaveUi();Touch();}));
        var intervalLabel=Ui.Text("待机间隔 "+prefs.IdleMinutes+" 分钟",13);idleSection.Children.Add(intervalLabel);
        var intervalSlider=new Slider {Minimum=1,Maximum=60,Value=prefs.IdleMinutes,TickFrequency=1,IsSnapToTickEnabled=true,Margin=new Thickness(0,10,0,12)};System.Windows.Automation.AutomationProperties.SetName(intervalSlider,"待机语音间隔，1 到 60 分钟");idleSection.Children.Add(intervalSlider);
        idleIntervalSlider=intervalSlider;
        var intervalRow=new WrapPanel {Margin=new Thickness(0,4,0,12)};var intervalButtons=new List<Button>();
        refreshIdle=()=>{intervalLabel.Text="待机间隔 "+prefs.IdleMinutes+" 分钟";if((int)intervalSlider.Value!=prefs.IdleMinutes)intervalSlider.Value=prefs.IdleMinutes;foreach(var b in intervalButtons){bool chosen=(int)b.Tag==prefs.IdleMinutes;b.BorderBrush=chosen?Ui.Brush("#FFC844"):Ui.Brush("#454543");b.Foreground=chosen?Ui.Brush("#FFC844"):Ui.Brush("#CDCDC7");}};
        intervalSlider.ValueChanged+=(s,e)=>SetIdleMinutes((int)Math.Round(intervalSlider.Value),true);
        foreach(var minutes in new[]{1,5,10,20,30}){var value=minutes;var b=TerminalButton(minutes+" 分钟",()=>SetIdleMinutes(value,true));b.Tag=value;b.Padding=new Thickness(10,7,10,7);b.Margin=new Thickness(0,0,8,6);intervalButtons.Add(b);intervalRow.Children.Add(b);}idleSection.Children.Add(intervalRow);refreshIdle();
        idleSection.Children.Add(Ui.Text("按所设间隔触发。正在对话或朗读时顺延。已导入原声 "+idleCatalog.Clips.Count+" 组，看屏待机只用合成语音。",12,"#BEBDB6"));
        var previews=new WrapPanel();previews.Children.Add(TerminalButton("试听 Fairy",()=>Speak(UserAddress+"，我是 "+(string.IsNullOrWhiteSpace(prefs.PersonaSelf)?"Fairy":prefs.PersonaSelf)+"。语音已就绪。",null)));previews.Children.Add(TerminalButton("试听原作",()=>Speak(null,Original())));previews.Children.Add(TerminalButton("预览提示",()=>SayBubble("主人，正在为您保持待命。需要帮助时，随时叫我。")));voiceSection.Children.Add(previews);
        var connection=Ui.Section("对话连接","CONNECTION / 每个入口独立保留上下文");var ds=new StackPanel();var keyLabel=Ui.Text("在这里粘贴 DeepSeek API Key",13,"#B9D9E8");keyLabel.Margin=new Thickness(0,0,0,7);ds.Children.Add(keyLabel);apiKey=new PasswordBox {Password=sessionKey,Padding=new Thickness(10),Background=Ui.Brush("#0D151E"),Foreground=Brushes.White,BorderBrush=Ui.Brush("#649DB7"),FontSize=15};System.Windows.Automation.AutomationProperties.SetName(apiKey,"DeepSeek API Key");ds.Children.Add(apiKey);var modelLabel=Ui.Text("模型名称",12,"#9FB7C7");modelLabel.Margin=new Thickness(0,12,0,7);ds.Children.Add(modelLabel);model=Ui.Box(prefs.DeepSeekModel,42);model.AcceptsReturn=false;model.Padding=new Thickness(10,8,10,8);model.VerticalScrollBarVisibility=ScrollBarVisibility.Disabled;ds.Children.Add(model);var keyNote=Ui.Text("粘贴 Key 后点击下方按钮。本次运行内保留，不写入文件。",11,"#8196A6");keyNote.Margin=new Thickness(0,8,0,0);ds.Children.Add(keyNote);
        var applyKey=TerminalButton("应用并返回对话",()=>{if(string.IsNullOrWhiteSpace(apiKey.Password))return;sessionKey=apiKey.Password.Trim();apiKey.Password=sessionKey;prefs.DeepSeekModel=model.Text.Trim();SaveUi();selectTab(false);providerNote.Text="DEEPSEEK / Key 已填写";SetStatus("DeepSeek Key 已应用，可发送消息。");});connectionApply=applyKey;applyKey.IsEnabled=!string.IsNullOrWhiteSpace(apiKey.Password);applyKey.Margin=new Thickness(0,12,0,0);applyKey.HorizontalAlignment=HorizontalAlignment.Left;ds.Children.Add(applyKey);apiKey.PasswordChanged+=(s,e)=>{applyKey.IsEnabled=!string.IsNullOrWhiteSpace(apiKey.Password);};connection.Children.Add(ds);
        var codexNote=Ui.Text("Codex 沿用本机 ChatGPT 登录。对话使用订阅额度，语音由本机 Fairy 模型合成。",13,"#A8BECC");codexNote.LineHeight=23;connection.Children.Add(codexNote);
        var showVoice=addPage("语音",new UIElement[]{voiceSection,idleSection});
        showAppearance=addPage("外观",new UIElement[]{sizeSection});
        showScreenSettings=addPage("屏幕协作",new UIElement[]{ScreenSettings()});
        showGameSettings=addPage("游戏陪伴",new UIElement[]{
            GameSettings()
        });
        addPage("人设",new UIElement[]{PersonaSettings()});
        showTaskSettings=addPage("任务与记忆",new UIElement[]{TaskSettings(),ScreenMemorySettings()});
        var showConnectionPage=addPage("模型与联网",new UIElement[]{connection,WebSettings()});
        showVoice();
        showConnection=()=>{showConnectionPage();settings.ScrollToTop();if(!Program.Preview&&apiKey.IsVisible)apiKey.Focus();};
        Action updateProvider=()=>{prefs.Provider=(string)providers.SelectedItem;if(composer!=null)composer.UpdateProvider(prefs.Provider);bool useCodex=prefs.Provider=="Codex";codex.BorderBrush=Ui.Brush(useCodex?"#FFC844":"#555654");deepseek.BorderBrush=Ui.Brush(useCodex?"#555654":"#FFC844");codex.Foreground=Ui.Brush(useCodex?"#FFC844":"#CDCDC7");deepseek.Foreground=Ui.Brush(useCodex?"#CDCDC7":"#FFC844");providerNote.Text=useCodex?"CODEX / 本机 ChatGPT 登录":string.IsNullOrWhiteSpace(apiKey.Password)?"DEEPSEEK / 请先点击「配置 Key」":"DEEPSEEK / Key 已填写";configureKey.Visibility=useCodex?Visibility.Collapsed:Visibility.Visible;ds.Visibility=useCodex?Visibility.Collapsed:Visibility.Visible;codexNote.Visibility=useCodex?Visibility.Visible:Visibility.Collapsed;SaveUi();};
        providers.SelectionChanged+=(s,e)=>{SetScreenEnabled(false);StopChat();UpdateSources(new List<WebSource>());updateProvider();output.Text=UserAddress+"，已切换到 "+prefs.Provider+"。您可以继续提问。";if(prefs.Provider=="DeepSeek"&&string.IsNullOrWhiteSpace(apiKey.Password))showConnection();else selectTab(false);};updateProvider();if(prefs.Provider=="DeepSeek"&&string.IsNullOrWhiteSpace(apiKey.Password))showConnection();else selectTab(false);
        panel.Closed+=(s,e)=>{CurrentTask.PanelDraft=input.Text;StopChat();sessionKey=apiKey.Password;prefs.DeepSeekModel=model.Text.Trim();SaveUi();panel=null;};
        if(Program.UiCheck)panel.Title="Fairy · UI Check";
        if(!Program.Preview){panel.Show();panel.Activate();}
    }
    void SaveUi(){if(!Program.Preview)prefs.Save();}
    static RenderTargetBitmap Render(FrameworkElement element,string path,int width,int height){element.Width=width;element.Height=height;element.Measure(new Size(width,height));element.Arrange(new Rect(0,0,width,height));element.UpdateLayout();var bmp=new RenderTargetBitmap(width,height,96,96,PixelFormats.Pbgra32);bmp.Render(element);var encoder=new PngBitmapEncoder();encoder.Frames.Add(BitmapFrame.Create(bmp));using(var stream=File.Create(path))encoder.Save(stream);return bmp;}
    public void RenderUi(){
        var dir=Path.Combine(Program.Root,"preview");Directory.CreateDirectory(dir);OpenPanel();
        Render((FrameworkElement)panel.Content,Path.Combine(dir,"conversation.png"),540,740);
        Render((FrameworkElement)panel.Content,Path.Combine(dir,"conversation-small.png"),480,620);
        var initialTaskName=defaultTask.Name;
        RenameCurrentTask("日常对话");
        panel.UpdateLayout();
        var selectedTaskLabel=(ContentPresenter)taskPicker.Template.FindName("SelectedValuePresenter",taskPicker);
        selectedTaskLabel.ApplyTemplate();
        var renderedTaskName=VisualTreeHelper.GetChild(selectedTaskLabel,0) as TextBlock;
        if(renderedTaskName==null||renderedTaskName.Text!="日常对话")
            throw new Exception("The task selector kept the old name after a rename");
        RenameCurrentTask(initialTaskName);
        var sendPoint=send.TranslatePoint(new Point(0,send.ActualHeight),(FrameworkElement)panel.Content);
        if(sendPoint.Y>620||sendPoint.X+send.ActualWidth>480||output.ActualHeight<75)throw new Exception("Compact conversation layout failed");
        input.Text="默认任务草稿";
        lastAnswer="默认任务回复";
        defaultTask.History["Codex"]=new List<Dictionary<string,string>> {
            new Dictionary<string,string>{{"role","user"},{"content","默认任务问题"}}
        };
        CreateTask("另一任务\n路线");
        if(CurrentTask==defaultTask||input.Text.Length!=0||lastAnswer.Length!=0||CurrentTask.History.Count!=0||
            ActiveMemory==screenMemory)throw new Exception("New task inherited previous context");
        var secondTask=CurrentTask;
        if(secondTask.Name!="另一任务\n路线"||secondTask.ToString()!="另一任务 · 路线")
            throw new Exception("Multiline task name display failed");
        input.Text="另一任务草稿";
        lastAnswer="另一任务回复";
        if(!ActiveMemory.Add("合成记忆","另一任务的独立观察"))throw new Exception("Task memory write failed");
        RenameCurrentTask("已改名的任务\n第二行");
        var reloadedTask=new ChatTask(secondTask.Id,SavedTaskName(secondTask.Id,"未命名"));
        if(reloadedTask.Name!="已改名的任务\n第二行"||CurrentTask!=secondTask)
            throw new Exception("Renaming did not preserve task identity");
        if(!reloadedTask.Memory.Context("独立观察").Contains("另一任务的独立观察"))
            throw new Exception("Task memory did not survive reload");
        SetTaskMemoryEnabled(false);
        if(MemoryContext("独立观察")!="")throw new Exception("Disabled task memory was injected");
        taskPicker.SelectedItem=defaultTask;
        if(input.Text!="默认任务草稿"||lastAnswer!="默认任务回复"||CurrentTask.History["Codex"].Count!=1||
            ActiveMemory==secondTask.Memory||TaskMemoryEnabled!=prefs.ScreenRemember)throw new Exception("Switching back did not restore task context");
        taskManagerPicker.SelectedItem=secondTask;
        if(taskPicker.SelectedItem!=secondTask||input.Text!="另一任务草稿"||lastAnswer!="另一任务回复"||ActiveMemory.Count!=1||TaskMemoryEnabled)
            throw new Exception("Task drafts or observations were mixed");
        showTaskSettings();
        Render((FrameworkElement)panel.Content,Path.Combine(dir,"task-center.png"),480,620);
        RemoveTask(secondTask);
        if(CurrentTask!=defaultTask||taskPicker.SelectedItem!=defaultTask||taskManagerPicker.SelectedItem!=defaultTask||
            deleteTaskButton.IsEnabled||prefs.TaskIds.Contains(secondTask.Id)||prefs.TaskNames.ContainsKey(secondTask.Id)||
            prefs.TasksWithMemoryDisabled.Contains(secondTask.Id)||input.Text!="默认任务草稿"||lastAnswer!="默认任务回复"||
            new ChatTask(secondTask.Id,secondTask.Name).Memory.Count!=0)
            throw new Exception("Deleting a task did not clear its state or restore the default task");
        defaultTask.History.Clear();
        input.Text="";
        lastAnswer="";
        showSettings();Render((FrameworkElement)panel.Content,Path.Combine(dir,"settings.png"),540,740);
        Render((FrameworkElement)panel.Content,Path.Combine(dir,"settings-small.png"),480,620);
        var previousInterval=prefs.IdleMinutes;SetIdleMinutes(1,false);if(idleIntervalSlider.Value!=1||idleMenuItems.Count(m=>m.IsChecked)!=1||!(bool)idleMenuItems[0].IsChecked)throw new Exception("One-minute idle control sync failed");idleIntervalSlider.Value=37;if(prefs.IdleMinutes!=37||idleMenuItems.Any(m=>m.IsChecked))throw new Exception("Custom idle interval failed");SetIdleMinutes(previousInterval,false);settingsScroll.ScrollToTop();panel.UpdateLayout();Render((FrameworkElement)panel.Content,Path.Combine(dir,"settings-idle.png"),540,740);settingsScroll.ScrollToTop();
        providers.SelectedItem="DeepSeek";showConnection();Render((FrameworkElement)panel.Content,Path.Combine(dir,"deepseek.png"),540,740);
        var keyPoint=apiKey.TranslatePoint(new Point(0,apiKey.ActualHeight),(FrameworkElement)panel.Content);if(keyPoint.Y>500||apiKey.ActualHeight<25)throw new Exception("DeepSeek Key must be visible without scrolling");
        var previousKey=sessionKey;apiKey.Password="ui-test-placeholder";connectionApply.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));if(sessionKey!="ui-test-placeholder")throw new Exception("DeepSeek Key apply failed");apiKey.Password=previousKey;sessionKey=previousKey;
        var notice=new FairyHint();notice.Preview();Render((FrameworkElement)notice.Content,Path.Combine(dir,"fairy-hint.png"),(int)notice.Width,(int)notice.Height);
        foreach(var width in new[]{650.0,380.0}){notice.Preview("主人，已完成对话入口切换。接下来需要处理什么？",width);if(!notice.TextFits)throw new Exception("Adaptive hint text overflow: "+width+" / "+notice.Width);Render((FrameworkElement)notice.Content,Path.Combine(dir,"fairy-hint-"+width+".png"),(int)notice.Width,(int)notice.Height);}
        CreateComposer();composer.Preview();Render((FrameworkElement)composer.Content,Path.Combine(dir,"quick-input.png"),470,220);
        composer.VerifyExtraActions(()=>Render((FrameworkElement)composer.Content,Path.Combine(dir,"quick-input-expanded.png"),470,220));
        foreach(var area in new[]{new Rect(0,0,1920,1040),new Rect(-1920,0,1920,1040),new Rect(0,0,800,600)})foreach(var side in new[]{56,72,88,104,204})foreach(var corner in new[]{0,1,2,3}){
            var pet=new Rect(corner%2==0?area.Left+8:area.Right-side-8,corner<2?area.Top+8:area.Bottom-side-8,side,side);
            var bubble=DockLayout.Caption(pet,area,DockLayout.CaptionWidth(pet,area)+pet.Width/2-12,76);var entry=DockLayout.Composer(pet,area,Math.Min(470,area.Width-12),220);
            if(!area.Contains(bubble)||!area.Contains(entry)||entry.IntersectsWith(pet)||Math.Abs((DockLayout.OnRight(pet,area)?bubble.Left:bubble.Right)-(pet.Left+pet.Width/2))>.01)throw new Exception("Docked popup placement failed");
            if(DockLayout.OnRight(pet,area)!=(corner%2==0))throw new Exception("Automatic caption side failed");
        }
        RenderHddMotion(dir);
        RenderJoinScenes(dir);
        DockLayout.VerifyResponsive(dir);
        RenderResponsiveScenes(dir);
        var pages=FairyHint.Split(new string('字',181)+"🧚");if(pages.Count<3||pages.Any(p=>p.Length>64||char.IsHighSurrogate(p[p.Length-1])||char.IsLowSurrogate(p[0])))throw new Exception("Hint pagination failed");
        SetStatus("Fairy · 随时待命");foreach(var value in new[]{32.0,48.0,64.0,80.0,137.0,180.0,360.0}){SetPetSize(value,false);if(eye.Width!=value||Width!=value+24)throw new Exception("Continuous pet size failed");Render((FrameworkElement)Content,Path.Combine(dir,"pet-"+value+".png"),(int)Width,(int)Height);}
        if(((Grid)Content).Children.Count!=2||((Grid)Content).Children[0]!=fairyFrame||((Grid)Content).Children[1]!=eye)throw new Exception("Desktop should contain only the Fairy image");
        File.WriteAllText(Path.Combine(dir,"render-check.json"),Json.Stringify(new{conversation=true,settings=true,hddSubtitle=true,desktopOnlyEye=true,noDuplicateAvatar=true,frameIntegrated=true,reversibleHddMotion=true,gameInputStyle=true,quickInput=true,screenCorners=true,negativeMonitorCoordinates=true,deepseek=true,compactLayout=true,continuousSizes=new[]{32,48,64,80,137,180,360},hintPagination=true,deepseekKeyVisible=true,deepseekKeyApply=true}));tray.Dispose();
    }
}
}
