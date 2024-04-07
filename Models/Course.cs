namespace Models
{
    public class Course
    {
        public string CourseName { get; }

        public string ExcelFullPath { get; set; }

        public string JsonFullPath { get; set; }

        public List<Student> Students { get; }

        public List<Assignment> Tasks { get; }

        public Course(string courseName) { 
            CourseName = courseName;

            ExcelFullPath = String.Empty;

            JsonFullPath = String.Empty;

            Students = new List<Student>();

            Tasks = new List<Assignment>();
        }

        public void AddStudent(Student student)
        {
            Students.Add(student);
        }

        public double getFinalGradesAverage()
        {
            double finalAvg = 0;

            foreach (Student student in Students)
            {
                finalAvg += student.getFinalGrade();
            }

            return finalAvg / Students.Count;
        }

        public Student? GetStudent(string id) 
        {
            Student? s1 = Students.Find(s => s.Id.ToString() == id);

            return s1;
        }

        public void AddAssignment(Assignment task) { 
            Tasks.Add(task); 
        }

        public override string ToString()
        {
            return CourseName;
        }
    }
}
