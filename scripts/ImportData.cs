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

public partial class ImportData : Node
{
    private readonly JsonSerializerOptions _options = new()
    {
        PropertyNameCaseInsensitive = true,
    };

    private readonly string _sampleJsonFilePath = "D:/Programming/_GAME_DEVELOP/New-Duhamell/resources/data/item_table.json"; 
    public List<MyItem> UseFileReadAllTextWithSystemTextJson()
    {
        var json = File.ReadAllText(_sampleJsonFilePath);
        List<MyItem> items = JsonSerializer.Deserialize<List<MyItem>>(json, _options);

        return items;
    }

    public List<MyItem> UseFileOpenReadTextWithSystemTextJson()
    {
        using FileStream json = File.OpenRead(_sampleJsonFilePath);
        List<MyItem> items = JsonSerializer.Deserialize<List<MyItem>>(json, _options);

        return items;
    }


    public override void _Ready()
    {
        var items = UseFileOpenReadTextWithSystemTextJson();
        
        foreach (var item in items)
        {
            GD.Print(item.ItemName + " " + item.ItemAttack + " " + item.ItemDefense + " " + item.ItemWeight + " " + item.ItemDurability);
        }

    }
}
