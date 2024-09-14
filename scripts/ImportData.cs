using Godot;
using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography.X509Certificates;
using System.Text.Json;

public class MyItem
{
    public string ItemName { get; set; } = string.Empty;
    public string ItemAttack { get; set; }
    public string ItemDefense { get; set; }
    public string ItemWeight { get; set; }
    public string ItemDurability { get; set; }
}

public class MyLootTable
{
    public string MapName { get; set; }
    public string ItemCountMin { get; set; }
    public string ItemCountMax { get; set; }
    public string Item1Name { get; set; }
    public string Item1Chance { get; set; }
    public string Item1MinQ { get; set; }
    public string Item1MaxQ { get; set; } 

    public string Item2Name { get; set; }
    public string Item2Chance { get; set; }
    public string Item2MinQ { get; set; } 
    public string Item2MaxQ { get; set; } 

    public string Item3Name { get; set; } 
    public string Item3Chance { get; set; } 
    public string Item3MinQ { get; set; } 
    public string Item3MaxQ { get; set; }
}

public partial class ImportData : Node
{
    private readonly JsonSerializerOptions _options = new()
    {
        PropertyNameCaseInsensitive = true,
    };

    private readonly string _item_table_filePath = "D:/Programming/_GAME_DEVELOP/New-Duhamell/resources/data/item_table.json";
    private readonly string _loot_table_filePath = "D:/Programming/_GAME_DEVELOP/New-Duhamell/resources/data/loot_table.json";

    /// <summary>
    /// parses the JSON files and prints them to the console.
    /// </summary>
    public override void _Ready()
    {
        GD.Print("Import Data");
        using FileStream json1 = File.OpenRead(_item_table_filePath);
        List<MyItem> items = JsonSerializer.Deserialize<List<MyItem>>(json1, _options);

        foreach (var item in items)
        {
            GD.Print(item.ItemName + " " + item.ItemAttack + " " + item.ItemDefense + " " + item.ItemWeight + " " + item.ItemDurability);
        }

        //var items = UseFileOpenReadTextWithSystemTextJson();
        using FileStream json2 = File.OpenRead(_loot_table_filePath);
        List<MyLootTable> loot_tables = JsonSerializer.Deserialize<List<MyLootTable>>(json2, _options);

        foreach (var loot_table in loot_tables)
        {
            GD.Print(loot_table.MapName + " " + loot_table.ItemCountMin + " " + loot_table.ItemCountMax + " " + 
                loot_table.Item1Name + " " + loot_table.Item1Chance + " " + loot_table.Item1MinQ + " " + loot_table.Item1MaxQ + " " +
                loot_table.Item2Name + " " + loot_table.Item2Chance + " " + loot_table.Item2MinQ + " " + loot_table.Item2MaxQ + " " +
                loot_table.Item3Name + " " + loot_table.Item3Chance + " " + loot_table.Item3MinQ + " " + loot_table.Item3MaxQ
                );
        }

    }
}
