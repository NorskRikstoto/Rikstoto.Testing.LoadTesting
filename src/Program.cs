using System.Diagnostics;

var numbers = Enumerable.Range(0, 10);

Parallel.ForEach(numbers, RunFromCmdPrompt);


static void RunFromCmdPrompt(int i) {
    Console.WriteLine($"start running #{i}");
    var process = new Process();
    process.StartInfo.FileName = "cmd.exe";
    process.StartInfo.Arguments = "/C k6 run .\\scripts\\k6-check.js"; // specify the command to execute
    process.StartInfo.UseShellExecute = false;
    process.StartInfo.RedirectStandardOutput = true;
    process.Start();

    string output = process.StandardOutput.ReadToEnd();
    process.WaitForExit();

    Console.WriteLine(output);
    Console.WriteLine($"stop running #{i}");
}