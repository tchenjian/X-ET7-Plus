//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------
using Bright.Serialization;
using System.Collections.Generic;


namespace ET
{

public sealed partial class UnitConfig: Bright.Config.BeanBase
{
    public UnitConfig(ByteBuf _buf) 
    {
        Id = _buf.ReadInt();
        Type = _buf.ReadInt();
        Name_l10n_key = _buf.ReadString(); Name = _buf.ReadString();
        Position = _buf.ReadInt();
        Height = _buf.ReadInt();
        Weight = _buf.ReadInt();
        PostInit();
    }

    public static UnitConfig DeserializeUnitConfig(ByteBuf _buf)
    {
        return new UnitConfig(_buf);
    }

    /// <summary>
    /// Id
    /// </summary>
    public int Id { get; private set; }
    /// <summary>
    /// Type
    /// </summary>
    public int Type { get; private set; }
    /// <summary>
    /// 名字
    /// </summary>
    public string Name { get; private set; }
    public string Name_l10n_key { get; }
    /// <summary>
    /// 位置
    /// </summary>
    public int Position { get; private set; }
    /// <summary>
    ///  
    /// </summary>
    public int Height { get; private set; }
    /// <summary>
    /// 体重
    /// </summary>
    public int Weight { get; private set; }

    public const int __ID__ = -568528378;
    public override int GetTypeId() => __ID__;

    public  void Resolve(Dictionary<string, IConfigSingleton> _tables)
    {
        PostResolve(); 
    }

    public  void TranslateText(System.Func<string, string, string> translator)
    {
        Name = translator(Name_l10n_key, Name);
    }

    public override string ToString()
    {
        return "{ "
        + "Id:" + Id + ","
        + "Type:" + Type + ","
        + "Name:" + Name + ","
        + "Position:" + Position + ","
        + "Height:" + Height + ","
        + "Weight:" + Weight + ","
        + "}";
    }
    
    partial void PostInit();
    partial void PostResolve();
}
}