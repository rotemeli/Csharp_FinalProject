namespace Models
{
    public class Course
    {
        public string CourseName { get; }

        public string FullPath { get; set; }

        public List<Student> Students { get; }

        public Course(string courseName) { 
            CourseName = courseName;

            FullPath = String.Empty;

            Students = new List<Student>();
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

        public Student? GetStudent(Student student) 
        {
            Student? s1 = Students.Find(s => student.Id == student.Id);

            return s1;
        }

        public override string ToString()
        {
            return CourseName;
        }
    }
}
