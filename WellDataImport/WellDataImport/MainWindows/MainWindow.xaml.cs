using Microsoft.Win32;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace WellDataImport
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void BtnSelectFile_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();

            openFileDialog.Title = "Выберите файл Excel для импорта";
            openFileDialog.Filter = "Файлы Excel|*.xlsx;*.xls|Все файлы|*.*";
            openFileDialog.FilterIndex = 1;
            openFileDialog.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);

            if (openFileDialog.ShowDialog() == true)
            {
                string selectedFileName = openFileDialog.FileName;
                tBFilePath.Text = selectedFileName;
            }
        }

        private void BtnDoImport_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(tBFilePath.Text))
            {
                MessageBox.Show("Введите путь к файлу!", "Импорт", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
        }
    }
}