using System.Threading.Tasks;

namespace DFC.Composite.Shell.Services.Mapping
{
    public interface IMapper<in TS, in TD>
    {
        Task Map(TS source, TD destination);
    }
}