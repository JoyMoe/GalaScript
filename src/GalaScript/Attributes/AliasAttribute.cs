using System;

namespace GalaScript.Attributes
{
    [AttributeUsage(AttributeTargets.Method)]
    class AliasAttribute : Attribute
    {
        public string Name { get; }

        public AliasAttribute(string name)
        {
            Name = name;
        }
    }
}
