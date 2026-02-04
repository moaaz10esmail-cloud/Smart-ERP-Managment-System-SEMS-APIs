namespace SEMS.Core.Enums;

public enum AttendanceStatus { Present, Absent, Late, OnLeave }
public enum InvoiceStatus { Draft, Sent, Paid, Cancelled }
public enum PaymentStatus { Pending, Completed, Failed, Refunded }
public enum PaymentDirection { In = 1, Out = 2 }
public enum LeaveStatus { Pending, Approved, Rejected, Cancelled }
public enum TaskStatus { Todo, InProgress, Done, Blocked }
public enum ProjectStatus { Planned, Active, Completed, OnHold, Cancelled }
public enum UserRole { Admin, Manager, HR, Finance, Sales, ProjectManager, Employee }
