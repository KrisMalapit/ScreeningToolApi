//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Manual changes to this file may cause unexpected behavior in your application.
//     Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace ScreeningToolApi.Models
{
    using System;
    using System.Collections.Generic;
    
    public partial class Employee
    {
        public int Id { get; set; }
        public string EmployeeId { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string ContactNo { get; set; }
        public string ContactPerson { get; set; }
        public string ContactPersonNo { get; set; }
        public string City { get; set; }
        public string Barangay { get; set; }
        public int Age { get; set; }
        public string Status { get; set; }
        public int DepartmentId { get; set; }
        public string PickUpPoint { get; set; }
        public System.DateTime Birthday { get; set; }
        public Nullable<int> Vaccinated { get; set; }
        public System.DateTime FirstDose { get; set; }
        public System.DateTime SecondDose { get; set; }
    
        public virtual Department Department { get; set; }
    }
}
