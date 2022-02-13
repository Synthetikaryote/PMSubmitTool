namespace SubmitTool
{
    using System;
    using System.IO;
    using LibGit2Sharp;

    class Program
    {
        private static readonly string repoPath = "git@github.com:Synthetikaryote/PMSubmitTool-Test.git";
        private static readonly string repoPathSSH = "ssh://git@github.com:Synthetikaryote/PMSubmitTool-Test.git";

        static void Main(string[] args)
        {
            if (args.Length < 2)
            {
                Console.WriteLine("Usage PMSubmitTool.exe filename category");
                return;
            }
        

            var fileName = args[0];
            var category = args[1];

            //Repository.Clone(repoPath, @"C:\SubmitTool\PMSubmitTool-Test", new CloneOptions { CredentialsProvider = MyCredentialsProvider });

            // Copy the file to the correct directory
            var destPath = @$"C:\SubmitTool\PMSubmitTool-Test\assets\textures\{category}";
            Directory.CreateDirectory(destPath);
            var formattedFileName = FormatFileName(fileName);
            var destFileName = Path.Combine(destPath, formattedFileName);
            if (!File.Exists(destFileName))
            {
                File.Copy(fileName, destFileName);
            }

            using (var repo = new Repository("C:/SubmitTool/PMSubmitTool-Test"))
            {
                var signature = new Signature("SubmitTool", "abc@def.com", DateTimeOffset.UtcNow);
                Commands.Stage(repo, $"{destFileName}");
                repo.Commit($"Committing {formattedFileName} under category {category}.", signature, signature);
                repo.Network.Push(repo.Network.Remotes["origin"], "refs/heads/master", new PushOptions { CredentialsProvider = MyCredentialsProvider });
            }

            string FormatFileName(string fileName) => Path.GetFileName(fileName);

            Credentials MyCredentialsProvider(string url, string usernameFromUrl, SupportedCredentialTypes types) =>
                new SshUserKeyCredentials
                { 
                    Username = "git",
                    Passphrase = string.Empty,
                    PublicKey = "SubmitTool.pub",
                    PrivateKey = "SubmitTool OpenSSH",
                };
        }
    }
}