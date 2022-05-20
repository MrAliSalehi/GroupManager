
using System.Reflection;
using Castle.Core.Internal;
using GroupManager.Common.Attributes;
using GroupManager.Common.Models;

namespace GroupManager.Common.Extensions;

public static class CommandDescription
{
    internal static IEnumerable<Type> GetImplementedClasses(this Type interfaceType)
    {

        var types = Assembly
            .GetExecutingAssembly()
            .GetTypes()
            .Where(mytype => mytype.GetInterfaces().Contains(interfaceType))
            .ToList();

        return types;
    }

    internal static IEnumerable<T> GetDescribeAttribute<T>(this IEnumerable<Type> implementedTypes) where T : Attribute
    {
        var allDescriptions = new List<T>();
        foreach (var type in implementedTypes)
        {
            foreach (var methodInfo in type.GetMethods(BindingFlags.Instance | BindingFlags.NonPublic))
            {
                var attributes = methodInfo.GetAttributes<T>().ToList();
                if (attributes is null or { Count: 0 })
                    continue;

                allDescriptions.AddRange(attributes);
            }


        }

        return allDescriptions;
    }

    internal static IEnumerable<Describer> Map(this IEnumerable<DescriberAttribute> attributes)
    {
        return attributes
            .Select(attribute => new Describer { Description = attribute.Description, Name = attribute.Name, Parameters = attribute.Parameters })
            .ToList();
    }

    internal static string BuildParametersToString(this Describer describer)
    {
        var output = $"Command: {describer.Name}\nDescription:{describer.Description}";
        if (describer.Parameters is not null or "")
            output += $"\n{describer.Parameters}";
        else
            output += "-No Parameters.";
        return output;
    }
}