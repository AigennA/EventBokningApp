# Bokningssystem – Fullstack Webbapplikation

Grupparbete i kursen Fullstack-applikation. Ett komplett eventbokningssystem byggt med **ASP.NET Core Web API** och **Blazor Server**, med JWT-autentisering, lagerarkitektur och premium UI.

---

## Teknikstack

| Lager | Teknologi |
|---|---|
| Backend API | ASP.NET Core 8 Web API |
| Frontend | Blazor Server |
| Databas | SQLite + Entity Framework Core 8 |
| Autentisering | JWT Bearer Tokens + BCrypt |
| Dokumentation | Swagger / OpenAPI |
| Tester | xUnit + InMemory EF |
| UI | Custom CSS (Inter font, Orange/Black/Green) |

---

## Arkitektur

```
Solution/
├── EventBokningApp/              # Huvud-projekt (API + Blazor Server)
│   ├── Controllers/              # REST API endpoints
│   │   ├── AuthController.cs     # POST /api/auth/login, /register
│   │   ├── BookingsController.cs # CRUD bokningar (JWT-skyddad)
│   │   ├── EventsController.cs   # CRUD events
│   │   └── VenuesController.cs   # CRUD venues
│   ├── Services/                 # Affärslogik
│   │   ├── BookingService.cs     # Bokningsregler
│   │   ├── AuthService.cs        # JWT-generering, lösenordshashning
│   │   └── BlazorAuthService.cs  # In-circuit auth state för Blazor
│   ├── Repositories/             # Dataaccesslager
│   │   ├── BookingRepository.cs
│   │   ├── EventRepository.cs
│   │   ├── VenueRepository.cs
│   │   └── UserRepository.cs
│   ├── Models/                   # Domänmodeller
│   │   ├── Booking.cs, Event.cs, Ticket.cs, Venue.cs, User.cs
│   ├── DTOs/                     # Data Transfer Objects
│   ├── Exceptions/               # BusinessException, NotFoundException
│   ├── Data/                     # AppDbContext + DatabaseSeeder
│   └── Components/               # Blazor UI
│       ├── Pages/                # 6 sidor
│       │   ├── Home.razor        # Startsida med hero och stats
│       │   ├── Events.razor      # Eventlista med filter
│       │   ├── EventDetail.razor # Eventdetalj + bokning
│       │   ├── Venues.razor      # Venuelista
│       │   ├── Login.razor       # Inloggningssida
│       │   ├── Register.razor    # Registreringssida
│       │   ├── MyBookings.razor  # Mina bokningar (inloggad)
│       │   └── Admin.razor       # Admin-panel (admin-roll)
│       ├── Shared/               # Återanvändbara komponenter
│       │   ├── EventCard.razor
│       │   ├── VenueCard.razor
│       │   └── BookingStatusBadge.razor
│       └── Layout/
│           └── MainLayout.razor  # Navbar + footer
└── EventBokningApp.Tests/        # Enhetstestprojekt
    ├── BookingServiceTests.cs    # 12 tester för BookingService
    └── RepositoryTests.cs        # Repository-tester
```

---

## Körinstruktioner

### Krav
- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- Git

### 1. Klona projektet
```bash
git clone <repository-url>
cd EventBokningApp
```

### 2. Kör applikationen
```bash
dotnet run
```

Applikationen startar på `http://localhost:5000`.
Databasen skapas och seedas automatiskt vid första körning.

### 3. Kör enhetstesterna
```bash
cd ../EventBokningApp.Tests
dotnet test
```

### 4. API-dokumentation
Öppna Swagger UI:
```
http://localhost:5000/swagger
```

---

## Demo-inloggning

| Roll | E-post | Lösenord |
|---|---|---|
| Admin | admin@bokningssystem.se | Admin@123 |

---

## API Endpoints

### Auth (öppen)
| Metod | Endpoint | Beskrivning |
|---|---|---|
| POST | `/api/auth/register` | Registrera ny användare |
| POST | `/api/auth/login` | Logga in, returnerar JWT |

### Events (öppen läsning, Admin för skriv)
| Metod | Endpoint | Beskrivning |
|---|---|---|
| GET | `/api/events` | Alla events |
| GET | `/api/events/upcoming` | Kommande events |
| GET | `/api/events/{id}` | Event med biljetter |
| POST | `/api/events` | Skapa event (Admin) |
| PUT | `/api/events/{id}` | Uppdatera event (Admin) |
| DELETE | `/api/events/{id}` | Ta bort event (Admin) |
| GET | `/api/events/{id}/bookings` | Bokningar för event (Admin) |

### Venues (öppen läsning, Admin för skriv)
| Metod | Endpoint | Beskrivning |
|---|---|---|
| GET | `/api/venues` | Alla venues |
| GET | `/api/venues/{id}` | Specifik venue |
| POST | `/api/venues` | Skapa venue (Admin) |
| PUT | `/api/venues/{id}` | Uppdatera venue (Admin) |
| DELETE | `/api/venues/{id}` | Ta bort venue (Admin) |

### Bookings (JWT-skyddat)
| Metod | Endpoint | Beskrivning |
|---|---|---|
| POST | `/api/bookings` | Skapa bokning (inloggad) |
| GET | `/api/bookings/{id}` | Hämta bokning (inloggad) |
| GET | `/api/bookings` | Alla bokningar (Admin) |
| DELETE | `/api/bookings/{id}` | Avboka (inloggad) |

---

## Tekniska krav – uppfyllda

### Backend
- [x] RESTful endpoints
- [x] Lagerarkitektur: Controller → Service → Repository
- [x] Dependency Injection för alla tjänster
- [x] Data Annotations på alla DTOs
- [x] JWT-autentisering (Bearer token)
- [x] Skyddade endpoints med `[Authorize]` och `[Authorize(Roles = "Admin")]`
- [x] Swagger-dokumentation med Bearer auth

### Frontend (Blazor Server)
- [x] 8 sidor/vyer (Home, Events, EventDetail, Venues, Login, Register, MyBookings, Admin)
- [x] 3 egna komponenter (EventCard, VenueCard, BookingStatusBadge)
- [x] Formulär med validering och DataAnnotationsValidator
- [x] Navigation med NavLink
- [x] Login/Logout flöde
- [x] Skyddade sidor (kontrolleras manuellt via BlazorAuthService)
- [x] Roll-baserad visning (Admin-sidor, AuthorizeView-liknande logik)

### Kod & Arkitektur
- [x] Clean Code-principer
- [x] SOLID (SRP, OCP, LSP, ISP, DIP)
- [x] 20+ enhetstester (xUnit)
- [x] Meningsfulla Git-commits
- [x] README med fullständig dokumentation

---

## Ansvarsfördelning (exempelstruktur)

| Funktion | Ansvarig |
|---|---|
| Backend API, Controllers, Services | Gruppmedlem A |
| Repository-lagret, EF Core | Gruppmedlem B |
| Blazor Frontend, UI | Gruppmedlem C |
| JWT Auth, tester | Gruppmedlem D |
| Design, CSS, README | Alla |
