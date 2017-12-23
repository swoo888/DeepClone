using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace DeepClone.CloningService
{
    class GeneralClassCloner : IDeepCloner
    {
        private static readonly MethodInfo memberwiseClone;
        private ICloningFactory factory;


        static GeneralClassCloner()
        {
            memberwiseClone = typeof(Object).GetMethod("MemberwiseClone", BindingFlags.NonPublic | BindingFlags.Instance);
        }

        public bool CanClone(object source)
        {
            var type = source.GetType();
            return (type.IsClass || (type.IsValueType && !type.IsPrimitive && !type.IsEnum)); //class or struct
        }

        public object DeepClone(object source, ICloningFactory factory)
        {
            this.factory = factory;
            var clonedObj = memberwiseClone.Invoke(source, null);
            factory.RegisterClonedObject(source, clonedObj);
            ClonePublicFieldsProperties(source, clonedObj);
            return clonedObj;
        }

        private void ClonePublicFieldsProperties(object source, object dest)
        {
            var fields = source.GetType().GetFields().Where(fi => fi.IsPublic);

            foreach (var f in fields)
            {
                var srcValue = f.GetValue(source);
                f.SetValue(dest, factory.CloneObject(srcValue, f.GetCustomAttributes<CloneableAttribute>()));
            }

            var properties = source.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance).Where(
                pi => pi.CanWrite &&
                pi.GetIndexParameters().Count() == 0);

            foreach (var p in properties)
            {
                var srcValue = p.GetValue(source);
                p.SetValue(dest, factory.CloneObject(srcValue, p.GetCustomAttributes<CloneableAttribute>()));
            }
        }
    }
}
