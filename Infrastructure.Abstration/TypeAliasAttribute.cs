using System;

namespace Infrastructure.Messaging
{
    [AttributeUsage(AttributeTargets.Class)]
    public class TypeAliasAttribute : Attribute
    {
        public string Name { get; }
        public bool IsDefault { get; set; }

        public TypeAliasAttribute(string name)
        {
            Name = name;
        }
    }
}
