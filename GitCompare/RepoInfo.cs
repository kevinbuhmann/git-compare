namespace GitCompare
{
    public class RepoInfo
    {
        private RepoInfo(string repoFolder, string name, string branch, RepoStatus status)
        {
            this.RepoFolder = repoFolder;
            this.Name = name;
            this.Branch = branch;
            this.Status = status;
        }

        public string RepoFolder { get; }

        public string Name { get; }

        public string Branch { get; }

        public RepoStatus Status { get; }

        public static RepoInfo FromGitRepo(string repoFolder, string name)
        {
            string branch = GitUtility.GetCurrentBranch(repoFolder);
            RepoStatus status = GitUtility.GetRepoStatus(repoFolder);

            return new RepoInfo(repoFolder, name, branch, status);
        }
    }
}
