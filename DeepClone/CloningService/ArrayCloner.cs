using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DeepClone.CloningService
{
    class ArrayCloner : IDeepCloner
    {
        public bool CanClone(object source)
        {
            return (source is Array);
        }

        public object DeepClone(object source, ICloningFactory factory)
        {
            var sourceArray = source as Array;
            var clonedArray = (source as Array).Clone() as Array;
            factory.RegisterClonedObject(source, clonedArray);
            for (int i = 0; i < sourceArray.Length; i++)
            {
                var sourceValue = sourceArray.GetValue(i);
                clonedArray.SetValue(factory.CloneObject(sourceArray.GetValue(i), CloningMode.Deep), i);
            }
            return clonedArray;
        }
    }
}
