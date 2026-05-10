namespace WebApplication1.Models
{
    public class CommitModel
    {
        public List<int> issueNumberList { get; set; }
        public string commit_message { get; set; }
        public string author_name { get; set; }
        public DateTime commit_date { get; set; }
    }


    public class IssueModel
    {
        public int number { get; set; } // #123
        public string title { get; set; }
        public string state { get; set; } // open/closed
        public IssueUser assignee { get; set; }
        public DateTime? created_at { get; set; }
        public DateTime? closed_at { get; set; }
        public double? ResolutionTimeHours => closed_at.HasValue && created_at.HasValue
       ? (closed_at.Value - created_at.Value).TotalHours
       : (double?)null;
    }

    public class IssueUser
    {
        public string login { get; set; }
    }
    public class CommitRoot
    {
        public Commit commit { get; set; }
    }

    public class Commit
    {
        public CommitAuthor author { get; set; }
        public string message { get; set; }
    }

    public class CommitAuthor
    {
        public string name { get; set; }
        public DateTime date { get; set; }
    }

}
