public interface IDataSource
{
    Task<IEnumerable<Resource>> Load();
}