using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using _7DTD_SingleMapRenderer.Core;

namespace _7DTD_SingleMapRenderer.CommandLine
{
    public static class CommandLine<T> where T : class, new()
    {
        public static T Parse(string[] args)
        {
            T result = new T();

            Dictionary<string, string> arguments = new Dictionary<string, string>();
            foreach (var item in args)
            {
                string arg = item.TrimStart('-', '/');
                //int firstsep = arg.IndexOf('=');
                string[] parts = arg.Split('=');

                string key = parts[0];
                string value = parts.Length > 1 ? parts[1] : "true";

                arguments.Add(key, value);
            }

            var props = typeof(T).GetProperties();
            var filtered = props.Where(p => p.CustomAttributes.Any(ca => ca.AttributeType == typeof(OptionAttribute))).ToList();

            foreach (var item in filtered)
            {
                var attr = item.GetCustomAttribute<OptionAttribute>();
                if (attr != null)
                {
                    string value = String.Empty;
                    // wenn weder Name noch ShortName angegeben wurde, dann weitersuchen
                    if (!arguments.TryGetValue(attr.ShortName, out value))
                        if (!arguments.TryGetValue(attr.Name, out value))
                            continue;

                    if (item.PropertyType.IsEnum)
                    {
                        int enumValue = -1;
                        TileSizes tilesize;
                        if (int.TryParse(value, out enumValue))
                            item.SetValue(result, enumValue);
                        else if (Enum.TryParse<TileSizes>(value, out tilesize))
                            item.SetValue(result, tilesize);
                    }
                    else
                    {
                        item.SetValue(result, Convert.ChangeType(value, item.PropertyType));
                    }
                }
            }

            return result;
        }

        public static void PrintHelp()
        {
            var version = (Assembly.GetExecutingAssembly().GetCustomAttributes(typeof(AssemblyFileVersionAttribute), true).FirstOrDefault() as AssemblyFileVersionAttribute).Version;
            Console.WriteLine("7 Days To Die - Single Map Renderer - v{0}", version);
            Console.WriteLine();
            //Console.WriteLine("Usage: ");
            var filename = System.IO.Path.GetFileName(Assembly.GetExecutingAssembly().Location);
            //Console.WriteLine("{0} /map=\"path to map file\" /image=\"path to resulting png\"", filename);
            Console.WriteLine("Switches:");

            var props = typeof(T).GetProperties();
            var filtered = props.Where(p => p.CustomAttributes.Any(ca => ca.AttributeType == typeof(OptionAttribute))).ToList();
            foreach (var item in filtered)
            {
                var attr = item.GetCustomAttribute<OptionAttribute>();
                if (attr != null)
                {
                    if (item.PropertyType == typeof(bool))
                        Console.WriteLine("  /{0,-3} or /{1,-22}{2}", attr.ShortName, attr.Name, attr.Help);
                    else
                        Console.WriteLine("  /{0,-3} or /{1,-22}{2}", attr.ShortName, attr.Name + "=<value>", attr.Help);
                }
            }

            Console.WriteLine();
            Console.WriteLine("Only the map-switch is required. All other switches are optional.");
            Console.WriteLine();
            Console.WriteLine("Example:");
            Console.WriteLine("{0} /map=\"%appdata%\\7DaysToDie\\Saves\\<Path to file>.map\" /i=\"%userprofile%\\Desktop\\map.png\" /bg", filename);
            Console.WriteLine("Will render the map to your desktop with the in-game background.");
        }
    }
}
