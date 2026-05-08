using Microsoft.Extensions.Configuration;
using Zyprix.Data.Interfaces;
using Zyprix.Data.Repositories;
using Zyprix.Services;
using Zyprix.Services.Interfaces;

namespace TrenchLooter.CronTasks
{
    public class DeleteKlines
    {
        /// <summary>
        /// This task will remove any klines older than specified cuttoff year.
        /// </summary>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public static async Task<bool> Run(CancellationToken cancellationToken)
        {
            try
            {
                ZypryxClient zypryxClient = new ZypryxClient();

                long startDate = new DateTimeOffset(DateTime.UtcNow).ToUnixTimeMilliseconds();
                long endDate = new DateTimeOffset(DateTime.UtcNow.AddYears(-7).AddDays(-2)).ToUnixTimeMilliseconds();

                int rowsDeleted = await zypryxClient.DeleteKlinesByDateRange(startDate, endDate);

                return rowsDeleted > 0;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return false;
            }
        }
    }
}
