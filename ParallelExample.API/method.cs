using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ParallelExample.API
{
    public class method
    {
        public YouTubeChannelResponse run()
        {
            var response = new YouTubeChannelResponse
            {
                Items = new List<YouTubeChannelItem>
            {
                new YouTubeChannelItem
                {
                    Statistics = new YouTubeChannelStatistics
                    {
                        SubscriberCount = 1000000
                    }
                },
                new YouTubeChannelItem
                {
                    Statistics = new YouTubeChannelStatistics
                    {
                        SubscriberCount = 500000
                    }
                }
            }
            };

            return response;
        }
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