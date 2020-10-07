//not avalonia, but we take those https://brianlagunas.com/a-better-way-to-data-bind-enums-in-wpf/
using Avalonia.Markup.Xaml;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Kanji.Interface.Utilities
{
    public class EnumBindingSourceExtension : MarkupExtension
    {
        private Type _enumType;
        public Type EnumType
        {
            get { return this._enumType; }
            set
            {
                if (value != this._enumType)
                {
                    if (null != value)
                    {
                        Type enumType = Nullable.GetUnderlyingType(value) ?? value;

                        if (!enumType.IsEnum)
                            throw new ArgumentException("Type must be for an Enum.");
                    }

                    this._enumType = value;
                }
            }
        }
        private string _name;
        public String Name
        {
            get => _name; 
            set
            {
                if (value != null && !Enum.GetNames(EnumType).Contains(value))
                    throw new ArgumentException("Name must be name of an enum in type");

                _name = value;
            }
        }

        public EnumBindingSourceExtension() { }

        public EnumBindingSourceExtension(Type enumType)
            : this(enumType, null)
        {
        }
        public EnumBindingSourceExtension(Type enumType, string name)
        {
            this.EnumType = enumType;
            this.Name = name;
        }

        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            if (null == this._enumType)
                throw new InvalidOperationException("The EnumType must be specified.");


            Type actualEnumType = Nullable.GetUnderlyingType(this._enumType) ?? this._enumType;
            Array enumValues = Enum.GetValues(actualEnumType);

            if (Name != null)
                return Enum.Parse(actualEnumType, Name);

            if (actualEnumType == this._enumType)
                return enumValues;

            Array tempArray = Array.CreateInstance(actualEnumType, enumValues.Length + 1);
            enumValues.CopyTo(tempArray, 1);
            return tempArray;
        }
    }
}
