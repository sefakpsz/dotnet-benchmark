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
    public class ApiParallelBencmarks
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
        public List<int> UnlimitedParallelVersion() => ParallelVersion(-1);

        [Benchmark]
        public List<int> LimitedParallelVersion() => ParallelVersion(4);

        [Benchmark]
        public async Task<List<int>> WhenAllVersion()
        {
            var youtubeSubscribersTasks = Enumerable.Range(0, TaskCount)
                .Select(_ => GetSubscriberCountAsync(_httpClient));

            var results = await Task.WhenAll(youtubeSubscribersTasks);

            return [.. results];
        }

        [Benchmark]
        public async Task<List<int>> AsyncParallelVersion1() => await AsyncParallelVersion(1);

        [Benchmark]
        public async Task<List<int>> AsyncParallelVersion10() => await AsyncParallelVersion(10);

        [Benchmark]
        public async Task<List<int>> AsyncParallelVersion100() => await AsyncParallelVersion(100);

        public List<int> ParallelVersion(int maxDegreeOfParallelism)
        {
            var list = new List<int>();

            var youtubeSubscribersTasks = Enumerable.Range(0, TaskCount)
                .Select(_ => new Func<int>(() => GetSubscriberCountAsync(_httpClient).GetAwaiter().GetResult())).ToList();

            Parallel.For(0, youtubeSubscribersTasks.Count,
            new ParallelOptions
            {
                MaxDegreeOfParallelism = maxDegreeOfParallelism
            }, i => list.Add(youtubeSubscribersTasks[i]()));

            return list;
        }

        public async Task<List<int>> AsyncParallelVersion(int batches)
        {
            var list = new List<int>();

            var youtubeSubscribersTasks = Enumerable.Range(0, TaskCount)
                .Select(_ => new Func<Task<int>>(() => GetSubscriberCountAsync(_httpClient))).ToList();

            await ParallelForEachAsync(youtubeSubscribersTasks, batches, async func =>
            {
                list.Add(await func());
            });

            return list;
        }

        public static Task ParallelForEachAsync<T>(IEnumerable<T> source, int degreeOfParallelism, Func<T, Task> body)
        {
            async Task AwaitPartition(IEnumerator<T> partition)
            {
                using (partition)
                {
                    while (partition.MoveNext())
                    {
                        await body(partition.Current);
                    }
                }
            }

            return Task.WhenAll(
                Partitioner
                .Create(source)
                .GetPartitions(degreeOfParallelism)
                .AsParallel()
                .Select(AwaitPartition)
            );
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