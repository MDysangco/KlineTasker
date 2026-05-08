using Cronos;
using Microsoft.Extensions.Configuration;
using TrenchLooter.CronTasks;
using Zyprix.Data.Repositories;
using Zyprix.Services;

internal class Program
{
	private static async Task Main(string[] args)
	{
        IConfiguration? config = new ConfigurationBuilder().AddJsonFile("appsettings.json").Build();

        Dictionary<string, string> cronJobs = config.GetSection("CronJob").Get<Dictionary<string, string>>();

        if (cronJobs == null)
        {
            Console.WriteLine("cronJobs is empty.");
            return;
        }

        CancellationTokenSource cancellationToken = new CancellationTokenSource();

        IEnumerable<Task> tasks = cronJobs.Select(job => RunScheduledJob(job.Key, job.Value, cancellationToken.Token));

        Console.WriteLine("Cron job started.");
        await Task.WhenAll(tasks);
    }


    private static async Task RunScheduledJob(string jobName, string cronExpression, CancellationToken cancellationToken)
    {
        CronExpression cron = CronExpression.Parse(cronExpression);
        TimeZoneInfo timeZone = TimeZoneInfo.Utc;

        Console.WriteLine($"{jobName} scheduled with cron: {cronExpression}");

        while(!cancellationToken.IsCancellationRequested)
        {
            try
            {
                DateTimeOffset now = DateTimeOffset.UtcNow;
                DateTimeOffset? next = cron.GetNextOccurrence(now, timeZone);

                if (next == null)
                {
                    break;
                }

                TimeSpan delay = next.Value - now;

                if (delay > TimeSpan.Zero)
                {
                    await Task.Delay(delay, cancellationToken);
                }
                Console.WriteLine($"{jobName} running at {DateTime.UtcNow:yyyy-MM-dd HH:mm:ss} UTC");

                await ExecuteJob(jobName, cancellationToken);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"{jobName} failed: {ex.Message}");
            }
        }
    }

    private static async Task ExecuteJob(string jobName, CancellationToken cancellationToken)
    {
        switch (jobName)
        {
            case "DeleteKlines":
                await DeleteKlines.Run(cancellationToken);
                break;
            case "UpdateKlines":
                await UpdateKlines.Run(cancellationToken);
                break;
            case "BackFillKlines":
                await BackFillKlines.Run(cancellationToken);
                break;
            case "UpdateCoins":
                await UpdateCoins.Run(cancellationToken);
                break;
            default:
                break;
        }
    }


}