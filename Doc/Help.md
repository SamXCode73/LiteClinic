# LiteClinic Help Guide

## Getting Started
- The main dashboard provides access to all core features using the left-side menu (collapsible or expandable).
- First step: personalize LiteClinic by renaming your clinic in **Settings** → change clinic name → click **Apply**.
- Add doctor and patient data from the dashboard. Double-tap to view and manage patient records.
- Configure Telegram notifications in **Settings** → click **Go to Configuration Page Helper**.

---

## Menu Overview

### Dashboard
- Displays today’s summary and appointment calendar.
- Double-tap a doctor’s name to view their appointments.
- Appointment statuses:
  - **Attending** – Patient is currently with the doctor.
  - **Attended** – Patient has already seen the doctor.
  - **Missed** – Patient did not come to the clinic.

### Doctors
- Add, edit, deactivate, or clear doctor profiles.
- Shortcuts: **F1 Add**, **F2 Edit**, **F3 Deactivate**, **F4 Clear**.
- ⚠️ Mandatory fields (marked with a red circle) must be filled, or notifications may fail.
- ⚠️ **IMPORTANT:** After adding a doctor, go to the **Open Scheduled Page** to set the doctor’s schedule.  
  This page shows which days of the month the doctor will attend.  

  **Why this matters:**  
  The appointment filtering system depends on the doctor’s schedule.  
  For example, if you have 100 doctors and you select an appointment date (e.g., the 14th, which is a Monday),  
  only doctors who are scheduled to attend the clinic on Mondays will appear in the appointment list.  
  Without setting the schedule, doctors will not be properly filtered and may not show up for appointments.

### Patients
- Manage patient records and view visit history.
- Shortcuts: **F1 Add**, **F2 Edit**, **F3 Deactivate**, **F4 Clear**.
- ⚠️ Mandatory fields must be filled for notifications to work properly.

### Appointments
- Schedule and control upcoming visits.
- Shortcuts: **F1 Add**, **F2 Edit**, **F3 Deactivate**, **F4 Clear**.
- Patient search supported by:
  - Name (first, middle, last, full)
  - Patient ID or code (e.g., PT000001)
  - Email, mobile number
  - Age or date of birth (dd/MM/yyyy)
- Double-click patient’s name to quickly add appointments.

### Reports
- Provides statistics for:
  - Total doctors (daily, weekly, monthly, yearly)
  - Total patients (daily, weekly, monthly, yearly)
  - Missed appointments (daily, weekly, monthly, yearly)
- Helps track clinic performance and trends.

### Users
- **User Management** → Add, update, deactivate, clear users. Shortcuts: F1–F4.
- **User Roles** → Manage predefined roles and permissions. Recommended to keep defaults unchanged.
- Default accounts:
  - Admin → `admin / admin1234`
  - User → `user / user1234`
- ⚠️ Change default passwords immediately after first login.

### Settings
- Change themes
- Manage notifications
- Configure Telegram bot
- Backup database
- Open log files for diagnostics

### About
- View version details and terms of use.

### Logout
- Exit securely to prevent unauthorized access.

---

## Troubleshooting
- Check internet connection and Windows notification settings.
- Ensure doctors/patients are added and not deactivated.
- Verify appointment dates/times are correct.
- Ensure Telegram app is installed and accessible.

---

## Support
- Contact: **samer.hmouda@outlook.com**
- Visit the Configuration Page Helper in Settings.

---

## Credits
- Developed by **Samer Hmouda**  
- Art Director: **Mohammad Hmouda**  
- Built with **.NET, WinUI 3, Telegram Bot API**  
- Icons: Google Icons  
- Images: Free resources credited to creators  
- Logo: Mohammad Hmouda  
- Special thanks to contributors and feedback providers
