using System;
using System.Collections.Generic;
using System.Linq;

namespace GitCompare
{
    public class Program
    {
        public static void Main(string[] args)
        {
            string folder = args[0];

            IEnumerable<string> repoFolders = GitUtility.FindGitRepoFolders(folder);

            IEnumerable<RepoInfo> repos = repoFolders
                .Select(repoFolder => RepoInfo.FromGitRepo(repoFolder, repoFolder.Replace(folder, string.Empty)))
                .ToList();

            IEnumerable<IGrouping<RepoStatusFlags, RepoInfo>> groups = repos
                .GroupBy(repo => repo.Status)
                .OrderByDescending(group => group.Key)
                .ToList();

            foreach (IGrouping<RepoStatusFlags, RepoInfo> group in groups)
            {
                Console.WriteLine(group.Key.ToStatusString());

                foreach (RepoInfo repo in group)
                {
                    Console.WriteLine($"   {repo.Name}");
                }

                Console.WriteLine();
            }

            Console.WriteLine();
            Console.Write("Press any key to exit...");
            Console.ReadKey();
        }
    }
}
