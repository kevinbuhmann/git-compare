using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace GitCompare
{
    public static class Program
    {
        public static string ApplicationName = "Git Compare";

        public static void Main(string[] args)
        {
            if (Console.Title == Assembly.GetEntryAssembly().Location)
            {
                Console.Title = ApplicationName;
            }

            string directory = GetDirectory(args);

            if (directory != null)
            {
                IEnumerable<string> repoFolders = FindRepos(directory);
                IEnumerable<RepoInfo> repos = CompareReps(directory, repoFolders);
                IEnumerable<IGrouping<RepoStatusFlags, RepoInfo>> groups = SortRepos(repos);
                OutputRepos(groups);
            }

            if (Console.Title == ApplicationName)
            {
                Console.Write("Press any key to exit...");
                Console.ReadKey();
            }
        }

        private static string GetDirectory(string[] args)
        {
            string directory = Directory.GetCurrentDirectory();

            if (args.Length == 1)
            {
                directory = args[0];

                if (!Directory.Exists(directory))
                {
                    Console.WriteLine($"ERROR: Specified directory '{directory}' does not exist.");
                    directory = null;
                }
            }
            else if (args.Length > 1)
            {
                Console.WriteLine("ERROR: Multiple arguments passed. Specify one argument which is the directory in which to find git repos; otherwise no arguments to use the current working directory.");
                directory = null;
            }
            
            directory = directory == null || directory.EndsWith(@"\") ? directory : $@"{directory}\";

            return directory;
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
