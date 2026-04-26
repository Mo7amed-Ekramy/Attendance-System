# 🎓 University Attendance & Coursework Management System

<p align="center">
  <img src="https://img.shields.io/badge/Backend-ASP.NET%20Core%20MVC-512bd4?style=for-the-badge&logo=dotnet" />
  <img src="https://img.shields.io/badge/Database-SQL%20Server-CC2927?style=for-the-badge&logo=microsoft-sql-server" />
  <img src="https://img.shields.io/badge/Real--Time-SignalR-orange?style=for-the-badge" />
  <img src="https://img.shields.io/badge/Architecture-Clean%20Layered-blue?style=for-the-badge" />
</p>

---

## 🚀 Overview | نظرة عامة
**University Attendance & Coursework Management System** is a robust, backend-driven web application designed to digitize and automate academic workflows. From tracking student attendance via QR codes to managing complex grading policies and real-time notifications.

### ✨ Key Features | المميزات الرئيسية
- 📅 **Smart Attendance:** Secure tracking for Lectures (QR/Code) and Sections.
- 📝 **Quiz Management:** Effortless recording and management of student marks.
- 🧮 **Automated Grading:** Dynamic calculation based on custom course policies (Best N Quizzes).
- ⚡ **Real-Time Dashboards:** Instant updates for students, TAs, and Doctors via SignalR.
- 🔔 **Instant Notifications:** Stay updated with quiz announcements and absence warnings.

---

## 🛠️ Tech Stack | التقنيات المستخدمة
- **Framework:** ASP.NET Core MVC (8.0)
- **ORM:** Entity Framework Core
- **Database:** Microsoft SQL Server
- **Real-Time:** ASP.NET Core SignalR
- **Frontend:** Bootstrap 5, JavaScript, CSS3
- **Authentication:** Cookie-based with Role-based Authorization

---

## 🏗️ Architecture | المعمارية
The project follows a **Clean Layered Architecture** to ensure scalability and maintainability:
`Controller ➡️ Service Layer (Business Logic) ➡️ DbContext ➡️ Database`

> [!IMPORTANT]
> All business rules and validations are encapsulated within the **Service Layer**, keeping controllers thin and focused only on request handling.

---

## 👥 User Roles | الصلاحيات
| Role | Capabilities |
| :--- | :--- |
| **🧑‍🎓 Student** | View Dashboard, Track Attendance/Marks, Submit Attendance via Code/QR. |
| **👨‍🏫 TA** | Manage Sections, Take Attendance, Create Quizzes & Record Marks. |
| **👨‍⚕️ Doctor** | Manage Courses, Set Grading Policies, Generate QR Codes, Monitor Risk Students. |
| **🛠️ Admin** | Full System Management (Users, Departments, Courses). |

---

## 🔗 Core Entities & ERD
The system is built on a complex relational database designed for academic integrity:
- **Academic Hierarchy:** `Department ➡️ Section ➡️ CourseSection ➡️ Enrollment`
- **Attendance:** Prevention of duplicate records and session locking logic.
- **Grading:** Flexible `CoursePolicy` for each doctor.

---

## ⚡ Real-Time Experience (SignalR)
We used **SignalR** to bridge the gap between the server and the client:
- **Instant Alerts:** No page refresh needed for absence warnings.
- **Live Updates:** Dashboards reflect new marks and attendance data immediately.

---

## 👥 The Team | فريق العمل

<table align="center">
  <tr>
    <td align="center">
      <img src="https://ui-avatars.com/api/?name=Mahmoud+Sayed&background=512bd4&color=fff" width="100" style="border-radius:50%"/><br/>
      <b>Mahmoud Sayed</b><br/>
      ⚙️ Backend Developer<br/>
      <a href="https://www.linkedin.com/in/mahmoud-sayed-mohamed/">
        <img src="https://img.shields.io/badge/LinkedIn-0077B5?style=flat&logo=linkedin&logoColor=white"/>
      </a>
    </td>
    <td align="center">
      <img src="https://ui-avatars.com/api/?name=Julia+Osama&background=ff69b4&color=fff" width="100" style="border-radius:50%"/><br/>
      <b>Julia Osama</b><br/>
      🎨 UI / UX Designer<br/>
      <a href="https://www.linkedin.com/in/julia-osama-756205386/">
        <img src="https://img.shields.io/badge/LinkedIn-0077B5?style=flat&logo=linkedin&logoColor=white"/>
      </a>
    </td>
    <td align="center">
      <img src="https://ui-avatars.com/api/?name=Mohamed+Ekramy&background=0077b5&color=fff" width="100" style="border-radius:50%"/><br/>
      <b>Mohamed Ekramy</b><br/>
      💻 Frontend Developer<br/>
      <a href="https://www.linkedin.com/in/mohamed-ekramy25/">
        <img src="https://img.shields.io/badge/LinkedIn-0077B5?style=flat&logo=linkedin&logoColor=white"/>
      </a>
    </td>
    <td align="center">
      <img src="https://ui-avatars.com/api/?name=Mohamed+Ibrahim&background=28a745&color=fff" width="100" style="border-radius:50%"/><br/>
      <b>Mohamed Ibrahim</b><br/>
      📊 Data Analyst<br/>
      <a href="https://www.linkedin.com/in/mohamed-ibrahim-bb2b2b324/">
        <img src="https://img.shields.io/badge/LinkedIn-0077B5?style=flat&logo=linkedin&logoColor=white"/>
      </a>
    </td>
  </tr>
</table>

---

## 🚀 How to Run
1. Clone the repository.
2. Update the connection string in `appsettings.json`.
3. Run `Update-Database` via Package Manager Console.
4. The system will automatically seed initial data (Departments, Users, Courses) via `DbSeeder`.

---
<p align="center"> ⭐ If you like this project, give it a star! </p>
