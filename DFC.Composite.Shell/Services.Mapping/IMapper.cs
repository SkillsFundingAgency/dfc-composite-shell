namespace DFC.Composite.Shell.Services.Mapping
{
    public interface IMapper<S,D>
    {
        void Map(S source, D destination);
    }
}