// /*---------------------------------------------------------------------------------------*/
//        Licensed to the Apache Software Foundation (ASF) under one
//        or more contributor license agreements.  See the NOTICE file
//        distributed with this work for additional information
//        regarding copyright ownership.  The ASF licenses this file
//        to you under the Apache License, Version 2.0 (the
//        "License"); you may not use this file except in compliance
//        with the License.  You may obtain a copy of the License at
// 
//          http://www.apache.org/licenses/LICENSE-2.0
// 
//        Unless required by applicable law or agreed to in writing,
//        software distributed under the License is distributed on an
//        "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY
//        KIND, either express or implied.  See the License for the
//        specific language governing permissions and limitations
//        under the License.
// /*---------------------------------------------------------------------------------------*/
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using ermeX.Common.ILGeneration;
using ermeX.Common.Memoization;

namespace ermeX.Common
{
    //TODO: SEE MEMOIZER THIS METHODS
    internal static class TypesHelper
    {
        private static readonly string ApplicationFolderPath = PathUtils.GetApplicationFolderPath();

        static TypesHelper()
        {
            //load ermeX assembies in the domain

            string[] files = Directory.GetFiles(ApplicationFolderPath, "ermeX.*.dll");
            foreach (var file in files)
                AppDomain.CurrentDomain.Load(Assembly.LoadFrom(file).GetName());
        }

        public static Type[] GetTypeArrayFromValues(object[] args)
        {
            if(args==null || args.Length==0)
                return new Type[]{};

            var typeArray = new Type[args.Length];
            for (int index = 0; index < args.Length; index++)
            {
                var arg = args[index];
                typeArray[index] = arg.GetType();
            }
            return typeArray;
        }

        private static readonly Func<string, bool, bool,Type> GetTypeFromDomainImpl = Memoizer.Memoize(
            (string fullName, bool throwExceptionIfNotFound ,bool onlySearchInternalAssemblies)=>
                {
                    if (String.IsNullOrEmpty(fullName)) throw new ArgumentNullException("fullName");
                    var result = Type.GetType(fullName);
                    if (result == null)
                    {
                        var assemblies = AppDomain.CurrentDomain.GetAssemblies();
                        if (onlySearchInternalAssemblies)
                            assemblies = assemblies.Where(x => x.GetName().FullName.StartsWith("ermeX")).ToArray();

                        foreach (var assembly in assemblies)
                        {
                            result = assembly.GetType(fullName, false);
                            if (result != null)
                                break;
                        }
                    }
                    if (result == null && throwExceptionIfNotFound)
                        throw new InvalidOperationException(String.Format("The type was not found: {0}", fullName));

                    return result;
                }
            );

        public static Type GetTypeFromDomain(string fullName, bool throwExceptionIfNotFound = true,
                                             bool onlySearchInternalAssemblies = true)
        {
            return GetTypeFromDomainImpl(fullName, throwExceptionIfNotFound, onlySearchInternalAssemblies);
        }

        private static readonly Func<string, bool, bool, Type> GetTypeFromDomainByClassNameImpl = Memoizer.Memoize
            (
            (string className, bool throwExceptionIfNotFound,bool onlySearchInternalAssemblies)=>
                {
                    Type result = null;
                    var assemblies = AppDomain.CurrentDomain.GetAssemblies();
                    if (onlySearchInternalAssemblies)
                        assemblies = assemblies.Where(x => x.GetName().FullName.StartsWith("ermeX")).ToArray();

                    foreach (var assembly in assemblies)
                    {
                        var tmpResult =
                            assembly.GetTypes().SingleOrDefault(
                                x => x != null && x.FullName.Split('.').Last() == className);
                        if (tmpResult != null)
                        {
                            if (result != null)
                                throw new InvalidOperationException(
                                    "Ensure the type is single in the domain at least two occurrences were found");
                            result = tmpResult;
                        }
                    }
                    if (result == null && throwExceptionIfNotFound)
                        throw new InvalidOperationException(String.Format("The type was not found: {0}", className));
                    return result;
                }
            );

        public static Type GetTypeFromDomainByClassName(string className, bool throwExceptionIfNotFound = true,
                                                        bool onlySearchInternalAssemblies = true)
        {
            return GetTypeFromDomainByClassNameImpl(className, throwExceptionIfNotFound, onlySearchInternalAssemblies);
        }

        private static readonly Func<Type, bool, Type[]> GetTypesFromDomainImplementingImpl = Memoizer.Memoize
            (
                (Type tBase, bool onlySearchInternalAssemblies) =>
                    {
                        var result = new List<Type>();
                        var assemblies = AppDomain.CurrentDomain.GetAssemblies();
                        if (onlySearchInternalAssemblies)
                            assemblies = assemblies.Where(x => x.GetName().FullName.StartsWith("ermeX")).ToArray();

                        foreach (var assembly in assemblies)
                            result.AddRange(
                                assembly.GetTypes().Where(x => x.TypeImplements(tBase) && x != tBase));
                        return result.ToArray();
                    }
            );

        public static Type[] GetTypesFromDomainImplementing<TBase>(bool onlySearchInternalAssemblies = true)
        {
            return GetTypesFromDomainImplementingImpl(typeof(TBase), onlySearchInternalAssemblies);
        }

        public static Assembly GetAssemblyFromDomain(string assemblyName)
        {
            Assembly[] assemblies = AppDomain.CurrentDomain.GetAssemblies();
            Assembly result = assemblies.SingleOrDefault(x => x.FullName.Split(',')[0] == assemblyName);

            if (result == null)
            {
                result = Assembly.Load(assemblyName);
            }
            return result;
        }

        public static Assembly[] GetAssembliesFromDomain(string excludeAssemblyNameStartsWith)
        {
            Assembly[] assemblies = AppDomain.CurrentDomain.GetAssemblies().Where(x=>!x.FullName.Split(',')[0].StartsWith(excludeAssemblyNameStartsWith)).ToArray();
            return assemblies;
        }

        public static bool TypeImplements<TBase>(this Type implementationType)
        {
            return implementationType.TypeImplements(typeof (TBase));
        }


        public static bool TypeImplements(this Type serviceImplementationType, Type baseType)
        {
            if (baseType == null) throw new ArgumentNullException("baseType");
            if (serviceImplementationType == null) throw new ArgumentNullException("serviceImplementationType");

            bool result = baseType.IsAssignableFrom(serviceImplementationType);
            return result;
        }

        private static readonly Func<Type, MethodInfo[]> GetPublicInstanceMethodsImpl = Memoizer.Memoize
            (
                (Type type) => type.GetMethods(BindingFlags.Public | BindingFlags.Instance));

        public static MethodInfo[] GetPublicInstanceMethods(Type type)
        {
            return GetPublicInstanceMethodsImpl(type);
        }

        public static MethodInfo[] GetPublicInstanceMethods(string fullTypeName)
        {
            return GetPublicInstanceMethods(GetTypeFromDomain(fullTypeName));
        }

        /// <summary>
        /// Retrieves the public instance methods that can be invoked using the argtypes
        /// </summary>
        /// <param name="type"></param>
        /// <param name="methodName"></param>
        /// <param name="argTypes"></param>
        /// <returns></returns>
        public static MethodInfo[] GetPublicInstanceMethods(Type type, string methodName, params Type[] argTypes)
        {
            var result=new List<MethodInfo>();
            try
            {
                if (argTypes == null || argTypes.Length == 0)
                    return new[]{GetPublicInstanceMethods(type).SingleOrDefault(x => x.Name == methodName)};

                var methodInfos = GetPublicInstanceMethods(type).Where(x => x.Name == methodName);
                foreach (var methodInfo in methodInfos)
                {
                    var parameterInfos = methodInfo.GetParameters();
                    if (parameterInfos.Length != argTypes.Length)
                        continue;
                    bool found = true;
                    for (int index = 0; index < parameterInfos.Length; index++)
                    {
                        var parameterInfo = parameterInfos[index];
                        if (parameterInfo.ParameterType != argTypes[index] && !argTypes[index].IsSubclassOf(parameterInfo.ParameterType) && !argTypes[index].TypeImplements(parameterInfo.ParameterType))
                        {
                            found = false;
                            break;
                        }
                    }
                    if (found)
                        result.Add(methodInfo);
                }
                return result.ToArray();
            }
            catch (Exception exception)
            {
                throw new ArgumentException(string.Format("type:{0} method:{1}", type.FullName, methodName), exception);
            }
        }

        public static object InvokeFast(MethodInfo method, object target,object[] args)
        {
            object result = null;

            if (Environment.Is64BitProcess)
            {
                try
                {
                    var invoker = ILHelper.GetMethodInvoker(method);
                    result = invoker(target, args);
                }
                catch (EntryPointNotFoundException)
                {
                    result = method.Invoke(target, args);
                    //TODO: THIS IS DUE A LIMITATION in the x86 COMPILER and its failing sometimes in the x64 as well
                }
            }
            else
            {
                result = method.Invoke(target, args);
            }

            return result;
        }

        public static MethodInfo GetPublicInstanceMethod(string type, string methodName)
        {
            return
                GetPublicInstanceMethods(type).SingleOrDefault(
                    x => x.Name == methodName);
        }
        //retrieves the public instance method that can strictly be invoked using the argtypes
        public static MethodInfo GetPublicInstanceMethod(Type type, string methodName, params Type[] argTypes )
        {
            try
            {
                if (argTypes == null || argTypes.Length == 0)
                    return GetPublicInstanceMethods(type).SingleOrDefault(x => x.Name == methodName);

                var methodInfos = GetPublicInstanceMethods(type).Where(x => x.Name == methodName);
                foreach (var methodInfo in methodInfos)
                {
                    var parameterInfos = methodInfo.GetParameters();
                    if (parameterInfos.Length != argTypes.Length)
                        continue;
                    bool found = true;
                    for (int index = 0; index < parameterInfos.Length; index++)
                    {
                        var parameterInfo = parameterInfos[index];
                        if (parameterInfo.ParameterType != argTypes[index])
                        {
                            found = false;
                            break;
                        }
                    }
                    if(found)
                        return methodInfo;
                }
                throw new EntryPointNotFoundException(
                    string.Format("Couldnt find the method:{0} in the type:{1} with the given parameters", methodName,
                                  type.FullName));
            }
            catch (Exception exception)
            {
                throw new ArgumentException(string.Format("type:{0} method:{1}", type.FullName, methodName), exception);
            }
        }

        public static Type[] GetConcreteTypesImplementingGenericType(Type genericType, Assembly[] searchAssemblies, string namespaceStartsWith)
        {
            if (genericType == null) throw new ArgumentNullException("genericType");
            if (searchAssemblies == null)
                throw new ArgumentNullException("searchAssemblies");

            var result = new List<Type>();

            foreach (var assembly in searchAssemblies)
            {
                var typesList = assembly.GetTypes();

                if(!string.IsNullOrEmpty(namespaceStartsWith))
                {
                    typesList= typesList.Where(x => !string.IsNullOrEmpty(x.Namespace) && x.Namespace.StartsWith(namespaceStartsWith)).ToArray();
                }
                foreach (var type in typesList)
                {
                    Type[] interfaces;
                    try
                    {
                        interfaces = type.GetInterfaces();
                    }
                    catch (Exception ex)
                    {
                        throw new Exception(String.Format("Type:{0} ", type.FullName), ex);
                    }
                    //finds the first generic type
                    Type baseType=type.BaseType;
                    while (baseType!=null && (!baseType.IsGenericType || baseType == typeof(object)))
                        baseType = baseType.BaseType;
                    if (baseType == typeof(object))
                        continue;

                    foreach (var @interface in interfaces)
                    {
                        //TODO:REMOVE
                        try
                        {
                            if ((baseType != null && baseType.IsGenericType &&
                                 genericType.IsAssignableFrom(baseType.GetGenericTypeDefinition())
                                 ||
                                 @interface.IsGenericType &&
                                 genericType.IsAssignableFrom(@interface.GetGenericTypeDefinition())))
                                result.Add(type);
                        }
                        catch (Exception ex)
                        {

                            throw new Exception(String.Format("Type:{0} basetype: {1} interface:{2} ", type.FullName, baseType.FullName, @interface.FullName), ex);
                        }

                    }
                }



            }
            return result.ToArray();

          
        }
        public static Type[] GetConcreteTypesImplementingGenericType(Type genericType, Assembly[] searchAssemblies)
        {
            return
                GetConcreteTypesImplementingGenericType(genericType, searchAssemblies, null);
        }
        public static Type[] GetTypesImplementing(Type typeImplemented, Assembly[] searchAssemblies, bool onlyConcreteTypes=true)
        {
            if (typeImplemented == null) throw new ArgumentNullException("typeImplemented");
            if (searchAssemblies == null) throw new ArgumentNullException("searchAssemblies");
            var result = new List<Type>();

            IEnumerable<Type> collection;
            if (!onlyConcreteTypes)
            {
                collection =
                    searchAssemblies.SelectMany(s => s.GetTypes()).Where(p => typeImplemented.IsAssignableFrom(p));
            }
            else
            {
                collection =
                    searchAssemblies.SelectMany(s => s.GetTypes()).Where(
                        p => !p.IsInterface && !p.IsAbstract && typeImplemented.IsAssignableFrom(p));
            }
            result.AddRange(collection);


            return result.ToArray();
        }

        public static IEnumerable<Type> GetInterfacesImplementing<TInterface>(Type type)
        {
            if (type == null) throw new ArgumentNullException("type");
            var result = type.FindInterfaces(OnFilter, typeof (TInterface));

            return result;
        }

        private static bool OnFilter(Type type, object criteria)
        {
            var typeInterface = (Type) criteria;
            Type[] interfaces;
            try
            {
                interfaces = type.GetInterfaces();
            }
            catch (Exception ex)
            {
                throw new Exception(String.Format("Type:{0} ", type.FullName), ex);
            }
            bool onFilter = interfaces.Contains(typeInterface);
            return onFilter;
        }


        public static Type[] GetGenericInterfaces(Type genericInterfaceType, Type type)
        {
            return type.GetInterfaces()
                .Where(
                    x => x.IsGenericType && x.GetGenericTypeDefinition().FullName == genericInterfaceType.FullName)
                .ToArray();
        }


        public static object ConvertFrom(string convertToType, object valueToConvert)
        {
            Type targetType = GetTypeFromDomain(convertToType);
            TypeConverter tc = TypeDescriptor.GetConverter(targetType);
            return tc.ConvertFrom(null, CultureInfo.InvariantCulture, valueToConvert);
        }


        public static Type[] GetInheritanceChain(string typeFullName,bool includeInterfaces=false)
        {
            Type type = GetTypeFromDomain(typeFullName);

            var result = new List<Type>();
            var currentType = type;
            while (currentType != null && currentType != typeof (object))
            {
                result.Add(currentType);
                currentType = currentType.BaseType;
            }

            if (includeInterfaces)
                result.AddRange(type.GetInterfaces());
            
            return result.ToArray();
        }
        
    }
}