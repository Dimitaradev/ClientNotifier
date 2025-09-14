# ClientNotifier API Documentation

## Base URL
- Development: `http://localhost:5000/api`
- Production: `http://localhost:5000/api`

## API Endpoints

### People Controller

#### 1. Get All People
- **GET** `/api/people`
- **Query Parameters:**
  - `search` (optional): Search by name, EGN, email, or phone
  - `birthdayToday` (optional): Filter people with birthday today
  - `namedayToday` (optional): Filter people with nameday today
  - `celebrationsThisWeek` (optional): Filter people with celebrations this week
- **Response:** Array of `PersonListDto`

#### 2. Get Person by ID
- **GET** `/api/people/{id}`
- **Response:** `PersonDto`

#### 3. Create Person
- **POST** `/api/people`
- **Body:** `CreatePersonDto`
  ```json
  {
    "firstName": "Иван",
    "lastName": "Иванов",
    "egn": "8001014509",
    "email": "ivan@example.com",
    "phoneNumber": "+359888123456",
    "notes": "VIP client",
    "notificationsEnabled": true
  }
  ```
- **Response:** Created `PersonDto`

#### 4. Update Person
- **PUT** `/api/people/{id}`
- **Body:** `UpdatePersonDto` (same as Create + id)
- **Response:** Updated `PersonDto`

#### 5. Delete Person
- **DELETE** `/api/people/{id}`
- **Response:** 204 No Content

#### 6. Get Upcoming Celebrations
- **GET** `/api/people/upcoming-celebrations?days=7`
- **Query Parameters:**
  - `days` (optional, default=7): Number of days to look ahead (1-365)
- **Response:** Array of people with upcoming birthdays/namedays

### NamedayMappings Controller

#### 1. Get All Nameday Mappings
- **GET** `/api/namedaymappings`
- **Query Parameters:**
  - `name` (optional): Filter by name
  - `month` (optional): Filter by month (1-12)
  - `day` (optional): Filter by day (1-31)
- **Response:** Array of `NamedayMappingDto`

#### 2. Get Nameday Mapping by ID
- **GET** `/api/namedaymappings/{id}`
- **Response:** `NamedayMappingDto`

#### 3. Create Nameday Mapping
- **POST** `/api/namedaymappings`
- **Body:** `CreateNamedayMappingDto`
  ```json
  {
    "name": "Николай",
    "month": 12,
    "day": 6
  }
  ```
- **Response:** Created `NamedayMappingDto`

#### 4. Update Nameday Mapping
- **PUT** `/api/namedaymappings/{id}`
- **Body:** `UpdateNamedayMappingDto`
- **Response:** Updated `NamedayMappingDto`

#### 5. Delete Nameday Mapping
- **DELETE** `/api/namedaymappings/{id}`
- **Response:** 204 No Content

#### 6. Get Namedays Grouped by Date
- **GET** `/api/namedaymappings/grouped-by-date?month=12`
- **Query Parameters:**
  - `month` (optional): Filter by specific month
- **Response:** Array of `NamedayGroupDto` with names grouped by date

#### 7. Get Today's Namedays
- **GET** `/api/namedaymappings/today`
- **Response:** `NamedayGroupDto` with today's namedays and count of people

#### 8. Import Namedays
- **POST** `/api/namedaymappings/import`
- **Body:** Array of `CreateNamedayMappingDto`
- **Response:** Import statistics (imported, skipped, errors)

## Data Models

### PersonDto
```json
{
  "id": 1,
  "firstName": "Иван",
  "lastName": "Иванов",
  "fullName": "Иван Иванов",
  "egn": "8001014509",
  "birthday": "1980-01-01T00:00:00",
  "nameday": "2025-01-07T00:00:00",
  "email": "ivan@example.com",
  "phoneNumber": "+359888123456",
  "notes": "VIP client",
  "notificationsEnabled": true,
  "createdAt": "2025-09-14T12:00:00Z",
  "updatedAt": null,
  "age": 45,
  "namedayFormatted": "07 януари",
  "birthdayFormatted": "01 януари 1980"
}
```

### PersonListDto
Includes all PersonDto fields plus:
- `hasBirthdayToday`: boolean
- `hasNamedayToday`: boolean
- `hasBirthdayThisWeek`: boolean
- `hasNamedayThisWeek`: boolean

### NamedayMappingDto
```json
{
  "id": 1,
  "name": "Иван",
  "month": 1,
  "day": 7,
  "dateDisplay": "07/01",
  "nextOccurrence": "2026-01-07T00:00:00"
}
```

### NamedayGroupDto
```json
{
  "month": 12,
  "day": 6,
  "dateDisplay": "06/12",
  "names": ["Никола", "Николай", "Николина"],
  "peopleCount": 5
}
```

## Features

1. **Automatic Birthday Extraction**: Birthday is automatically calculated from the Bulgarian EGN
2. **EGN Validation**: Full validation including checksum verification
3. **Automatic Nameday Assignment**: When creating/updating a person, their nameday is automatically assigned based on their first name
4. **Smart Search**: Search across multiple fields (name, EGN, email, phone)
5. **Celebration Tracking**: Easy filtering for today's and upcoming celebrations
6. **Bulk Import**: Import multiple nameday mappings at once

## Error Handling

All endpoints return appropriate HTTP status codes:
- `200 OK`: Successful GET requests
- `201 Created`: Successful POST requests
- `204 No Content`: Successful DELETE requests
- `400 Bad Request`: Invalid input data
- `404 Not Found`: Resource not found
- `409 Conflict`: Duplicate resource (e.g., duplicate EGN)
- `500 Internal Server Error`: Server errors

## CORS

CORS is enabled in development mode to allow frontend applications from any origin.

## Swagger Documentation

When running in development mode, Swagger UI is available at:
- `http://localhost:5000/swagger`

This provides an interactive interface to test all API endpoints.
