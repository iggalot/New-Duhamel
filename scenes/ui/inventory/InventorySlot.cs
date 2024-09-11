using Godot;
using System;

public partial class InventorySlot : Control
{
    public ItemController Item { get; set; } = null;

    public void AddItemToInventorySlot(ItemController itemController)
    {
        Item = itemController;

        GD.Print("instantiating slot scene for item " + Item.Name);

        //GameManager _gameManager = GetTree().Root.GetNode<GameManager>("GameManager");
        //PlayerController _playerController = _gameManager.Player;
        //Inventory _inventory = _playerController.GetNode<Inventory>("Inventory"); ;

        //SlotScene.Instantiate();
        

    }

}
