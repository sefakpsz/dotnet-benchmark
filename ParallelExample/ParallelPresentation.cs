using System.Collections.Concurrent;
using System.Net.Http.Json;
using BenchmarkDotNet.Attributes;
using Microsoft.CodeAnalysis;


/*
Paralell method says that okey I have 10 engineers I am going to split the work that I have to do into 10 segments when I give it to them
they're going to go do it and then they're going to come back to me with the results and I'm going to aggregate them and return them

However

WhenAll something different. Research it...
*/

namespace ParalellExample
{
    [MemoryDiagnoser]
    public class ParallelPresentation
    {
        private static readonly HttpClient _httpClient = new();
        private const int TaskCount = 1000;

        [Benchmark]
        public async Task<List<int>> ForEachVersion()
        {
            var list = new List<int>();

            var youtubeSubscribersTasks = Enumerable.Range(0, TaskCount)
                .Select(_ => new Func<Task<int>>(() => GetSubscriberCountAsync(_httpClient))).ToList();

            foreach (var youtubeSubscribersTask in youtubeSubscribersTasks)
            {
                list.Add(await youtubeSubscribersTask());
            }

            return list;
        }

        [Benchmark]
        public List<int> ParallelVersion()
        {
            var list = new List<int>();

            var youtubeSubscribersTasks = Enumerable.Range(0, TaskCount)
                .Select(_ => new Func<int>(() => GetSubscriberCountAsync(_httpClient).GetAwaiter().GetResult())).ToList();

            Parallel.For(0, youtubeSubscribersTasks.Count,
            new ParallelOptions
            {
                MaxDegreeOfParallelism = -1
            }, i => list.Add(youtubeSubscribersTasks[i]()));

            return list;
        }

        private static async Task<int> GetSubscriberCountAsync(HttpClient client)
        {
            string apiUrl = $"http://localhost:5152/youtube";

            HttpResponseMessage response = await client.GetAsync(apiUrl);
            if (response.IsSuccessStatusCode)
            {
                var data = await response.Content.ReadFromJsonAsync<YouTubeChannelResponse>();
                if (data != null && data.Items != null && data.Items.Count > 0)
                {
                    return data.Items[0].Statistics.SubscriberCount;
                }
                else
                {
                    Console.WriteLine("No data received from YouTube API.");
                }
            }
            else
            {
                Console.WriteLine($"Failed to fetch data from YouTube API. Status code: {response.StatusCode}");
            }
            return -1; // Return -1 if subscriber count cannot be fetched
        }

        public class YouTubeChannelResponse
        {
            public List<YouTubeChannelItem> Items { get; set; }
        }

        public class YouTubeChannelItem
        {
            public YouTubeChannelStatistics Statistics { get; set; }
        }

        public class YouTubeChannelStatistics
        {
            public int SubscriberCount { get; set; }
        }
    }
}