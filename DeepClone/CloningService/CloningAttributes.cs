using System;

namespace DeepClone.CloningService
{
    public enum CloningMode
    {
        Deep = 0,
        Shallow = 1,
        Ignore = 2,
    }

    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
    public sealed class CloneableAttribute : Attribute
    {
        public CloningMode Mode { get; }

        public CloneableAttribute(CloningMode mode)
        {
            Mode = mode;
        }
    }
}
