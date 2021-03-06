﻿using System;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.CompilerServices;
#if NETFX_CORE
using System.Linq;
#endif

namespace Lex.Db
{
  static class TypeHelper
  {
#if NETFX_CORE
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static IEnumerable<Type> GetInterfaces(this Type type)
    {
      return type.GetTypeInfo().ImplementedInterfaces;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Type[] GetGenericArguments(this Type type)
    {
      return type.GenericTypeArguments;
    }

    static IEnumerable<TypeInfo> GetTypeHierarchy(this Type type)
    {
      var stop = typeof(object);

      do
      {
        var info = type.GetTypeInfo();

        yield return info;

        type = info.BaseType;
        
        if (type == null)
          break;

      } while (type != stop);
    }

    public static MethodInfo GetMethod(this Type type, string name)
    {
      return (from t in GetTypeHierarchy(type)
              from m in t.GetDeclaredMethods(name)
              select m).SingleOrDefault();
    }

    public static MethodInfo GetMethod(this Type type, string name, params Type[] args)
    {
      return (from t in GetTypeHierarchy(type)
              from m in t.GetDeclaredMethods(name)
              where CheckArgs(m.GetParameters(), args)
              select m).SingleOrDefault();
    }

    static bool CheckArgs(ParameterInfo[] parameterInfo, Type[] args)
    {
      if (args.Length != parameterInfo.Length)
        return false;

      for (var i = 0; i < args.Length; i++)
        if (parameterInfo[i].ParameterType != args[i])
          return false;

      return true;
    }

    public static ConstructorInfo GetConstructor(this Type type, params Type[] args)
    {
      return type.GetTypeInfo().DeclaredConstructors.SingleOrDefault(i => i.IsPublic && CheckArgs(i.GetParameters(), args));
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static MethodInfo GetGetMethod(this PropertyInfo prop)
    {
      return prop.GetMethod;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static MethodInfo GetSetMethod(this PropertyInfo prop)
    {
      return prop.SetMethod;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool IsValueType(this Type type)
    {
      return type.GetTypeInfo().IsValueType;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool IsGenericType(this Type type)
    {
      return type.GetTypeInfo().IsGenericType;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool IsEnum(this Type type)
    {
      return type.GetTypeInfo().IsEnum;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static T GetCustomAttribute<T>(this Type type) where T : Attribute
    {
      return type.GetTypeInfo().GetCustomAttribute<T>();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool IsAssignableFrom(this Type type, Type source)
    {
      return type.GetTypeInfo().IsAssignableFrom(source.GetTypeInfo());
    }

#else

#if !NET40
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
    public static T GetCustomAttribute<T>(this Type type) where T : Attribute
    {
      return (T)Attribute.GetCustomAttribute(type, typeof(T));
    }

#if !NET40
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
    public static bool IsValueType(this Type type)
    {
      return type.IsValueType;
    }

#if !NET40
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
    public static bool IsGenericType(this Type type)
    {
      return type.IsGenericType;
    }

#if !NET40
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
#endif
    public static bool IsEnum(this Type type)
    {
      return type.IsEnum;
    }

#endif

    public static IEnumerable<MethodInfo> GetStaticMethods(this Type type)
    {
#if NETFX_CORE
      return from t in GetTypeHierarchy(type)
             from m in t.DeclaredMethods
             where m.IsStatic
             select m;
#else
      return type.GetMethods(BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public);
#endif
    }

    public static MethodInfo GetPublicStaticMethod(this Type type, string name)
    {
#if NETFX_CORE
      return (from t in GetTypeHierarchy(type)
              from m in t.GetDeclaredMethods(name)
              where m.IsStatic
              select m).SingleOrDefault();
#else
      return type.GetMethod(name, BindingFlags.Static | BindingFlags.Public);
#endif
    }

    public static MethodInfo GetPublicInstanceMethod(this Type type, string name)
    {
#if NETFX_CORE
      return (from t in GetTypeHierarchy(type)
              from m in t.GetDeclaredMethods(name)
              where !m.IsStatic
              select m).SingleOrDefault();
#else
      return type.GetMethod(name, BindingFlags.Instance | BindingFlags.Public);
#endif
    }

    public static MethodInfo GetStaticMethod(this Type type, string name)
    {
#if NETFX_CORE
      return (from t in GetTypeHierarchy(type)
              from m in t.DeclaredMethods
              where m.IsStatic && m.Name == name
              select m).SingleOrDefault();
#else
      return type.GetMethod(name, BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public);
#endif
    }

    public static MethodInfo GetPrivateInstanceMethod(this Type type, string name)
    {
#if NETFX_CORE
      return (from t in GetTypeHierarchy(type)
              from m in t.DeclaredMethods
              where m.IsPrivate && !m.IsStatic && m.Name == name
              select m).SingleOrDefault();
#else
      return type.GetMethod(name, BindingFlags.Instance | BindingFlags.NonPublic);
#endif
    }

    public static IEnumerable<FieldInfo> GetPublicInstanceFields(this Type type)
    {
#if NETFX_CORE
      return from t in GetTypeHierarchy(type)
             from f in t.DeclaredFields
             where f.IsPublic && !f.IsStatic
             select f;
#else
      return type.GetFields(BindingFlags.Public | BindingFlags.Instance);
#endif
    }

    public static IEnumerable<PropertyInfo> GetPublicInstanceProperties(this Type type)
    {
#if NETFX_CORE
      return from t in GetTypeHierarchy(type)
             from p in t.DeclaredProperties
             let get = p.GetMethod
             where get != null && get.IsPublic && !get.IsStatic
             select p;
#else
      return type.GetProperties(BindingFlags.Public | BindingFlags.Instance);
#endif
    }
  }
}
