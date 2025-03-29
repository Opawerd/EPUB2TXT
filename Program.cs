﻿using System.IO.Compression;
using System.IO.Enumeration;
using System.Xml;
using System.Xml.Linq;
namespace EPUB2TXT;


public static class EPUBConverter
{
    public static String? FileName { get; set; }
    private static String BookPath { get; set; } = "./books/"; 
    private static String TempFolder = "./temp/";
    private static String ConvertedFolder = "./completed/";
    
    public static bool InitialCheck()
    {
        bool firstCheck = !new DirectoryInfo(TempFolder).Exists;
        if(firstCheck) new DirectoryInfo(TempFolder).Create();

        firstCheck = !new DirectoryInfo(BookPath).Exists;
        if(firstCheck) new DirectoryInfo(BookPath).Create();

        firstCheck = !new DirectoryInfo(ConvertedFolder).Exists;
        if(firstCheck) new DirectoryInfo(ConvertedFolder).Create();

        return firstCheck;
    }

    public static List<String> GetFileNames()
    {
        string[] files = Directory.GetFiles(BookPath);
        List<String> output = [];

        // 파일명 출력
        foreach (var file in files)
        {
            output.Add(Path.GetFileName(file));
        }
        return output;
    }
    private static bool FileExist()
    {
        return File.Exists(BookPath + FileName) || FileName?.Substring(FileName.Length - 5) != ".epub"; 
    }
    private static void Unzip()
    {   
        ZipFile.ExtractToDirectory(BookPath + FileName, "./temp/" + FileName.Substring(0, (FileName.Length - 5)));
    }

    // private static void Clear()
    // {
    //     // Due to the permisson problem, this thing could not delete any files! WTF. I tried most of things! (Probably) StackOverflow lied to me again!
    //     // Anyway, I'll left this garbage behind, good luck with these thing. (or just manually delete unziped files! it's not that hard.)
    //     File.Delete("./temp");
    // }

    public static void FuncTest()
    {
        EPUBConverter.InitialCheck();
    }
    /**
    / 1. Unzip the EPUB File
    / 2. Read volume.opf file <- Done
    / 3. Read the file names from pakage/spine; each itemref's idref <- Done
    / 4. Get into each files and only read body of each elements.
    / 5. Join the whole text into one string and write it into txt file.
    / 6, PROFIT
    **/
    public static String Convert()
    {
        string curPath = "";
        string title;
        string body = "";
        
        if(!EPUBConverter.FileExist()) return FileName;
        title = FileName.Substring(0, FileName.Length - 5);
        // Unzip files into some rando place (or in temp folder)
        EPUBConverter.Unzip();

        XElement xmlReader = XElement.Load(TempFolder + title + "/OEBPS/volume.opf");
        XElement curText;
        xmlReader = xmlReader.Element("{http://www.idpf.org/2007/opf}spine");

        foreach(var names in xmlReader.Elements())
        {
            curPath = TempFolder + title + "/OEBPS/Text/" + names.Attribute("idref")?.Value;
            curText = XElement.Load(curPath);
            
                foreach(var lines in curText.Elements())
                {
                    foreach(var line in lines.Elements())
                    {
                        body = body + line.Value + "\n";
                    }
                }          
        }   

        using(StreamWriter writer = File.CreateText(ConvertedFolder + title + ".txt"))
        {     
            writer.Write(body);
        }

        return "Done!";
    }
}

class ActuallProgram
{
    static void Main(string[] args)
    {
        if(EPUBConverter.InitialCheck())
        {
            Console.WriteLine("Please rerun the program after place .epub files into 'books' folder!");
        }
        else
        {
            // Should create some sort of function that read everything inside BookPath, instead of manualy enter file name into array.
            List<String> bookNames = EPUBConverter.GetFileNames();
            //Set file name and Saved Path
            foreach(var titles in bookNames)
            {
                EPUBConverter.FileName = titles;
                Console.WriteLine(EPUBConverter.Convert());
            }
        }
        

    }
}