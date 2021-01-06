﻿using System;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.InteropServices;

namespace BaseLibrary.Utility
{
	public static class ReflectionUtility
	{
		public const BindingFlags DefaultFlags_Static = BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic;
		public const BindingFlags DefaultFlags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;

		public static void SetValue(this Type type, string name, object value, object obj = null, BindingFlags flags = DefaultFlags)
		{
			PropertyInfo propertyInfo = type.GetProperty(name, flags);
			if (propertyInfo != null) propertyInfo.SetValue(obj, value);
			else type.GetField(name, flags)?.SetValue(obj, value);
		}

		public static void SetValue(this object obj, string name, object value, BindingFlags flags = DefaultFlags) => obj.GetType().SetValue(name, value, obj, flags);

		public static T Invoke<T>(this MethodInfo info, object target, params object[] args) => (T)info.Invoke(target, args);
		public static void Invoke(this MethodInfo info, object target, params object[] args) => info.Invoke(target, args);

		// public static T CreateInstance<T>(params object[] args)
		// {
		// 	if (args.Length == 0)
		// 	{
		// 		var constuctors = typeof(T).GetConstructors()
		// 			.Where(ctor =>
		// 			{
		// 				var parameters = ctor.GetParameters();
		// 				return !parameters.Any() || parameters.All(x => x.GetCustomAttribute<OptionalAttribute>() != null);
		// 			});
		// 	}
		// 	
		//
		//
		// 	NewExpression newExp = Expression.New(typeof(T));
		//
		// 	// Create a new lambda expression with the NewExpression as the body.
		// 	var lambda = Expression.Lambda<Func<T>>(newExp);
		//
		// 	// Compile our new lambda expression.
		// 	return lambda.Compile().Invoke();
		//
		// 	return args.Length <= 0 ? Activator.CreateInstance<T>() : (T)Activator.CreateInstance(typeof(T), args);
		// }
	}
}