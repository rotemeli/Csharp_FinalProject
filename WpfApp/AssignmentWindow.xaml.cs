using Models;
using System.IO;
using System.Text.Json;
using System.Windows;
using System.Windows.Controls;

namespace WpfApp
{
    /// <summary>
    /// Interaction logic for AssignmentWindow.xaml
    /// </summary>
    public partial class AssignmentWindow : Window
    {

        private Course Course;

        private Label CourseNameAndAverage;

        public AssignmentWindow(Course course, Label courseNameAndAverage)
        {
            InitializeComponent();

            this.Course = course;
            this.CourseNameAndAverage = courseNameAndAverage;
            PutTasksOnView();
        }

        // Show the tasks on the window
        private void PutTasksOnView() {
            foreach(Assignment task in Course.Tasks)
            {
                AssignmentListBox.Items.Add(task);
            }
        }

        // Updates the json file when a students task's grade change
        private void UpdateAllStudentsInCourseJsonFile(string taskName)
        {
            string jsonString = File.ReadAllText(Course.JsonFullPath);
            var students = JsonSerializer.Deserialize<List<Dictionary<string, string>>>(jsonString);

            if (students != null)
            {
                foreach(var dict in students)
                {
                    Student? s1 = Course.GetStudent(dict.ElementAt(2).Value);
                    if (s1 != null)
                    {
                        Grade? grade = s1.Grades.Find(g => g.TaskName == taskName.Split("-")[0]);
                        if (grade != null)
                        {
                            dict[taskName] = grade.Score;
                        }
                    }
                }

                var options = new JsonSerializerOptions { WriteIndented = true };
                jsonString = JsonSerializer.Serialize(students, options);

                // Update the json file to the current date
                string newFileName = Course.JsonFullPath.Split("_")[0] + "_" + 
                    DateTime.Now.ToString("dd-MM-yyyy") + ".json";
                File.Move(Course.JsonFullPath, newFileName);
                Course.JsonFullPath = newFileName;

                File.WriteAllText(Course.JsonFullPath, jsonString);
            }
        }

        // When clicked, the user gives all students an additional grade on an assignment
        private void AddFactorBtn_Click(object sender, RoutedEventArgs e)
        {
            var isNumber = double.TryParse(FactorValue.Text, out double factor);
            if (!isNumber || factor < 1 || factor > 100)
            {
                FactorValue.Text = String.Empty;
                MessageBox.Show("Invalid value!");
            }
            else
            {
                Object item = AssignmentListBox.SelectedItem;
                if (item != null)
                {
                    Assignment task = (Assignment)item;
                    foreach (Student student in Course.Students)
                    {
                        var taskName = task.TaskName.Split("-")[0];
                        Grade? grade = student.Grades.Find(g => g.TaskName == taskName);
                        if (grade != null)
                        {
                            double score = double.Parse(grade.Score) + factor;
                            if (score >= 100)
                            {
                                grade.Score = "100";
                            }
                            else
                            {
                                grade.Score = score.ToString();
                            }
                        }
                    }
                    UpdateAllStudentsInCourseJsonFile(task.TaskName);
                    string avg = Course.getFinalGradesAverage().ToString("F4");
                    CourseNameAndAverage.Content = $"{Course.CourseName} (Average: {avg})";
                    this.Close();
                }
                else {
                    MessageBox.Show("Select an assignment!");
                }
            }
        }
    }
}
