using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VsDevDeleteTempBuildFiles
{
  class Program
  {
    public static List<string> TempFolders = new List<string> { "bin", "obj", "Publish" };
    static void Main(string[] args)
    {
      if (args.Length == 0)
      {
        Console.WriteLine("Bad parameters! See source code.");
      }

      var allFolders = new List<DirectoryInfo>();
      foreach (string rootFolder in args)
      {
        var root = new DirectoryInfo(rootFolder);
        foreach (string tmp in TempFolders)
        {
          var tmpFolders = root.GetDirectories(tmp, SearchOption.AllDirectories);
          allFolders.AddRange(tmpFolders);
        }
      }

      var models = allFolders.Select(di => new FolderModel(di)).Where(fm => fm.IsValid && fm.Size > 0).OrderBy(fm => fm.Name).ToList();
      long totalSize = 0;
      string csv = "Filename;Size;SizeMb;SizeGb";
      foreach (var fmodel in models)
      {
        totalSize += fmodel.Size;
        Console.WriteLine(fmodel.Description);
        csv += fmodel.CsvLine + Environment.NewLine;
      }

      var totalModel = new FolderModel("Sum", totalSize);

      Console.WriteLine(totalModel.Description);

      string sanatizedPath = string.Join("_", args.Select(ag => "(" + ag.Replace(":", "").Replace("\\", "_") + ")"));
      string csvFullName = "dev-clean_" + sanatizedPath + ".csv";
      File.WriteAllText(csvFullName, csv);

      Console.WriteLine("Data saved to " + csvFullName);

      Console.WriteLine("Do you want to move these files ?");
      var key = Console.ReadKey();
      if (key.KeyChar.ToString().ToUpper() == "Y")
      {
        Console.WriteLine("");
        Console.WriteLine("Enter folder to move files to:");
        string moveto = Console.ReadLine();
        var mdi = new DirectoryInfo(moveto);
        if (!mdi.Exists)
        {
          mdi.Create();
        }
        foreach (var fm in models)
        {
          string target = fm.Name;
          foreach (string root in args)
          {
            target = target.Replace(root, mdi.FullName);
          }
          var srcdi = new DirectoryInfo(fm.Name);
          try
          {
            var destdi = new DirectoryInfo(target);
            if (!destdi.Parent.Exists)
            {
              destdi.Parent.Create();
            }
            srcdi.MoveTo(target);
            Console.WriteLine("Folder '" + fm.Name + "' moved to: '" + target + "'");
          }
          catch (Exception e)
          {
            Console.WriteLine("Exception while moving folder '" + fm.Name + "' to '" + target + "'. Exception message: " + e.Message);
          }          
        }
      }
    }

    public static long GetFolderSize(DirectoryInfo folder)
    {
      return folder.EnumerateFiles("*", SearchOption.AllDirectories).Sum(fi => fi.Length);
    }

  }
  
  [Serializable]
  public class FolderModel
  {
    public static List<string> TempFolders = new List<string> { "bin", "obj", "Publish" };

    public string Name { get; set; }
    public long Size { get; set; }

    public bool IsValid { get; set; }

    public decimal SizeMb
    {
      get
      {
        return Math.Round((decimal)this.Size / 1048576m, 2);
      }
    }

    public decimal SizeGb
    {
      get
      {
        return Math.Round((decimal)this.Size / 1073741824m, 2);
      }
    }

    public string CsvLine
    {
      get
      {
        return string.Format("\"{0}\";{1};{2};{3}", this.Name, this.Size, this.SizeMb, this.SizeGb);
      }
    }

    public string Description
    {
      get
      {
        if (this.SizeGb > 0)
        {
          return string.Format("{0} - {1}b - {2}Mb - {3}Gb", this.Name, this.Size, this.SizeMb, this.SizeGb);
        }
        else
        {
          return string.Format("{0} - {1}b - {2}Mb", this.Name, this.Size, this.SizeMb);
        }
      }
    }

    public FolderModel()
    {
      this.Name = "";
      this.Size = 0;
      this.IsValid = false;
    }

    public FolderModel(string folderName, long size)
    {
      this.Name = folderName;
      this.Size = size;
      this.IsValid = true;
    }

    public FolderModel(DirectoryInfo folder)
    {
      this.Name = folder.FullName;
      this.IsValid = IsFolderValid(folder);
      if (this.IsValid)
      {
        this.Size = folder.EnumerateFiles("*", SearchOption.AllDirectories).Sum(fi => fi.Length);
      }
    }

    private bool IsFolderValid(DirectoryInfo folder)
    {
      // ako u parent folderu nema visual studio project ne znam što je to
      if (!folder.Parent.GetFiles("*.csproj").Any())
      {
        return false;
      }

      foreach (string tmp in TempFolders)
      {
        if (folder.FullName.Contains("\\" + tmp + "\\"))
        {
          return false;
        }
      }
      return true;
    }

  }
}
