using System;
using System.Linq.Expressions;
using System.Reflection;
using HarmonyLib;
using UnityEngine;

namespace tinygrox.DuckovMods.NumericalStats
{
    public static class ReflectionHelper
    {
        // private const BindingFlags BFlags = BindingFlags.NonPublic | BindingFlags.Instance;
        // 万能 “取” 委托，能够帮你从 TInstance 取 TField 类型私有变量 fieldName 的值回来
        public static Func<TInstance, TField> CreateFieldGetter<TInstance, TField>(string fieldName)
        {
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

                return Expression.Lambda<Func<TInstance, TField>>(body, instanceParam).Compile();
            }
            catch (Exception ex)
            {
                Debug.LogError($"[NumericalStats] {ex}");
                return instance => default;
            }
        }

        // 万能 "设" 委托，能够帮你直接将 fieldName 的值设置为 TValue 的值
        public static Action<TInstance, TValue> CreateFieldSetter<TInstance, TValue>(string fieldName)
        {
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

                return Expression.Lambda<Action<TInstance, TValue>>(assignExpr, instanceParam, valueParam).Compile();
            }
            catch (Exception ex)
            {
                Debug.LogError($"[ReflectionHelper] Exception while creating setter for '{fieldName}': {ex}");
                return (instance, value) => { };
            }
        }
    }
}
