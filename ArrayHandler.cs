using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Reflection;
using System;

namespace vilistaa.arrayHandler
{
    public static class ArrayHandler
    {
        public static List<string> FindArrayNamesByType(MonoBehaviour scriptInstance, Type typeToCheck)
        {
            List<string> arrayNames = new List<string>();

            FieldInfo[] fields = scriptInstance.GetType().GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);

            foreach (FieldInfo field in fields)
            {
                if (field.FieldType.IsArray && field.FieldType.Equals(typeToCheck))
                {
                    arrayNames.Add(field.Name);
                }
            }

            return arrayNames;
        }
    }
}
