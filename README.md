🎓 University Attendance & Coursework Management System

🏫 نظام إدارة الحضور وأعمال السنة للجامعة

---

🚀 Overview | نظرة عامة

✨ A smart system to manage:

- 📅 Attendance (Sections & Lectures)
- 📝 Quiz Marks
- 📊 Coursework Grades
- 📈 Reports & Analytics
- 🔔 Notifications
- ⚡ Real-Time Dashboards

---

✨ نظام متكامل لإدارة:

- تسجيل الحضور
- درجات الكويز
- حساب أعمال السنة
- التقارير
- الإشعارات
- لوحات تحكم فورية

---

🛠️ Tech Stack | التقنيات المستخدمة

- 🧠 ASP.NET Core MVC
- 🗄️ Entity Framework Core
- 💾 SQL Server
- ⚡ SignalR (Real-Time)
- 🎨 Bootstrap
- 🌐 JavaScript

---

👥 Roles | المستخدمين

🧑‍🎓 Student | الطالب

✔️ Can:

- 📊 View dashboard
- 📚 View courses
- 📅 View attendance
- 📝 View quiz marks
- 🔔 Receive notifications
- 📲 Submit attendance via Code / QR

---

👨‍🏫 Teaching Assistant (TA) | المعيد

✔️ Can:

- 📚 View assigned sections
- 📅 Take section attendance
- 📝 Create quizzes
- ✍️ Record quiz marks
- 📈 View reports

---

👨‍🏫 Doctor | الدكتور

✔️ Can:

- 📚 Manage courses
- ⚙️ Configure grading policy
- 📅 Take lecture attendance
- 🔑 Generate QR / Code
- 📊 View reports
- ⚠️ Monitor at-risk students

---

🛠️ Admin | الأدمن (قريبًا)

- 👥 Manage users
- 🏫 Manage departments
- 📚 Manage courses
- 🔄 Full CRUD operations

---

🏫 Academic Structure | الهيكل الأكاديمي

- 🧩 Departments: AI | CS | SE | IS
- 🔢 Each department → 7 sections
- 👥 Each section → ~25 students

---

🧱 Core Entities | الكيانات الأساسية

🧑‍🎓 Student

- Department
- Section
- Enrollments

---

📚 Course

- Shared across departments

---

🧑‍🏫 CourseSection

Course + DepartmentSection + TA

---

🔗 Enrollment

Student + CourseSection

---

📅 Attendance System | نظام الحضور

🎯 Types

📌 Section Attendance

- Manual by TA
- Saved once
- 🔒 Locked after saving

---

🎯 Lecture Attendance

- QR / Code
- Student submits attendance

---

⚠️ Rules

- ❌ No editing after save
- ❌ No duplicate attendance
- ✅ Student must be enrolled
- ❌ Invalid code rejected

---

📝 Quiz System | نظام الكويز

🚫 Not an exam system

✔️ Only for recording marks

Rules:

- Created by TA
- Has MaxMark
- Missing quiz = 0
- Cannot exceed max mark

---

📊 Course Policy | سياسة المادة

Configured by Doctor:

- 🎯 Total Coursework
- 📅 Section Attendance
- 📅 Lecture Attendance
- 📝 Quiz Marks
- ⚠️ Allowed Absences
- 🏆 Best Quizzes Count

---

🧮 Grade Calculation | حساب الدرجات

Section Attendance

(present / total) * weight

Lecture Attendance

(present / total) * weight

Quiz

- Convert to %
- Take best N
- Normalize

---

🔔 Notifications | الإشعارات

Students receive:

- 📝 Quiz announcements
- 📅 Attendance updates
- ⚠️ Absence warnings

---

⚡ Real-Time System | النظام الفوري

🚀 Powered by SignalR

---

🔴 Real-Time Notifications

✨ Instant updates without refresh:

- 📢 Quiz announced
- 📅 Attendance updated
- ⚠️ Absence warning
- 🚫 Absence limit exceeded

---

🔁 Flow

Action
↓
Saved in DB
↓
SignalR push
↓
User receives instantly

---

📊 Real-Time Dashboard

🧑‍🎓 Student Dashboard

Updates instantly when:

- Attendance changes
- Quiz marks updated
- Absence warning triggered

---

👨‍🏫 TA Dashboard

Updates when:

- Students submit attendance
- Section stats change
- Quiz marks saved

---

👨‍🏫 Doctor Dashboard

Updates when:

- Lecture attendance updates
- Course stats change
- At-risk students detected

---

🌐 HTTP Rules | قواعد الراوتس

Method| Usage
GET| View data
POST| Modify data

---

🔥 Important

Attendance → POST
Quiz → POST
Course Config → POST

---

🚏 Routes | الراوتس

🎓 Student

GET /Student/Dashboard
GET /Student/Courses
GET /Student/CourseDetails/{courseId}
GET /Student/Attendance/{courseId}
GET /Student/QuizMarks/{courseId}
GET /Student/Notifications

---

👨‍🏫 TA

GET /TA/Dashboard
GET /TA/Sections
GET /TA/SectionDetails/{sectionId}

---

👨‍🏫 Doctor

GET /Doctor/Dashboard
GET /Doctor/Courses
GET /Doctor/CourseDetails/{courseId}

---

📅 Attendance

GET  /Attendance/Section/{sectionId}
POST /Attendance/SaveSectionAttendance

GET  /Attendance/Lecture/{courseId}
POST /Attendance/SaveLectureAttendance

POST /Attendance/GenerateCode
POST /Attendance/SubmitCode
POST /Attendance/CloseSession

---

📝 Quiz

GET  /Quiz/Create/{sectionId}
POST /Quiz/Create

GET  /Quiz/RecordMarks/{quizId}
POST /Quiz/SaveMarks

POST /Quiz/Close

---

📚 Course

GET  /Course/Configure/{courseId}
POST /Course/SaveConfiguration

---

📊 Reports

GET /Reports/Section/{sectionId}
GET /Reports/Lecture/{sessionId}
GET /Reports/Course/{courseId}
GET /Reports/Student/{studentId}/{courseId}

---

🔔 Notifications

GET  /Notifications
POST /Notifications/MarkAsRead
POST /Notifications/MarkAllAsRead

---

🏗️ Architecture | المعمارية

Controller → Service → DbContext

✔️ Controllers → Requests only
✔️ Services → Business Logic
✔️ Database → Data

---

⚠️ Important Notes | ملاحظات مهمة

- ❌ No business logic in Controllers
- ✔️ Use ViewModels
- ✔️ Validate all actions in Services
- ✔️ Keep Controllers clean

---

🚀 Future Improvements | تحسينات مستقبلية

- 🛠️ Admin Panel
- 🔐 Authentication & Roles
- 📱 Mobile App
- 📡 Advanced Real-Time Analytics
- 🤖 AI-based student performance tracking

---

⭐ If you like the project, give it a star!