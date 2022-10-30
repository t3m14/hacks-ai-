using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using System.Diagnostics;
using Excel = Microsoft.Office.Interop.Excel;

namespace WindowsFormsApplication1
{

    public partial class Form1 : Form
    {

        List<char> ABC;
        //счетчик и "сбрасыватель" счетчика
        int abcIt = 0;
        int abc = 26; //26 букв в англ. алфавите ;)

        public Form1()
        {
            InitializeComponent();
            label5.Text = "";
        }


        private void Form1_Load(object sender, EventArgs e)
        {

            ABC = new List<char>();

            for (int i = 65; i < 91; i++) //ABCDEF... англ. алфавит
            {
                ABC.Add((char)i);
            }

        }


        private void button1_Click(object sender, EventArgs e)  //Загружаем данные в dataGridView с Excel файла и отрисовываем dataGridView
        {

            bool isSelected = false;
            openFileDialog1.InitialDirectory = "C:";
            openFileDialog1.Title = "Выбрать файл";
            openFileDialog1.FileName = "";
            openFileDialog1.Filter = "Файлы xlsx|*.xlsx";

            if (openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                Prop.Path = openFileDialog1.FileName;
                isSelected = true;
                Prop.FileName = openFileDialog1.SafeFileName;
            }
            else
            {
                MessageBox.Show("Файл не выбран");
            }


            if (isSelected)  //Файл выбран, начинаем загрузку в dataGridView
            {

                this.Cursor = Cursors.AppStarting; //Меняем вид корсора пока происходит загрузка из файла на dataGridView.
                dataGridView2.RowHeadersWidth = 60; //Ширина столбца, заголовков строк
                abcIt = 0;
                abc = 26;


                Excel.Application xlApp;
                Excel.Workbook xlWorkBook;
                Excel.Worksheet xlWorkSheet;
                

                object misValue = System.Reflection.Missing.Value;

                xlApp = new Excel.Application();


                var workbooks = xlApp.Workbooks;

                xlWorkBook = workbooks.Open(Prop.Path, 0, false, 5, "", "", true, Microsoft.Office.Interop.Excel.XlPlatform.xlWindows, "\t", false, false, 0, true, 1, 0);

                var worksheets = xlWorkBook.Worksheets;

                xlWorkSheet = (Excel.Worksheet)worksheets.get_Item(1);

                //Счетчики
                int NullRows = 0;
                int NullColums = 0;


                //Считаем сколько у нас заполненных строк и столбцов в excel файле. Считать будем по заголовкам. Все остальное в расчет не брал.

                //Количество строк. Подсчет ведется для создания количества строк на dataGridView.
                for (int x = 1; x < 256; x++) //То есть больше 256х256 он искать не будет. Но и лишние - пустые ячейки он считать не будет.
                {
                    if (xlWorkSheet.Cells[x,1].Value == null) //Если ячейка пустая, то наш счетчик считает до 5 и выходит из цикла. (Вспомнилась считалочка, "Я считаю до пяти, не могу до десяти")
                    {
                        NullRows++;

                        if (NullRows == 5)
                        {
                            Prop.X = x - 5;
                            break;
                        }
                    }
                    else //Если же после пустых ячеек(до 5), есть еще заполненная ячейка, то "обнуляем" наш счетчик.
                    {
                        if (NullRows >= 1)
                        {
                            NullRows = 0;
                        }
                    }
                }

                //Количество столбцов. Подсчет ведется для создания количества столбцов на dataGridView.
                for (int y = 1; y < 256; y++)
                {
                    if (xlWorkSheet.Cells[1, y].Value == null)
                    {
                        NullColums++;
                        if (NullColums == 5)
                        {
                            Prop.Y = y - 5;
                            break;
                        }
                    }
                    else
                    {
                        if (NullColums >= 1)
                        {
                            NullColums = 0;
                        }
                    }
                }


                //Рисуем dataGridView1
                dataGridView2.RowCount = Prop.X;
                dataGridView2.ColumnCount = Prop.Y;


                //названия/имена "x" и "y" малость перепутаны ;)
                for (int x = 0; x < Prop.Y; x++)
                {

                    if ((string)dataGridView2.Columns[x].HeaderCell.Value == "") //Если вместо "АА" "АВ" и тд. будет "", то...
                    {
                        //Если дойдем до "AZ", то после следующей "махинации", следующий заголовок столбца станет "BA"..."BZ","CA"..."CZ" и тд.
                        if ((x - 26) == 26)
                        {
                            abcIt++;
                            abc += 26;
                        }

                        dataGridView2.Columns[x].HeaderCell.Value = ABC[abcIt].ToString() + ABC[x - abc].ToString();  //Пишем в заголовки столбцов (AA AB AC AD.. DA DB... ZZ дальше ZZ выдаст ошибку, индекс за пределами массива. Если расчет верен, ZZ будет столбцом № 17602 в Excel файле)

                    }

                    for (int y = 0; y < Prop.X; y++)
                    {
                        if (dataGridView2.Rows[y].HeaderCell.Value == null) //проверка на null - чтобы лишний раз не записывать одно и то же число в одну и ту же ячейку
                        {
                            dataGridView2.Rows[y].HeaderCell.Value = (y + 1).ToString();  //Записываем в dataGridView, в строковые заголовки, значения от 1 до Prop.Y
                        }

                        //Записываем в dataGridView
                        dataGridView2[x, y].Value = xlWorkSheet.Cells[y + 1, x + 1].Value;
                    }
                }

                //Высвобождаем/"обнулаем" ссылки и закрываем документы... в общем делается для того, чтобы не выскакивали лишнии ошибки, поскольку процесс нужно "убить" - завершить его выполнение
                //(Завершение программы "не стандартным путем" - Вы не нажали на красный крестик. Приведет к тому, что процесс "Microsoft Excel", нужно будет закрыть через деспетчер задач, либо программно проверять, есть ли процесс "Microsoft Excel" и закрывать его, иначе работа с нашим документом вызовет ошибку)
                xlWorkBook.Close(true, misValue, misValue);
                xlApp.Quit();

                Marshal.ReleaseComObject(xlApp);
                Marshal.ReleaseComObject(xlWorkBook);
                Marshal.ReleaseComObject(worksheets);
                Marshal.ReleaseComObject(xlWorkSheet);

                this.Cursor = Cursors.Default; //Меняем вид корсора в первоисходное положение
            }   

        }


        private void button2_Click(object sender, EventArgs e)
        {
            // creating Excel Application  
            Microsoft.Office.Interop.Excel._Application app = new Microsoft.Office.Interop.Excel.Application();
            // creating new WorkBook within Excel application  
            Microsoft.Office.Interop.Excel._Workbook workbook = app.Workbooks.Add(Type.Missing);
            // creating new Excelsheet in workbook  
            Microsoft.Office.Interop.Excel._Worksheet worksheet = null;
            // see the excel sheet behind the program  
            app.Visible = true;
            // get the reference of first sheet. By default its name is Sheet1.  
            // store its reference to worksheet  
            //worksheet = workbook.Sheets["Sheet1"];
            worksheet = workbook.ActiveSheet;
            // changing the name of active sheet  
            worksheet.Name = "Exported from gridview";
            // storing header part in Excel  
            for (int i = 1; i < dataGridView2.Columns.Count + 1; i++)
            {
                worksheet.Cells[1, i] = dataGridView2.Columns[i - 1].HeaderText;
            }
            // storing Each row and column value to excel sheet  
            for (int i = 0; i < dataGridView2.Rows.Count - 1; i++)
            {
                for (int j = 0; j < dataGridView2.Columns.Count; j++)
                {
                    worksheet.Cells[i + 2, j + 1] = dataGridView2.Rows[i].Cells[j].Value.ToString();
                }
            }
            try
            {

                // save the application  
                workbook.SaveAs("c:\\output.xls", Type.Missing, Type.Missing, Type.Missing, Type.Missing, Type.Missing, Microsoft.Office.Interop.Excel.XlSaveAsAccessMode.xlExclusive, Type.Missing, Type.Missing, Type.Missing, Type.Missing);
                // Exit from the application  
                app.Quit();
            }
            catch
            {

            }
        }
        private void button3_Click(object sender, EventArgs e)
        {
            if (textBox1.Text == "" || textBox2.Text == "" || textBox3.Text == "")
            {
                MessageBox.Show("Не заполнены все поля");
            }
            else
            {
                int rowId = dataGridView2.Rows.Add();
                DataGridViewRow row = dataGridView2.Rows[rowId];

                row.Cells["ID"].Value = Convert.ToString(textBox1.Text);
                row.Cells["Color"].Value = Convert.ToString(textBox2.Text);
                row.Cells["Car"].Value = Convert.ToString(textBox3.Text);
                row.Cells["Tech1"].Value = Convert.ToString(textBox4.Text);
                row.Cells["Tech2"].Value = Convert.ToString(textBox5.Text);
                row.Cells["Tech3"].Value = Convert.ToString(textBox6.Text);
                row.Cells["Site"].Value = Convert.ToString(textBox7.Text);
                row.Cells["OpComp"].Value = Convert.ToString(textBox8.Text);

            }
        }

        private void button4_Click(object sender, EventArgs e)
        {
            int n;
            n = dataGridView2.RowCount;
            label5.Text = Convert.ToString(n - 1);
        }

        private void button6_Click(object sender, EventArgs e)
        {
            dataGridView2.Rows.Clear();
        }

        private void button5_Click(object sender, EventArgs e)
        {
            if (dataGridView2.RowCount <= 1)
                return;

            int index = dataGridView2.CurrentRow.Index;

            if (index == dataGridView2.RowCount - 1)
            {
                label5.Text = "Строка не выделена";
                return;
            }

            dataGridView2.Rows.RemoveAt(index);
        }

        private void button7_Click(object sender, EventArgs e)
        {

            ProcessStartInfo startInfo = new ProcessStartInfo("python");
            Process process = new Process();

            string directory = @"C:\Users\Егор\Desktop\WindowsFormsApplication1\WindowsFormsApplication1\";
            string script = "tutu.py";

            startInfo.WorkingDirectory = directory;
            startInfo.Arguments = script;
            startInfo.UseShellExecute = false;
            startInfo.CreateNoWindow = true;
            startInfo.RedirectStandardError = true;
            startInfo.RedirectStandardOutput = true;

            process.StartInfo = startInfo;

            process.Start();

            process.Close();






        }
    }
}
