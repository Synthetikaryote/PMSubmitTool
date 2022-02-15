namespace SubmitTool
{
    using System;
    using System.Diagnostics;
    using System.IO;
    using System.Linq;

    class Program
    {
        // Listing out all the valid characters works well in this case, as there aren't too many.
        private const string ValidCharacters = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ1234567890_ ";

        static void Main(string[] args)
        {
            if (args.Length < 2)
            {
                Console.WriteLine("Usage PMSubmitTool.exe filename category");
                return;
            }

            // Get the path and category from the arguments passed in
            string filePath = args[0];
            string fileName = Path.GetFileName(filePath);
            string category = args[1];

            // Attempt to create the category folder on the repository, in case it doesn't exist.
            string destPath = @$"C:\SubmitTool\PMSubmitTool-Test\assets\textures\{category}";
            Directory.CreateDirectory(destPath);

            // Do the file name validation
            string formattedFileName = FormatFileName(fileName);

            string destFilePath = Path.Combine(destPath, formattedFileName);

            // Find a file name that doesn't conflict with what already exists.
            // As long as the current attempt exists, try again, incrementing the number after the underscore.
            while (File.Exists(destFilePath))
            {
                destFilePath = IncrementFilePath(destFilePath);
                formattedFileName = Path.GetFileName(destFilePath);
            }

            // Copy the file to the correct directory on the locally cloned repository.
            File.Copy(filePath, destFilePath);

            // Stage the file.
            Console.WriteLine(RunGitCommand($"add {destFilePath}"));

            // Commit with a commit message.
            Console.WriteLine(RunGitCommand($"commit -m \"[Submit Tool] Committing {formattedFileName} under category {category}.\""));

            // Push the commit to the origin.
            Console.WriteLine(RunGitCommand("push origin"));
        }

        // Strip out invalid characters and PascalCase it.
        static string FormatFileName(string fileName)
        {
            string extension = Path.GetExtension(fileName);
            fileName = Path.GetFileNameWithoutExtension(fileName);

            // Strip out all characters except the valid ones.  Although they're not valid, this leaves in spaces for now.
            fileName = new string(fileName.Where(ValidCharacters.Contains).ToArray());

            // Split up the string into parts, with space being the separator.
            string[] parts = fileName.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

            // Uppercase the first letter of each word and lowercase the rest.
            string UppercaseFirstLetter(string s) => char.ToUpperInvariant(s[0]) + s.Substring(1).ToLowerInvariant();
            string[] pascalCased = parts.Select(UppercaseFirstLetter).ToArray();

            // Return the string that has been put back together.
            return $"{string.Join(string.Empty, pascalCased)}{extension}";
        }

        // Attempt to find a number after an underscore and increment it.  Otherwise, add on _1.
        static string IncrementFilePath(string filePath)
        {
            // Split the path into parts to work with the file name alone
            string directory = Path.GetDirectoryName(filePath);
            string fileName = Path.GetFileNameWithoutExtension(filePath);
            string extension = Path.GetExtension(filePath);

            // Check if there's an underscore at all
            if (fileName.Contains("_"))
            {
                // Split up by underscores and look at the last part.
                string[] parts = fileName.Split('_');
                string lastPart = parts.Last();

                // If the last part is a number, increment it and put it all back together.
                if (int.TryParse(lastPart, out int number))
                {
                    // Put it back together including the underscores it had.
                    string composed = $"{string.Join("_", parts.Take(parts.Length - 1))}_{number + 1}";
                    return $"{Path.Combine(directory, composed)}{extension}";
                }
            }

            return $"{Path.Combine(directory, fileName)}_1{extension}";
        }

        // Run git commands through cmd, but capture and return the output.
        static string RunGitCommand(string arguments)
        {
            // Doing this filled out version compared to just Process.Start prevents many windows from popping up.
            var gitInfo = new ProcessStartInfo
            {
                FileName = "git",
                Arguments = arguments,
                WorkingDirectory = @"C:\SubmitTool\PMSubmitTool-Test",
                CreateNoWindow = true,
                RedirectStandardError = true,
                RedirectStandardOutput = true,
                UseShellExecute = false
            };

            string error = null;
            string output = null;
            using (var process = new Process { StartInfo = gitInfo })
            {
                process.Start();
                error = process.StandardError.ReadToEnd();
                output = process.StandardOutput.ReadToEnd();
                process.WaitForExit();
            }

            // Return both the error and the output, separated by a newline if both exist.
            return string.Join("\n", error, output);
        }
    }
}