namespace ServerLibrary.Resource.Model;

public interface IDataSet<T>
    where T : class
{
    bool Load(T data);
    bool PostProcess(T data);
}