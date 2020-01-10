﻿namespace Octokit
{
	public class GitHubCommitFile
	{
		public string Filename { get; set; }

		public int Additions { get; set; }

		public int Deletions { get; set; }

		public int Changes { get; set; }

		public string Status { get; set; }

		public string BlobUrl { get; set; }

		public string ContentsUrl { get; set; }

		public string RawUrl { get; set; }

		public string Sha { get; set; }

		public string Patch { get; set; }

		public string PreviousFileName { get; set; }
	}
}