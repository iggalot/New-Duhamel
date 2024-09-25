using Godot;

public partial class PersistentDataHandler : Node
{
    [Signal] public delegate void DataLoadedEventHandler();

    public bool Value { get; set; } = false;

    public override void _Ready()
    {
        GetValue();
        return;
    }

    public void SetValue()
    {
        GlobalSaveManager.Instance.AddPersistentValue(GetPersistenceName());
        return;
    }

    public void GetValue()
    {
        Value = GlobalSaveManager.Instance.CheckPersistentValue(GetPersistenceName());
        EmitSignal(SignalName.DataLoaded);
        return;
    }

    private string GetPersistenceName()
    {
        // "res://levels/area01/01.tscn/treasurechest/PersistentDataHandler"
        return GetTree().CurrentScene.SceneFilePath + "/" + GetParent().Name + "/" + Name;
    }
}
