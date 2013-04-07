using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using ermeX.DAL.DataAccess.UoW;

namespace ermeX.Tests.Common.Reflection
{
	public static class PrivateInspector
	{
		public static void SetStaticPrivateVariable(Type type, string vbleName, object newValue)
		{
			var fieldInfo = type.GetField(vbleName,
														 BindingFlags.Static | BindingFlags.SetField | BindingFlags.NonPublic);
			fieldInfo.SetValue(null, newValue);
		}

		public static void SetStaticPrivateProperty(Type type, string propertyName, object newValue)
		{
			var propertyInfo = type.GetProperty(propertyName,
						   BindingFlags.Static | BindingFlags.SetProperty | BindingFlags.NonPublic);
			propertyInfo.SetValue(null, newValue, null);
		}

		public static object GetPrivateVariableValue(object obj, string vbleName)
		{
			var fieldInfo = obj.GetType().GetField(vbleName,
														 BindingFlags.GetField | BindingFlags.NonPublic);
			return fieldInfo.GetValue(obj);
		}
	}
}
