using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;

namespace CodexFairy {
public static partial class DockLayout {
    public static Rect Area(Window anchor) {
        var source=PresentationSource.FromVisual(anchor);
        if(source==null||source.CompositionTarget==null)return SystemParameters.WorkArea;
        var point=anchor.PointToScreen(new Point(anchor.Width/2,anchor.Height/2));
        var bounds=System.Windows.Forms.Screen.FromPoint(new System.Drawing.Point((int)point.X,(int)point.Y)).WorkingArea;
        var matrix=source.CompositionTarget.TransformFromDevice;
        return new Rect(matrix.Transform(new Point(bounds.Left,bounds.Top)),matrix.Transform(new Point(bounds.Right,bounds.Bottom)));
    }
    public static bool OnRight(Rect pet,Rect area){return area.Right-pet.Right>=pet.Left-area.Left;}
    public static double CaptionWidth(Rect pet,Rect area){return Math.Max(90,Math.Min(620,Math.Max(area.Right-pet.Right,pet.Left-area.Left)-10));}
    static double Clamp(double value,double min,double max){return Math.Max(min,Math.Min(value,Math.Max(min,max)));}
    public static Rect Caption(Rect pet,Rect area,double width,double height){var right=OnRight(pet,area);double center=pet.Left+pet.Width/2;return new Rect(Clamp(right?center:center-width,area.Left,area.Right-width),Clamp(pet.Top+(pet.Height-height)/2,area.Top,area.Bottom-height),width,height);}
    public static Rect Composer(Rect pet,Rect area,double width,double height){return PlaceComposer(pet,area).Panel;}
    public static Rect Bounds(Window w){return new Rect(w.Left,w.Top,w.Width,w.Height);}
}
public sealed class FairyComposer : Window {
    public readonly TextBox Draft=Ui.Box("",54);readonly Button provider,submit,read,web,sources;readonly TextBlock status=Ui.Text("Enter 发送 · Shift+Enter 换行",13,"#FFC844");
    readonly Action<string> sendText;readonly Action stop;readonly HddInputSurface plate=new HddInputSurface();readonly Grid body=new Grid();readonly HddMotion motion;readonly ScaleTransform popScale=new ScaleTransform(1,1);readonly TranslateTransform popSlide=new TranslateTransform();bool busy,below=true,placing;string dock="below";
    public FairyComposer(Action<string> sendText,Action<string> readText,Action switchProvider,Action settings,Action stop,Action<double> frame=null,Action toggleWeb=null,Action showSources=null){
        this.sendText=sendText;this.stop=stop;Title="Fairy · 指令输入";WindowStyle=WindowStyle.None;ResizeMode=ResizeMode.NoResize;AllowsTransparency=true;Background=Brushes.Transparent;ShowInTaskbar=false;Topmost=true;Width=470;Height=220;Content=plate;plate.Child=body;var transforms=new TransformGroup();transforms.Children.Add(popScale);transforms.Children.Add(popSlide);plate.RenderTransform=transforms;
        body.RowDefinitions.Add(new RowDefinition {Height=new GridLength(62)});body.RowDefinitions.Add(new RowDefinition());body.RowDefinitions.Add(new RowDefinition {Height=new GridLength(47)});
        var title=Ui.Text("向 FAIRY 发话",15,"#F4F3EE");title.FontStyle=FontStyles.Italic;title.FontWeight=FontWeights.Black;title.Margin=new Thickness(32,9,0,0);title.HorizontalAlignment=HorizontalAlignment.Left;body.Children.Add(title);
        var bar=new StackPanel {Orientation=Orientation.Horizontal,HorizontalAlignment=HorizontalAlignment.Right,VerticalAlignment=VerticalAlignment.Top,Margin=new Thickness(170,23,17,0)};body.Children.Add(bar);
        provider=Small("CODEX ▾",switchProvider);bar.Children.Add(provider);web=Small("联网",()=>{if(toggleWeb!=null)toggleWeb();});web.ToolTip="按需联网搜索，下条消息生效";bar.Children.Add(web);sources=Small("来源",()=>{if(showSources!=null)showSources();});sources.IsEnabled=false;bar.Children.Add(sources);read=Small("朗读",()=>{if(!string.IsNullOrWhiteSpace(Draft.Text))readText(Draft.Text);});bar.Children.Add(read);bar.Children.Add(Small("设置",settings));bar.Children.Add(Small("×",Dismiss));
        var inputArea=new Grid {Margin=new Thickness(28,5,27,6)};Grid.SetRow(inputArea,1);body.Children.Add(inputArea);
        Draft.Height=double.NaN;Draft.MinHeight=32;Draft.FontSize=17;Draft.FontWeight=FontWeights.Bold;Draft.FontStyle=FontStyles.Italic;Draft.BorderThickness=new Thickness(0);Draft.Background=Brushes.Transparent;Draft.Foreground=Ui.Brush("#F1F0EB");Draft.CaretBrush=Ui.Brush("#FFC844");Draft.SelectionBrush=Ui.Brush("#626262");Draft.Padding=new Thickness(0,3,5,2);Draft.MaxLength=4000;inputArea.Children.Add(Draft);
        System.Windows.Automation.AutomationProperties.SetName(Draft,"向 Fairy 发话，Enter 发送，Shift 加 Enter 换行");
        var placeholder=Ui.Text("在这里输入你的指令…",17,"#AAA9A3");placeholder.FontWeight=FontWeights.Bold;placeholder.FontStyle=FontStyles.Italic;placeholder.Margin=new Thickness(1,3,0,0);placeholder.IsHitTestVisible=false;inputArea.Children.Add(placeholder);Draft.TextChanged+=(s,e)=>placeholder.Visibility=Draft.Text.Length==0?Visibility.Visible:Visibility.Collapsed;
        Draft.PreviewKeyDown+=(s,e)=>{if(e.Key==Key.Enter&&Keyboard.Modifiers!=ModifierKeys.Shift){e.Handled=true;if(!busy)Submit();}else if(e.Key==Key.Escape){Dismiss();e.Handled=true;}};
        var footer=new DockPanel {Margin=new Thickness(28,0,28,13),LastChildFill=true};Grid.SetRow(footer,2);body.Children.Add(footer);
        submit=Small("发送 ▶",Submit);submit.FontSize=14;submit.FontWeight=FontWeights.ExtraBold;submit.FontStyle=FontStyles.Italic;submit.Foreground=Ui.Brush("#FFC844");submit.Margin=new Thickness(10,0,0,0);DockPanel.SetDock(submit,Dock.Right);footer.Children.Add(submit);
        status.FontStyle=FontStyles.Italic;status.FontWeight=FontWeights.Bold;status.TextWrapping=TextWrapping.NoWrap;status.TextTrimming=TextTrimming.CharacterEllipsis;status.VerticalAlignment=VerticalAlignment.Center;footer.Children.Add(status);
        motion=new HddMotion((f,r)=>{
            var pop=Pop(r);plate.Clip=null;plate.Visibility=r>0?Visibility.Visible:Visibility.Hidden;plate.Opacity=1;body.Opacity=1;
            bool horizontal=dock=="left"||dock=="right";plate.RenderTransformOrigin=horizontal?new Point(dock=="right"?0:1,.5):new Point(.5,below?0:1);popScale.ScaleX=popScale.ScaleY=.86+.14*pop;popSlide.X=horizontal?(dock=="right"?1:-1)*14*(1-pop):0;popSlide.Y=horizontal?0:(below?1:-1)*14*(1-pop);plate.IsHitTestVisible=r>=.98;if(frame!=null)frame(f);
        },open=>{if(open){if(!Program.Preview){Draft.Focus();Draft.CaretIndex=Draft.Text.Length;}}else Hide();},.30);

        PreviewKeyDown+=(s,e)=>{if(e.Key==Key.Escape){Dismiss();e.Handled=true;}};
        IsVisibleChanged+=(s,e)=>{if(!IsVisible)motion.Snap(0);};Closed+=(s,e)=>motion.Snap(0);
    }
    static double Pop(double progress){var t=Math.Max(0,Math.Min(1,progress))-1;return 1+2.2*t*t*t+1.2*t*t;}
    public static Rect RevealRect(double w,double h,double r,bool below){return new Rect(0,below?0:h*(1-r),w,Math.Max(0,h*r));}
    static Button Small(string label,Action action){var b=Ui.Button(label,action);b.Padding=new Thickness(6,3,6,3);b.Margin=new Thickness(0);b.FontSize=12;b.Background=Brushes.Transparent;b.BorderThickness=new Thickness(0);b.Foreground=Ui.Brush("#CDCDC7");return b;}
    void Submit(){if(busy){stop();return;}if(!string.IsNullOrWhiteSpace(Draft.Text))sendText(Draft.Text.Trim());}
    public void UpdateProvider(string name){provider.Content=(name=="DeepSeek"?"DEEPSEEK":"CODEX")+" ▾";}
    public void SetWeb(bool enabled){web.Foreground=Ui.Brush(enabled?"#FFC844":"#777772");web.ToolTip=enabled?"联网已开启，点击关闭":"联网已关闭，点击开启";}
    public void SetSources(int count){sources.IsEnabled=count>0;sources.ToolTip=count>0?"打开本次回复的 "+count+" 个来源":"本次回复未引用网页";}
    public void SetBusy(bool value){busy=value;submit.Content=value?"停止 ■":"发送 ▶";read.IsEnabled=!value;}
    public bool IsBusy {get{return busy;}}public bool IsOpening {get{return motion.Opening;}}
    public void SetStatus(string text){status.Text=text;status.ToolTip=text+"\nEnter 发送 · Shift+Enter 换行";}
    public void Place(Window anchor){if(placing)return;placing=true;try{var plan=DockLayout.PlaceComposer(DockLayout.Bounds(anchor),DockLayout.Area(anchor));if(Math.Abs(anchor.Left-plan.Pet.Left)>.1)anchor.Left=plan.Pet.Left;if(Math.Abs(anchor.Top-plan.Pet.Top)>.1)anchor.Top=plan.Pet.Top;Width=plan.Panel.Width;Height=plan.Panel.Height;dock=plan.Direction;below=dock=="below";Left=plan.Panel.Left;Top=plan.Panel.Top;}finally{placing=false;}}
    public void Present(Window anchor){Place(anchor);if(!IsVisible){motion.Snap(0);Show();}Activate();Draft.Focus();Draft.CaretIndex=Draft.Text.Length;motion.Play(true);}
    public void Dismiss(){if(IsVisible)motion.Play(false);}
    public void Preview(){Draft.Text="帮我整理一下接下来要做的事。";status.Text="确认指令，交给 Fairy 处理。";motion.Preview(1);}
    public void PreviewPlacement(ComposerPlacement plan){Width=plan.Panel.Width;Height=plan.Panel.Height;dock=plan.Direction;below=dock=="below";plate.Width=Width;plate.Height=Height;Preview();}
    public void PreviewMotion(double progress,bool growsDown){below=growsDown;motion.Preview(progress);}
}
public sealed partial class Pet {
    FairyComposer composer;
    void CreateComposer(){if(composer!=null)return;composer=new FairyComposer(AskMessage,text=>Speak(text,null),()=>{SetScreenEnabled(false);StopChat();UpdateSources(new System.Collections.Generic.List<WebSource>());prefs.Provider=prefs.Provider=="Codex"?"DeepSeek":"Codex";if(panel!=null)providers.SelectedItem=prefs.Provider;SaveUi();composer.UpdateProvider(prefs.Provider);composer.SetStatus("已切换到 "+prefs.Provider);},()=>{OpenPanel();if(prefs.Provider=="DeepSeek")showConnection();else showSettings();},StopChat,value=>SetFrame(false,value),ToggleWeb,ShowSources);composer.SetWeb(prefs.SearchWeb);composer.SetSources(lastSources.Count);composer.UpdateProvider(prefs.Provider);composer.SetBusy(chatting);composer.SetStatus(lastStatus);composer.Closed+=(s,e)=>composer=null;}
    void ToggleComposer(){Touch();CreateComposer();if(composer.IsVisible&&composer.IsOpening)composer.Dismiss();else composer.Present(this);}
    void SetChatBusy(bool value){chatting=value;if(send!=null)send.IsEnabled=!value;if(composer!=null)composer.SetBusy(value);}
    void RepositionPopups(){if(IsLoaded&&eye.Width>DockLayout.MaximumSize(DockLayout.Area(this))){SetPetSize(eye.Width,false);return;}if(composer!=null&&composer.IsVisible)composer.Place(this);if(hint!=null&&hint.IsVisible)hint.FollowAnchor();}
    public void TestQuickChat(){
        System.Threading.SynchronizationContext.SetSynchronizationContext(new System.Windows.Threading.DispatcherSynchronizationContext(Dispatcher));
        prefs.Provider="Codex";prefs.ReadAnswers=false;CreateComposer();composer.Draft.Text="只回复：快捷输入已连通。";
        var task=SubmitMessage(composer.Draft.Text);var frame=new System.Windows.Threading.DispatcherFrame();var timeout=DateTime.UtcNow.AddMinutes(3);
        var pump=new System.Windows.Threading.DispatcherTimer {Interval=TimeSpan.FromMilliseconds(100)};pump.Tick+=(s,e)=>{if(task.IsCompleted||DateTime.UtcNow>timeout)frame.Continue=false;};pump.Start();
        if(!task.IsCompleted)System.Windows.Threading.Dispatcher.PushFrame(frame);pump.Stop();
        if(!task.IsCompleted)throw new Exception("Quick chat test timed out");task.GetAwaiter().GetResult();
        if(panel!=null||input!=null||providers!=null||lastAnswer.IndexOf("快捷输入已连通",StringComparison.Ordinal)<0||composer.IsBusy)throw new Exception("Quick chat without full panel failed");
        System.IO.File.WriteAllText(System.IO.Path.Combine(Program.Data,"quick-chat-check.json"),Json.Stringify(new{fullPanelCreated=false,quickComposer=true,codexReply=true,sharedPersona=true,busyReset=true}));tray.Dispose();
    }
}
}
