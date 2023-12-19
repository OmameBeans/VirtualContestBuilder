using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace VirtualContestBuilder
{
    public static class Config
    {
        public static List<Contest> Contests =
        [
            new() {
                Id = "44046b60-4810-4f3f-962f-b199e70ed668",
                Name = "obicvirtualcontest",
                Title = "OBICバーチャルコンテスト",
                Memo = "社内バーチャルコンテストです。出題はABCのみからで、難易度は灰灰茶茶緑緑水水です。試験管は含みません。",
                EveryDayStartTime = new TimeSpan(21,0,0),
                DurationSeconds = 7200,
                PenaltySeconds = 300,
                problemInfos =
                [
                    new() {
                        MinDiff = 0,
                        MaxDiff = 199,
                        Point = 100,
                        IsExperimental = false,
                        DuplicateRemoveDays = 0,
                    },
                    new()
                    {
                        MinDiff = 200,
                        MaxDiff = 399,
                        Point = 200,
                        IsExperimental = false,
                        DuplicateRemoveDays = 0,
                    },
                    new()
                    {
                        MinDiff = 400,
                        MaxDiff = 599,
                        Point = 300,
                        IsExperimental = false,
                        DuplicateRemoveDays = 0,
                    },
                    new()
                    {
                        MinDiff = 600,
                        MaxDiff = 799,
                        Point = 400,
                        IsExperimental = false,
                        DuplicateRemoveDays = 0,
                    },
                    new()
                    {
                        MinDiff = 800,
                        MaxDiff = 999,
                        Point = 500,
                        IsExperimental = false,
                        DuplicateRemoveDays = 0,
                    },
                    new()
                    {
                        MinDiff = 1000,
                        MaxDiff = 1199,
                        Point = 600,
                        IsExperimental = false,
                        DuplicateRemoveDays = 0,
                    },
                    new()
                    {
                        MinDiff = 1200,
                        MaxDiff = 1399,
                        Point = 700,
                        IsExperimental = false,
                        DuplicateRemoveDays = 0,
                    },
                    new()
                    {
                        MinDiff = 1400,
                        MaxDiff = 1599,
                        Point = 800,
                        IsExperimental = false,
                        DuplicateRemoveDays = 0,
                    },
                ]
            }
        ];
    }

    public class Contest
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Title { get; set; }
        public string Memo { get; set; }
        public TimeSpan EveryDayStartTime { get; set; }
        public int DurationSeconds { get; set; }
        public int PenaltySeconds { get; set; }
        public List<ProblemInfo> problemInfos { get; set; }
    }

    public class ProblemInfo
    {
        public int MinDiff { get; set; }
        public int MaxDiff { get; set; }
        public int Point { get; set; }
        public bool IsExperimental { get; set; }
        public int DuplicateRemoveDays { get; set; }
    }

    public class CandicateProblem
    {
        public string Id { get; set; }
        public int Point { get; set; }
        public int Order { get; set; }
    }

    public class ProblemModel
    {
        [DataMember(Name = "is_experimental")]
        public bool IsExperimental { get; set; }
        [DataMember(Name = "difficulty")]
        public int Difficulty { get; set; }
    }
}
