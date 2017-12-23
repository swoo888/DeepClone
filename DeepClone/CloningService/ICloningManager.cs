
namespace DeepClone.CloningService
{
    interface ICloningManager
    {
        T Clone<T>(T source);
    }
}
