using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Xml.Linq;
using Rocket.API;
using Rocket.Core.Logging;
using SeniorS.BarricadeRestrictions.Helpers;
using SeniorS.BarricadeRestrictions.Models;

// ReSharper disable InconsistentNaming
namespace SeniorS.BarricadeRestrictions;

public class Configuration : IRocketPluginConfiguration
{
    public void LoadDefaults()
    {
        hexDefaultMessagesColor = "#2BC415";
        hexErrorMessagesColor = "#F82302";

        bypassLimitPermission = "s.restrictions.bypass";
        shouldAdminBypassLimit = false;
        
        restrictions = [
            new Restriction(1241, 0, false, true),
            new Restriction(386, 2, false, true),
        ];
    }

    public string hexDefaultMessagesColor { get; set; }
    
    public string hexErrorMessagesColor { get; set; }

    [ConfigComment("Permission to bypass all restrictions")]
    public string bypassLimitPermission { get; set; }
    
    [ConfigComment("Should the admins bypass the restriction limits?")]
    public bool shouldAdminBypassLimit { get; set; }
    
    public List<Restriction> restrictions { get; set; }

    internal void LoadComments()
    {
        string pluginName = BarricadeRestrictions.Instance.Assembly.GetName().Name;

        string configPath = Path.Combine(Environment.CurrentDirectory, "Plugins", pluginName, $"{pluginName}.configuration.xml");

        if (!File.Exists(configPath))
        {
            throw new FileNotFoundException($"Configuration file not found: {configPath}");
        }

        string xmlText = File.ReadAllText(configPath);
        XDocument doc = XDocument.Parse(xmlText);

        foreach (PropertyInfo property in GetType().GetProperties())
        {
            XElement element = doc.Root;
            AddComment(property, ref element);
        }

        doc.Save(configPath);
    }

    private void AddComment(PropertyInfo property, ref XElement mainElement)
    {
        XElement element = mainElement.Element(property.Name);
        if (element == null) return; 
        Type elementType = GetCollectionElementType(property.PropertyType);
    
        if (elementType != null)
        {
            List<XElement> elements = element.Elements().ToList();
            for (int i = 0; i < elements.Count; i++)
            {
                XElement childElement = elements[i];
                foreach (PropertyInfo subProperty in elementType.GetProperties())
                {
                    AddComment(subProperty, ref childElement);
                }
            }
        }
        else if (element.Elements().Any())
        {
            foreach (PropertyInfo subProperty in property.PropertyType.GetProperties())
            {
                AddComment(subProperty, ref element);
            }
        }
        
        ConfigCommentAttribute subCommentAttribute = property.GetCustomAttribute<ConfigCommentAttribute>();
        if (subCommentAttribute == null) return;
        
        element?.AddBeforeSelf(new XComment(subCommentAttribute.Comment));
    }
    
    private Type GetCollectionElementType(Type type)
    {
        if (type == typeof(string)) return null;
    
        Type collectionInterface = type.GetInterfaces()
            .FirstOrDefault(i => i.IsGenericType && 
                                 i.GetGenericTypeDefinition() == typeof(IEnumerable<>));
    
        return collectionInterface?.GetGenericArguments()[0];
    }
}