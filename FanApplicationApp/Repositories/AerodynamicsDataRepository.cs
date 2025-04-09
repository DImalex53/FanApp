using SpeedCalc.Models;
using Microsoft.EntityFrameworkCore;
using SpeedCalc.Data;

namespace SpeedCalc.Repositories
{
    public class AerodynamicsDataRepository : IAerodynamicsDataRepository
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<AerodynamicsDataRepository> _logger;

        public AerodynamicsDataRepository(
            ApplicationDbContext context,
            ILogger<AerodynamicsDataRepository> logger)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<AerodynamicsData> GetByIdAsync(Guid id)
        {
            try
            {
                return await _context.AerodynamicsData
                    .AsNoTracking()
                    .FirstOrDefaultAsync(a => a.Id == id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при получении данных аэродинамики по ID: {Id}", id);
                throw;
            }
        }

        public async Task<IEnumerable<AerodynamicsData>> GetAllAsync()
        {
            try
            {
                return await _context.AerodynamicsData
                    .AsNoTracking()
                    .OrderBy(a => a.Type)
                    .ThenBy(a => a.MinSpeed)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при получении всех данных аэродинамики");
                throw;
            }
        }

        public async Task<IEnumerable<AerodynamicsData>> GetByTypeAsync(AerodynamicsType? type)
        {
            try
            {
                var query = _context.AerodynamicsData.AsNoTracking();

                if (type.HasValue)
                {
                    query = query.Where(a => a.Type == type.Value);
                }

                return await query
                    .OrderBy(a => a.MinSpeed)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при получении данных аэродинамики по типу: {Type}", type);
                throw;
            }
        }

        public async Task<IEnumerable<AerodynamicsData>> GetByBladeTypeAsync(string bladeType)
        {
            if (string.IsNullOrWhiteSpace(bladeType))
                throw new ArgumentException("Тип лопастей не может быть пустым", nameof(bladeType));

            try
            {
                return await _context.AerodynamicsData
                    .Where(a => a.TypeOfBlades != null && a.TypeOfBlades.Contains(bladeType))
                    .AsNoTracking()
                    .OrderBy(a => a.Type)
                    .ThenBy(a => a.MinSpeed)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при получении данных аэродинамики по типу лопастей: {BladeType}", bladeType);
                throw;
            }
        }

        public async Task<IEnumerable<AerodynamicsData>> GetBySpeedRangeAsync(double minSpeed, double maxSpeed)
        {
            if (minSpeed < 0 || maxSpeed < 0 || minSpeed > maxSpeed)
                throw new ArgumentException("Недопустимый диапазон скоростей");

            try
            {
                return await _context.AerodynamicsData
                    .Where(a => a.MinSpeed <= maxSpeed && a.MaxSpeed >= minSpeed)
                    .AsNoTracking()
                    .OrderBy(a => a.Type)
                    .ThenBy(a => a.MinSpeed)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при получении данных аэродинамики по диапазону скоростей: {MinSpeed}-{MaxSpeed}", minSpeed, maxSpeed);
                throw;
            }
        }

        public async Task<IEnumerable<AerodynamicsData>> GetByAerodynamicSchemeAsync(string schemeName)
        {
            if (string.IsNullOrWhiteSpace(schemeName))
                throw new ArgumentException("Название схемы не может быть пустым", nameof(schemeName));

            try
            {
                return await _context.AerodynamicsData
                    .Where(a => a.Name != null && a.Name.Contains(schemeName))
                    .AsNoTracking()
                    .OrderBy(a => a.Type)
                    .ThenBy(a => a.MinSpeed)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при получении данных аэродинамики по схеме: {SchemeName}", schemeName);
                throw;
            }
        }

        public async Task<IEnumerable<AerodynamicsData>> GetByFanMarkAsync(string fanMark)
        {
            if (string.IsNullOrWhiteSpace(fanMark))
                throw new ArgumentException("Марка вентилятора не может быть пустой", nameof(fanMark));

            try
            {
                return await _context.AerodynamicsData
                    .Where(a => a.NewMarkOfFan != null && a.NewMarkOfFan.Contains(fanMark))
                    .AsNoTracking()
                    .OrderBy(a => a.Type)
                    .ThenBy(a => a.MinSpeed)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при получении данных аэродинамики по марке вентилятора: {FanMark}", fanMark);
                throw;
            }
        }

        public async Task<IEnumerable<AerodynamicsData>> GetByFanMarkDAsync(string fanMarkD)
        {
            if (string.IsNullOrWhiteSpace(fanMarkD))
                throw new ArgumentException("Марка вентилятора D не может быть пустой", nameof(fanMarkD));

            try
            {
                return await _context.AerodynamicsData
                    .Where(a => a.NewMarkOfFanD != null && a.NewMarkOfFanD.Contains(fanMarkD))
                    .AsNoTracking()
                    .OrderBy(a => a.Type)
                    .ThenBy(a => a.MinSpeed)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при получении данных аэродинамики по марке вентилятора D: {FanMarkD}", fanMarkD);
                throw;
            }
        }

        public async Task<(IEnumerable<AerodynamicsData> Items, int TotalCount)> GetPaginatedAsync(
            int pageNumber,
            int pageSize,
            string sortField = null,
            bool ascending = true)
        {
            if (pageNumber < 1)
                throw new ArgumentException("Номер страницы должен быть положительным", nameof(pageNumber));

            if (pageSize < 1)
                throw new ArgumentException("Размер страницы должен быть положительным", nameof(pageSize));

            try
            {
                var query = _context.AerodynamicsData.AsNoTracking();

                // Применяем сортировку, если указано поле
                if (!string.IsNullOrWhiteSpace(sortField))
                {
                    var propertyInfo = typeof(AerodynamicsData).GetProperty(sortField);
                    if (propertyInfo == null)
                    {
                        throw new ArgumentException($"Неверное поле для сортировки: {sortField}");
                    }

                    query = ascending
                        ? query.OrderByProperty(sortField)
                        : query.OrderByPropertyDescending(sortField);
                }
                else
                {
                    query = query.OrderBy(a => a.Type).ThenBy(a => a.MinSpeed);
                }

                var totalCount = await query.CountAsync();
                var items = await query
                    .Skip((pageNumber - 1) * pageSize)
                    .Take(pageSize)
                    .ToListAsync();

                return (items, totalCount);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при постраничном получении данных аэродинамики");
                throw;
            }
        }

        public async Task AddAsync(AerodynamicsData data)
        {
            if (data == null)
                throw new ArgumentNullException(nameof(data));

            try
            {
                await _context.AerodynamicsData.AddAsync(data);
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при добавлении данных аэродинамики");
                throw;
            }
        }

        public async Task UpdateAsync(AerodynamicsData data)
        {
            if (data == null)
                throw new ArgumentNullException(nameof(data));

            try
            {
                var existing = await _context.AerodynamicsData.FindAsync(data.Id);
                if (existing == null)
                    throw new KeyNotFoundException($"Данные аэродинамики с ID {data.Id} не найдены");

                _context.Entry(existing).CurrentValues.SetValues(data);
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при обновлении данных аэродинамики с ID: {Id}", data.Id);
                throw;
            }
        }

        public async Task DeleteAsync(Guid id)
        {
            try
            {
                var data = await _context.AerodynamicsData.FindAsync(id);
                if (data != null)
                {
                    _context.AerodynamicsData.Remove(data);
                    await _context.SaveChangesAsync();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при удалении данных аэродинамики с ID: {Id}", id);
                throw;
            }
        }

        public async Task<bool> ExistsAsync(Guid id)
        {
            try
            {
                return await _context.AerodynamicsData.AnyAsync(a => a.Id == id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при проверке существования данных аэродинамики с ID: {Id}", id);
                throw;
            }
        }
    }

    public static class QueryableExtensions
    {
        public static IOrderedQueryable<T> OrderByProperty<T>(this IQueryable<T> source, string propertyName)
        {
            return source.OrderBy(x => EF.Property<object>(x, propertyName));
        }

        public static IOrderedQueryable<T> OrderByPropertyDescending<T>(this IQueryable<T> source, string propertyName)
        {
            return source.OrderByDescending(x => EF.Property<object>(x, propertyName));
        }
    }
}