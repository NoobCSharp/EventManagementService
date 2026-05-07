namespace EventManagementService.Repositories
{
    public interface IUnitOfWork
    {
        /// <summary>
        /// Сохраняет изменения в хранилище данных.
        /// </summary>
        Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
    }
}
