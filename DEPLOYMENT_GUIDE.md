# Birthday & Nameday Client Notifier - Deployment Guide

## 🎉 Easy Setup Guide for Your Business

This guide will help you set up the Client Birthday & Nameday Notifier on your Windows laptop.

### Prerequisites
- Windows 10 or Windows 11
- At least 500MB free disk space
- Internet connection (for initial setup)

---

## 📦 Quick Installation Steps

### Step 1: Install .NET Runtime
1. Download .NET 9 Runtime from: https://dotnet.microsoft.com/download/dotnet/9.0
2. Choose "Download x64" under ".NET Runtime" (not SDK)
3. Run the installer and click "Install"

### Step 2: Extract the Application
1. Create a folder on your Desktop called `ClientNotifier`
2. Extract all files from the provided ZIP into this folder

### Step 3: First-Time Setup
1. Open the `ClientNotifier` folder
2. Double-click on `SETUP.bat` (we'll create this)
3. The application will:
   - Create the database
   - Load Bulgarian nameday data
   - Be ready to use!

### Step 4: Configure Email (Optional)
1. Open `appsettings.Production.json` in Notepad
2. Find the "EmailSettings" section
3. Update these fields:
   ```json
   "SenderEmail": "your-business-email@gmail.com",
   "Username": "your-business-email@gmail.com",
   "Password": "your-app-password"
   ```
4. Save and close the file

> **Note**: For Gmail, you need an "App Password". See: https://support.google.com/accounts/answer/185833

---

## 🚀 Running the Application

### Starting the Application
1. Double-click `START_APP.bat` on your Desktop
2. A console window will open showing "Now listening on: http://localhost:5000"
3. Your browser will automatically open to the application

### Stopping the Application
- Press `Ctrl+C` in the console window, or
- Simply close the console window

---

## 💾 Data Management

### Database Location
Your client data is stored in: `ClientNotifierData\ClientNotifier.db`

### Backup Your Data
1. Close the application
2. Copy the entire `ClientNotifierData` folder to a USB drive or cloud storage
3. To restore: Replace the folder with your backup

### Adding/Editing Namedays
1. Open `Data\bulgarian-namedays.json` in Notepad
2. Add new entries following this format:
   ```json
   { "name": "Име", "month": 1, "day": 15 }
   ```
3. Save the file and restart the application

---

## 🛠️ Troubleshooting

### Application Won't Start
- Make sure .NET 9 Runtime is installed
- Check that Windows Defender isn't blocking the app
- Try running as Administrator

### Can't Send Emails
- Verify your email settings in `appsettings.Production.json`
- For Gmail: Ensure 2-factor authentication is enabled and use an App Password
- Check your internet connection

### Database Issues
- Delete the `ClientNotifierData` folder (this will remove all data!)
- Restart the application to create a fresh database

---

## 📱 Daily Usage

### Accessing the Application
- Bookmark: http://localhost:5000
- Works best in Chrome or Edge

### Daily Workflow
1. Start the app in the morning
2. Check today's birthdays and namedays on the dashboard
3. Send greetings through the interface
4. The app will remind you at 9:00 AM daily

### Managing Clients
- Click "Clients" to view all clients
- Use "Add Client" to add new people
- Enter their EGN to auto-calculate birthday
- Nameday is assigned automatically based on first name

---

## 🔒 Security Notes

- The application runs locally on your computer
- No data is sent to the internet (except emails)
- Keep your `ClientNotifierData` folder backed up
- Don't share your email password with anyone

---

## 📞 Support

If you need help:
1. Check this guide first
2. Restart the application
3. Contact your IT support person

---

## 🎯 Quick Reference

- **Start App**: Double-click `START_APP.bat`
- **Stop App**: Press `Ctrl+C` or close console
- **Access App**: http://localhost:5000
- **Backup Data**: Copy `ClientNotifierData` folder
- **Edit Namedays**: Edit `Data\bulgarian-namedays.json`

---

Made with ❤️ for your business needs
