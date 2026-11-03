using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;

namespace CodexFairy {
public sealed partial class Pet {
    readonly List<WebSource> lastSources=new List<WebSource>();Button sourceButton;CheckBox webCheck;
    void UpdateSources(IEnumerable<WebSource> sources){lastSources.Clear();lastSources.AddRange(sources);if(composer!=null)composer.SetSources(lastSources.Count);if(sourceButton!=null){sourceButton.Visibility=lastSources.Count>0?Visibility.Visible:Visibility.Collapsed;sourceButton.Content="来源 "+lastSources.Count;}}
    void ShowSources(){var menu=new ContextMenu {Placement=PlacementMode.MousePoint};menu.Items.Add(new MenuItem {Header="本次回复的网页来源",IsEnabled=false});foreach(var source in lastSources){var saved=source;var label=source.Title.Length>65?source.Title.Substring(0,65)+"…":source.Title;var item=new MenuItem {Header=label,ToolTip=source.Url};item.Click+=(s,e)=>{if(SourceList.PublicUrl(saved.Url))Process.Start(new ProcessStartInfo(saved.Url){UseShellExecute=true});};menu.Items.Add(item);}if(lastSources.Count>0)menu.IsOpen=true;}
    void AddSourcesButton(DockPanel speaker){sourceButton=Ui.Button("来源 "+lastSources.Count,ShowSources);sourceButton.FontSize=11;sourceButton.Padding=new Thickness(7,3,7,3);sourceButton.Margin=new Thickness(6,0,0,0);sourceButton.ToolTip="打开这次回复的来源链接";sourceButton.Visibility=lastSources.Count>0?Visibility.Visible:Visibility.Collapsed;DockPanel.SetDock(sourceButton,Dock.Right);speaker.Children.Insert(0,sourceButton);}
    void ToggleWeb(){prefs.SearchWeb=!prefs.SearchWeb;if(webCheck!=null)webCheck.IsChecked=prefs.SearchWeb;if(composer!=null)composer.SetWeb(prefs.SearchWeb);SaveUi();SetStatus(prefs.SearchWeb?"Fairy · 联网已开启，将按问题需要搜索":"Fairy · 联网已关闭");}
    StackPanel WebSettings(){var section=Ui.Section("联网搜索","WEB / 按问题需要查询并提供来源");webCheck=Ui.Check("允许联网搜索",prefs.SearchWeb,value=>{prefs.SearchWeb=value;if(composer!=null)composer.SetWeb(value);SaveUi();});section.Children.Add(webCheck);var note=Ui.Text("Codex 使用内置搜索；DeepSeek 使用官方 Search / Fetch 接口和当前 Key，按对应账户规则计费。可在快捷输入顶部切换「联网」，查询完成后点击「来源」打开网页。",12,"#A8BECC");note.LineHeight=21;section.Children.Add(note);return section;}
}
}
