using System.Text.Json.Serialization;

namespace ShopApp.Models;

[JsonDerivedType(typeof(Tool), typeDiscriminator: "tool")]
[JsonDerivedType(typeof(Accessory), typeDiscriminator: "accessory")]
[JsonDerivedType(typeof(Clothing), typeDiscriminator: "clothing")]
[JsonDerivedType(typeof(MasterClass), typeDiscriminator: "masterclass")]
[JsonDerivedType(typeof(Yarn), typeDiscriminator: "yarn")]
[JsonDerivedType(typeof(YarnBobbin), typeDiscriminator: "yarnbobbin")]
public abstract class Product
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public string Brand { get; set; } = string.Empty;
    public string Country { get; set; } = string.Empty;
    public int Count { get; set; }
    public string? ImageUrl { get; set; }
    public string? ImageUrl2 { get; set; }
    public string? ImageUrl3 { get; set; }
    public string? ImageUrl4 { get; set; }
}

public class Tool : Product
{
    public string Material { get; set; } = string.Empty; // Материал (например, металл, пластик)
    public string Size { get; set; } = string.Empty; // Размер (например, 10 см)
    public string Purpose { get; set; } = string.Empty; // Назначение (например, для вязания)
    public float WeightGramm { get; set; } // Вес (перенесли из Product)
}

public class Accessory : Product
{
    public string Material { get; set; } = string.Empty; // Материал
    public string Size { get; set; } = string.Empty; // Размер
    public string Type { get; set; } = string.Empty; // Тип аксессуара (например, сумка, чехол)
    public float WeightGramm { get; set; } // Вес (перенесли из Product)
}

public class Clothing : Product
{
    public string Size { get; set; } = string.Empty; // Размер (S, M, L)
    public string Fabric { get; set; } = string.Empty; // Тип ткани
    public string Season { get; set; } = string.Empty; // Сезонность (зима, лето)
    public string Color { get; set; } = string.Empty; // Цвет (перенесли из Product)
    public float WeightGramm { get; set; } // Вес (перенесли из Product)
}

public class MasterClass : Product
{
    public DateTime EventDate { get; set; } // Дата проведения
    public int DurationHours { get; set; } // Продолжительность (в часах)
    public string DifficultyLevel { get; set; } = string.Empty; // Уровень сложности (начинающий, продвинутый)
    public string Format { get; set; } = string.Empty; // Формат (онлайн, оффлайн)
}

public class Yarn : Product
{
    public string Type { get; set; } = string.Empty;
    public string Color { get; set; } = string.Empty; // Цвет (перенесли из Product)
    public float WeightGramm { get; set; } // Вес (перенесли из Product)
    public string ToolsSize { get; set; } = string.Empty;
    public float Length { get; set; }
}

public class YarnBobbin : Yarn
{
}