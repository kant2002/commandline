﻿using System;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;

namespace CommandLine.Infrastructure
{
    internal class LocalizableAttributeProperty
    {
        private string _propertyName;
        private string _value;

#if NET6_0_OR_GREATER
        [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicParameterlessConstructor)]
#endif
        private Type _type;
        private PropertyInfo _localizationPropertyInfo;

        public LocalizableAttributeProperty(string propertyName)
        {
            _propertyName = propertyName;
        }

        public string Value
        {
            get { return GetLocalizedValue(); }
            set
            {
                _localizationPropertyInfo = null;
                _value = value;
            }
        }


#if NET6_0_OR_GREATER
        [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicParameterlessConstructor)]
#endif
        public Type ResourceType
        {
            set
            {
                _localizationPropertyInfo = null;
                _type = value;
            }
        }

        [SuppressMessage("Trimming", "IL2080:'this' argument does not satisfy 'DynamicallyAccessedMembersAttribute' in call to target method. The source field does not have matching annotations.",
            Justification = "GetProperty actually use only public properties for discovery.")]
        private string GetLocalizedValue()
        {
            if (String.IsNullOrEmpty(_value) || _type == null)
                return _value;
            if (_localizationPropertyInfo == null)
            {
                // Static class IsAbstract
                if (!_type.IsVisible)
                    throw new ArgumentException($"Invalid resource type '{_type.FullName}'! {_type.Name} is not visible for the parser! Change resources 'Access Modifier' to 'Public'", _propertyName);
                PropertyInfo propertyInfo = _type.GetProperty(_value, BindingFlags.Public | BindingFlags.GetProperty | BindingFlags.Static);
                if (propertyInfo == null || !propertyInfo.CanRead || (propertyInfo.PropertyType != typeof(string) && !propertyInfo.PropertyType.CanCast<string>()))
                    throw new ArgumentException($"Invalid resource property name! Localized value: {_value}", _propertyName);
                _localizationPropertyInfo = propertyInfo;
            }

            return _localizationPropertyInfo.GetValue(null, null).Cast<string>();
        }
    }

}
