﻿using System;
using System.Collections;
using System.Reflection;

namespace WellFired.Shared
{
	public interface IReflectionHelper
	{
		bool IsAssignableFrom(Type first, Type second);
		bool IsEnum(Type type);
		PropertyInfo GetProperty(Type type, string name);
		MethodInfo GetMethod(Type type, string name);
		FieldInfo GetField(Type type, string name);
		bool IsValueType(Type type);
		
		MethodInfo GetNonPublicStaticMethod(Type type, string name);
		MethodInfo GetNonPublicMethod(Type type, string name);
	    MethodInfo GetNonPublicMethod(Type type, string name, Type[] types);
	    PropertyInfo GetNonPublicInstanceProperty(Type type, string name);
		FieldInfo GetNonPublicInstanceField(Type type, string name);
	}
}