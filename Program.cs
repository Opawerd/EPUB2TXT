using System.Xml;
namespace EPUB2TXT;

public static class EPUBConverter
{
    private static String? fileName { get; set; }
    private static String savedPath { get; set; } = "./"; 
    private static bool FileExist()
    {
        if(fileName?.Substring(fileName.Length - 5) != ".epub") return false;
        return File.Exists(fileName); 
    }
    /**
    / 1. Unzip the EPUB File
    / 2. Read volume.opf file
    / 3. Read the file names from pakage/spine; each itemref's idref
    / 4. Get into each files and only read body of each elements.
    / 5. Join the whole text into one string and write it into txt file.
    / 6, PROFIT
    **/
    public static String Convert()
    {
        if(!EPUBConverter.FileExist()) return "You should enter proper EPUB file path.";
        XmlTextReader fileList = new XmlTextReader(fileName);
        return "Done!";
    }
}
