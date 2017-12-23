using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DeepClone.CloningService
{
    class StringCloner : IDeepCloner
    {
        public bool CanClone(object source)
        {
            return (source is string);
        }

        public object DeepClone(object source, ICloningFactory factory)
        {
            return string.Copy(source as string);
        }
    }
}
