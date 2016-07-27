using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;

namespace GitCompare
{
    public static class GitUtility
    {
        public static IEnumerable<string> FindGitRepoFolders(string folder)
        {
            List<string> repoFolders = new List<string>();

            if (IsGitRepo(folder))
            {
                repoFolders.Add(folder);
            }
            else
            {
                string[] subFolders = Directory.GetDirectories(folder);
                repoFolders.AddRange(subFolders.SelectMany(subFolder => FindGitRepoFolders(subFolder)));
            }

            return repoFolders.ToArray();
        }

        public static string GetCurrentBranch(string repoFolder)
        {
            return ExecuteGitCommand(repoFolder, "branch").Trim().RemoveLeadingStar().Trim();
        }

        public static RepoStatusFlags GetRepoStatus(string repoFolder)
        {
            RepoStatusFlags status = RepoStatusFlags.CleanAndUpToDate;

            if (HasUncommittedChanges(repoFolder))
            {
                status |= RepoStatusFlags.UncommittedChanges;
            }

            if (HasIncomingChanges(repoFolder))
            {
                status |= RepoStatusFlags.IncomingChanges;
            }

            if (HasOutgoingChanges(repoFolder))
            {
                status |= RepoStatusFlags.OutgoingChanges;
            }

            return status;
        }

        private static bool IsGitRepo(string folder)
        {
            return Directory.Exists(Path.Combine(folder, ".git"));
        }

        private static bool HasUncommittedChanges(string repoFolder)
        {
            string status = ExecuteGitCommand(repoFolder, "status");

            return status.Contains("nothing to commit") == false;
        }

        private static bool HasIncomingChanges(string repoFolder)
        {
            ExecuteGitCommand(repoFolder, "fetch");

            string branch = GetCurrentBranch(repoFolder);

            string log = ExecuteGitCommand(repoFolder, $"log ..origin/{branch}").Trim();
            return string.IsNullOrEmpty(log) == false;
        }

        private static bool HasOutgoingChanges(string repoFolder)
        {
            ExecuteGitCommand(repoFolder, "fetch");

            string branch = GetCurrentBranch(repoFolder);

            string log = ExecuteGitCommand(repoFolder, $"log origin/{branch}..").Trim();
            return string.IsNullOrEmpty(log) == false;
        }

        private static string ExecuteGitCommand(string repoFolder, string arguments)
        {
            Process gitProcess = new Process();
            gitProcess.StartInfo = new ProcessStartInfo()
            {
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true,

                WorkingDirectory = repoFolder,
                FileName = "git",
                Arguments = arguments
            };

            gitProcess.Start();
            string output = gitProcess.StandardOutput.ReadToEnd();
            gitProcess.WaitForExit();

            return output;
        }
    }
}
