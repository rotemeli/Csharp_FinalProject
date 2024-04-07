namespace Models
{
    public class Course
    {
        public string CourseName { get; }

        public string ExcelFullPath { get; set; }

        public string JsonFullPath { get; set; }

        public List<Student> Students { get; }

        public List<Assignment> Tasks { get; }

        // Constructor
        public Course(string courseName) { 
            CourseName = courseName;

            ExcelFullPath = String.Empty;

            JsonFullPath = String.Empty;

            Students = new List<Student>();

            Tasks = new List<Assignment>();
        }

        // Add a student to course's student list
        public void AddStudent(Student student)
        {
            Students.Add(student);
        }

        // Return the final grade point average of the course
        public double getFinalGradesAverage()
        {
            double finalAvg = 0;

            foreach (Student student in Students)
            {
                finalAvg += student.getFinalGrade();
            }

            return finalAvg / Students.Count;
        }

        // Return a student by a given ID
        public Student? GetStudent(string id) 
        {
            Student? s1 = Students.Find(s => s.Id.ToString() == id);

            return s1;
        }

        // Add a assignment to the tasks list
        public void AddAssignment(Assignment task) { 
            Tasks.Add(task); 
        }

        public override string ToString()
        {
            return CourseName;
        }
    }
}
