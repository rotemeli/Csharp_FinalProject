﻿using Microsoft.Win32;
using Models;
using System.IO;
using System.Text;
using System.Text.Json;
using System.Windows;
using System.Windows.Controls;

namespace WpfApp
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private string JsonFilesPath;
        
        public MainWindow()
        {
            InitializeComponent();

            // Creates a directory that will contain all the JSON files
            string currDir = Directory.GetCurrentDirectory();
            JsonFilesPath = System.IO.Path.Combine(currDir, "JsonFiles");
            if (!Directory.Exists(JsonFilesPath))
            {
                Directory.CreateDirectory(JsonFilesPath);
            }

            // Reads the previously generated JSON files
            InitJsonFiles();
        }

        // Convert the json files in the project and add them to the list of courses
        private void InitJsonFiles()
        {
            string currDir = Directory.GetCurrentDirectory();
            string jsonDir = System.IO.Path.Combine(currDir, "JsonFiles");
            string[] jsonFiles = Directory.GetFiles(jsonDir, "*.json");
            foreach (string file in jsonFiles)
            {
                Course? c = ConvertJsonFileToCourseObject(file);
                if (c != null && !CoursesBox.Items.Contains(c))
                {
                    CoursesBox.Items.Add(c);
                }
            }
        }

        #region ConvertFunctions

        // Convert a list of dictionaries each representing a student to a course object
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

        // Convert json file to a course object
        private Course? ConvertJsonFileToCourseObject(string file) 
        {
            string jsonContent = File.ReadAllText(file);

            FileInfo fileInfo = new FileInfo(file);
            string ext = fileInfo.Extension;
            string courseName = fileInfo.Name.Replace(ext, "");
            courseName = courseName.Split("_")[0];

            // Deserialize the JSON content back to List<Dictionary<string, string>>
            var deserializedList = JsonSerializer.Deserialize<List<Dictionary<string, string>>>(jsonContent);
            if(deserializedList == null) {
                return null;
            }
            Course? course = ConvertListToCourseObject(deserializedList, courseName);
            if(course != null)
            {
                course.JsonFullPath = file;
            }
            return course;
        }

        // Convert csv file to a json file
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
            string jsonFilePath = System.IO.Path.Combine(JsonFilesPath, $"{jsonFileName}.json");
            File.WriteAllText(jsonFilePath, userJsonText);
        }
        #endregion

        #region View Updates
        // Clear the view of the WPF window
        private void ClearView()
        {
            ExcelFullPath?.Clear();

            if (CourseNameAndAverage != null)
            {
                CourseNameAndAverage.Content = "Course Name (Final Grades Average)";
            }

            StudentsInCourse?.Items.Clear();

            StudentDetails?.Clear();

            if (Grades != null)
            {
                Grades.Items.Clear();

                FinalGrade.Content = "Final Grade:";
            }
            if (FactorBtn != null)
            {
                FactorBtn.Visibility = Visibility.Hidden;
            }

            CourseName.Content = "Course Name";
        }

        // Show a given course on the window
        private void PutCourseOnView(Course course)
        {
            if(course == null) { return; }

            ClearView();

            ExcelFullPath.Text = course.ExcelFullPath;
            string avg = course.getFinalGradesAverage().ToString("F4");
            CourseNameAndAverage.Content = $"{course.CourseName} (Average: {avg})";

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
        #endregion

        #region Help functions

        private static void IfFileExistThenDelete(string fileName)
        {
            // Check if the json file already exists
            string currDir = Directory.GetCurrentDirectory();
            string jsonDir = System.IO.Path.Combine(currDir, "JsonFiles");

            string[] jsonFiles = Directory.GetFiles(jsonDir, "*.json");
            foreach (var jsonFile in jsonFiles)
            {
                FileInfo fi = new FileInfo(jsonFile);
                string fName = fi.Name.Split("_")[0];
                if (fName.Equals(fileName))
                {
                    File.Delete(jsonFile);
                }
            }
        }

        private Course? GetCourseFromComboBox(string courseName)
        {
            foreach(var c in CoursesBox.Items)
            {
                if(c is Course course && course.CourseName == courseName)
                {
                    return course;
                }
            }
            return null;
        }

        // Updates the json file when a student's grades change
        private void UpdateStudentInCourseJsonFile(Course course, Student student)
        {
            string jsonString = File.ReadAllText(course.JsonFullPath);
            var students = JsonSerializer.Deserialize<List<Dictionary<string, string>>>(jsonString);

            if (students != null)
            {
                var dict = students.Find(s => s.ContainsValue(student.Id.ToString()));

                foreach (var task in course.Tasks)
                {
                    Grade? grade = student.Grades.Find(g => g.TaskName == task.TaskName.Split("-")[0]);
                    if (grade != null && dict != null)
                    {
                        dict[task.TaskName] = grade.Score;
                    }

                }

                var options = new JsonSerializerOptions { WriteIndented = true };
                jsonString = JsonSerializer.Serialize(students, options);

                // Update the json file to the current date
                string newFileName = course.JsonFullPath.Split("_")[0] + "_" +
                    DateTime.Now.ToString("dd-MM-yyyy") + ".json";
                File.Move(course.JsonFullPath, newFileName);
                course.JsonFullPath = newFileName;

                File.WriteAllText(course.JsonFullPath, jsonString);
            }
        }
        #endregion

        #region Event Listeners
        // A function that lets the user open a CSV file and displays it in a window
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

                string jsonFileName = fileNameWithoutExt + "_" + DateTime.Now.ToString("dd-MM-yyyy");

                IfFileExistThenDelete(fileNameWithoutExt);

                ConvertCsvFileToJsonObject(file, jsonFileName);

                string jsonFilePath = Path.Combine(JsonFilesPath, $"{jsonFileName}.json");

                Course? c = ConvertJsonFileToCourseObject(jsonFilePath);

                // Check if the ComboBox already contains the course
                Course? courseToRemove = null;
                if( c != null )
                {
                    courseToRemove = GetCourseFromComboBox(c.CourseName);

                    if (courseToRemove != null)
                    {
                        CoursesBox.Items.Remove(courseToRemove);
                    }
                }
                
                if(c != null )
                {
                    c.ExcelFullPath = fileInfo.FullName;
                    PutCourseOnView(c);
                }
            }
        }

        // Handles the event triggered when the selection of the student list in a course changes
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

        // Handles the click event of the Save Grades button
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
                                        MessageBox.Show("Invalid grade!");
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
                    UpdateStudentInCourseJsonFile(course, s);
                }
            }
        }

        // Handles the selection change event for the courses ComboBox
        private void CoursesBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            Object item = CoursesBox.SelectedItem;
            if (CoursesBox.SelectedIndex != 0)
            {
                Course course = (Course)item;
                PutCourseOnView(course);
            }
            else
            {
                ClearView();
            }
        }

        /* When the factor button is clicked, 
         * this method opens a new window that allows the user to give a factor to a given task
         */
        private void FactorBtn_Click(object sender, RoutedEventArgs e)
        {
            Object item = CoursesBox.SelectedItem;
            if (CoursesBox.SelectedIndex != 0)
            {
                StudentsInCourse.UnselectAll();
                StudentDetails?.Clear();
                if (Grades != null)
                {
                    Grades.Items.Clear();

                    FinalGrade.Content = "Final Grade:";
                }
                Course course = (Course)item;
                AssignmentWindow AssignmentWindow = new AssignmentWindow(course, CourseNameAndAverage);
                AssignmentWindow.Show();
            }
        }
        #endregion
    }
}
