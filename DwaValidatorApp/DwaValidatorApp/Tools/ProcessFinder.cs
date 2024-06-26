using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace DwaValidatorApp.Tools
{
    public static class ProcessFinder
    {
        private static readonly Regex newlineRegex = new Regex(Environment.NewLine, RegexOptions.Compiled);

        public static IEnumerable<int> GetPids(int port)
        {
            ProcessStartInfo pStartInfo = new ProcessStartInfo
            {
                FileName = "netstat.exe",
                Arguments = "-a -n -o",
                CreateNoWindow = true,
                UseShellExecute = false,
                RedirectStandardInput = true,
                RedirectStandardOutput = true,
                RedirectStandardError = true
            };

            using Process process = new Process();
            process.StartInfo = pStartInfo;
            process.Start();
            StreamReader soStream = process.StandardOutput;
            string output = soStream.ReadToEnd();

            if(process.ExitCode != 0)
                return default;

            var prcs = from line in newlineRegex.Split(output)
                       where !line.Trim().StartsWith("Proto")
                       select line.Split(' ', StringSplitOptions.RemoveEmptyEntries)
                       into parts
                       let len = parts.Length
                       where len > 2
                       select new
                       {
                           Protocol = parts[0],
                           Port = int.Parse(parts[1].Split(':').Last()),
                           PID = int.Parse(parts[len - 1])
                       };

            return prcs.Where(x => x.Port == port)
                       .Select(x => x.PID)
                       .Distinct()
                       .ToList();
        }
    }
}
