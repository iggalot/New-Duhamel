using Godot;

public partial class Boomerang : Node2D
{
    [Signal] public delegate void BoomerangRemovedEventHandler();
    enum State { INACTIVE, THROW, RETURN }

    PlayerController player { get; set; }
    Vector2 direction { get; set; }
    float speed { get; set; }
    State state { get; set; }

    [Export] public float acceleration { get; set; } = 500.0f;
    [Export] public float maxSpeed { get; set; } = 400.0f;

    AnimationPlayer animationPlayer { get; set; }

    public override void _Ready()
    {
        animationPlayer = GetNode<AnimationPlayer>("AnimationPlayer");

        Visible = false;
        state = State.INACTIVE;
        player = GlobalPlayerManager.Instance.player;
    }

    public override void _PhysicsProcess(double delta)
    {
        if(state == State.THROW)
        {
            speed -= acceleration * (float)delta;
            Position += direction * speed * (float)delta;

            if(speed <= 0.0f)
            {
                state = State.RETURN;
            }
            return;
        }
        else if (state == State.RETURN)
        {
            direction = GlobalPosition.DirectionTo(player.GlobalPosition);
            speed += acceleration * (float)delta;
            Position += direction * speed * (float)delta;

            if (GlobalPosition.DistanceTo(player.GlobalPosition) < 10.0f)
            {
                EmitSignal(SignalName.BoomerangRemoved);
                QueueFree();
            }

            return;
        }
        return;
    }

    public void Throw(Vector2 throw_direction)
    {
        GD.Print("boomerang thrown");
        direction = throw_direction;
        speed = maxSpeed;
        state = State.THROW;
        animationPlayer.Play("boomerang");
        Visible = true;
        return;
    }



}
