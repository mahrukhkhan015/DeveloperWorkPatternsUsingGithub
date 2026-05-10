using WebApplication1.Models;

namespace WebApplication1.Models
{
    public class DashboardViewModel
    {
        public string repo { get; set; }
        public List<(string Author, int Count)> TopContributors { get; set; }
        public int TotalCommits { get; set; }
        public Dictionary<string, int> CommitsPerDay { get; set; }
        public Dictionary<int, int> CommitsPerHour { get; set; }
        public List<CommitModel> RecentCommits { get; set; }

        public int WeekdayCommits { get; set; } 
        public int WeekendCommits { get; set; } 
        public int WeekdayCommitsAvg { get; set; } 
        public int WeekendCommitsAvg { get; set; } 
        public int DayCommits { get; set; } 
        public int NightCommits { get; set; } 
    }
}
