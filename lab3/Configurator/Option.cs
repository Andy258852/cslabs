using System;

namespace Configurator
{
    public struct Option
    {
        public string Name { get; }
        public Type CType { get; }
        public object DefaultValue { get; }
        public bool IsNecessary { get; }

        public Option(string Name, bool IsNecessary, Type CType, object DefaultValue = null)
        {
            this.Name = Name;
            this.IsNecessary = IsNecessary;
            this.CType = CType;
            if(DefaultValue != null)
                if (!CType.IsInstanceOfType(DefaultValue))
                    throw new TypeAccessException();
            this.DefaultValue = DefaultValue;
        }
    }
}
