using Microsoft.Win32;
using Models;
using System.Diagnostics;
using System.IO;
using System.Net.Security;
using System.Runtime.Intrinsics.X86;
using System.Text;
using System.Text.Json;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Xml.Schema;

namespace WpfApp
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            InitJsonFiles();
        }

        private Course? ConvertListToCourseObject(List<Dictionary<string, string>> deserialized, string courseName) 
        {
            if(deserialized == null || deserialized.Count == 0)
            {
                return null;
            }
            Course course = new Course(courseName);
            bool flag = true;
            foreach (var dict in deserialized) 
            {
                int i = 0;
                Student student = new Student();
                foreach (var pair in dict)
                {
                    switch(i)
                    {
                        case 0:
                            student.Name = pair.Value;
                            break;
                        case 1:
                            student.LastName = pair.Value;
                            break;
                        case 2:
                            student.Id = int.Parse(pair.Value);
                            break;
                        case 3:
                            student.Year = int.Parse(pair.Value);
                            break;
                        default:
                            string task = pair.Key;
                            var props = task.Split("-");
                            double percent = double.Parse(props[1].Replace("%", ""));
                            Grade g = new Grade(props[0], percent, pair.Value);
                            student.AddGrade(g);
                            if (flag) {
                                course.AddAssignment(new Assignment(task));
                            }
                            break;
                    }
                    ++i;
                }
                flag = false;
                course.AddStudent(student);
            }
            return course;
        }

        private Course? ConvertJsonFileToCourseObject(string file) 
        {
            string jsonContent = File.ReadAllText(file);

            FileInfo fileInfo = new FileInfo(file);
            string ext = fileInfo.Extension;
            string courseName = fileInfo.Name.Replace(ext, "");

            // Deserialize the JSON content back to List<Dictionary<string, string>>
            var deserializedList = JsonSerializer.Deserialize<List<Dictionary<string, string>>>(jsonContent);
            if(deserializedList == null) {
                return null;
            }
            return ConvertListToCourseObject(deserializedList, courseName);
        }

        private void InitJsonFiles()
        {
            string[] jsonFiles = Directory.GetFiles(Directory.GetCurrentDirectory(), "*.json");
            foreach (string file in jsonFiles)
            {
                FileInfo fileInfo = new FileInfo(file);
                if (fileInfo.Name.Any(char.IsDigit) && File.Exists(file)) 
                {
                    Course? c = ConvertJsonFileToCourseObject(file);
                    if (c != null && !CoursesBox.Items.Contains(c))
                    {
                        CoursesBox.Items.Add(c);
                    }
                }
            }
        }

        private void ConvertCsvFileToJsonObject(string path, string jsonFileName)
        {
            var csv = new List<string[]>();
            var lines = File.ReadAllLines(path);

            foreach (string line in lines)
                csv.Add(line.Split(','));

            var properties = lines[0].Split(',');

            var listObjResult = new List<Dictionary<string, string>>();

            for (int i = 1; i < lines.Length; i++)
            {
                var objResult = new Dictionary<string, string>();
                for (int j = 0; j < properties.Length; j++)
                    objResult.Add(properties[j], csv[i][j]);

                listObjResult.Add(objResult);
            }

            JsonSerializerOptions options = new JsonSerializerOptions { WriteIndented = true };
            string userJsonText = JsonSerializer.Serialize<List<Dictionary<string, string>>>(listObjResult, options);
            File.WriteAllText($"{jsonFileName}.json", userJsonText);
        }

        private void PutCourseOnView(Course course, string fullPath)
        {
            ClearView();
            ExcelFullPath.Text = fullPath;
            string avg = course.getFinalGradesAverage().ToString("F4");
            CourseNameAndAverage.Content = $"{course.CourseName} (Average: {avg})";
            course.FullPath = fullPath;

            if(!CoursesBox.Items.Contains(course)) {
                CoursesBox.Items.Add(course);
            }
            
            CoursesBox.SelectedItem = course;

            List<Student> studentsList = new List<Student>();
            foreach (var student in course.Students)
            {
                studentsList.Add(student);
            }
            
            studentsList.Sort((x, y) => String.Compare(x.Name, y.Name));
            
            foreach (var student in studentsList)
            {
                StudentsInCourse.Items.Add(student);
            }
            if(FactorBtn != null)
            {
                FactorBtn.Visibility = Visibility.Visible;
            }
            CourseName.Content = course.CourseName;
        }

        private void FileDialogBtn_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog fileDialog = new OpenFileDialog();
            fileDialog.Filter = "CSV files (*.csv)|*.csv";
            if (fileDialog.ShowDialog() == true)
            {
                var file = fileDialog.FileName;
                FileInfo fileInfo = new FileInfo(file);
                string ext = fileInfo.Extension;

                string fileNameWithoutExt = fileInfo.Name.Replace(ext, "");
                

                string jsonFileName = fileNameWithoutExt + "-" + DateTime.Now.ToString("dd-MM-yyyy");
                ConvertCsvFileToJsonObject(file, jsonFileName);
                Course? c = ConvertJsonFileToCourseObject($"{jsonFileName}.json");
                if(c != null )
                {
                    PutCourseOnView(c, file);
                }
            }
        }

        private void StudentsInCourse_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            Object item = StudentsInCourse.SelectedItem;
            if (item != null)
            {
                StudentDetails.Clear();
                Student s = (Student)item;
                StringBuilder str = new StringBuilder();
                str.Append($"Name: {s.Name}\nLast Name: {s.LastName}");
                str.Append($"\nID: {s.Id}\nYear: {s.Year}");

                StudentDetails.Text = str.ToString();

                Grades.Items.Clear();
                foreach (var grade in s.Grades) 
                { 
                    StackPanel sp = new StackPanel();
                    sp.Orientation = Orientation.Horizontal;

                    Label taskName = new Label();
                    taskName.Content = grade.ToString();
                    taskName.Width = 70;
                    taskName.HorizontalAlignment = HorizontalAlignment.Left;
                    sp.Children.Add(taskName);

                    TextBox tb = new TextBox();
                    tb.Text = grade.Score.ToString();
                    tb.Width = 70;
                    tb.HorizontalAlignment = HorizontalAlignment.Center;
                    tb.Margin = new Thickness(15,0,0,0);
                    sp.Children.Add(tb);

                    Label percent = new Label();
                    percent.Content = grade.Percentage.ToString() + "%";
                    percent.Width = 40;
                    percent.HorizontalAlignment = HorizontalAlignment.Right;
                    percent.Margin = new Thickness(5, 0, 0, 0);
                    sp.Children.Add(percent);

                    Grades.Items.Add(sp);
                }
                FinalGrade.Content = $"Final Grade:\t{s.getFinalGrade().ToString("F2")}";

            }
        }

        private void SaveGradesBtn_Click(object sender, RoutedEventArgs e)
        {
            Object obj = StudentsInCourse.SelectedItem;
            if (obj != null)
            {
                Student s = (Student)obj;
                int index = 0;
                foreach (var item in Grades.Items)
                {
                    if (item is StackPanel stackPanel)
                    {

                        foreach (var child in stackPanel.Children)
                        {
                            if (child is TextBox textBox)
                            {
                                if (textBox.Text == String.Empty)
                                {
                                    s.Grades[index].Score = "0";
                                    textBox.Text = "0";
                                }
                                else {
                                    var isNumber = double.TryParse(textBox.Text, out double score);
                                    if (isNumber && score >= 0 && score <= 100)
                                    {
                                        s.Grades[index].Score = textBox.Text;
                                    }
                                    else
                                    {
                                        textBox.Text = s.Grades[index].Score;
                                    }
                                }
                                ++index;
                            }
                        }
                    }
                }
                FinalGrade.Content = $"Final Grade:\t{s.getFinalGrade().ToString("F2")}";

                Object courseObj = CoursesBox.SelectedItem;
                if (CoursesBox.SelectedIndex != 0)
                {
                    Course course = (Course)courseObj;
                    string avg = course.getFinalGradesAverage().ToString("F4");
                    CourseNameAndAverage.Content = $"{course.CourseName} (Average: {avg})";
                }
                
            }
        }

        private void ClearView() 
        {
            ExcelFullPath?.Clear();

            if (CourseNameAndAverage != null) {
                CourseNameAndAverage.Content = "Course Name (Final Grades Average)";
            }
            
            StudentsInCourse?.Items.Clear();

            StudentDetails?.Clear();

            if (Grades != null) {
                Grades.Items.Clear();

                FinalGrade.Content = "Final Grade:";
            }
            if(FactorBtn != null)
            {
                FactorBtn.Visibility = Visibility.Hidden;
            }
            
            CourseName.Content = "Course Name";

        }

        private void CoursesBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            Object item = CoursesBox.SelectedItem;
            if (CoursesBox.SelectedIndex != 0)
            {
                Course course = (Course)item;
                PutCourseOnView(course, course.FullPath);
            }
            else
            {
                ClearView();
            }
        }

        private void FactorBtn_Click(object sender, RoutedEventArgs e)
        {
            Object item = CoursesBox.SelectedItem;
            if (CoursesBox.SelectedIndex != 0)
            {
                Course course = (Course)item;
                AssignmentWindow AssignmentWindow = new AssignmentWindow(course);
                AssignmentWindow.Show();
                FactorBtn.Visibility = Visibility.Visible;
            }
        }
    }
}
