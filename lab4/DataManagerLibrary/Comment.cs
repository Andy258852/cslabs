using System;

namespace DataManagerLibrary
{
    public class Comment
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public bool Gender { get; set; }
        public int Age { get; set; }
        public string Nat { get; set; }
        public string PhoneNumber { get; set; }
        public string Country { get; set; }
        public string Company { get; set; }
        public DateTime Date { get; set; }
        public string Text { get; set; }

        public Comment() { }

        public Comment(object[] vs)
        {
            FirstName = ((string)vs[0]).Trim();
            LastName = ((string)vs[1]).Trim();
            Gender = (bool)vs[2];
            Age = (int)vs[3];
            Nat = ((string)vs[4]).Trim();
            PhoneNumber = ((string)vs[5]).Trim();
            Country = (string)vs[6];
            Company = ((string)vs[7]).Trim();
            Date = (DateTime)vs[8];
            Text = ((string)vs[9]).Trim();
        }
    }
}
