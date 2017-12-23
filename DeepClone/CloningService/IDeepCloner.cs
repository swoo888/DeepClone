
namespace DeepClone.CloningService
{
    interface IDeepCloner
    {
        bool CanClone(object source);
        object DeepClone(object source, ICloningFactory factory);
    }
}
