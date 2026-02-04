# Smart ERP Management System (SEMS) - APIs

Welcome to the **Smart ERP Management System (SEMS)** backend repository. This project is a comprehensive Enterprise Resource Planning (ERP) solution built with **.NET 8**, designed to streamline business operations across various departments including CRM, Finance, HR, Inventory, and Project Management.

## 🚀 Overview

SEMS is designed using **Clean Architecture** principles to ensure scalability, maintainability, and testability. It provides a robust set of RESTful APIs to manage core business processes efficiently.

## 🛠 Tech Stack

-   **Framework**: .NET 8 Web API
-   **ORM**: Entity Framework Core
-   **Database**: SQLite / SQL Server (Configurable)
-   **Architecture**: Clean Architecture (Domain-Driven Design)
-   **Authentication**: JWT (JSON Web Tokens) with Refresh Tokens
-   **Documentation**: Swagger / OpenAPI

## 📦 Key Modules & Features

### 🤝 Customer Relationship Management (CRM)
The CRM module allows businesses to manage interactions with current and potential customers.
-   **Customers**: Manage customer profiles and details.
-   **Contacts**: Store contact information for individuals associated with customers.
-   **Opportunities**: Track sales opportunities and their stages.
-   **Sales Orders**: Manage orders placed by customers.
-   **Contracts**: Handle customer contracts and agreements.
-   **Complaints**: Track and resolve customer complaints.
-   **Communication Logs**: Record history of interactions (calls, emails, meetings).

### 💰 Finance
Comprehensive financial management tools.
-   **Invoices**: Create and manage invoices (supports partial payments).
-   **Payments**: Record payments (IN/OUT) and link them to invoices.
-   **Bank Accounts**: Manage company bank accounts and track balances.
-   **Budgets & Expenses**: Plan budgets and track organizational expenses.
-   **Transactions**: Record general financial transactions.

### 👥 Human Resources (HR)
Manage your workforce effectively.
-   **Employees**: detailed employee profiles.
-   **Departments & Roles**: Organizational structure management.
-   **Attendance**: Track employee attendance and working hours.
-   **Leave Requests**: Manage leave applications and approvals.
-   **Payroll**: Process salaries and benefits.

### 📦 Inventory & Supply Chain
Keep track of your stock and supplies.
-   **Products**: Manage product catalog.
-   **Warehouses**: Multi-warehouse support.
-   **Stocks**: Real-time stock level tracking.
-   **Purchase Orders**: Manage procurement from suppliers.
-   **Suppliers**: Vendor management.

### 🚀 Project Management
Plan, execute, and track projects.
-   **Projects**: Create and manage projects.
-   **Tasks & Milestones**: Break down projects into actionable tasks and milestones.
-   **Resource Allocation**: Assign employees and resources to projects.
-   **Time Logs**: Track time spent on tasks.

### 🔐 Security & Identity
-   **User Management**: Role-based access control (RBAC).
-   **Permissions**: Granular permission settings.
-   **Audit Logs**: Track system activities for security and compliance.

### 🏢 Multi-Tenancy
-   Built-in support for multi-tenancy to serve multiple organizations within a single deployment.

## ⚙️ Getting Started

### Prerequisites
-   [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
-   [Visual Studio 2022](https://visualstudio.microsoft.com/) or [VS Code](https://code.visualstudio.com/)

### Installation

1.  **Clone the repository**
    ```bash
    git clone https://github.com/moaaz10esmail-cloud/Smart-ERP-Managment-System-SEMS-APIs.git
    cd Smart-ERP-Managment-System-SEMS-APIs
    ```

2.  **Configure Database**
    Update `appsettings.json` with your connection string if necessary.

3.  **Run Migrations**
    ```bash
    dotnet ef database update --project SEMS.Infrastructure --startup-project SEMS.API
    ```

4.  **Run the API**
    ```bash
    dotnet run --project SEMS.API
    ```

5.  **Explore API**
    Open your browser and navigate to `https://localhost:5001/swagger` (or the configured port) to view the API documentation.

## 🤝 Contributing

Contributions are welcome! Please feel free to submit a Pull Request.

## 📄 License

This project is licensed under the MIT License.
