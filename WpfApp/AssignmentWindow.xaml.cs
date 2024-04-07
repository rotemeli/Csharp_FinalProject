using Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

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
    }
}
