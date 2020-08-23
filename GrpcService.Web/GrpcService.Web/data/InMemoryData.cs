namespace GrpcService.Web.data
{
    using Google.Protobuf.WellKnownTypes;
    using GrpcServer.Web.protos;
    using GrpcService.Web.protos;
    using System;
    using System.Collections.Generic;

    /// <summary>
    /// Defines the <see cref="InMemoryData" />.
    /// </summary>
    public static class InMemoryData
    {
        /// <summary>
        /// Defines the Employees.
        /// </summary>
        public static List<Employee> Employees = new List<Employee> {
            new Employee
         {
             Id = 1,
             No = 1994,
             FirstName = "Chandler",
             LastName = "Bing",
             //Salary = 2200
             MonthSalary = new MonthSalary
             {
                 Basic = 5000f,
                 Bonus = 125.5f
             },
             Status = EmployeeStatus.Normal,
             LastModified = Timestamp.FromDateTime(DateTime.UtcNow)
         },
            new Employee
         {
             Id = 2,
             No = 1991,
             FirstName = "Test1",
             LastName = "Hung",
             //Salary = 2400
             MonthSalary = new MonthSalary
             {
                 Basic = 4000f,
                 Bonus = 125.5f
             },
             Status = EmployeeStatus.Onvacation,
             LastModified = Timestamp.FromDateTime(DateTime.UtcNow)
         },
            new Employee
         {
             Id = 3,
             No = 1990,
             FirstName = "Test2",
             LastName = "Ho",
             //Salary = 2500
             MonthSalary = new MonthSalary
             {
                 Basic = 5000f,
                 Bonus = 125.5f
             },
             Status = EmployeeStatus.Resigned,
             LastModified = Timestamp.FromDateTime(DateTime.UtcNow)
         }
        };
    }
}
