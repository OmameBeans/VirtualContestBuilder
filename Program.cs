using VirtualContestBuilder;
using System.IO.Compression;
using System.Text.Encodings.Web;
using System.Text.Json;
using static System.Net.Mime.MediaTypeNames;
using System.Text;
using System.Security.Cryptography.X509Certificates;

internal class Program
{
    private static async Task<Dictionary<string,ProblemModel>> GetProblems(string requestUrl)
    {
        using var httpClient = new HttpClient();
        httpClient.DefaultRequestHeaders.Add("Referer", "https://kenkoooo.com/atcoder/");
        httpClient.DefaultRequestHeaders.Add("Accept-Encoding", "gzip");
        var responce = await httpClient.GetAsync(requestUrl, HttpCompletionOption.ResponseContentRead);

        Console.WriteLine(responce.StatusCode);

        Dictionary<string, Dictionary<string,JsonElement>> problemDict = [];

        if (responce.IsSuccessStatusCode)
        {
            var byteArray = await responce.Content.ReadAsByteArrayAsync();
            using var ms = new MemoryStream(byteArray);
            using var ds = new GZipStream(ms, CompressionMode.Decompress);
            using var reader = new StreamReader(ds);
            var jsonText = await reader.ReadToEndAsync();

            var options = new JsonSerializerOptions
            {
                Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
            };
            problemDict = JsonSerializer.Deserialize<Dictionary<string, Dictionary<string,JsonElement>>>(jsonText, options);
        }

        var copyProblemDict = new Dictionary<string, ProblemModel>();
        foreach (var problem in problemDict)
        {
            var model = new ProblemModel();
            foreach (var element in problem.Value)
            {
                if(element.Key == "difficulty")
                {
                    model.Difficulty = element.Value.GetInt32();
                }
                
                if(element.Key == "is_experimental")
                {
                    model.IsExperimental = element.Value.GetBoolean();
                }
            }
            copyProblemDict.Add(problem.Key, model);
        }

        return copyProblemDict;
    }

    private static List<CandicateProblem> CreateCandicateProblems(Dictionary<string,ProblemModel> problemDist)
    {
        var contest = Config.Contests.FirstOrDefault();
        var candicateProblems = new List<CandicateProblem>();

        foreach (var problem in contest.problemInfos.Select((Value,Index) => new { Value,Index}))
        {
            var problemId = problem.Index;
            var problemDetail = problem.Value;

            var minDiff = problemDetail.MinDiff;
            var maxDiff = problemDetail.MaxDiff;
            var li = problemDist.Where(x =>
                                    minDiff <= x.Value.Difficulty
                                    && x.Value.Difficulty <= maxDiff
                                    && x.Key.Contains("abc")
                                    && x.Value.IsExperimental == problemDetail.IsExperimental
                                    )
                                .ToList();
            var count = li.Count;
            if (count == 0)
            {
                Console.WriteLine("候補となる問題が存在しません");
                return [];
            }
            var rnd = new Random();
            var index = rnd.Next(count);
            candicateProblems.Add(new CandicateProblem
            {
                Id = li[index].Key,
                Point = problemDetail.Point,
                Order = problemId,
            });
        }

        return candicateProblems;
    }

    private static async Task<bool> CreateVirtualContest(List<CandicateProblem> candicateProblems,Contest contest)
    {
        using var httpClient = new HttpClient();
        httpClient.DefaultRequestHeaders.Add("Referer", "https://kenkoooo.com/atcoder/");
        httpClient.DefaultRequestHeaders.Add("Cookie", "token=xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx");
        httpClient.DefaultRequestHeaders.Add("Accept-Encoding", "gzip");

        var today = DateTime.Now.Date + contest.EveryDayStartTime;
        var todayOffset = new DateTimeOffset(today);
        var problemDict = new Dictionary<string, dynamic>
        {
            {"title",$"{today:yyyy/MM/dd} {contest.Title}" },
            {"memo",contest.Memo },
            {"start_epoch_second", todayOffset.ToUnixTimeSeconds()},
            {"duration_second",contest.DurationSeconds },
            {"mode",null },
            {"is_public",false },
            {"penalty_second",contest.PenaltySeconds },
        };

        var problemJsonText = JsonSerializer.Serialize(problemDict, new JsonSerializerOptions
        {
            WriteIndented = true,
        });

        Console.WriteLine(problemJsonText);

        var content = new StringContent(problemJsonText, Encoding.UTF8, "application/json");
        var responce = await httpClient.PostAsync("https://kenkoooo.com/atcoder/internal-api/contest/create", content);

        if (responce.IsSuccessStatusCode)
        {
            Console.WriteLine("コンテストの作成に成功しました");
        }
        else
        {
            Console.WriteLine("コンテストの作成に失敗しました");
            return false;
        }

        var byteArray = await responce.Content.ReadAsByteArrayAsync();
        using var ms = new MemoryStream(byteArray);
        using var ds = new GZipStream(ms, CompressionMode.Decompress);
        using var reader = new StreamReader(ds);
        var jsonText2 = await reader.ReadToEndAsync();

        var options = new JsonSerializerOptions
        {
            Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
        };
        var dict = JsonSerializer.Deserialize<Dictionary<string, dynamic>>(jsonText2, options);

        var contest_id = dict["contest_id"];

        Console.WriteLine($"コンテスト作成：https://kenkoooo.com/atcoder/#/contest/show/{contest_id}");

        var contestDict = new Dictionary<string, dynamic>
        {
            {"contest_id", contest_id },
            {"problems", candicateProblems},
        };

        var contestJsonText = JsonSerializer.Serialize(contestDict, new JsonSerializerOptions
        {
            WriteIndented = true,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        });

        Console.WriteLine(contestJsonText);

        var updateContent = new StringContent(contestJsonText,Encoding.UTF8, "application/json");
        var updateResponce = await httpClient.PostAsync("https://kenkoooo.com/atcoder/internal-api/contest/item/update", updateContent);

        if(updateResponce.IsSuccessStatusCode)
        {
            Console.WriteLine("問題の設定に成功しました");
        } else
        {
            Console.WriteLine("問題の設定に失敗しました");
            return false;
        }

        return true;
    }

    private static async Task Main(string[] args)
    {
        var problemDist = await GetProblems("https://kenkoooo.com/atcoder/resources/problem-models.json");

        var candicatePromlems = CreateCandicateProblems(problemDist);

        var created = await CreateVirtualContest(candicatePromlems, Config.Contests.FirstOrDefault());
    }
}