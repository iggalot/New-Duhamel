using Godot;

public partial class HurtBox : Area2D
{
    [Export] public float damage = 1;

    CharacterBody2D ownerController;


    public override void _Ready()
    {
        AreaEntered += OnAreaEntered;

        ownerController = this.Owner as CharacterBody2D;

        if (ownerController == null)
            return;
        else if (ownerController is PlayerController)
        {
            damage = ((PlayerController)ownerController).MeleeDamage;
        }
        else if (ownerController is MonsterController)
        {
            damage = ((MonsterController)ownerController).MeleeDamage;
        }
    }

    public override void _Process(double delta)
    {
    }

    public void OnAreaEntered(Area2D area)
    {
        if (area is HitBox)
        {
            if (((HitBox)area).GetParent() == this.GetParent())
            {
                return;  // we are hitting our selves so exit
            }
            else
            {
                ((HitBox)area).TakeDamage(this);
            }
        }
        return;
    }
}
