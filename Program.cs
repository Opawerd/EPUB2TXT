﻿using System.IO.Compression;
using System.IO.Enumeration;
using System.Xml;
using System.Xml.Linq;
using System.Text;
namespace EPUB2TXT;

public static class XhtmlTextExtractor
{
    // XHTML 네임스페이스를 상수로 정의합니다.
    private static readonly XNamespace XhtmlNs = "http://www.w3.org/1999/xhtml";

    /// <summary>
    /// 주어진 XElement (예: body 요소) 내에서 텍스트를 추출합니다.
    /// <br/> 태그는 줄바꿈 문자로 변환됩니다.
    /// </summary>
    /// <param name="element">텍스트를 추출할 XElement입니다.</param>
    /// <returns>추출된 텍스트 문자열입니다.</returns>
    public static string GetTextWithLineBreaks(XElement element)
    {
        if (element == null)
        {
            return string.Empty;
        }

        StringBuilder sb = new StringBuilder();
        ExtractTextRecursive(element, sb);
        return sb.ToString();
    }

    private static void ExtractTextRecursive(XElement parentElement, StringBuilder sb)
    {
        foreach (XNode node in parentElement.Nodes())
        {
            if (node is XText textNode)
            {
                // 텍스트 노드의 값을 추가합니다.
                sb.Append(textNode.Value);
            }
            else if (node is XElement elementNode)
            {
                // 요소 노드인 경우
                if (elementNode.Name == XhtmlNs + "br")
                {
                    // <br/> 태그이면 줄바꿈 문자를 추가합니다.
                    sb.Append('\n');
                }
                else
                {
                    // 다른 요소(<p>, <span> 등)이면 재귀적으로 내부 텍스트를 추출합니다.
                    // 예를 들어 <p>안에 <br/>이 있을 수 있습니다.
                    ExtractTextRecursive(elementNode, sb);

                    // 참고: <p> 태그와 같은 블록 레벨 요소 뒤에 자동으로 줄바꿈을 추가하고 싶다면
                    // 여기에 로직을 추가할 수 있습니다. (예: if (elementNode.Name == XhtmlNs + "p") sb.Append('\n');)
                    // 하지만 현재 요청은 <br/>만 처리하는 것이므로, 이 부분은 주석 처리하거나 필요에 따라 수정합니다.
                    // XHTML 구조에 따라 <p> 다음에 항상 <br/>이 오는 경우라면 이 로직은 불필요합니다.
                }
            }
        }
    }
}


public static class EPUBConverter
{
    public static String? FileName { get; set; }
    private static String BookPath { get; set; } = "./books/";
    private static String TempFolder = "./temp/";
    private static String ConvertedFolder = "./completed/";

    public static bool InitialCheck()
    {
        bool firstCheck = !new DirectoryInfo(TempFolder).Exists;
        if (firstCheck) new DirectoryInfo(TempFolder).Create();

        firstCheck = !new DirectoryInfo(BookPath).Exists;
        if (firstCheck) new DirectoryInfo(BookPath).Create();

        firstCheck = !new DirectoryInfo(ConvertedFolder).Exists;
        if (firstCheck) new DirectoryInfo(ConvertedFolder).Create();

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

        if (!EPUBConverter.FileExist()) return FileName;
        title = FileName.Substring(0, FileName.Length - 5);
        // Unzip files into some rando place (or in temp folder)
        EPUBConverter.Unzip();

        if (File.Exists(TempFolder + title + "/OEBPS/volume.opf"))
        {
            VolumeExist(curPath, body, title);
        }
        else
        {
            NoVolume(curPath, body, title);
        }


        return "Done!";
    }

    private static void VolumeExist(String curPath, String body, String title)
    {
        XElement xmlReader = XElement.Load(TempFolder + title + "/OEBPS/volume.opf");
        XElement curText;
        xmlReader = xmlReader.Element("{http://www.idpf.org/2007/opf}spine");

        foreach (var names in xmlReader.Elements())
        {
            curPath = TempFolder + title + "/OEBPS/Text/" + names.Attribute("idref")?.Value;
            curText = XElement.Load(curPath);

            foreach (var lines in curText.Elements())
            {
                foreach (var line in lines.Elements())
                {
                    body = body + line.Value + "\n";
                }
            }
        }

        using (StreamWriter writer = File.CreateText(ConvertedFolder + title + ".txt"))
        {
            writer.Write(body);
        }
    }
    private static void NoVolume(String curPath, String body, String title)
    {
        XElement xmlreader = XElement.Load(TempFolder + title + "/EPUB/content.opf");
        XDocument curText;
        XElement bodyElement;
        xmlreader = xmlreader.Element("{http://www.idpf.org/2007/opf}manifest");

        foreach (var names in xmlreader.Elements())
        {
            if (names.Attribute("media-type")?.Value != "application/xhtml+xml") continue;
            curPath = TempFolder + title + "/EPUB/" + names.Attribute("href")?.Value;
            curText = XDocument.Load(curPath);
            bodyElement = curText.Root?.Element("{http://www.w3.org/1999/xhtml}body");

            body = body + XhtmlTextExtractor.GetTextWithLineBreaks(bodyElement) + "\n";
        }

        using (StreamWriter writer = File.CreateText(ConvertedFolder + title + ".txt"))
        {
            writer.Write(body);
        }
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