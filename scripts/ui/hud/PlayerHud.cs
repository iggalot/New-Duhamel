using Godot;
using System.Collections.Generic;

public partial class PlayerHud : CanvasLayer
{
    private const int hp_per_heart = 10;  // how many hitpoints is a heart worth?

    // the data list of hearts for our character / player HUD
    List<HeartGui> Hearts { get; set; } = new List<HeartGui>();

    // stores the reference to the hearts container node in the scene -- needed for added new hearts
    HFlowContainer HeartsContainer { get; set; } 


    public override void _Ready()
    {
        GD.Print("Player HUD Ready");
        // Set our global variable for the player hud
        GlobalPlayerManager.Instance.playerHud = this;

        this.HeartsContainer = GetNode<HFlowContainer>("Control/HFlowContainer");

        // if we preload a heart GUI scene into the the hboxcontainer, this code will manipulate those children nodes
        //foreach(Control child in this.HeartsContainer.GetChildren())
        //{
        //    if ((child is HeartGui) == true)
        //    {
        //        Hearts.Add(child as HeartGui);
        //        child.Visible = true;
        //    }
        //}

        PopulateHeartContainers();


    }

    private void PopulateHeartContainers()
    {
        // clear the data list so we can rebuild it if needed
        this.Hearts.Clear();

        // clear the existing HFlowContainercontainer and then repopulate it with the proper number of hearts
        this.HeartsContainer.GetChildren().Clear();

        // however, we are dynamically loading hearts based on our max hit points, and the scale factor of 
        // hitpoints per heart.
        for (int i = 0; i < GlobalPlayerManager.Instance.player.MaxHitPoints / hp_per_heart; i++)
        {
            PackedScene heartGUIScene = (PackedScene)ResourceLoader.Load("res://scenes/ui/heart_gui.tscn");
            HeartGui heartGuiNode = heartGUIScene.Instantiate() as HeartGui;
            heartGuiNode.Name = "Heart" + i;
            heartGuiNode.Visible = false;

            // add our heart gui scene to th tree
            this.HeartsContainer.AddChild(heartGuiNode);

            // Add our heart record to the data list
            this.Hearts.Add(heartGuiNode as HeartGui);
        }

        // set the hearts based on the players current number of hitpoints and maxhitpoints
        UpdateHitPoints(GlobalPlayerManager.Instance.player.HitPoints,
            GlobalPlayerManager.Instance.player.MaxHitPoints);
    }

    public void UpdateHitPoints(float hp, float max_hp)
    {
        UpdateMaxHP(max_hp);

        for (int i = 0; i < max_hp / hp_per_heart; i++)
        {
            UpdateHeart(i, hp);
        }

        return;
    }

    public void UpdateHeart(int index, float hp)
    {
        // the last value in this clamp function is the max value for our heart state 0, 1, or 2
        // --- so use a 2 here
        // --- if we change to quarter heart configurations in a graphic in the future, then we need to change this number
        // --- This function will calmp our values between 0 and 2 as a precaution.
        int value = (int)Mathf.Clamp(hp - index * hp_per_heart, 0, 2); 
        
        // update the data in our heart
        this.Hearts[index].Value = value;

        return;
    }

    public void UpdateMaxHP(float max_hp)
    {
        int heart_count = Mathf.RoundToInt(max_hp / hp_per_heart);

        for (int i = 0; i < this.Hearts.Count; i++)
        {
            if (i < heart_count)
            {
                this.Hearts[i].Visible = true;
            }
            else
            {
                this.Hearts[i].Visible = false;
            }
        }
    }

}
