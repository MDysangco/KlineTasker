using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
                long startDate = new DateTimeOffset(DateTime.UtcNow).ToUnixTimeMilliseconds();
                long endDate = new DateTimeOffset(DateTime.UtcNow.AddYears(-7).AddDays(-2)).ToUnixTimeMilliseconds();

                int rowsDeleted = await StoredProcedures.DeleteKlinesByDateRange(startDate, endDate);

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
