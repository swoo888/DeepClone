using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DeepClone.CloningService
{
    class ListCloner : IDeepCloner
    {
        public bool CanClone(object source)
        {
            return (source is IList);
        }

        public object DeepClone(object source, ICloningFactory factory)
        {
            var sourceList = source as IList;
            var clonedList = Activator.CreateInstance(source.GetType()) as IList;
            factory.RegisterClonedObject(source, clonedList);
            for (int i = 0; i < sourceList.Count; i++)
            {
                var sourceValue = sourceList[i];
                clonedList.Insert(i, factory.CloneObject(sourceList[i], CloningMode.Deep));
            }
            return clonedList;
        }
    }
}
