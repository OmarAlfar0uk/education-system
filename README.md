<div align="center">

# 🎓 Education System
### Modular Online Examination, Attendance & Student Lifecycle Management Platform

[![.NET 8](https://img.shields.io/badge/.NET-8.0-512BD4?style=for-the-badge&logo=dotnet&logoColor=white)](https://dotnet.microsoft.com/)
[![C#](https://img.shields.io/badge/C%23-12-239120?style=for-the-badge&logo=csharp&logoColor=white)](https://learn.microsoft.com/en-us/dotnet/csharp/)
[![Clean Architecture](https://img.shields.io/badge/Architecture-Clean%20%26%20Domain--Driven-blue?style=for-the-badge&logo=diagram-project&logoColor=white)](#-system-architecture)
[![License: MIT](https://img.shields.io/badge/License-MIT-yellowgreen?style=for-the-badge)](LICENSE)
[![Author](https://img.shields.io/badge/Author-Omar%20Alfarouk-orange?style=for-the-badge&logo=github&logoColor=white)](https://github.com/OmarAlfar0uk)

<p align="center">
  <a href="#-key-features">Key Features</a> •
  <a href="#-system-architecture">System Architecture</a> •
  <a href="#-domain-model">Domain Model</a> •
  <a href="#-tech-stack">Tech Stack</a> •
  <a href="#-getting-started">Getting Started</a> •
  <a href="#-author">Author</a>
</p>

</div>

---

## 📌 Executive Overview

**Education System** is a robust academic administration and online testing engine designed to streamline modern institutional operations. It combines fine-grained **Attendance Tracking**, an **Audit/Activity Logging** infrastructure, categorized question banks, and an asynchronous **Email Queueing Service** to provide an enterprise-grade digital classroom experience.

> [!NOTE]
> Designed using **Feature Slices with Minimal Endpoints**, **Entity Type Configurations** via EF Core Fluent API, and **Domain Abstractions** (`IEmailQueueService`, `INotificationService`, `ICurrentUserService`).

---

## ✨ Key Features

| ⚡ Feature | 💡 Description | 🛠 Engineering Detail |
|---|---|---|
| **📝 Online Exam Engine** | Configurable online exams with question categorization | Category-driven question distribution and automated evaluation |
| **📋 Attendance Monitoring** | Daily course attendance records and student ratios | Entity-mapped configurations with presence/absence auditing |
| **🛡️ Activity & Audit Logs** | Comprehensive tracking of user operations | `ActivityLogConfiguration` auditing administrative and exam events |
| **📨 Asynchronous Email Queue** | Background email queueing to prevent thread blocking | `IEmailQueueService` decoupling email dispatch from web requests |
| **🔐 Role-Based Access** | Secure user lifecycle for Students, Lecturers, and Admins | Custom `ApplicationUser` entity extending ASP.NET Core Identity |

---

## 🏛 System Architecture

```mermaid
flowchart TD
    subgraph Clients["🖥️ Users & Portals"]
        Student["👨‍🎓 Students"]
        Instructor["👨‍🏫 Instructors / Lecturers"]
        Admin["🛠️ Administrators"]
    end

    subgraph Core["⚙️ Education System Endpoints"]
        Accounts["🔐 Accounts Feature<br/>(Auth, Roles, Profile)"]
        Attendance["📋 Attendance Feature<br/>(Absence Tracking)"]
        Categories["📚 Categories Feature<br/>(Course Taxonomies)"]
        Exams["📝 Online Exam Feature<br/>(Testing & Grading)"]
    end

    subgraph Services["📬 Infrastructure Services"]
        EmailQueue["📨 Email Queue Service"]
        Notify["🔔 Notification Service"]
        Audit["🛡️ Activity Logger"]
    end

    subgraph DataTier["🗄️ Database Tier"]
        DB[("Microsoft SQL Server<br/>(EF Core Fluent Configurations)")]
    end

    Clients --> Core
    Core --> Services
    Core --> DB
    Services --> DB
```

---

## ⚡ Tech Stack

| Category | Technology | Purpose |
|---|---|---|
| **Platform** | ![.NET 8](https://img.shields.io/badge/.NET_8-512BD4?style=flat-square&logo=dotnet&logoColor=white) ![C#](https://img.shields.io/badge/C%23_12-239120?style=flat-square&logo=csharp&logoColor=white) | Primary backend application runtime |
| **Architecture** | ![Domain Driven](https://img.shields.io/badge/Domain_Driven-Design-blue?style=flat-square) ![Minimal Endpoints](https://img.shields.io/badge/Minimal_Endpoints-Fast_Routing-purple?style=flat-square) | Clean domain abstractions and endpoint dispatching |
| **Database & ORM** | ![EF Core](https://img.shields.io/badge/EF_Core-8.0-512BD4?style=flat-square&logo=dotnet&logoColor=white) ![SQL Server](https://img.shields.io/badge/MS_SQL_Server-CC292B?style=flat-square&logo=microsoftsqlserver&logoColor=white) | Fluent API entity configurations and migrations |
| **Queuing & Mail** | ![Queue Service](https://img.shields.io/badge/Async_Queue-Email-orange?style=flat-square) | Background email queuing and notification engine |

---

## 🚀 Getting Started

### Prerequisites
- [.NET 8.0 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- SQL Server (LocalDB or Docker instance)

### Setup Instructions

1. **Clone the repository:**
   ```bash
   git clone https://github.com/OmarAlfar0uk/education-system.git
   cd education-system
   ```

2. **Configure Settings:**
   Configure connection string in `appsettings.json`:
   ```json
   {
     "ConnectionStrings": {
       "DefaultConnection": "Server=localhost;Database=EducationSystemDb;Trusted_Connection=True;TrustServerCertificate=True;"
     }
   }
   ```

3. **Update Database & Launch:**
   ```bash
   dotnet ef database update
   dotnet run
   ```

---

## 👨‍💻 Author

**Omar Alfarouk**  
*Full-Stack .NET & Software Engineer*  

- 🌐 **GitHub:** [@OmarAlfar0uk](https://github.com/OmarAlfar0uk)
- 💼 **LinkedIn:** [omar-alfarouk](https://www.linkedin.com/in/omar-alfarouk-252471251/)
- 📧 **Email:** [omaralfarouk646@gmail.com](mailto:omaralfarouk646@gmail.com)

---

<div align="center">
  <sub>Built with ❤️ by Omar Alfarouk. Licensed under the <a href="LICENSE">MIT License</a>.</sub>
</div>
