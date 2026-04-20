# Library Management System

A comprehensive Library Management System built with a decoupled architecture featuring an ASP.NET Core Web API backend and an ASP.NET Core MVC frontend.

## 🚀 Project Overview

This system is designed to streamline library operations, from managing book catalogs to tracking member borrowings and handling subscriptions. It provides a secure, role-based environment for both administrators and library members.

### Key Features

- **📚 Catalog Management**: Full CRUD operations for books and categories, including cover image support.
- **👥 Member Management**: Administrative tools to manage library members, their profiles, and roles.
- **🔖 Borrowing System**: Comprehensive tracking of book loans, returns, and history.
- **💳 Subscription Plans**: Tiered membership system where administrators can assign and manage subscription plans.
- **🔐 Secure Access**: Role-based access control (RBAC) powered by ASP.NET Core Authentication, ensuring only authorized users can perform sensitive actions.

## 🛠️ Technology Stack

### Backend
- **Framework**: ASP.NET Core 8.0 Web API
- **ORM**: Entity Framework Core
- **Database**: SQL Server
- **Architecture**: Feature-based directory structure for better maintainability.

### Frontend
- **Framework**: ASP.NET Core MVC
- **Styling**: Modern CSS with a focus on premium user experience.
- **Authentication**: Cookie-based authentication for seamless session management.

## 📁 Project Structure

- **`/Backend`**: The RESTful API service handling business logic and data persistence.
- **`/Frontend`**: The MVC web application providing the user interface.
- **`/Database`**: Contains SQL scripts for database initialization and schema definitions.

## 🚦 Getting Started

1.  **Database Setup**: Run the scripts in `/Database` to initialize your SQL Server instance.
2.  **Run Backend**: Start the `Backend` project to host the API.
3.  **Run Frontend**: Start the `Frontend` project to access the management dashboard.