using System;
using System.Collections.Generic;
using System.Threading.Tasks;

public class Sheep
{
    public DateTime TimeCreated { get; set; }
    public int Id { get; set; }
    public string Color { get; set; }
    public double MeatWeightKg { get; set; }
    public double WoolWeightKg { get; set; }
    public string Status { get; set; }
    public DateTime NgayXuat { get; set; }
    private string[] colors = { "Đen", "Trắng", "Xám" };
    private static Random random = new Random(); // Fix: Create a static Random instance

    public Sheep()
    {
        TimeCreated = DateTime.Now;
        Color = colors[random.Next(colors.Length)]; // Fix: Use the static Random instance
        MeatWeightKg = random.Next(30, 61); // Assuming these are initialized elsewhere
        WoolWeightKg = MeatWeightKg * random.Next(3, 8) / 100.0; // Assuming these are initialized elsewhere
    }

    public override string ToString()
    {
        return $"[{TimeCreated:HH:mm:ss}] {Color} sheep | Meat: {MeatWeightKg}kg | Wool: {WoolWeightKg:F2}kg";
    }
}
public class SheepExportData
{
    public DateTime Time { get; set; }
    public int ExportCount { get; set; }
}

public class ExportDataGroup
{
    public DateTime TimeInterval { get; set; }
    public int ExportCount { get; set; }
}
