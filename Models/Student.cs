using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Models
{
    public class Student
    {
        public string Name { get; set; }

        public string LastName { get; set; }

        public int Id { get; set; }

        public int Year { get; set; }

        public List<Grade> Grades { get; }

        // Constructor
        public Student() { 
            Name = String.Empty;
            LastName = String.Empty;
            Id = 0;
            Year = 0;
            Grades = new List<Grade>();
        }

        // Add a given grade to the student's Grades list
        public void AddGrade(Grade grade) 
        {  
            Grades.Add(grade);
        }

        public double getFinalGrade() {
            double finalGrade = 0;

            foreach (Grade grade in Grades) {
                finalGrade += double.Parse(grade.Score) * (grade.Percentage / 100);
            }

            return finalGrade;
        }

        public override string ToString()
        {
            return $"{Name} {LastName}";
        }

    }
}
