using System.Windows;
using System.Windows.Input;
using System.Windows.Navigation;
using System.Diagnostics;
using System.Data;

namespace UZIqueue
{
    public partial class MainWindow : Window
    {
        DataTable dataTable = null;
        DBXML bXML = new DBXML();
        public MainWindow()
        {
            InitializeComponent();
            textBox.Focus();
            dataTable = bXML.LoadDataTableFromXML("userTable", typeof(RString));
            dataGrid.DataContext = dataTable;
        }

        private void Hyperlink_RequestNavigate(object sender, RequestNavigateEventArgs e)
        {
            textBox.Focus();
            Process.Start(new ProcessStartInfo(e.Uri.AbsoluteUri));
            e.Handled = true;
        }

        private void textBox_PreviewKeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                if (textBox.Text.Length == 20 && textBox.Text.Substring(0, 2) == "01")
                {
                    dataTable.Rows.Add(new RString("", textBox.Text).GetRow(dataTable));
                    bXML.SaveDataTableInXML(dataTable);
                }
                textBox.Text = "";
            }
        }

        private void button_Click(object sender, RoutedEventArgs e)
        {
            MgBox messageBox = new MgBox();
            if (messageBox.ShowDialog() == true)
            {
                dataTable.Rows.Clear();
                bXML.SaveDataTableInXML(dataTable);
            }
            textBox.Focus();
        }
    }
}
