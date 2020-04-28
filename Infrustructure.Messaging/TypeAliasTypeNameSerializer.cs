using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using EasyNetQ;

namespace Infrastructure.Messaging
{
    public class TypeAliasTypeNameSerializer : ITypeNameSerializer
    {
        private readonly DefaultTypeNameSerializer _defaultTypeNameSerializer = new DefaultTypeNameSerializer();
        private readonly Dictionary<string, Type> _nameToType = new Dictionary<string, Type>();
        private readonly Dictionary<Type, string> _typeToName = new Dictionary<Type, string>();

        public TypeAliasTypeNameSerializer(params Assembly[] assemblies)
        {
            foreach (var type in assemblies.SelectMany(x => x.GetTypes()))
            {
                foreach (var attribute in type.GetCustomAttributes<TypeAliasAttribute>())
                {
                    _nameToType.Add(attribute.Name, type);
                    if (attribute.IsDefault)
                    {
                        _typeToName.Add(type, attribute.Name);
                    }
                }
            }
        }

        public string Serialize(Type type)
        {
            if (_typeToName.TryGetValue(type, out var alias))
            {
                return alias;
            }

            return _defaultTypeNameSerializer.Serialize(type);
        }

        public Type DeSerialize(string typeName)
        {
            if (_nameToType.TryGetValue(typeName, out var type))
            {
                return type;
            }

            return _defaultTypeNameSerializer.DeSerialize(typeName);
        }
    }
}
