﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gem.Network.Utilities
{
    public static class ObjectExtensions
    {
        /// <summary>
        /// Get object's property value by it's name using reflection
        /// </summary>
        /// <param name="propName">The property's name</param>
        /// <param name="obj">The object</param>
        /// <returns>The property's value</returns>
        public static object GetValue(this object obj,string propName)
        {
            return obj.GetType().GetProperty(propName).GetValue(obj, null);
        }

        /// <summary>
        /// Get object's property value by it's name using reflection
        /// </summary>
        /// <param name="propName">The property's name</param>
        /// <param name="obj">The object</param>
        /// <returns>The property's value</returns>
        public static void SetValue(this object obj, string propName,object propValue)
        {
            obj.GetType().GetProperty(propName).SetValue(obj, propValue);
        }
    }
}
