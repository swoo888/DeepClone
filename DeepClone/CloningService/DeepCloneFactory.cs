using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace DeepClone.CloningService
{
    class DeepCloneFactory : ICloningFactory
    {
        private Hashtable cloningModeOverrides;
        private Hashtable clonedObjectsReference;
        private List<IDeepCloner> cloners;

        public DeepCloneFactory()
        {
            cloningModeOverrides = new Hashtable();
            clonedObjectsReference = new Hashtable();
            cloners = new List<IDeepCloner>();
        }

        public void AddCloningModeOverrides(object source, CloningMode mode)
        {
            if (!cloningModeOverrides.Contains(source))
                cloningModeOverrides.Add(source, mode);
        }

        public object CloneObject(object source, CloningMode mode)
        {
            if (source == null)
                return null;
            var modeOverride = cloningModeOverrides[source];
            if (modeOverride != null)
                mode = (CloningMode) modeOverride; 
            var type = source.GetType();
            if (mode == CloningMode.Ignore)
                return GetDefaultValue(type);
            if (type.IsValueType && (type.IsPrimitive || type.IsEnum))
                return source;
            var clonedRef = GetClonedObjReference(source);
            if (clonedRef != null)
                return clonedRef;
            if (mode == CloningMode.Shallow)
                return source;
            foreach (var p in cloners)
            {
                if (p.CanClone(source))
                    return p.DeepClone(source, this);
            }
            throw new Exception(string.Format("No Cloner exists for object: {0}, type: {1}", source, source.GetType()));
        }

        public object GetClonedObjReference(object source)
        {
            return source == null ? null : clonedObjectsReference[source];
        }

        public void RegisterClonedObject(object source, object cloned)
        {
            if(!clonedObjectsReference.Contains(source))
                clonedObjectsReference.Add(source, cloned);
        }

        public void Reset()
        {
            cloningModeOverrides.Clear();
            clonedObjectsReference.Clear();
        }

        private CloningMode GetCloningMode(object source, IEnumerable<CloneableAttribute> attrs)
        {
            if (source == null)
                return CloningMode.Shallow;
            var m = cloningModeOverrides[source];
            if (m != null)
                return (CloningMode)m;
            if (attrs == null || attrs.Count() <= 0)
                return CloningMode.Shallow;
            if (attrs.Any(a => a.Mode == CloningMode.Ignore))
                return CloningMode.Ignore;
            if (attrs.Any(a => a.Mode == CloningMode.Shallow))
                return CloningMode.Shallow;
            if (attrs.Any(a => a.Mode == CloningMode.Deep))
                return CloningMode.Deep;
            return CloningMode.Shallow;
        }

        private object GetDefaultValue(Type t)
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

        public object CloneObject(object source, IEnumerable<CloneableAttribute> attributes)
        {
            var srcRefValue = GetClonedObjReference(source);
            if (srcRefValue != null)
                return srcRefValue;
            var mode = GetCloningMode(source, attributes);
            return CloneObject(source, mode);
        }

        public void RegisterCloners(IDeepCloner cloner)
        {
            cloners.Add(cloner);
        }
    }
}
