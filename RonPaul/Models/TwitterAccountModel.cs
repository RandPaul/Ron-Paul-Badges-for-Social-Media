using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Raven.Client.Indexes;

namespace RonPaul.Models
{
    public class TwitterAccountModel
    {
        public string Id { get; set; }
        public string TwitterUserId { get; set; }
        public string UserName { get; set; }
        public string TwitterAccessKey { get; set; }
        public string TwitterAccessSecret { get; set; }
        public string TwitterOriginalProfilePictureURL { get; set; }

        public Controllers.TwitterController.BadgeType BadgeType { get; set; }
    }

    public class UniqueTwitterBadgeTypesResult
    {
        public Controllers.TwitterController.BadgeType BadgeType { get; set; }
        public int Count { get; set; }
    }

    public class UniqueTwitterBadgeTypesIndex : AbstractIndexCreationTask<TwitterAccountModel, UniqueTwitterBadgeTypesResult>
    {
        public UniqueTwitterBadgeTypesIndex()
        {
            Map = models => from model in models
                            select new
                            {
                                BadgeType = model.BadgeType,
                                Count = 1
                            };
            Reduce = results => from result in results
                                group result by result.BadgeType into g
                                select new
                                {
                                    BadgeType = g.Key,
                                    Count = g.Sum(x => x.Count)
                                };
        }
    }
}