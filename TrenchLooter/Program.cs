using Cronos;
using Microsoft.Extensions.Configuration;
using System.Collections.Concurrent;
using TrenchLooter.CronTasks;
using Zyprix.Data.Repositories;
using Zyprix.Services;

internal class Program
{

	private static readonly ConcurrentDictionary<string, bool> JobLocks = new();

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

        IEnumerable<Task> tasks = cronJobs.Select(job => RunScheduledJob(job.Key, job.Value, config, cancellationToken.Token));

        Console.WriteLine("Cron job started.");
        await Task.WhenAll(tasks);
    }


    private static async Task RunScheduledJob(string jobName, string cronExpression, IConfiguration config, CancellationToken cancellationToken)
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

				if (!JobLocks.TryAdd(jobName, true))
				{
					Console.WriteLine($"{jobName} skipped (already running)");
					continue;
				}

				try
				{
					Console.WriteLine($"{jobName} running at {DateTime.UtcNow:yyyy-MM-dd HH:mm:ss} UTC");
					await ExecuteJob(jobName, config, cancellationToken); 
                    Console.WriteLine($"{jobName} finished at {DateTime.UtcNow:yyyy-MM-dd HH:mm:ss} UTC");
				}
				finally
				{
					JobLocks.TryRemove(jobName, out _);
				}
			}
			catch (Exception ex)
            {
                Console.WriteLine($"{jobName} failed: {ex.Message}");
            }
        }
    }

    private static async Task ExecuteJob(string jobName, IConfiguration config, CancellationToken cancellationToken)
    {
        switch (jobName)
        {
            case "DeleteKlines":
                await DeleteKlines.Run(config, cancellationToken);
                break;
            case "UpdateKlines":
                await UpdateKlines.Run(config, cancellationToken);
                break;
            case "BackFillKlines":
                await BackFillKlines.Run(config, cancellationToken);
                break;
            case "UpdateCoins":
                await UpdateCoins.Run(config, cancellationToken);
                break;
            default:
                break;
        }
    }


}