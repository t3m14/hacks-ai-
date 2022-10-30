using System;

using Excel = Microsoft.Office.Interop.Excel;


namespace WindowsFormsApplication1
{
    public class Prop
    {

        static string path;
        public static string Path
        {
            get
            {
                return path;
            }
            set
            {
                path = value;
            }
        }

        static string fileName = "Лист Microsoft Excel.xlsx";
        public static string FileName
        {
            get
            {
                return fileName;
            }
            set
            {
                fileName = value;
            }
        }

        public static int X { get; set; }
        public static int Y { get; set; }

    }
            
}
