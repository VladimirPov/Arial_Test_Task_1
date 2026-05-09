using Microsoft.Win32;
using OfficeOpenXml;
using System.IO;
using System.Windows;
using WellDataImport.Services;

namespace WellDataImport
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private DatabaseService _databaseService;
        private ExcelImportService _importService;
        private string _currentDatabasePath = "..\\database";

        public MainWindow()
        {
            InitializeComponent();
            ExcelPackage.License.SetNonCommercialPersonal("WellDataImport");
            _importService = new ExcelImportService();
            _databaseService = new(_currentDatabasePath);
        }

        private void TestConnection()
        {
            if (_databaseService.TestConnection(out string error))
            {
                MessageBox.Show("Подключение успешно!", "Информация", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            else
            {
                MessageBox.Show($"Не удалось подключиться к базе данных. Ошибка: {error}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                Close();
            }
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
            DoImport(tBFilePath.Text);
        }

        private void DoImport(string path)
        {
            if (_databaseService == null)
            {
                return;
            }
            if (!Path.Exists(path))
            {
                MessageBox.Show($"Указан не верный путь к файлу", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            try
            {
                var importedHoles = _importService.ImportHolesFromExcel(path);
                int insertedHoles = 0;
                
                if (importedHoles.Any())
                {
                    insertedHoles = _databaseService.InsertHoles(importedHoles);
                }
                MessageBox.Show($"Импорт скважин завершен!\nДобавлено скважин: {insertedHoles} \nПропущено дубликатов: {importedHoles.Count - insertedHoles}",
                    "Результат", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при импорте скважин:\n{ex.Message}", "Ошибка",
                              MessageBoxButton.OK, MessageBoxImage.Error);
            }
            try
            {
                int insertedAssay = 0;
                var importedAssay = _importService.ImportAssayFromExcel(path);
                if (importedAssay.Any())
                {
                    insertedAssay = _databaseService.InsertAssay(importedAssay);
                }
                MessageBox.Show($"Импорт проб завершен!\nДобавлено проб: {insertedAssay} \nПропущено дубликатов: {importedAssay.Count - insertedAssay}",
              "Результат", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при импорте проб:\n{ex.Message}", "Ошибка",
                              MessageBoxButton.OK, MessageBoxImage.Error);
            }
            LoadDataFromDatabase();
        }

        private void LoadDataFromDatabase()
        {
            try
            {
                var data = _databaseService.GetAllAssay();

                dataGridView.ItemsSource = data;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при загрузке данных:\n{ex.Message}", "Ошибка",
                              MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void BtnViewDB_Click(object sender, RoutedEventArgs e)
        {
            if (_databaseService != null)
            {
                LoadDataFromDatabase();
            }
            else
            {
                return;
            }
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            TestConnection();
        }
    }
}