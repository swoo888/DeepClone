using System;
using System.Collections;

namespace DeepClone.CloningService
{
    class DeepCloneManager : ICloningManager
    {
        private ICloningFactory cloningFactory;

        public DeepCloneManager(ICloningFactory factory)
        {
            cloningFactory = factory;
        }

        public T Clone<T>(T source)
        {
            cloningFactory.Reset();
            SetupCloningModeOverrides(source);
            return (T)cloningFactory.CloneObject(source, CloningMode.Shallow);
        }

        private void SetupCloningModeOverrides(object source)
        {
            cloningFactory.AddCloningModeOverrides(source, CloningMode.Deep);
            if (source is Array)
            {
                var arraySource = source as Array;
                for (int i = 0; i < arraySource.Length; i++)
                {
                    if (arraySource.GetValue(i) is IList)
                    {
                        cloningFactory.AddCloningModeOverrides(arraySource.GetValue(i), CloningMode.Deep); //top level List gets deep cloned
                    }
                }
            }
            else if (source is IList)
            {
                var sourceList = source as IList;
                for (int i = 0; i < sourceList.Count; i++)
                {
                    if (sourceList[i] is IList)
                    {
                        cloningFactory.AddCloningModeOverrides(sourceList[i], CloningMode.Deep);
                    }
                }
            }
        }


    }
}
