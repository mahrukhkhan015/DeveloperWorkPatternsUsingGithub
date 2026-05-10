using Microsoft.AspNetCore.Mvc;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Text.RegularExpressions;
using WebApplication1.Models;

namespace WebApplication1.Controllers
{
    //[ApiController]
    //[Route("[controller]/[action]")]
    public class HomeController : Controller
    {

        //DYNAMIC REPO URL
        [HttpGet]
        public IActionResult Index()
        {
            return View(new DashboardViewModel());
        }
        
        [HttpGet]
        public async Task<IActionResult> GetRepoData(string repo = "dotnet/runtime")
        {
            repo = Regex.Replace(repo, @"[^a-zA-Z0-9/_\.-]", "");
            repo = repo.Trim();
            if (string.IsNullOrEmpty(repo)) {
                //TempData["Error"] = "Please provide correct input";

                //return RedirectToAction("Index");
                return BadRequest("Please provide correct input");
            }
            var parts = repo.Split('/');

            if (parts.Length != 2){

                //TempData["Error"] = "Please provide correct input";

                //return RedirectToAction("Index");
                return BadRequest("Please provide correct input");
            }
            var owner = parts[0];
            var name = parts[1];


            using var client = new HttpClient();

            // GitHub API requires a User-Agent
            client.DefaultRequestHeaders.UserAgent.Add(
                new ProductInfoHeaderValue("CSharpApp", "1.0"));
            client.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", );
            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };
            int page = 1;
            int perPage = 100;
            List<CommitModel> commitData = new List<CommitModel>();

            try
            {
                while (true)
                {
                    var url = $"https://api.github.com/repos/{owner}/{name}/commits?per_page={perPage}&page={page}";
                    var response = await client.GetAsync(url);
                    if (!response.IsSuccessStatusCode)
                        break;

                    var json = await response.Content.ReadAsStringAsync();

                    var commits = JsonSerializer.Deserialize<List<CommitRoot>>(json, options);
                    // Stop if no more commits
                    if (commits == null || commits.Count == 0)
                        break;

                    foreach (var c in commits)
                    {
                        commitData.Add(new CommitModel() { commit_message = c.commit.message, author_name = c.commit.author.name, commit_date = c.commit.author.date });
                    }
                    page++;

                    if (page > 10) break; // cap at 1000 commits TO AVOID FETCH LIMIT : 5000 requests/hour
                }
            }
            catch (Exception e)
            {
                //TempData["Error"] = "Failed to fetch data for given input";

                //return RedirectToAction("Index");
                return BadRequest("Failed to fetch data for given input");
            }

            if (commitData.Count == 0)
            {
                //TempData["Error"] = "Failed to fetch data for given input";

                //return RedirectToAction("Index");
                return BadRequest("Failed to fetch data for given input");

            }

            var topContributors = commitData
    .GroupBy(c => c.author_name)
    .Select(g => (Author: g.Key, Count: g.Count()))
    .OrderByDescending(x => x.Count)
    .Take(5)
    .ToList();

            var commitsPerDay = commitData
    .GroupBy(c => c.commit_date.Date)
    .ToDictionary(g => g.Key.ToString("yyyy-MM-dd"), g => g.Count());

            var commitsPerHour = commitData
                .GroupBy(c => c.commit_date.Hour)
                .OrderBy(g => g.Key)
                .ToDictionary(g => g.Key, g => g.Count());

 
          

            var weekdayCommits = commitData.Count(c => c.commit_date.DayOfWeek >= DayOfWeek.Monday &&
                                     c.commit_date.DayOfWeek <= DayOfWeek.Friday);

            var weekdayCommitsAvg = weekdayCommits / 5;

            var weekendCommits = commitData.Count(c => c.commit_date.DayOfWeek == DayOfWeek.Saturday ||
                                                       c.commit_date.DayOfWeek == DayOfWeek.Sunday);

            var weekendCommitsAvg = weekendCommits / 2;

            var duringHours = commitData.Count(c =>
                c.commit_date.Hour >= 9 &&
                c.commit_date.Hour <= 18);

            var afterHours = commitData.Count(c =>
                c.commit_date.Hour < 9 ||
                c.commit_date.Hour > 18);

            var dailyCounts = commitData
    .GroupBy(c => c.commit_date.Date)
    .Select(g => g.Count())
    .ToList();
            var avg = dailyCounts.Average();
            var max = dailyCounts.Max();
            //High max + low avg → “burst coder”
            //Stable values → “consistent contributor”

            var intensity = commitData
    .GroupBy(c => c.commit_date.Date)
    .Select(g => new {
        Date = g.Key,
        Level = g.Count() <= 2 ? "Low"
              : g.Count() <= 6 ? "Medium"
              : "High"
    });

            var viewModel = new DashboardViewModel
            {
                repo = repo,
                TopContributors = topContributors,
                TotalCommits = commitData.Count,
                CommitsPerDay = commitsPerDay,
                CommitsPerHour = commitsPerHour,
                WeekdayCommits = weekdayCommits,
                WeekendCommits = weekendCommits,
                WeekdayCommitsAvg = weekdayCommitsAvg,
                WeekendCommitsAvg = weekendCommitsAvg,
                DayCommits = duringHours,
                NightCommits = afterHours
                //RecentCommits = commitData.Take(10).ToList()
            };

            return Ok(viewModel);

        }

        [HttpGet]
        public async Task<IActionResult> GetInsights(List<CommitModel> commitData)
        {
            var weekdayCommits = commitData.Count(c => c.commit_date.DayOfWeek >= DayOfWeek.Monday &&
                                           c.commit_date.DayOfWeek <= DayOfWeek.Friday);

            var weekendCommits = commitData.Count(c => c.commit_date.DayOfWeek == DayOfWeek.Saturday ||
                                                       c.commit_date.DayOfWeek == DayOfWeek.Sunday);

            var afterHours = commitData.Count(c => c.commit_date.Hour >= 18);
            var dailyCounts = commitData
    .GroupBy(c => c.commit_date.Date)
    .Select(g => g.Count())
    .ToList();
            var avg = dailyCounts.Average();
            var max = dailyCounts.Max();
            //High max + low avg → “burst coder”
            //Stable values → “consistent contributor”

            var intensity = commitData
    .GroupBy(c => c.commit_date.Date)
    .Select(g => new {
        Date = g.Key,
        Level = g.Count() <= 2 ? "Low"
              : g.Count() <= 6 ? "Medium"
              : "High"
    });

            return Ok(new { weekdayCommits, weekendCommits, afterHours });
        }


            // HTML DATA OF DASHBOARD
            [HttpGet]
        public async Task<IActionResult> IndexHTMLDATAOFDASHBOARD()
        {
            using var client = new HttpClient();

            // GitHub API requires a User-Agent
            client.DefaultRequestHeaders.UserAgent.Add(
                new ProductInfoHeaderValue("CSharpApp", "1.0"));

            //var url = "https://api.github.com/repos/dotnet/runtime/commits";
            var url = "https://api.github.com/repos/dotnet/runtime/commits?per_page=100";
            var response = await client.GetStringAsync(url);
            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };

            var commits = JsonSerializer.Deserialize<List<CommitRoot>>(response, options);
            List<CommitModel> commitData = new List<CommitModel>();
            foreach (var c in commits)
            {
                commitData.Add(new CommitModel() { commit_message = c.commit.message, author_name = c.commit.author.name, commit_date = c.commit.author.date });
            }
            //return Ok(commitData);
            //return View(commitData);


            var commitsPerDay = commitData
    .GroupBy(c => c.commit_date.Date)
    .ToDictionary(g => g.Key.ToShortDateString(), g => g.Count());

            var commitsPerHour = commitData
                .GroupBy(c => c.commit_date.Hour)
                .ToDictionary(g => g.Key, g => g.Count());

            var viewModel = new DashboardViewModel
            {
                TotalCommits = commitData.Count,
                CommitsPerDay = commitsPerDay,
                CommitsPerHour = commitsPerHour,
                RecentCommits = commitData.Take(10).ToList()
            };

            return View(viewModel);

        }


        [HttpGet("/Home/FetchCommitsData")]
        public async Task<object> FetchCommitsData()
        {
            using var client = new HttpClient();

            // GitHub API requires a User-Agent
            client.DefaultRequestHeaders.UserAgent.Add(
                new ProductInfoHeaderValue("CSharpApp", "1.0"));


            var issuesUrl = "https://api.github.com/repos/dotnet/runtime/issues?state=all&per_page=100&page=1";
            List<IssueModel> allIssues = new List<IssueModel>();


            while (true)
            {
                var issuesResponse = await client.GetStringAsync(issuesUrl);

                var issuesPage = JsonSerializer.Deserialize<List<IssueModel>>(issuesResponse, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true,
                    DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull
                });


                allIssues.AddRange(issuesPage);

                //if (!HasNextLink(issuesResponse.hEA .Headers)) break; // check Link header for "next"
                //issuesUrl = GetNextLink(issuesResponse.Headers);      // parse out the URL for rel="next"
            }



            var url = "https://api.github.com/repos/dotnet/runtime/commits";

            var response = await client.GetStringAsync(url);
            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };

            var commits = JsonSerializer.Deserialize<List<CommitRoot>>(response, options);
            List<CommitModel> commitData = new List<CommitModel>();
            var csvFile = "commits.csv";

            // Write header
            //using (var writer = new StreamWriter(csvFile))
            //{
            //    writer.WriteLine("Author,Date,Message");

            var regex = new Regex(@"#(\d+)");
            foreach (var c in commits)
            {
                //
                var cd = new CommitModel() { commit_message = c.commit.message, author_name = c.commit.author.name, commit_date = c.commit.author.date };
                cd.issueNumberList = new List<int>();
                var match = regex.Match(c.commit.message);
                if (match.Success)
                {
                    cd.issueNumberList.Add(int.Parse(match.Groups[1].Value));
                }
                commitData.Add(cd);
                //
                //        // Escape any commas in the message
                //        var message = c.commit.message.Replace(",", " ");
                //        writer.WriteLine($"{c.commit.author.name},{c.commit.author.date},{message}");
            }
            //}

            // Step 4: Compute commits per developer per day
            var csiPerDev = commitData.Where(c => c.issueNumberList?.Count > 0)
                .GroupBy(c => new { c.author_name, c.commit_date.Date })
                .Select(g => new
                {
                    Developer = g.Key.author_name,
                    Date = g.Key.Date,
                    DistinctIssues = g.SelectMany(x => x.issueNumberList).Distinct().Count(),
                    IssueNumbers = g.SelectMany(x => x.issueNumberList).Distinct().ToList() // keep list for later mapping

                }).ToList();

            //      var perIssueData = csiPerDev
            //.SelectMany(csi => csi.IssueNumbers, (csi, issueNum) => new { csi.Developer, csi.Date, csi.DistinctIssues, IssueNumber = issueNum })
            //.Join(
            //    issues,
            //    c => c.IssueNumber,
            //    i => i.number,
            //    (c, i) => new
            //    {
            //        Developer = c.Developer,
            //        Date = c.Date,
            //        CSI = c.DistinctIssues,
            //        IssueNumber = i.number,
            //        ResolutionTimeHours = i.ResolutionTimeHours
            //    }
            //)
            //.Where(x => x.ResolutionTimeHours.HasValue)
            //.ToList();


            //      return perIssueData;
        }

        static bool HasNextLink(HttpResponseHeaders headers)
        {
            if (!headers.TryGetValues("Link", out var links))
                return false;

            return links.Any(l => l.Contains("rel=\"next\""));
        }

        static string GetNextLink(HttpResponseHeaders headers)
        {
            var linkHeader = headers.GetValues("Link").First();

            var links = linkHeader.Split(',');

            foreach (var link in links)
            {
                if (link.Contains("rel=\"next\""))
                {
                    var start = link.IndexOf('<') + 1;
                    var end = link.IndexOf('>');
                    return link.Substring(start, end - start);
                }
            }

            return null;
        }


        [HttpGet]

        public async Task<string> FetchCommits()
        {
            using var client = new HttpClient();

            // GitHub API requires a User-Agent
            client.DefaultRequestHeaders.UserAgent.Add(
                new ProductInfoHeaderValue("CSharpApp", "1.0"));

            var url = "https://api.github.com/repos/dotnet/runtime/commits";

            var response = await client.GetStringAsync(url);

            Console.WriteLine(response);
            return response;
        }
    }


}
