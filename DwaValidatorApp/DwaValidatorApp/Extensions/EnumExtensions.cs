﻿using System.ComponentModel.DataAnnotations;
using System.Reflection;

namespace DwaValidatorApp.Extensions
{
    public static class EnumExtensions
    {
        public static string GetDisplayName(this Enum enumValue)
        {
            return enumValue.GetType()
              .GetMember(enumValue.ToString())
              .First()
              .GetCustomAttribute<DisplayAttribute>()
              ?.GetName()
              ?? enumValue.ToString();
        }
    }
}
