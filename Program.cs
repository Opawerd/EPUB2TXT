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
        ZipFile.ExtractToDirectory(FileName, "./temp/" + FileName.Substring(0, (FileName.Length - 5)));
    }

    private static void Clear()
    {
        // Due to the permisson problem, this thing could not delete any files! WTF.
        // Anyway, I'll left garbage behind, good luck with does things. (or just manually delete unziped files! it's not that hard.)
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
        string title;
        string body = "";
        
        if(!EPUBConverter.FileExist()) return "You should enter proper EPUB file path.";
        title = FileName.Substring(0, FileName.Length - 5);
        // Unzip files into some rando place (or in temp folder)
        EPUBConverter.Unzip();

        XElement xmlReader = XElement.Load("./temp/" + title + "/OEBPS/volume.opf");
        XElement curText;
        xmlReader = xmlReader.Element("{http://www.idpf.org/2007/opf}spine");

        foreach(var names in xmlReader.Elements())
        {
            curPath = "./temp/" + title + "/OEBPS/Text/" + names.Attribute("idref")?.Value;
            curText = XElement.Load(curPath);
            using(StreamWriter writer = File.CreateText("./completed/"+ "book.txt"))
            {
                foreach(var lines in curText.Elements())
                {
                    foreach(var line in lines.Elements())
                    {
                        body = body + line.Value + "\n";
                    }
                }
                writer.Write(body);
            }
            
        }        

        return "Done!";
    }
}

class ActuallProgram
{
    static void Main(string[] args)
    {
        //Set file name and Saved Path
        EPUBConverter.FileName = "./books/Book.epub";
        Console.WriteLine(EPUBConverter.Convert());
        //Console.WriteLine(EPUBConverter.Convert());
    }
}