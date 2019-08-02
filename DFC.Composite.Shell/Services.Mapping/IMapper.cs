namespace DFC.Composite.Shell.Services.Mapping
{
    public interface IMapper<in TS, in TD>
    {
        void Map(TS source, TD destination);
    }
}