using System;
using System.Diagnostics;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;

namespace MeadowClimaHackKit.ServiceAccessLayer
{
    public static class DateTimeService
    {
        private static string City = "Auckland";
        private static string Region = "Pacific";
        static string clockDataUri = $"http://worldtimeapi.org/api/timezone/{Region}/{City}/";

        static DateTimeService() { }

        public static async Task GetTimeAsync(HttpClient client)
        {
            //using (HttpClient client = new HttpClient())
            {
                try
                {
                    client.Timeout = new TimeSpan(0, 5, 0);

                    HttpResponseMessage response = await client.GetAsync($"{clockDataUri}");

                    var stopwatch = new Stopwatch();
                    stopwatch.Start();

                    response.EnsureSuccessStatusCode();
                    string json = await response.Content.ReadAsStringAsync();
                    var values = JsonSerializer.Deserialize<DateTimeEntity>(json);
                    
                    stopwatch.Stop();

                    if (values != null)
                    {
                        var dateTimeOffset = values.datetime.Add(stopwatch.Elapsed);
                        var dateTime = new DateTime(
                            dateTimeOffset.Year,
                            dateTimeOffset.Month,
                            dateTimeOffset.Day,
                            dateTimeOffset.Hour,
                            dateTimeOffset.Minute,
                            dateTimeOffset.Second
                        );

                        MeadowApp.Device.PlatformOS.SetClock(dateTime);
                        Console.WriteLine($"SetClock({dateTime});");
                    }
                    else
                    {
                        Console.WriteLine($"Failed to set SetClock. DateTime={DateTime.Now};");
                    }
                }
                catch (TaskCanceledException)
                {
                    Console.WriteLine("Request timed out.");
                }
                catch (Exception e)
                {
                    Console.WriteLine($"Request went sideways: {e.Message}");
                }
            }
        }
    }

    public class DateTimeEntity
    {
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
#pragma warning disable IDE1006 // Naming Styles
        public string abbreviation { get; set; }
        public string client_ip { get; set; }
        public DateTimeOffset datetime { get; set; }
        public long day_of_week { get; set; }
        public long day_of_year { get; set; }
        public bool dst { get; set; }
        public object dst_from { get; set; }
        public long dst_offset { get; set; }
        public object dst_until { get; set; }
        public long raw_offset { get; set; }
        public string timezone { get; set; }
        public long unixtime { get; set; }
        public DateTimeOffset utc_datetime { get; set; }
        public string utc_offset { get; set; }
        public long week_number { get; set; }
#pragma warning restore IDE1006 // Naming Styles
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
    }
}
