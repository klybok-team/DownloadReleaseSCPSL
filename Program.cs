using System.Threading.Tasks;
using System;
using System.IO;
using System.IO.Compression;
using System.Net;
using System.Formats.Tar;
using System.Text.RegularExpressions;
using System.Text.Encodings.Web;
using System.Web;
public class Program
{
    public static async Task Main(string[] args)
    {
        Console.WriteLine("Привет.\nПрограмма удалит любые папки, которые находятся в её директории.\nСохраняет временные файлы в temp папку.\nПрограмма создаст файл, в который вы сможете вставлять свои ссылки на файлы.\nОбрабатывает только .tar.gz | .zip файлы.\nФайл кушает все ссылки через \\n\n");

        string tempPath = Path.GetTempPath();

        tempPath = Path.Combine(tempPath + $"DownloadRelease");

        if (File.Exists(tempPath)) File.Delete(tempPath);
        Directory.CreateDirectory(tempPath);

        string path = Directory.GetCurrentDirectory();

        string PathToDownload = Path.Combine(path, "download.txt");

        var toDownload = new string[] { "https://misaka-zerotwo.github.io/SL-References/Dev.zip",
            "https://github.com/Exiled-team/Exiled/releases/latest/download/Exiled.tar.gz" };

        if (!File.Exists(PathToDownload))
        {
            File.WriteAllText(PathToDownload, "https://misaka-zerotwo.github.io/SL-References/Dev.zip\nhttps://github.com/Exiled-team/Exiled/releases/latest/download/Exiled.tar.gz");
        }
        else toDownload = File.ReadAllText(PathToDownload).Split("\n");

        foreach(var file in Directory.GetFiles(path))
        {
            if (file == Environment.ProcessPath) continue;

            if (file.EndsWith("download.txt")) continue;

            if (file.EndsWith("DownloadRelease.dll")) continue;

            File.Delete(file);
        }

        foreach (var dir in Directory.GetDirectories(path))
        {
            Directory.Delete(dir, true);
        }

        try
        {
#pragma warning disable
            using (WebClient client = new WebClient())
            {
                foreach(string url in toDownload)
                {
                    var ssl = Regex.Match(url, @"[^\/]+$").ToString();

                    Console.WriteLine($"Скачиваем {ssl} из {url}..\n");

                    client.DownloadFile(url, Path.Combine(tempPath, ssl));
                }
            }
#pragma warning enable

            foreach(var fff in Directory.GetFiles(tempPath))
            {
                if(fff.EndsWith(".zip"))
                {
                    Console.WriteLine($"Распаковываем {fff} как .zip..\n");

                    ZipFile.ExtractToDirectory(fff, path);
                }

                if(fff.EndsWith(".tar.gz"))
                {
                    Console.WriteLine($"Распаковываем {fff} как .tar.gz..\n");

                    using FileStream compressedFileStream = File.Open(fff, FileMode.Open);
                    using var decompressor = new GZipStream(compressedFileStream, CompressionMode.Decompress);

                    TarFile.ExtractToDirectory(decompressor, path, true);
                }
            }

            foreach (var directory in Directory.GetDirectories(path))
            {
                if (directory == path) continue;

                Process(directory, path);
            }
        } catch (Exception ex)
        {
            Console.WriteLine(ex);
        }

        foreach(var directory in Directory.GetDirectories(path))
        {
            Directory.Delete(directory, true);
        }

        Console.WriteLine("Воу, пацан готов.");
        Console.WriteLine($"{DateTime.Now} - {path} (файлы куда производилась распаковка)");

        Console.ReadLine();
    }
    public static void Process(string filepath, string pathto)
    {
        if (File.Exists(filepath))
            ProcessFile(filepath, pathto);
        else ProcessDirectory(filepath, pathto);
    }
    public static void ProcessDirectory(string filepath, string pathto)
    {
        foreach (string fileName in Directory.GetFiles(filepath))
            ProcessFile(fileName, pathto);

        foreach (string subdirectory in Directory.GetDirectories(filepath))
            ProcessDirectory(subdirectory, pathto);
    }
    public static void ProcessFile(string filepath, string pathto)
    {
        File.Copy(filepath, Path.Combine(pathto, Path.GetFileName(filepath)), true);
    }
}