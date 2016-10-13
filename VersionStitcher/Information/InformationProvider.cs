﻿// Version Stitcher - Injects git information at post-build step
// https://github.com/picrap/VersionStitcher
// MIT License - http://opensource.org/licenses/MIT
namespace VersionStitcher.Information
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using LibGit;
    using LibGit2Sharp;

    public class InformationProvider
    {
        /// <summary>
        /// Gets the information.
        /// </summary>
        /// <param name="projectPath">The project path.</param>
        /// <returns></returns>
        public object GetInformation(string projectPath)
        {
            return GetGitInformation(projectPath) ?? GetBuildInformation();
        }

        /// <summary>
        /// Fills the build information.
        /// </summary>
        /// <param name="buildInformation">The build information.</param>
        private static void FillBuildInformation(BuildInformation buildInformation)
        {
            buildInformation.BuildTime = DateTime.Now;
            buildInformation.BuildTimeUTC = buildInformation.BuildTime.ToUniversalTime();
        }

        /// <summary>
        /// Gets the build information.
        /// </summary>
        /// <returns></returns>
        private static BuildInformation GetBuildInformation()
        {
            var buildInformation = new BuildInformation();
            FillBuildInformation(buildInformation);
            return buildInformation;
        }

        /// <summary>
        /// Gets the git information.
        /// </summary>
        /// <param name="assemblyPath">The assembly path.</param>
        /// <returns></returns>
        private static GitInformation GetGitInformation(string assemblyPath)
        {
            var directories = GetAncestors(assemblyPath);
            using (var repository = GitRepository.TryLoad(directories.ToArray()))
            {
                if (repository == null)
                    return null;

                var gitInformation = new GitInformation();
                var currentBranch = repository.Repository.Head;
                gitInformation.BranchName = currentBranch.Name;
                gitInformation.BranchRemoteName = currentBranch.Remote?.Name;
                var latestCommit = currentBranch.Commits.First();
                gitInformation.CommitID = latestCommit.Id.Sha;
                gitInformation.CommitShortID = latestCommit.Id.Sha.Substring(0, 8);
                gitInformation.CommitMessage = latestCommit.Message.Trim();
                gitInformation.CommitAuthor = latestCommit.Author.ToString();
                gitInformation.CommitTime = latestCommit.Author.When.LocalDateTime;
                gitInformation.CommitTimeIso = gitInformation.CommitTime.ToString("o");
                var repositoryStatus = repository.Repository.RetrieveStatus();
                gitInformation.IsDirty = repositoryStatus.IsDirty;
                gitInformation.IsDirtyLiteral = repositoryStatus.IsDirty ? "dirty" : "";
                var tags = repository.Repository.Tags.Where(t => t.Target.Id == latestCommit.Id).OrderBy(t => t.Name).ToArray();
                gitInformation.CommitTags = string.Join(" ", tags.Select(t => t.Name));

                FillBuildInformation(gitInformation);
                return gitInformation;
            }
        }

        private static IEnumerable<string> GetAncestors(string file)
        {
            for (;;)
            {
                file = Path.GetDirectoryName(file);
                if (file == null)
                    break;

                yield return file;
            }
        }
    }
}
