using Mawi.Views;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mawi
{
    [AttributeUsage(AttributeTargets.Property)]
    public class CssNameAttribute : Attribute
    {
        public string Name { get; private set; }
        public string Unit { get; private set; }
        public bool Transform { get; private set; }
        public CssNameAttribute(string name, bool transform = false, string unit = "")
        {
            Name = name;
            Unit = unit;
            Transform = transform;
        }
    }
}