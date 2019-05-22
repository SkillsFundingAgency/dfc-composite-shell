namespace DFC.Composite.Shell.Services.Mapping
{
    public interface IMapper<S,D>
    {
        D Map(S source);
    }
}