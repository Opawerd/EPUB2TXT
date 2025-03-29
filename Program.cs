using System.IO.Compression;
using System.IO.Enumeration;
using System.Xml;
using System.Xml.Linq;
namespace EPUB2TXT;


public static class EPUBConverter
{
    public static String? FileName { get; set; }
    private static String SavedPath { get; set; } = "./"; 
    private static bool FileExist()
    {
        if(FileName?.Substring(FileName.Length - 5) != ".epub") return false;
        return File.Exists(FileName); 
    }
    private static void Unzip()
    {   
        ZipFile.ExtractToDirectory(FileName, "./temp");
    }

    private static void Clear()
    {
        File.Delete("./temp");
    }

    public static void FuncTest()
    {
        EPUBConverter.Unzip();
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

        if(!EPUBConverter.FileExist()) return "You should enter proper EPUB file path.";

        // Unzip files into some rando place (or in temp folder)
        EPUBConverter.Unzip();

        XElement xmlReader = XElement.Load("/home/tgco7874/book/volume.opf");
        XElement curText;
        xmlReader = xmlReader.Element("{http://www.idpf.org/2007/opf}spine");
        foreach(var names in xmlReader.Elements())
        {
            curPath = "/home/tgco7874/book/Text" + names.Attribute("idref")?.Value;
            curText = XElement.Load(curPath);

            foreach(var lines in curText.Elements())
            {
                foreach(var line in lines.Elements())
                {
                    Console.WriteLine(line.Value);
                }
            }
        }        

        // Remove temporary unziped files
        EPUBConverter.Clear();
        return "Done!";
    }
}

class ActuallProgram
{
    static void Main(string[] args)
    {
        //Set file name and Saved Path
        EPUBConverter.FileName = "Book.epub";
        EPUBConverter.FuncTest();
        //Console.WriteLine(EPUBConverter.Convert());
    }
}