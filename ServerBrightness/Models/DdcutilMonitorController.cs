using System.Diagnostics;
using System.Text.RegularExpressions;

namespace ServerBrightnnes.Models;

public class DdcutilMonitorController : IMonitorController
{
    public void SetBrightness(uint brightness)
    {
        RunCliCommand(10,"setvcp", brightness);
    }

    public void SetContrast(uint contrast)
    {
        RunCliCommand(12,"setvcp", contrast);

    }

    public int GetBrightness()
    {
        var p = RunCliCommand(10,"getvcp", 10);
        var output = p.StandardOutput.ReadToEnd();


        // foreach (Match m in Regex.Match(output.Split(':')[1].Trim(), "\\d+"))
        // {
        //     Console.WriteLine("match: "+m.Value+"  "+m.Index);
        //
        // }
        var m = Regex.Match(output.Split(':')[1].Trim(), "\\d+");
        Console.WriteLine("match: "+m.Value+"  "+m.Index);
            
        var brigth = int.Parse(m.Value);
        
        Console.WriteLine("cyurrrent bright: "+brigth);
        
        return brigth;
        
         
    }

    public int GetContrast()
    {
        var p = RunCliCommand(10,"getvcp", 12);
        var output = p.StandardOutput.ReadToEnd();


        // foreach (Match m in Regex.Match(output.Split(':')[1].Trim(), "\\d+"))
        // {
        //     Console.WriteLine("match: "+m.Value+"  "+m.Index);
        //
        // }
        var m = Regex.Match(output.Split(':')[1].Trim(), "\\d+");
        Console.WriteLine("match: "+m.Value+"  "+m.Index);
            
        var contrast = int.Parse(m.Value);
        
        Console.WriteLine("cyurrrent contrast: "+contrast);
        
        return contrast;
    }
    
    private Process RunCliCommand(int op,string mode,uint args, string file="ddcutil",bool waitForExit = true)
    {
       
            
            
            
            
        var psi = new ProcessStartInfo();
        psi.FileName = "/usr/bin/bash";
        psi.Arguments = $"-c \"{file} -d 1 {mode} {op} {args}\"";
        psi.UseShellExecute = false;
        psi.CreateNoWindow = true;
        psi.RedirectStandardOutput = true;

        Console.WriteLine("Executing: "+psi.FileName+" "+psi.Arguments);
            
            
        var process = Process.Start(psi);
            
            
        if (waitForExit)
            process.WaitForExit();

        Console.WriteLine(process.Id);
        //var output = process.StandardOutput.ReadToEnd();
            
        return process;
    }
}