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
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;

namespace ermeX.Common.ILGeneration
{
	internal static class ILHelper
	{
		#region object creation

		public delegate object CreateInstanceHandler(object[] parameters);

		private static readonly Dictionary<string, CreateInstanceHandler> Handlers =
			new Dictionary<string, CreateInstanceHandler>();

		private static readonly object SyncGeneration = new object();

		public static TResult CreateInstance<TResult>(Type typeImpl, params object[] parameters)
		{
			var ptypes = GetParameterTypes(parameters);
			var key = GetKey(typeImpl.FullName, ptypes);

			if (!Handlers.ContainsKey(key))
				lock (SyncGeneration)
					if (!Handlers.ContainsKey(key))
						CreateHandler(typeImpl, key, ptypes);
			var instanceHandler = default(TResult);
			try
			{
				CreateInstanceHandler createInstanceHandler = Handlers[key];
				instanceHandler = (TResult) createInstanceHandler(parameters);
				return instanceHandler;
			}
			catch (TypeAccessException ex)
			{
				ThrowAccessException(typeImpl, ex);
			}
			catch (MethodAccessException ex)
			{
				ThrowAccessException(typeImpl, ex);
			}
			return instanceHandler;
		}

		private static void ThrowAccessException(Type typeImpl, Exception exception)
		{
			throw new InvalidOperationException(
				string.Format(
					"Could not access to the constructor of the type {0}. The constructor must be made either 1)public or 2)internal when the assembly exposes internals to {1}.{3} Exception:{2}",
					typeImpl.FullName, Assembly.GetExecutingAssembly().FullName, exception, Environment.NewLine));
		}


		private static void CreateHandler(Type objtype, string key, Type[] ptypes)
		{
			var dynamicMethod = new DynamicMethod(key, typeof (object),
			                                      new[] {typeof (object[])},
			                                      typeof (ILHelper).Module);
			var cons = objtype.GetConstructor(ptypes);
			if(cons==null)
				throw new TypeAccessException(string.Format("Consturctor not found for type {0}.",objtype.FullName));
			var ilGenerator = dynamicMethod.GetILGenerator();

			ilGenerator.Emit(OpCodes.Nop);
			for (int i = 0; i < ptypes.Length; i++)
			{
				ilGenerator.Emit(OpCodes.Ldarg_0);
				ilGenerator.Emit(OpCodes.Ldc_I4, i);
				ilGenerator.Emit(OpCodes.Ldelem_Ref);
				ilGenerator.Emit(ptypes[i].IsValueType ? OpCodes.Unbox_Any : OpCodes.Castclass, ptypes[i]);
			}
			ilGenerator.Emit(OpCodes.Newobj, cons);
			ilGenerator.Emit(OpCodes.Ret);
			var ci = (CreateInstanceHandler) dynamicMethod.CreateDelegate(typeof (CreateInstanceHandler));
			Handlers.Add(key, ci);

		}


		private static Type[] GetParameterTypes(params object[] parameters)
		{
			if (parameters == null)
				return new Type[0];
			var values = new Type[parameters.Length];
			for (int i = 0; i < parameters.Length; i++)
				values[i] = parameters[i].GetType();
			return values;
		}

		private static string GetKey(string prefix, params Type[] types)
		{
			if (types == null || types.Length == 0)
				return string.Format("{0}null", prefix);
			return string.Format("{0}{1}", prefix, string.Concat<Type>(types));
		}

		#endregion

		#region Invoker

		public delegate object FastInvokeHandler(object target,
		                                         object[] parameters);

		private static readonly object MethodInvocationSyncLock = new object();

		private static readonly Dictionary<string, FastInvokeHandler> MethodInvokers =
			new Dictionary<string, FastInvokeHandler>();

		public static FastInvokeHandler GetMethodInvoker(MethodInfo methodInfo)
		{
			string key = methodInfo.ToString();

			if (!MethodInvokers.ContainsKey(key))
				lock (MethodInvocationSyncLock)
					if (!MethodInvokers.ContainsKey(key))
					{
						var dynamicMethod = new DynamicMethod(string.Empty, typeof (object),
						                                      new[] {typeof (object), typeof (object[])},
						                                      methodInfo.DeclaringType.Module);
						var il = dynamicMethod.GetILGenerator();
						var ps = methodInfo.GetParameters();
						var paramTypes = new Type[ps.Length];
						for (int i = 0; i < paramTypes.Length; i++)
						{
							paramTypes[i] = ps[i].ParameterType;
						}
						var locals = new LocalBuilder[paramTypes.Length];
						for (int i = 0; i < paramTypes.Length; i++)
						{
							locals[i] = il.DeclareLocal(paramTypes[i]);
						}
						for (int i = 0; i < paramTypes.Length; i++)
						{
							il.Emit(OpCodes.Ldarg_1);
							EmitFastInt(il, i);
							il.Emit(OpCodes.Ldelem_Ref);
							il.Emit(paramTypes[i].IsValueType ? OpCodes.Unbox_Any : OpCodes.Castclass, paramTypes[i]);
							il.Emit(OpCodes.Stloc, locals[i]);
						}
						il.Emit(OpCodes.Ldarg_0);
						for (int i = 0; i < paramTypes.Length; i++)
						{
							il.Emit(OpCodes.Ldloc, locals[i]);
						}
						il.EmitCall(OpCodes.Call, methodInfo, null);
						if (methodInfo.ReturnType == typeof (void))
							il.Emit(OpCodes.Ldnull);
						else
						{
							if (methodInfo.ReturnType.IsValueType)
								il.Emit(OpCodes.Box, methodInfo.ReturnType);
						}
						il.Emit(OpCodes.Ret);
						var invoker = (FastInvokeHandler) dynamicMethod.CreateDelegate(
							typeof (FastInvokeHandler));
						MethodInvokers.Add(key, invoker);
					}
			return MethodInvokers[key];

		}

		private static void EmitFastInt(ILGenerator il, int value)
		{
			switch (value)
			{
				case -1:
					il.Emit(OpCodes.Ldc_I4_M1);
					return;
				case 0:
					il.Emit(OpCodes.Ldc_I4_0);
					return;
				case 1:
					il.Emit(OpCodes.Ldc_I4_1);
					return;
				case 2:
					il.Emit(OpCodes.Ldc_I4_2);
					return;
				case 3:
					il.Emit(OpCodes.Ldc_I4_3);
					return;
				case 4:
					il.Emit(OpCodes.Ldc_I4_4);
					return;
				case 5:
					il.Emit(OpCodes.Ldc_I4_5);
					return;
				case 6:
					il.Emit(OpCodes.Ldc_I4_6);
					return;
				case 7:
					il.Emit(OpCodes.Ldc_I4_7);
					return;
				case 8:
					il.Emit(OpCodes.Ldc_I4_8);
					return;
			}

			if (value > -129 && value < 128)
			{
				il.Emit(OpCodes.Ldc_I4_S, (SByte) value);
			}
			else
			{
				il.Emit(OpCodes.Ldc_I4, value);
			}
		}

		#endregion
	}
}

