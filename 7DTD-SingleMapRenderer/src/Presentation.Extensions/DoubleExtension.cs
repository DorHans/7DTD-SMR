using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Markup;

namespace _7DTD_SingleMapRenderer.Presentation.Extensions
{
    public class DoubleExtension : MarkupExtension
    {
        public double Value { get; set; }

        public DoubleExtension(double value)
        {
            this.Value = value;
        }

        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            return this.Value;
        }
    }
}
