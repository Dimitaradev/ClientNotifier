# Admin Interface Guide - ClientNotifier

## 🎯 Complete Admin UI Features

The admin interface is now complete with all the functionality you need to manage clients!

### 📍 Access the Admin Interface

1. **Start the API:**
   ```powershell
   cd C:\WORK\repos\ClientNotifier\ClientNotifier.API
   dotnet run
   ```

2. **Open Admin Page:**
   - Go to: http://localhost:5000/admin.html
   - Or start from http://localhost:5000 and click "Администрация"

## 🛠️ Available Features

### 1. Dashboard (Табло)
- **Total client count**
- **Today's birthdays** with names
- **Today's namedays** with names  
- **This week's celebrations** count
- **Real-time alerts** for today's celebrations

### 2. Client Management (Клиенти)
- **Search** - by name, EGN, email, or phone
- **Add new client** - with automatic birthday from EGN
- **Edit client** - update all information
- **Delete client** - with confirmation
- **View celebrations** - see who has birthday/nameday today

### 3. Birthdays View (Рождени дни)
- **Filter options:**
  - Today only
  - This week
  - This month
  - All birthdays
- **Shows age** for each person
- **Contact info** displayed inline

### 4. Namedays View (Имени дни)
- **Filter options:**
  - Today only
  - This week
  - This month
  - All namedays
- **Automatic assignment** based on first name
- **Contact info** for quick access

### 5. Upcoming Celebrations (Предстоящи)
- **Choose period:**
  - Next 7 days
  - Next 14 days
  - Next 30 days
- **Grouped by date**
- **Shows both** birthdays and namedays
- **Today highlighted** in green

## 💡 How to Use

### Adding a New Client:
1. Go to "Клиенти" tab
2. Click "➕ Добави клиент"
3. Fill in the form:
   - **Name** (required)
   - **EGN** (required) - birthday calculated automatically
   - **Email/Phone** (optional)
   - **Notes** (optional)
   - **Enable notifications** checkbox
4. Click "Запази"

### Editing a Client:
1. Find the client in the list
2. Click the ✏️ button
3. Update information
4. Click "Запази"

### Searching:
- Type in the search box
- Searches in: name, EGN, email, phone
- Results update as you type

### Viewing Celebrations:
- **Dashboard** - quick overview
- **Birthdays/Namedays** tabs - detailed lists
- **Upcoming** tab - see what's coming

## 🎨 Visual Indicators

- 🎂 = Has birthday today
- 🎊 = Has nameday today
- **Green highlight** = Today's celebrations
- **Green checkmark** = Notifications enabled
- **Red X** = Notifications disabled

## 📱 Mobile Friendly

The interface is responsive and works on:
- Desktop computers
- Tablets
- Smartphones

## 🔄 Real-time Updates

- Dashboard refreshes automatically
- Manual refresh buttons available
- Changes appear immediately

## 🚀 Performance

- Fast search and filtering
- Handles hundreds of clients
- Instant updates
- No page reloads needed

## 📝 Next Steps

Now that you have a complete UI, you can:
1. Import existing clients via Excel
2. Manually add/edit clients
3. Monitor daily celebrations
4. Plan ahead with upcoming views

The system is ready for daily use!
