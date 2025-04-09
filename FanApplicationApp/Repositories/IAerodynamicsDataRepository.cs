using SpeedCalc.Models;

namespace SpeedCalc.Repositories;


public interface IAerodynamicsDataRepository
{
    Task<AerodynamicsData> GetByIdAsync(Guid id);
    Task<IEnumerable<AerodynamicsData>> GetAllAsync();
    Task AddAsync(AerodynamicsData data);
    Task UpdateAsync(AerodynamicsData data);
    Task DeleteAsync(Guid id);
    Task<bool> ExistsAsync(Guid id);

    Task<IEnumerable<AerodynamicsData>> GetByTypeAsync(AerodynamicsType? type);
    Task<IEnumerable<AerodynamicsData>> GetByBladeTypeAsync(string bladeType);
    Task<IEnumerable<AerodynamicsData>> GetBySpeedRangeAsync(double minSpeed, double maxSpeed);
    Task<IEnumerable<AerodynamicsData>> GetByAerodynamicSchemeAsync(string schemeName);
    Task<IEnumerable<AerodynamicsData>> GetByFanMarkAsync(string fanMark);
    Task<IEnumerable<AerodynamicsData>> GetByFanMarkDAsync(string fanMarkD);

    Task<(IEnumerable<AerodynamicsData> Items, int TotalCount)> GetPaginatedAsync(
        int pageNumber,
        int pageSize,
        string sortField = null,
        bool ascending = true);
}
