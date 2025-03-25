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
        if(!EPUBConverter.FileExist()) return "You should enter proper EPUB file path.";
        XElement xmlReader = XElement.Load(FileName + "/OEBPS/volume.opf");
        xmlReader = xmlReader.Element("{http://www.idpf.org/2007/opf}spine");
        foreach(var names in xmlReader.Elements())
        {
            Console.WriteLine(names.Attribute("idref")?.Value);
        }        

        // Every text files are in OEBPS/Text folder
        Console.WriteLine(xmlReader);
        return "Done!";
    }
}

class ActuallProgram
{
    static void Main(string[] args)
    {
        
    }
}