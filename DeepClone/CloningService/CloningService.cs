using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace DeepClone.CloningService
{
    public interface ICloningService
    {
        T Clone<T>(T source);
    }

    public class AttributeBaseCloner : ICloningService
    {
        private static readonly MethodInfo memberwiseClone;

        static AttributeBaseCloner()
        {
            memberwiseClone = typeof(Object).GetMethod("MemberwiseClone", BindingFlags.NonPublic | BindingFlags.Instance);
        }

        public T Clone<T>(T source)
        {
            return (T)CloneObject(source, CloningMode.Shallow, new Hashtable(), true);
        }


        private void ClonePublicFieldsProperties(object source, object dest, Hashtable referenceTable)
        {
            var fields = source.GetType().GetFields().Where(fi => fi.IsPublic);

            foreach (var f in fields)
            {
                var srcValue = f.GetValue(source);
                var srcRefValue = GetSrcRefValue(referenceTable, srcValue);
                var mode = GetCloneMode(f.GetCustomAttributes<CloneableAttribute>());
                f.SetValue(dest, srcRefValue != null ? srcRefValue : CloneObject(srcValue, mode, referenceTable));
            }

            var properties = source.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance).Where(
                pi => pi.CanWrite &&
                pi.GetIndexParameters().Count() == 0);

            foreach (var p in properties)
            {
                var srcValue = p.GetValue(source);
                var srcRefValue = GetSrcRefValue(referenceTable, srcValue);
                var mode = GetCloneMode(p.GetCustomAttributes<CloneableAttribute>());
                p.SetValue(dest, srcRefValue != null ? srcRefValue : CloneObject(srcValue, mode, referenceTable));
            }
        }

        private object GetSrcRefValue(Hashtable referenceTable, object srcValue)
        {
            return srcValue == null ? null : referenceTable[srcValue];
        }

        private CloningMode GetCloneMode(IEnumerable<CloneableAttribute> attrs)
        {
            if (attrs.Any(a => a.Mode == CloningMode.Ignore))
                return CloningMode.Ignore;
            if (attrs.Any(a => a.Mode == CloningMode.Shallow))
                return CloningMode.Shallow;
            if (attrs.Any(a => a.Mode == CloningMode.Deep))
                return CloningMode.Deep;
            return CloningMode.Shallow;
        }

        public object CloneObject(Object obj, CloningMode mode, Hashtable referenceTable, bool createNewInstance = false)
        {
            if (obj == null)
                return null;
            var type = obj.GetType();
            if (mode == CloningMode.Ignore)
                return GetDefaultValue(type);
            if (type.IsValueType && (type.IsPrimitive || type.IsEnum))
                return obj;
            if (mode == CloningMode.Shallow && !createNewInstance)
                return obj;
            if (type == typeof(String))
            {
                return string.Copy(obj as String);
            }
            if (type.IsArray)
            {
                var sourceArray = obj as Array;
                var clonedArray = (obj as Array).Clone() as Array;
                referenceTable[obj] = clonedArray;
                for (int i = 0; i < sourceArray.Length; i++)
                {
                    var sourceValue = sourceArray.GetValue(i);
                    var sourceRefValue = GetSrcRefValue(referenceTable, sourceValue);
                    if(createNewInstance && sourceValue is IList)
                        mode = CloningMode.Deep; //deep copy top level list in array
                    clonedArray.SetValue(sourceRefValue != null ? sourceRefValue : CloneObject(
                        sourceArray.GetValue(i), mode, referenceTable), i);
                }
                return clonedArray;
            }
            if (obj is IList)
            {
                var sourceList = obj as IList;
                var clonedList = Activator.CreateInstance(type) as IList;
                referenceTable[obj] = clonedList;
                for (int i = 0; i < sourceList.Count; i++)
                {
                    var sourceValue = sourceList[i];
                    var sourceRefValue = GetSrcRefValue(referenceTable, sourceValue);
                    clonedList.Insert(i, sourceRefValue != null ? sourceRefValue : CloneObject(sourceList[i], mode, referenceTable));
                }
                return clonedList;
            }

            // Class types
            var clonedObj = memberwiseClone.Invoke(obj, null);
            referenceTable[obj] = clonedObj;
            ClonePublicFieldsProperties(obj, clonedObj, referenceTable);
            return clonedObj;
        }

        public object GetDefaultValue(Type t)
        {
            if (t.IsValueType)
            {
                return Activator.CreateInstance(t);
            }
            else
            {
                return null;
            }
        }

    }
}
