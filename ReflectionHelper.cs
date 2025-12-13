using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;
using HarmonyLib;
using UnityEngine;

namespace tinygrox.DuckovMods.NumericalStats
{
    public static class ReflectionHelper
    {
        private static readonly Dictionary<(Type, string), Delegate> s_getterCache = new Dictionary<(Type, string), Delegate>();
        private static readonly Dictionary<(Type, string), Delegate> s_setterCache = new Dictionary<(Type, string), Delegate>();

        // private const BindingFlags BFlags = BindingFlags.NonPublic | BindingFlags.Instance;
        // 万能 “取” 委托，能够帮你从 TInstance 取 TField 类型私有变量 fieldName 的值回来
        public static Func<TInstance, TField> CreateFieldGetter<TInstance, TField>(string fieldName)
        {
            (Type, string fieldName) key = (typeof(TInstance), fieldName);
            if (s_getterCache.TryGetValue(key, out Delegate getter))
            {
                return (Func<TInstance, TField>)getter;
            }

            try
            {
                FieldInfo fieldInfo = AccessTools.Field(typeof(TInstance), fieldName);
                if (fieldInfo == null)
                {
                    return instance => default;
                }

                ParameterExpression instanceParam = Expression.Parameter(typeof(TInstance), "instance");

                MemberExpression fieldExpr = Expression.Field(instanceParam, fieldInfo);

                Expression body = fieldExpr;
                if (fieldInfo.FieldType != typeof(TField))
                {
                    body = Expression.Convert(fieldExpr, typeof(TField));
                }

                Func<TInstance, TField> newGetter = Expression.Lambda<Func<TInstance, TField>>(body, instanceParam).Compile();
                s_getterCache[key] = newGetter;
                return newGetter;
            }
            catch (Exception ex)
            {
                Debug.LogError($"Exception while creating getter for '{fieldName}': {ex}");
                // Debug.LogError($"[ReflectionHelper] ");
                return instance => default;
            }
        }

        // 万能 "设" 委托，能够帮你直接将 fieldName 的值设置为 TValue 的值
        public static Action<TInstance, TValue> CreateFieldSetter<TInstance, TValue>(string fieldName)
        {
            (Type, string fieldName) key = (typeof(TInstance), fieldName);
            if (s_setterCache.TryGetValue(key, out Delegate setter))
            {
                return (Action<TInstance, TValue>)setter;
            }

            try
            {
                FieldInfo fieldInfo = AccessTools.Field(typeof(TInstance), fieldName);
                if (fieldInfo == null)
                {
                    Debug.LogError($"[ReflectionHelper] Setter failed: Field '{fieldName}' not found in type '{typeof(TInstance).Name}'.");
                    return (instance, value) => { };
                }

                ParameterExpression instanceParam = Expression.Parameter(typeof(TInstance), "instance");
                ParameterExpression valueParam = Expression.Parameter(typeof(TValue), "value");

                MemberExpression fieldExpr = Expression.Field(instanceParam, fieldInfo);

                BinaryExpression assignExpr = Expression.Assign(fieldExpr, Expression.Convert(valueParam, fieldInfo.FieldType));

                Action<TInstance, TValue> newSetter = Expression.Lambda<Action<TInstance, TValue>>(assignExpr, instanceParam, valueParam).Compile();
                s_setterCache[key] = newSetter;
                return newSetter;
            }
            catch (Exception ex)
            {
                Debug.LogError($"[ReflectionHelper] Exception while creating setter for '{fieldName}': {ex}");
                return (instance, value) => { };
            }
        }

        // 调用有返回值的 method
        public static Func<TInstance, object[], TReturn> CreateMethodCaller<TInstance, TReturn>(string methodName, params Type[] parameterTypes)
        {
            try
            {
                MethodInfo methodInfo = AccessTools.Method(typeof(TInstance), methodName, parameterTypes);
                if (methodInfo == null)
                {
                    Debug.LogError($"[ReflectionHelper] Caller failed: Method '{methodName}' not found in type '{typeof(TInstance).Name}'.");
                    return (instance, args) => default;
                }

                ParameterExpression instanceParam = Expression.Parameter(typeof(TInstance), "instance");
                ParameterExpression argsParam = Expression.Parameter(typeof(object[]), "args");

                ParameterInfo[] paramInfos = methodInfo.GetParameters();
                Expression[] argExpressions = new Expression[paramInfos.Length];
                for (int i = 0; i < paramInfos.Length; i++)
                {
                    Expression arrayAccess = Expression.ArrayIndex(argsParam, Expression.Constant(i));
                    argExpressions[i] = Expression.Convert(arrayAccess, paramInfos[i].ParameterType);
                }

                MethodCallExpression callExpr = Expression.Call(instanceParam, methodInfo, argExpressions);

                if (methodInfo.ReturnType == typeof(void))
                {
                    BlockExpression block = Expression.Block(callExpr, Expression.Default(typeof(TReturn)));
                    return Expression.Lambda<Func<TInstance, object[], TReturn>>(block, instanceParam, argsParam).Compile();
                }

                Expression convertedBody = Expression.Convert(callExpr, typeof(TReturn));
                return Expression.Lambda<Func<TInstance, object[], TReturn>>(convertedBody, instanceParam, argsParam).Compile();
            }
            catch (Exception ex)
            {
                Debug.LogError($"[ReflectionHelper] Exception while creating caller for '{methodName}': {ex}");
                return (instance, args) => default;
            }
        }

        // 调用无返回值的 method
        public static Action<TInstance, object[]> CreateVoidMethodCaller<TInstance>(string methodName, params Type[] parameterTypes)
        {
            try
            {
                MethodInfo methodInfo = AccessTools.Method(typeof(TInstance), methodName, parameterTypes);
                if (methodInfo == null)
                {
                    Debug.LogError($"[ReflectionHelper] Void Caller failed: Method '{methodName}' not found in type '{typeof(TInstance).Name}'.");
                    return (instance, args) => { };
                }

                ParameterExpression instanceParam = Expression.Parameter(typeof(TInstance), "instance");
                ParameterExpression argsParam = Expression.Parameter(typeof(object[]), "args");

                ParameterInfo[] paramInfos = methodInfo.GetParameters();
                Expression[] argExpressions = new Expression[paramInfos.Length];
                for (int i = 0; i < paramInfos.Length; i++)
                {
                    Expression arrayAccess = Expression.ArrayIndex(argsParam, Expression.Constant(i));
                    argExpressions[i] = Expression.Convert(arrayAccess, paramInfos[i].ParameterType);
                }

                MethodCallExpression callExpr = Expression.Call(instanceParam, methodInfo, argExpressions);

                return Expression.Lambda<Action<TInstance, object[]>>(callExpr, instanceParam, argsParam).Compile();
            }
            catch (Exception ex)
            {
                Debug.LogError($"[ReflectionHelper] Exception while creating void caller for '{methodName}': {ex}");
                return (instance, args) => { };
            }
        }
    }
}
