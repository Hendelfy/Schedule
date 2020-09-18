using System.Collections.Generic;

namespace Schedule
{
    public class SubjModel
    {
        public int Start { get; set; }
        public int Count { get; set; }
        public string Type { get; set; }
        public string SubjectName { get; set; }
        public string Cab { get; set; }

        
    }

    public class User
    {
        public int Id { get; set; }
        public int State { get; set; }
        public long ChatId { get; set; }
        public string Login { get; set; }
        public string Password { get; set; }
        public List<DailySubjects> DailySubjectsList { get; set; }
    }

    public class DailySubjects
    {
        public int Day { get; set; }//0-5
        public List<SubjModel> Subjects { get; set; }
    }
}