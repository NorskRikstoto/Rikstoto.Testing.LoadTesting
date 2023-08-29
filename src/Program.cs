using System.Diagnostics;

var numbers = Enumerable.Range(8000001, 300);

Parallel.ForEach(numbers,
                    new ParallelOptions
                    {
                        MaxDegreeOfParallelism = Convert.ToInt32(Math.Ceiling((Environment.ProcessorCount * 0.75) * 8.0))
                    },
                    RunFromCmdPrompt);


static void RunFromCmdPrompt(int i) {
    Console.WriteLine($"customerId: {i}");
    var process = new Process();
    var portNumber = 3000;
    //var customerId = 8000001;
    process.StartInfo.FileName = "cmd.exe";
    //process.StartInfo.Arguments = $"/C k6 run --address \"localhost:{portNumber + i}\" .\\tests\\k6-check.js"; // specify the command to execute
    process.StartInfo.Arguments = $"/C k6 run -e customerId={i} -e env=test2 --address \"localhost:{portNumber + i - 8000000}\" .\\tests\\k6-test.js";
    process.StartInfo.UseShellExecute = false;
    //process.StartInfo.RedirectStandardOutput = true;
    process.Start();

    //string output = process.StandardOutput.ReadToEnd();
    //process.WaitForExit();

    //Console.WriteLine(output);
    //Console.WriteLine($"stop running #{i}");
}  