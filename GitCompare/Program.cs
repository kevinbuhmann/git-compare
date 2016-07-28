﻿using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace GitCompare
{
    public class Program
    {
        public static void Main(string[] args)
        {
            if (args.Length != 1)
            {
                Console.WriteLine("ERROR: Must specify exactly one argument with is the folder in which to find git repos.");
                return;
            }

            string folder = args[0];
            folder = folder.EndsWith(@"\") ? folder : $@"{folder}\";

            if (!Directory.Exists(folder))
            {
                Console.WriteLine($"ERROR: Specified folder {folder} does not exist.");
                return;
            }

            Console.WriteLine($"Finding repos in {folder}...");
            IEnumerable<string> repoFolders = GitUtility.FindGitRepoFolders(folder);

            string progress = $"(0/{repoFolders.Count()})";
            Console.Write($"Comparing repos... {progress}");
            ConcurrentBag<RepoInfo> repos = new ConcurrentBag<RepoInfo>();
            Parallel.ForEach(repoFolders, repoFolder =>
            {
                RepoInfo repo = RepoInfo.FromGitRepo(repoFolder, repoFolder.Replace(folder, string.Empty));
                repos.Add(repo);

                Console.Write(new string('\b', progress.Length));
                progress = $"({repos.Count}/{repoFolders.Count()})";
                Console.Write(progress);
            });
            Console.WriteLine();

            Console.WriteLine("Sorting...");
            IEnumerable<IGrouping<RepoStatusFlags, RepoInfo>> groups = repos
                .GroupBy(repo => repo.Status)
                .OrderByDescending(group => group.Key)
                .ToList();
            Console.WriteLine();

            foreach (IGrouping<RepoStatusFlags, RepoInfo> group in groups)
            {
                Console.WriteLine(group.Key.ToStatusString());

                foreach (RepoInfo repo in group.OrderBy(r => r.Name))
                {
                    Console.WriteLine($"   {repo.Name}");
                }

                Console.WriteLine();
            }
        }
    }
}
