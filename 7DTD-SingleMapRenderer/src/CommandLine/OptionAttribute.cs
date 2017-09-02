using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace _7DTD_SingleMapRenderer.CommandLine
{
    [AttributeUsage(AttributeTargets.Property, Inherited = false, AllowMultiple = false)]
    internal sealed class OptionAttribute : Attribute
    {
        // See the attribute guidelines at 
        //  http://go.microsoft.com/fwlink/?LinkId=85236

        // These are positional arguments
        readonly string shortname;
        readonly string name;
        readonly string help;

        public string ShortName { get { return shortname; } }
        public string Name { get { return name; } }
        public string Help { get { return help; } }

        // This is a named argument
        public object Default { get; set; }

        public OptionAttribute(string shortname, string name, string help)
        {
            this.shortname = shortname;
            this.name = name;
            this.help = help;
        }
    }
}
