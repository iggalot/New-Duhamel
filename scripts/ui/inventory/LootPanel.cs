using Godot;
using Godot.Collections;
using System;

public partial class LootPanel : Control
{
    string map_name;
    int loot_count;
    Dictionary<string, int> loot_dict = new Dictionary<string, int>();

}
