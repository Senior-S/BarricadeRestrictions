using System;

namespace SeniorS.BarricadeRestrictions.Helpers;

[AttributeUsage(AttributeTargets.Field | AttributeTargets.Property, AllowMultiple = false)]
public class ConfigCommentAttribute(string comment) : Attribute
{
    public string Comment { get; } = comment;
}