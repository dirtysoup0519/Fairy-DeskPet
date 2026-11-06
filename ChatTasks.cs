using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace CodexFairy {
sealed class ChatTask : INotifyPropertyChanged {
    string name;
    public string Name {
        get{return name;}
        set{
            if(name==value)return;
            name=value;
            var changed=PropertyChanged;
            if(changed!=null)changed(this,new PropertyChangedEventArgs("DisplayName"));
        }
    }
    public event PropertyChangedEventHandler PropertyChanged;
    public readonly string Id;
    public string DisplayName {get{return Name.Replace("\r","").Replace("\n"," · ");}}
    public override string ToString(){return DisplayName;}
    public string Answer="", PanelDraft="", QuickDraft="";
    public readonly Dictionary<string,List<Dictionary<string,string>>> History =
        new Dictionary<string,List<Dictionary<string,string>>>();
    public readonly ScreenMemory Memory;

    public ChatTask(string id, string name) {
        Id=id;
        Name=name;
        if(id.Length==0)return;
        string root=Program.Preview?Path.Combine(Program.Root,"preview","task-memory"):Program.Data;
        Memory=new ScreenMemory(Path.Combine(root,"memory","tasks",id,"screen-memory.dat"));
    }
}

public sealed partial class Pet {
    readonly ChatTask defaultTask = new ChatTask("","默认任务");
    readonly List<ChatTask> chatTasks = new List<ChatTask>();
    ChatTask selectedTask;
    ChatTask CurrentTask {get{return selectedTask??defaultTask;}}
    ComboBox taskPicker, taskManagerPicker;
    Button newTaskButton, managedNewTaskButton, renameTaskButton, deleteTaskButton;
    Action showTaskSettings=()=>{};
    bool updatingTasks;

    void LoadChatTasks() {
        chatTasks.Add(defaultTask);
        if(Program.Preview)return;
        defaultTask.Name=SavedTaskName(defaultTask.Id,defaultTask.Name);
        if(prefs.TaskIds==null)prefs.TaskIds=new List<string>();
        if(prefs.TasksWithMemoryDisabled==null)prefs.TasksWithMemoryDisabled=new List<string>();
        foreach(var saved in prefs.TaskIds){
            Guid id;
            if(!Guid.TryParseExact(saved,"N",out id)||id==Guid.Empty)continue;
            string key=id.ToString("N");
            if(chatTasks.Exists(existing=>existing.Id==key))continue;
            var task=new ChatTask(key,SavedTaskName(key,"任务 "+(chatTasks.Count+1)));
            chatTasks.Add(task);
            if(key==prefs.ActiveTaskId)selectedTask=task;
        }
    }

    void SwitchTask(ChatTask task) {
        if(task==CurrentTask)return;
        var previous=CurrentTask;
        previous.Answer=lastAnswer;
        if(panel!=null)previous.PanelDraft=input.Text;
        if(composer!=null)previous.QuickDraft=composer.Draft.Text;
        StopChat();
        StopSpeech();
        if(hint!=null)hint.Dismiss();
        if(memoryView!=null)memoryView.Close();
        bool observing=screenEnabled;
        SetScreenEnabled(false);
        gameScene=new GameScene();
        gameProgressAt=DateTime.MinValue;
        screenPrevious="";
        selectedTask=task;
        prefs.ActiveTaskId=task.Id;
        SaveUi();
        UpdateSources(new List<WebSource>());
        lastAnswer=task.Answer;
        if(panel!=null){
            input.Text=task.PanelDraft;
            output.Text=lastAnswer.Length>0?lastAnswer:"当前任务可以开始对话。";
        }
        if(composer!=null){composer.Draft.Text=task.QuickDraft;composer.SetTask(task.ToString());}
        RefreshMemoryStatus();
        RefreshTaskPickers();
        if(observing)SetScreenEnabled(true);
        SetStatus("当前任务 · "+task.ToString());
    }

    string SavedTaskName(string id,string fallback) {
        string name;
        return prefs.TaskNames!=null&&prefs.TaskNames.TryGetValue(id.Length==0?"default":id,out name)&&
            !string.IsNullOrWhiteSpace(name)?name:fallback;
    }

    void StoreTaskName(ChatTask task) {
        if(prefs.TaskNames==null)prefs.TaskNames=new Dictionary<string,string>();
        prefs.TaskNames[task.Id.Length==0?"default":task.Id]=task.Name;
    }

    string AskTaskName(string title,string initial) {
        var dialog=new Window {
            Title="Fairy · "+title,Owner=panel,WindowStartupLocation=WindowStartupLocation.CenterOwner,
            Width=365,SizeToContent=SizeToContent.Height,ResizeMode=ResizeMode.NoResize,ShowInTaskbar=false,
            Background=Ui.Brush("#181818"),Foreground=Ui.Brush("#F4F3EE")
        };
        var body=new StackPanel {Margin=new Thickness(17)};
        body.Children.Add(Ui.Text("任务名称",14,"#FFC844"));
        var name=Ui.Box(initial,84);
        name.AcceptsReturn=true;name.MaxLength=32;name.Margin=new Thickness(0,8,0,12);
        System.Windows.Automation.AutomationProperties.SetName(name,"任务名称");
        body.Children.Add(name);
        body.Children.Add(Ui.Caption("Enter 换行 · Ctrl+Enter 确定"));
        var actions=new StackPanel {Orientation=Orientation.Horizontal,HorizontalAlignment=HorizontalAlignment.Right};
        var save=TerminalButton("确定",()=>dialog.DialogResult=true);
        save.IsEnabled=!string.IsNullOrWhiteSpace(initial);
        var cancel=TerminalButton("取消",()=>dialog.DialogResult=false);
        cancel.IsCancel=true;
        name.TextChanged+=(s,e)=>save.IsEnabled=!string.IsNullOrWhiteSpace(name.Text);
        name.PreviewKeyDown+=(s,e)=>{
            if(e.Key==Key.Enter&&Keyboard.Modifiers==ModifierKeys.Control){
                e.Handled=true;
                if(save.IsEnabled)dialog.DialogResult=true;
            }
        };
        actions.Children.Add(cancel);actions.Children.Add(save);body.Children.Add(actions);
        dialog.Content=body;
        dialog.Loaded+=(s,e)=>{name.Focus();name.SelectAll();};
        return dialog.ShowDialog()==true?name.Text.Trim():null;
    }

    void CreateTask(string name) {
        var task=new ChatTask(Guid.NewGuid().ToString("N"),name);
        if(prefs.TaskIds==null)prefs.TaskIds=new List<string>();
        if(prefs.TasksWithMemoryDisabled==null)prefs.TasksWithMemoryDisabled=new List<string>();
        prefs.TaskIds.Add(task.Id);
        StoreTaskName(task);
        if(!TaskMemoryEnabled)prefs.TasksWithMemoryDisabled.Add(task.Id);
        chatTasks.Add(task);
        SwitchTask(task);
    }

    void RenameCurrentTask(string name) {
        var task=CurrentTask;
        if(task.Name==name)return;
        task.Name=name;
        StoreTaskName(task);
        SaveUi();
        RefreshTaskPickers();
        if(composer!=null)composer.SetTask(task.ToString());
        if(memoryView!=null)memoryView.Title="Fairy · "+task.ToString()+" · 长期记忆";
        RefreshMemoryStatus();
        SetStatus("当前任务 · "+task.ToString());
    }

    void DeleteCurrentTask() {
        var task=CurrentTask;
        if(task==defaultTask||chatting)return;
        var decision=MessageBox.Show(panel,"删除「"+task.ToString()+"」及其对话、草稿和长期记忆？此操作无法撤销。",
            "Fairy · 删除任务",MessageBoxButton.YesNo,MessageBoxImage.Warning,MessageBoxResult.No);
        if(decision!=MessageBoxResult.Yes||task!=CurrentTask||chatting)return;
        RemoveTask(task);
    }

    void RemoveTask(ChatTask task) {
        if(task==defaultTask||task!=CurrentTask)return;
        task.Memory.Clear();
        if(task.Memory.Error!=""){
            RefreshMemoryStatus();
            SetStatus(task.Memory.Error);
            return;
        }
        prefs.TaskIds.Remove(task.Id);
        if(prefs.TaskNames!=null)prefs.TaskNames.Remove(task.Id);
        if(prefs.TasksWithMemoryDisabled!=null)prefs.TasksWithMemoryDisabled.Remove(task.Id);
        chatTasks.Remove(task);
        SwitchTask(defaultTask);
    }

    void OpenTaskNameDialog(bool rename) {
        if(chatting)return;
        var name=AskTaskName(rename?"重命名任务":"新建任务",rename?CurrentTask.Name:"任务 "+(chatTasks.Count+1));
        if(name==null)return;
        if(rename)RenameCurrentTask(name);
        else CreateTask(name);
    }

    ComboBox CreateTaskPicker() {
        var picker=new ComboBox {Style=ScreenComboStyle,Margin=new Thickness(0,0,8,0),DisplayMemberPath="DisplayName"};
        System.Windows.Automation.AutomationProperties.SetName(picker,"当前任务");
        picker.SelectionChanged+=(s,e)=>{
            var task=picker.SelectedItem as ChatTask;
            if(!updatingTasks&&task!=null&&!chatting)SwitchTask(task);
        };
        return picker;
    }

    void RefreshTaskPickers() {
        updatingTasks=true;
        try{
            foreach(var picker in new[]{taskPicker,taskManagerPicker}){
                if(picker==null)continue;
                picker.ItemsSource=chatTasks.ToArray();
                picker.SelectedItem=CurrentTask;
                picker.ToolTip=CurrentTask.ToString();
            }
        }finally{updatingTasks=false;}
        RefreshTaskControls();
    }

    void RefreshTaskControls() {
        if(taskPicker!=null)taskPicker.IsEnabled=!chatting;
        if(taskManagerPicker!=null)taskManagerPicker.IsEnabled=!chatting;
        foreach(var button in new[]{newTaskButton,managedNewTaskButton,renameTaskButton})
            if(button!=null)button.IsEnabled=!chatting;
        if(deleteTaskButton!=null)deleteTaskButton.IsEnabled=!chatting&&CurrentTask!=defaultTask;
    }

    FrameworkElement TaskBar() {
        var row=new DockPanel {Margin=new Thickness(0,0,0,8)};
        var manage=TerminalButton("管理",()=>showTaskSettings());
        DockPanel.SetDock(manage,Dock.Right);row.Children.Add(manage);
        newTaskButton=TerminalButton("新任务 +",()=>OpenTaskNameDialog(false));
        DockPanel.SetDock(newTaskButton,Dock.Right);row.Children.Add(newTaskButton);
        taskPicker=CreateTaskPicker();row.Children.Add(taskPicker);
        RefreshTaskPickers();
        return row;
    }

    StackPanel TaskSettings() {
        var section=Ui.Section("任务与记忆","选择任务后，可管理该任务的名称和长期记忆。");
        taskManagerPicker=CreateTaskPicker();
        section.Children.Add(taskManagerPicker);
        var actions=new WrapPanel {Margin=new Thickness(0,8,0,0)};
        managedNewTaskButton=TerminalButton("新任务",()=>OpenTaskNameDialog(false));
        renameTaskButton=TerminalButton("重命名",()=>OpenTaskNameDialog(true));
        deleteTaskButton=TerminalButton("删除任务",DeleteCurrentTask);
        deleteTaskButton.ToolTip="默认任务保留，可清空它的长期记忆";
        actions.Children.Add(managedNewTaskButton);actions.Children.Add(renameTaskButton);actions.Children.Add(deleteTaskButton);
        section.Children.Add(actions);
        RefreshTaskPickers();
        return section;
    }

    string UserAddress {get{return string.IsNullOrWhiteSpace(prefs.PersonaAddress)?"主人":prefs.PersonaAddress.Trim();}}

    string PersonaPreferences() {
        var text="";
        if(!string.IsNullOrWhiteSpace(prefs.PersonaAddress))
            text+="\n【用户设置的称呼】称呼用户为「"+UserAddress+"」。";
        if(!string.IsNullOrWhiteSpace(prefs.PersonaSelf))
            text+="\n【用户设置的自称】自称用「"+prefs.PersonaSelf.Trim()+"」。";
        if(!string.IsNullOrWhiteSpace(prefs.PersonaNotes))
            text+="\n【用户设置的人设】以下偏好覆盖默认称呼与说话风格，能力与资料边界保持有效。\n"+prefs.PersonaNotes.Trim();
        return text;
    }

    StackPanel PersonaSettings() {
        var page=Ui.Section("Fairy 人设","设置应用于对话、屏幕协作和游戏陪伴的下一次回答。");
        page.Children.Add(Ui.Caption("Fairy 怎么称呼你（留空沿用原人设）"));
        var address=Ui.Box(prefs.PersonaAddress??"",38);
        address.MaxLength=24;address.AcceptsReturn=false;
        page.Children.Add(address);
        page.Children.Add(Ui.Caption("Fairy 怎么自称（可留空）"));
        var self=Ui.Box(prefs.PersonaSelf??"",38);
        self.MaxLength=24;self.AcceptsReturn=false;
        page.Children.Add(self);
        page.Children.Add(Ui.Caption("人设与语气偏好（可留空）"));
        var notes=Ui.Box(prefs.PersonaNotes??"",150);
        notes.MaxLength=4000;
        page.Children.Add(notes);
        var status=Ui.Caption("");
        page.Children.Add(TerminalButton("保存人设",()=>{
            prefs.PersonaAddress=address.Text.Trim();
            prefs.PersonaSelf=self.Text.Trim();
            prefs.PersonaNotes=notes.Text.Trim();
            SaveUi();
            status.Text="已保存，下次回答生效。";
        }));
        page.Children.Add(status);
        return page;
    }
}
}
