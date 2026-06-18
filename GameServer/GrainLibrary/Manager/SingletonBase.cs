using BindingFlags = System.Reflection.BindingFlags;

namespace ServerLibrary.Manager;

public abstract class SingletonBase<T>
    where T: class
{
    private static readonly Lazy<T> _instance = new Lazy<T>(() =>
    {
        var constructor = typeof(T).GetConstructor(
            BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic,
            null, Type.EmptyTypes, null);

        if (constructor == null)
        {
            throw new InvalidOperationException($"{typeof(T).Name} 클래스에 매개변수가 없는 생성자가 없습니다.");
        }

        return (T)constructor.Invoke(null);
    });

    public static T Instance => _instance.Value;    
}