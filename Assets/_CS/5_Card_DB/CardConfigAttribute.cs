using System;

[AttributeUsage(AttributeTargets.Class, Inherited = false)]
public class CardConfigAttribute : Attribute
{
    public string Id { get; private set; }
    
    // Localization Fields
    public string NameEn { get; set; }
    public string NameKo { get; set; }
    
    public string DescEn { get; set; }
    public string DescKo { get; set; }
    
    public string FlavorEn { get; set; }
    public string FlavorKo { get; set; }

    // Optional: Is this card monster usage only?
    public bool IsMonsterOnly { get; set; } = false;

    public CardConfigAttribute(string id)
    {
        this.Id = id;
    }
}
