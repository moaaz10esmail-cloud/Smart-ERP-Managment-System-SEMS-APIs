namespace SEMS.Core.Identity;

public static class Permissions
{
    public static class Employees
    {
        public const string View = "Employees.View";
        public const string Create = "Employees.Create";
        public const string Edit = "Employees.Edit";
        public const string Delete = "Employees.Delete";
    }

    public static class Invoices
    {
        public const string View = "Invoices.View";
        public const string Create = "Invoices.Create";
        public const string Pay = "Invoices.Pay";
        public const string Cancel = "Invoices.Cancel";
    }

    public static class Users
    {
        public const string View = "Users.View";
        public const string ManageRoles = "Users.ManageRoles";
    }

    public static class Tenants
    {
        public const string View = "Tenants.View";
        public const string Create = "Tenants.Create";
    }

    public static class Attendance
    {
        public const string View = "Attendance.View";
        public const string Create = "Attendance.Create";
        public const string Approve = "Attendance.Approve";
    }
}
