using Godot;
using System;

namespace scripts.interfaces
{
    public class AttributesManager
    {
        public bool IsEquippable { get; set; } = false;
        public bool IsIneractable { get; set; } = false;
        public bool IsPickable { get; set; } = false;
        public bool IsPlayerMoveable { get; set; } = false;
        public bool IsUsable { get; set; } = false;


        private PlayerMoveable _playerMoveable { get; set; } = new PlayerMoveable();
        private Equippable _equippable { get; set; } = new Equippable();
        private Pickable _pickable { get; set; } = new Pickable();
        private Interactable _interactable { get; set; } = new Interactable();
        private Usable _usable { get; set; } = new Usable();



        public Pickable DoPickable { get => IsPickable ? _pickable : null; }
        public Equippable DoEquippable { get => IsEquippable ? _equippable : null; }
        public PlayerMoveable DoPlayerMoveable { get => IsPlayerMoveable ? _playerMoveable : null; }
        public Interactable DoInteractable { get => IsIneractable ? _interactable : null; }
        public Usable DoUsable { get => IsUsable ? _usable : null; }
    }
}
