


using NuGet.SampleSharedModels.DTO;
using NuGet.SampleSharedModels.Results;

namespace SampleAppApi.Interfaces.ExternalServices
{
    public interface IDataAccessClient
    {
        Task<ServiceResult<IEnumerable<MovieDTORead>>> GetAll();
        Task<ServiceResult<MovieDTORead>> GetById(string id);
        Task<ServiceResult<MovieDTORead>> Add(MovieDTOAdd movieDTOAdd);
        Task<ServiceResult<MovieDTORead>> Update(string id, MovieDTOUpdate movieDTOUpdate);
        Task<ServiceResult<bool>> Delete(string id);
    }
}
