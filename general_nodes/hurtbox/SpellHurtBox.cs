using Godot;

public partial class SpellHurtBox : Area2D
{
    // which spell owns this hurtbox
    Area2D ownerController;


    public override void _Ready()
    {
        AreaEntered += OnAreaEntered;

        ownerController = this.Owner as Area2D;
    }

    public override void _Process(double delta)
    {
    }

    public void OnAreaEntered(Area2D area)
    {
        // check if we are triggering ourself
        GD.Print("SpellHurtBox has hit something -- " + area.GetParent().Name);
        if (area is SpellHitBox)
        {
            if(((SpellHitBox)area).GetParent() == this.GetParent())
            {
                return;  // we are hitting our selves so exit
            } else
            {
                ((SpellHitBox)area).TakeSpellDamage(this);
            }
        }
        return;
    }
}
