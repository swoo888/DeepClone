using System.Collections.Generic;

namespace DeepClone.CloningService
{
    interface ICloningFactory
    {
        object CloneObject(object source, CloningMode mode);
        object CloneObject(object source, IEnumerable<CloneableAttribute> attributes);

        void RegisterClonedObject(object source, object cloned);
        void AddCloningModeOverrides(object source, CloningMode mode);
        void Reset();

        void RegisterCloners(IDeepCloner cloner);

    }
}
