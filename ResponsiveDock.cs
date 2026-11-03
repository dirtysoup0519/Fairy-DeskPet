using System;
using System.IO;
using System.Collections.Generic;
using System.Windows;
namespace CodexFairy {
public sealed class ComposerPlacement {public Rect Pet,Panel;public string Direction;}
public static partial class DockLayout {
    public static double MaximumSize(Rect area){return Math.Max(32,Math.Min(360,Math.Floor(Math.Min(area.Width-40,area.Height-212))));}
    public static ComposerPlacement PlaceComposer(Rect pet,Rect area){
        double width=Math.Min(470,area.Width-16),height=220,gap=8;
        double below=area.Bottom-8-pet.Bottom-gap,above=pet.Top-area.Top-8-gap;
        double left=pet.Left-area.Left-8-gap,right=area.Right-8-pet.Right-gap;
        string direction;
        if(below>=220)direction="below";
        else if(above>=220)direction="above";
        else if(Math.Max(below,above)>=164){direction=below>=above?"below":"above";height=Math.Min(220,Math.Max(below,above));}
        else if(Math.Max(left,right)>=width)direction=right>=left?"right":"left";
        else if(area.Height-pet.Height-24>=164){
            // Shift only the distance needed to leave a readable input panel.
            double downY=area.Bottom-8-gap-164-pet.Height,upY=area.Top+8+gap+164;
            direction=Math.Abs(pet.Top-downY)<=Math.Abs(pet.Top-upY)?"below":"above";
            pet.Y=direction=="below"?downY:upY;height=164;
        }else{
            double rightX=area.Right-8-gap-width-pet.Width,leftX=area.Left+8+gap+width;
            direction=Math.Abs(pet.Left-rightX)<=Math.Abs(pet.Left-leftX)?"right":"left";
            pet.X=direction=="right"?rightX:leftX;
        }
        double x=Clamp(pet.Left+(pet.Width-width)/2,area.Left+8,area.Right-width-8),y=Clamp(pet.Top+(pet.Height-height)/2,area.Top+8,area.Bottom-height-8);
        if(direction=="below")y=pet.Bottom+gap;
        else if(direction=="above")y=pet.Top-gap-height;
        else if(direction=="right")x=pet.Right+gap;
        else x=pet.Left-gap-width;
        return new ComposerPlacement {Pet=pet,Panel=new Rect(x,y,width,height),Direction=direction};
    }
    public static void VerifyResponsive(string directory){
        var areas=new[]{new Rect(0,0,800,600),new Rect(0,0,1366,728),new Rect(0,0,1280,693),new Rect(0,0,960,520),new Rect(-1920,0,1920,1040),new Rect(0,0,640,360)};
        int count=0;var oldFailures=new List<object>();
        foreach(var area in areas){int firstCenter=0,firstCorner=0;
            for(int size=32;size<=MaximumSize(area);size++)foreach(var fx in new[]{0.0,.5,1.0})foreach(var fy in new[]{0.0,.5,1.0}){
                double side=size+24;var pet=new Rect(area.Left+8+(area.Width-side-16)*fx,area.Top+8+(area.Height-side-16)*fy,side,side);
                // Record the previous fixed-height placement's first overlap.
                double oldY=pet.Bottom+8;if(oldY+220>area.Bottom)oldY=pet.Top-228;oldY=Clamp(oldY,area.Top,area.Bottom-220);
                var old=new Rect(Clamp(pet.Left+(side-470)/2,area.Left,area.Right-470),oldY,470,220);
                if(old.IntersectsWith(pet)){if(fx==.5&&fy==.5&&firstCenter==0)firstCenter=size;if(fx==0&&fy==0&&firstCorner==0)firstCorner=size;}
                var plan=PlaceComposer(pet,area);
                if(!area.Contains(plan.Panel)||!area.Contains(plan.Pet)||plan.Panel.IntersectsWith(plan.Pet)||plan.Panel.Height<164||plan.Panel.Width<470)throw new Exception("Responsive composer failed: "+area+" / "+size+" / "+fx+","+fy);
                count++;
            }
            oldFailures.Add(new{workspace=area.ToString(),maximum=MaximumSize(area),previousFirstCenterOverlap=firstCenter,previousFirstTopLeftOverlap=firstCorner});
        }
        File.WriteAllText(Path.Combine(directory,"responsive-dock-check.json"),Json.Stringify(new{cases=count,noOverlap=true,onScreen=true,minInputHeight=164,previousFailures=oldFailures}));
    }
}
public sealed partial class Pet {
    void RenderResponsiveScenes(string directory){
        var areas=new[]{new Rect(0,0,800,600),new Rect(0,0,960,520),new Rect(0,0,640,360)};
        foreach(var area in areas)foreach(var centered in new[]{false,true}){
            double size=DockLayout.MaximumSize(area),side=size+24;var initial=new Rect(centered?(area.Width-side)/2:8,centered?(area.Height-side)/2:8,side,side);var plan=DockLayout.PlaceComposer(initial,area);var scene=new System.Windows.Controls.Canvas {Background=System.Windows.Media.Brushes.White};
            var rim=new FairyFrame {Width=side,Height=side,Progress=1};System.Windows.Controls.Canvas.SetLeft(rim,plan.Pet.Left);System.Windows.Controls.Canvas.SetTop(rim,plan.Pet.Top);scene.Children.Add(rim);
            var eyeImage=new System.Windows.Controls.Image {Source=frames[0],Width=size,Height=size};System.Windows.Controls.Canvas.SetLeft(eyeImage,plan.Pet.Left+12);System.Windows.Controls.Canvas.SetTop(eyeImage,plan.Pet.Top+12);scene.Children.Add(eyeImage);
            var notice=new FairyHint();var bounds=notice.PreviewAttached("主人，我在。请下达指令。",plan.Pet,area);notice.PreviewMotion(1,DockLayout.OnRight(plan.Pet,area));var caption=(System.Windows.FrameworkElement)notice.Content;notice.Content=null;caption.Width=notice.Width;caption.Height=76;System.Windows.Controls.Canvas.SetLeft(caption,bounds.Left);System.Windows.Controls.Canvas.SetTop(caption,bounds.Top);scene.Children.Add(caption);
            var entry=new FairyComposer(text=>{},text=>{},()=>{},()=>{},()=>{});entry.PreviewPlacement(plan);var inputSurface=(System.Windows.FrameworkElement)entry.Content;entry.Content=null;System.Windows.Controls.Canvas.SetLeft(inputSurface,plan.Panel.Left);System.Windows.Controls.Canvas.SetTop(inputSurface,plan.Panel.Top);scene.Children.Add(inputSurface);
            Render(scene,Path.Combine(directory,"responsive-"+area.Width+"x"+area.Height+"-"+(centered?"center":"corner")+".png"),(int)area.Width,(int)area.Height);
            if(entry.Draft.ActualHeight<32)throw new Exception("Compact composer draft clipped");
        }
    }
}
}
