# How to Run ClientNotifier API

## 🚀 Quick Start (Command Line)

1. **Open PowerShell or Terminal**
2. **Navigate to API folder:**
   ```powershell
   cd C:\WORK\repos\ClientNotifier\ClientNotifier.API
   ```
3. **Run the API:**
   ```powershell
   dotnet run
   ```
4. **Wait for startup** - You'll see:
   ```
   info: Microsoft.Hosting.Lifetime[14]
         Now listening on: http://localhost:5000
   ```
5. **Open Swagger UI:** http://localhost:5000/swagger

---

## 🎯 Visual Studio Instructions

### Initial Setup (One Time)
1. **Open Visual Studio**
2. **File → Open → Project/Solution**
3. **Select:** `C:\WORK\repos\ClientNotifier\ClientNotifier.sln`
4. **Set Startup Project:**
   - Right-click `ClientNotifier.API` in Solution Explorer
   - Select "Set as Startup Project"

### Running the API
1. **Select Run Profile** (dropdown next to green play button):
   - `http` - Runs on http://localhost:5000
   - `https` - Runs on https://localhost:5001
   - `IIS Express` - Uses IIS Express

2. **Press F5** or click the green **Play button**

3. **Browser opens automatically** to Swagger UI

---

## 🔧 Visual Studio Code Instructions

1. **Open VS Code in project folder:**
   ```powershell
   cd C:\WORK\repos\ClientNotifier
   code .
   ```

2. **Open Terminal** (Ctrl+`)

3. **Navigate to API:**
   ```powershell
   cd ClientNotifier.API
   ```

4. **Run the API:**
   ```powershell
   dotnet run
   ```

5. **Or use VS Code Run:**
   - Press `F5`
   - Select `.NET Core` if prompted
   - Choose `ClientNotifier.API`

---

## ✅ Verify It's Working

1. **Check Console Output:**
   - Should see "Database migrations applied successfully"
   - Should see "Now listening on: http://localhost:5000"

2. **Test Swagger:**
   - Open: http://localhost:5000/swagger
   - You should see API documentation

3. **Test an Endpoint:**
   ```powershell
   Invoke-RestMethod -Uri "http://localhost:5000/api/namedaymappings/today" -Method Get
   ```

---

## 🛑 Stopping the API

### From Command Line:
- Press `Ctrl+C` in the terminal

### From Visual Studio:
- Press `Shift+F5` or click Stop button

### From VS Code:
- Press `Shift+F5` or click Stop in debug toolbar

---

## 🐛 Troubleshooting

### Port Already in Use
If you see "Address already in use":
```powershell
# Find what's using port 5000
netstat -ano | findstr :5000

# Kill the process (replace PID with actual number)
taskkill /F /PID [PID]
```

### IIS Express Issues
If IIS Express is causing problems:
```powershell
# Stop all IIS Express instances
taskkill /F /IM iisexpress.exe
```

### Database Issues
If database errors occur:
1. Delete the database file:
   ```powershell
   Remove-Item C:\WORK\repos\ClientNotifier\ClientNotifier.API\Data\ClientNotifier.db
   ```
2. Run the API again - it will recreate the database

---

## 📝 Configuration

### Change Port
Edit `Properties\launchSettings.json`:
```json
"applicationUrl": "http://localhost:5000"
```

### Production Mode
```powershell
dotnet run --environment Production
```

### Custom Settings
Edit `appsettings.json` or `appsettings.Development.json`

---

## 🎉 What's Available

Once running, you can:
- **View API Docs:** http://localhost:5000/swagger
- **Manage People:** CRUD operations for clients
- **Manage Namedays:** CRUD operations for nameday mappings
- **Query Celebrations:** Find today's birthdays/namedays
- **Import Data:** Bulk import nameday mappings

---

## 💡 Tips

1. **First Time Running:**
   - Database is created automatically
   - Test data is added in Development mode
   - Bulgarian namedays are loaded from JSON

2. **Daily Use:**
   - Just run `dotnet run` from API folder
   - Or press F5 in Visual Studio
   - Swagger opens automatically

3. **For Your Mom's Business:**
   - Use the batch files in production:
     - `SETUP.bat` - First time setup
     - `START_APP.bat` - Daily use
