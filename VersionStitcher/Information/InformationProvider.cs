namespace VersionStitcher.Information
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using NGit;
    using NGit.Api;
    using NGit.Errors;
    using NGit.Storage.File;
    using Sharpen;

    public class InformationProvider
    {
        public object GetInformation(string projectPath, string solutionPath)
        {
            return GetGitInformation(projectPath, solutionPath) ?? GetBuildInformation();
        }

        private static void FillBuildInformation(BuildInformation buildInformation)
        {
            buildInformation.BuildTime = DateTime.Now;
            buildInformation.BuildTimeUTC = buildInformation.BuildTime.ToUniversalTime();
        }

        private static BuildInformation GetBuildInformation()
        {
            var buildInformation = new BuildInformation();
            FillBuildInformation(buildInformation);
            return buildInformation;
        }

        private GitInformation GetGitInformation(string projectPath, string solutionPath)
        {
            var repository = FindGitRepository(projectPath, solutionPath);
            if (repository == null)
                return null;

            var gitInformation = new GitInformation();
            var branches = repository.BranchList().Call();

            //gitInformation.BranchName = currentBranch.;
            //gitInformation.BranchRemoteName = currentBranch.Remote?.Name;
            //var latestCommit = currentBranch.Commits.First();
            //gitInformation.CommitID = latestCommit.Id.Sha;
            //gitInformation.CommitShortID = latestCommit.Id.Sha.Substring(0, 8);
            //gitInformation.CommitMessage = latestCommit.Message.Trim();
            //gitInformation.CommitAuthor = latestCommit.Author.ToString();
            var repositoryStatus = repository.Status().Call();
            gitInformation.IsDirty = !repositoryStatus.IsClean();
            gitInformation.IsDirtyLiteral = !repositoryStatus.IsClean() ? "dirty" : "";
            FillBuildInformation(gitInformation);
            return gitInformation;
        }

        private static Git FindGitRepository(string projectPath, string solutionPath)
        {
            foreach (var path in GetPaths(projectPath, solutionPath))
            {
                try
                {
                    return  Git.Open(path);
                }
                catch (RepositoryNotFoundException) { }
                catch (FileNotFoundException) { }
                catch (IOException) { }
            }
            return null;
        }

        private static IEnumerable<string> GetPaths(string projectPath, string solutionPath)
        {
            var projectDir = GetFullPath(Path.GetDirectoryName(projectPath));
            yield return projectDir;
            if (solutionPath != null)
            {
                var solutionDir = GetFullPath(Path.GetDirectoryName(solutionPath));
                yield return solutionDir;
            }
        }

        private static string GetFullPath(string path)
        {
            if (string.IsNullOrEmpty(path))
                path = ".";
            return path;
        }
    }
}
