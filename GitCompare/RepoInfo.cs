namespace GitCompare
{
    public class RepoInfo
    {
        private RepoInfo(string repoFolder, string name, string branch, RepoStatusFlags status)
        {
            this.RepoFolder = repoFolder;
            this.Name = name;
            this.Branch = branch;
            this.Status = status;
        }

        public string RepoFolder { get; }

        public string Name { get; }

        public string Branch { get; }

        public RepoStatusFlags Status { get; }

        public static RepoInfo FromGitRepo(string repoFolder, string name)
        {
            string branch = GitUtility.GetCurrentBranch(repoFolder);
            RepoStatusFlags status = GitUtility.GetRepoStatus(repoFolder);

            return new RepoInfo(repoFolder, name, branch, status);
        }
    }
}
