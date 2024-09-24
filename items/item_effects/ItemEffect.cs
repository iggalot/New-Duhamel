using Godot;

public partial class ItemEffect : Resource
{
    [Export] public string useDescription { get; set; }

    public virtual void Use()
    {
        return;
    }
}
