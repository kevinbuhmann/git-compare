using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace GitCompare
{
    public static class Program
    {
        public static void Main(string[] args)
        {
            string folder = GetFolder(args);

            if (folder != null)
            {
                IEnumerable<string> repoFolders = FindRepos(folder);

                DateTime start = DateTime.Now;

                IEnumerable<RepoInfo> repos = CompareReps(folder, repoFolders);

                DateTime end = DateTime.Now;
                Console.WriteLine($"{(end - start).TotalSeconds} seconds");

                IEnumerable<IGrouping<RepoStatusFlags, RepoInfo>> groups = SortRepos(repos);
                OutputRepos(groups);
            }
        }

        private static string GetFolder(string[] args)
        {
            string folder = null;

            if (args.Length == 1)
            {
                folder = args[0];
                folder = folder.EndsWith(@"\") ? folder : $@"{folder}\";

                if (!Directory.Exists(folder))
                {
                    folder = null;
                    Console.WriteLine($"ERROR: Specified folder {folder} does not exist.");
                }
            }
            else
            {
                Console.WriteLine("ERROR: Must specify exactly one argument with is the folder in which to find git repos.");
            }

            return folder;
        }

        private static IEnumerable<string> FindRepos(string folder)
        {
            Console.WriteLine($"Finding repos in {folder}...");
            IEnumerable<string> repoFolders = GitUtility.FindGitRepoFolders(folder);
            return repoFolders;
        }

        private static IEnumerable<RepoInfo> CompareReps(string folder, IEnumerable<string> repoFolders)
        {
            string progress = $"(0/{repoFolders.Count()})";
            Console.Write($"Comparing repos... {progress}");
            List<RepoInfo> repos = new List<RepoInfo>();
            Parallel.ForEach(repoFolders, repoFolder =>
            {
                RepoInfo repo = RepoInfo.FromGitRepo(repoFolder, repoFolder.Replace(folder, string.Empty));

                lock (repos)
                {
                    repos.Add(repo);

                    Console.Write(new string('\b', progress.Length));
                    progress = $"({repos.Count}/{repoFolders.Count()})";
                    Console.Write(progress);
                }
            });
            Console.WriteLine();
            return repos;
        }

        private static IEnumerable<IGrouping<RepoStatusFlags, RepoInfo>> SortRepos(IEnumerable<RepoInfo> repos)
        {
            Console.WriteLine("Sorting...");
            IEnumerable<IGrouping<RepoStatusFlags, RepoInfo>> groups = repos
                .OrderBy(repo => repo.Name)
                .GroupBy(repo => repo.Status)
                .OrderByDescending(group => group.Key)
                .ToList();
            Console.WriteLine();
            return groups;
        }

        private static void OutputRepos(IEnumerable<IGrouping<RepoStatusFlags, RepoInfo>> groups)
        {
            foreach (IGrouping<RepoStatusFlags, RepoInfo> group in groups)
            {
                Console.WriteLine(group.Key.ToStatusString());

                foreach (RepoInfo repo in group)
                {
                    Console.WriteLine($"   {repo.Name}");
                }

                Console.WriteLine();
            }
        }
    }
}
