using Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using static System.Formats.Asn1.AsnWriter;

namespace WpfApp
{
    /// <summary>
    /// Interaction logic for AssignmentWindow.xaml
    /// </summary>
    public partial class AssignmentWindow : Window
    {

        private Course course;
        public AssignmentWindow(Course course)
        {
            InitializeComponent();
            this.course = course;
            PutTasksOnView();
        }

        private void PutTasksOnView() {
            foreach(Assignment task in course.Tasks)
            {
                AssignmentListBox.Items.Add(task);
            }
        }

        private void UpdateAllStudentsInCourseJsonFile(string taskName)
        {
            string jsonString = File.ReadAllText(course.JsonFullPath);
            var students = JsonSerializer.Deserialize<List<Dictionary<string, string>>>(jsonString);

            if (students != null)
            {
                foreach(var dict in students)
                {
                    Student? s1 = course.GetStudent(dict["ZehutNumber"]);
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

                File.WriteAllText(course.JsonFullPath, jsonString);
            }
        }

        private void AddFactorBtn_Click(object sender, RoutedEventArgs e)
        {
            var isNumber = double.TryParse(FactorValue.Text, out double factor);
            if (!isNumber || factor < 0 || factor > 100)
            {
                MessageBox.Show("Invalid value!");
            }
            else
            {
                Object item = AssignmentListBox.SelectedItem;
                if (item != null)
                {
                    Assignment task = (Assignment)item;
                    foreach (Student student in course.Students)
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
                    this.Close();
                }
                else {
                    MessageBox.Show("Select an assignment!");
                }
            }
        }
    }
}
