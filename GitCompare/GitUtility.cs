using System;
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
                string[] subFolders = new string[] { };
                try
                {
                    subFolders = Directory.GetDirectories(folder);
                }
                catch
                {
                }

                if (subFolders.Any())
                {
                    repoFolders.AddRange(subFolders.SelectMany(subFolder => FindGitRepoFolders(subFolder)));
                }
            }

            return repoFolders.ToArray();
        }

        public static string GetCurrentBranch(string repoFolder)
        {
            return ExecuteGitCommand(repoFolder, "branch")
                .Split(new string[] { "\r", "\n" }, StringSplitOptions.RemoveEmptyEntries)
                .Where(line => line.StartsWith("*"))
                .First()
                .RemoveLeadingStar()
                .Trim();
        }

        public static RepoStatus GetRepoStatus(string repoFolder)
        {
            string status = ExecuteGitCommand(repoFolder, "status");
            bool hasUncommittedChanges = status.Contains("nothing to commit") == false;

            ExecuteGitCommand(repoFolder, "fetch");
            string branch = GetCurrentBranch(repoFolder);

            string incomingLog = ExecuteGitCommand(repoFolder, $"log ..origin/{branch}").Trim();
            bool hasIncomingChanges = string.IsNullOrEmpty(incomingLog) == false;

            string outgoingLog = ExecuteGitCommand(repoFolder, $"log origin/{branch}..").Trim();
            bool hasOutgoingChanges = string.IsNullOrEmpty(outgoingLog) == false;

            RepoStatus repoStatus = RepoStatus.CleanAndUpToDate;

            if (hasUncommittedChanges)
            {
                repoStatus |= RepoStatus.UncommittedChanges;
            }

            if (hasIncomingChanges)
            {
                repoStatus |= RepoStatus.IncomingChanges;
            }

            if (hasOutgoingChanges)
            {
                repoStatus |= RepoStatus.OutgoingChanges;
            }

            return repoStatus;
        }

        private static bool IsGitRepo(string folder)
        {
            return Directory.Exists(Path.Combine(folder, ".git"));
        }

        private static string ExecuteGitCommand(string repoFolder, string arguments)
        {
            string output = null;

            using (Process gitProcess = new Process())
            {
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
                output = gitProcess.StandardOutput.ReadToEnd();
                gitProcess.WaitForExit();
            }

            return output;
        }
    }
}
