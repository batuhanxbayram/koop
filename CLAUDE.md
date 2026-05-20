*# Koop — S.S. 75 No'lu Yarımca Motorlu Taşıyıcılar Kooperatifi*

*## Proje Özeti (Claude için Bağlam Dosyası)*

&#x20;

*---*

&#x20;

*## Teknoloji Yığını*

&#x20;

*| Katman | Teknoloji |*

*|---|---|*

*| Backend | .NET 8, ASP.NET Core Web API, EF Core 8, SignalR, SQL Server |*

*| Auth | ASP.NET Core Identity + JWT Bearer |*

*| Frontend | React 18, Vite, Material Tailwind, Axios, SignalR JS Client |*

*| DevOps | Docker, Nginx (reverse proxy), GitHub Actions (CI/CD) |*

*| Canlı URL | https://75ymkt.com (frontend) / https://75ymkt.com/api (backend) |*

&#x20;

*---*

&#x20;

*## Çözüm Yapısı*

&#x20;

*```*

*Koop.sln*

*├── WebApi/WebApi/          → ASP.NET Core Web API (entry point)*

*├── Koop.Data/              → EF Core DbContext, Migrations, Repository*

*├── Koop.Entity/            → Entity sınıfları + DTO'lar*

*├── Koop.Service/           → TokenService, JWT ayarları*

*├── Koop.Core/              → (Henüz boş)*

*└── src/                    → React frontend (Vite)*

*```*

&#x20;

*---*

&#x20;

*## Veritabanı Şeması*

&#x20;

*### Tablolar*

*- \*\*AspNetUsers\*\* (AppUser) — Identity + `FullName`, `RefreshToken`, `RefreshTokenExpireTime`*

*- \*\*AspNetRoles\*\* (AppRole) — Identity, roller: `Admin`, `User`*

*- \*\*Vehicles\*\* — `Id (bigint PK)`, `AppUserId (FK, nullable)`, `LicensePlate (nvarchar 20)`, `DriverName (nvarchar 150)`, `IsActive (bit)`*

*- \*\*Routes\*\* — `Id (bigint PK)`, `RouteName (nvarchar 100, UNIQUE)`, `IsActive (bit)`*

*- \*\*RouteVehicleQueues\*\* — `Id (bigint PK)`, `RouteId (FK)`, `VehicleId (FK)`, `QueueTimestamp (datetime2)` — (RouteId, VehicleId) UNIQUE index*

*### İlişkiler*

*- AppUser → Vehicles: 1'e çok (AppUser'ın araçları)*

*- Route → RouteVehicleQueues: 1'e çok (cascade delete)*

*- Vehicle → RouteVehicleQueues: 1'e çok (cascade delete)*

*- Sıra = `QueueTimestamp` alanına göre ASC sıralama*

*---*

&#x20;

*## API Endpointleri*

&#x20;

*### Auth — `/api/Auth`*

*| Method | Endpoint | Açıklama |*

*|---|---|---|*

*| POST | `/Auth/Login` | `{username, password}` → JWT token string döner |*

&#x20;

*### Users — `/api/Users`*

*| Method | Endpoint | Auth | Açıklama |*

*|---|---|---|---|*

*| GET | `/Users` | - | Tüm kullanıcılar (UserVehicleDto: id, fullName, licensePlate, phoneNumber) |*

*| POST | `/Users` | - | Yeni kullanıcı oluştur (CreateUserDto), varsayılan rol: User |*

*| PUT | `/Users/{id}` | Admin | fullName ve opsiyonel password güncelle |*

*| DELETE | `/Users/{id}` | Admin | Kullanıcı sil (araçlardan AppUserId null yapılır) |*

*| GET | `/Users/me` | Auth | Token'dan mevcut kullanıcı bilgisi + araç plakası |*

*| POST | `/Users/change-my-password` | Auth | Kendi şifresini değiştir (ChangeMyPasswordDto) |*

*| GET | `/Users/with-roles` | Admin | Kullanıcılar + rolleri |*

*| GET | `/Users/without-vehicle` | - | Araçsız kullanıcı listesi |*

&#x20;

*### Admin Routes — `/api/admin/routes` (Requires Admin)*

*| Method | Endpoint | Açıklama |*

*|---|---|---|*

*| GET | `/admin/routes` | Tüm güzergahlar |*

*| POST | `/admin/routes` | Yeni güzergah (CreateRouteDto: routeName) |*

*| GET | `/admin/routes/{id}` | Tek güzergah |*

*| PUT | `/admin/routes/{id}` | Güzergah güncelle (UpdateRouteDto: routeName, isActive) |*

*| PATCH | `/admin/routes/{id}/set-active` | IsActive toggle |*

*| DELETE | `/admin/routes/{id}` | Güzergah sil |*

&#x20;

*### Admin Vehicles — `/api/admin/vehicles` (Requires Admin)*

*| Method | Endpoint | Açıklama |*

*|---|---|---|*

*| GET | `/admin/vehicles` | Tüm araçlar (VehicleDto) |*

*| POST | `/admin/vehicles` | Yeni araç (CreateVehicleDto: licensePlate, appUserId?) |*

*| GET | `/admin/vehicles/{id}` | Tek araç |*

*| PUT | `/admin/vehicles/{id}` | Araç güncelle (UpdateVehicleDto) |*

*| PATCH | `/admin/vehicles/{id}/assign-user` | Kullanıcı ata (appUserId?) |*

*| PATCH | `/admin/vehicles/{id}/set-active` | IsActive toggle |*

*| DELETE | `/admin/vehicles/{id}` | Araç sil |*

*| GET | `/admin/vehicles/idle-warnings?days=7` | Hareketsiz araçlar |*

*| POST | `/admin/vehicles/{routeId}/move-first-to-end` | İlk aracı sona gönder |*

*| POST | `/admin/vehicles/move-to-end` | Araç sona gönder (routeId query + MoveVehicleDto) |*

*| POST | `/admin/vehicles/move-to-front` | Araç öne al (routeId query + MoveVehicleDto) |*

&#x20;

*### Queue — `/api/routes/{routeId}/queue`*

*| Method | Endpoint | Auth | Açıklama |*

*|---|---|---|---|*

*| GET | `/queues/all` | Auth | Tüm aktif güzergahlar + sıradaki araçlar |*

*| GET | `/routes/{routeId}/queue` | Auth | Belirli güzergahın sırası |*

*| POST | `/routes/{routeId}/queue` | Admin | Sıraya araç ekle (AddVehicleToQueueDto: vehicleId) |*

*| DELETE | `/routes/{routeId}/queue/{vehicleId}` | Auth | Sıradan araç çıkar |*

*| POST | `/routes/{routeId}/queue/reorder` | Admin | Sıra yeniden düzenle (ReorderQueueDto: orderedVehicleIds\[]) |*

*| POST | `/routes/{routeId}/queue/move-to-end` | Admin | Araç sona gönder (MoveVehicleDto: vehicleId) |*

&#x20;

*### SignalR Hub*

*- \*\*URL:\*\* `/hubs/queue`*

*- \*\*Event (Server→Client):\*\* `ReceiveQueueUpdate` — herhangi bir sıra değişikliğinde tetiklenir*

*- Tüm araç/güzergah/sıra değiştirme işlemleri bu eventi yayınlar*

*---*

&#x20;

*## Önemli DTOs*

&#x20;

*```csharp*

*// Auth*

*LoginDto: { Username, Password }*

*TokenResponseDto: { AccessToken, AccessTokenExpiration, RefreshToken }*

&#x20;

*// User*

*CreateUserDto: { UserName, FullName, Password, ConfirmPassword, PhoneNumber?, Roles? }*

*UpdateUserDto: { FullName, Password?, ConfirmPassword? }*

*UserVehicleDto: { Id(Guid), FullName, LicensePlate, PhoneNumber }*

*ChangeMyPasswordDto: { CurrentPassword, NewPassword, ConfirmNewPassword }*

&#x20;

*// Vehicle*

*CreateVehicleDto: { AppUserId?(Guid), LicensePlate, DriverName? }*

*UpdateVehicleDto: { AppUserId?, LicensePlate, DriverName?, IsActive }*

*VehicleDto: { Id(long), LicensePlate, DriverName?, PhoneNumber?, IsActive, AppUserId?, UserFullName }*

&#x20;

*// Route*

*CreateRouteDto: { RouteName }*

*UpdateRouteDto: { RouteName, IsActive }*

*RouteWithQueueDto: { RouteId, RouteName, QueuedVehicles\[] }*

*SetActiveDto: { IsActive }*

*ReorderQueueDto: { OrderedVehicleIds: long\[] }*

*MoveVehicleDto: { VehicleId }*

*AddVehicleToQueueDto: { VehicleId }*

*```*

&#x20;

*---*

&#x20;

*## Frontend Yapısı*

&#x20;

*### Sayfalar (`src/pages/dashboard/`)*

*| Dosya | Route | Rol | Açıklama |*

*|---|---|---|---|*

*| `home.jsx` | `/anasayfa/arac-siralari` | Hepsi | Tüm güzergah sıralarını gösterir, SignalR ile canlı |*

*| `queuemanagementpage.jsx` | `/anasayfa/sira-yonetimi` | Admin | Sürükle-bırak sıra yönetimi, araç ekle/çıkar |*

*| `dispatch.jsx` | `/anasayfa/ozel-gorev` | Admin | Güzergah bazlı araç "Gönder" işlemi |*

*| `DispatchDetailPage.jsx` | `/anasayfa/ozel-gorev/:routeId` | Admin | Tek güzergah detay, plaka arama, sona gönder |*

*| `profile.jsx` | `/anasayfa/kullanicilar` | Admin | Kullanıcı CRUD |*

*| `tables.jsx` | `/anasayfa/araclar` | Admin | Araç CRUD, kullanıcı atama, idle uyarıları |*

*| `notifications.jsx` | `/anasayfa/guzergahlar` | Admin | Güzergah CRUD |*

*| `myprofile.jsx` | `/anasayfa/profilim` | User | Kendi bilgileri, sıra durumu, şifre değiştir |*

*| `TVQueuePage.jsx` | `/tv/monitor?routeId=X` | Public | TV/ekran görünümü, SignalR canlı |*

&#x20;

*### Önemli Bileşenler (`src/widgets/layout/`)*

*- `sidenav.jsx` — Role göre menü filtreleme (userRole: admin/user)*

*- `dashboard-navbar.jsx` — Breadcrumb, logout*

*- `AddVehicleModal`, `EditVehicleModal`, `AssignUserModal` — Araç modalleri*

*- `AddUserModal`, `EditUserModal` — Kullanıcı modalleri*

*- `AddRouteModal`, `EditRouteModal` — Güzergah modalleri*

*- `VehicleQueueCard.jsx` — Sıradaki araç kartı (ilk 3 yeşil)*

*### State Yönetimi*

*- Context: `src/context/index.jsx` — MaterialTailwindControllerProvider*

*- State: `openSidenav`, `sidenavColor/Type`, `fixedNavbar`, `userRole`*

*- `userRole` localStorage'dan okunur/yazılır (`admin` veya `user`)*

*### Auth Akışı*

*1. `/auth/giris` → `POST /Auth/Login` → JWT token*

*2. Token `localStorage.authToken`'a kaydedilir*

*3. `jwtDecode` ile rol çözülür, `localStorage.userRole`'e yazılır*

*4. `PrivateRoute` token yoksa `/auth/giris`'e yönlendirir*

*5. Axios interceptor: 401 gelirse localStorage temizlenir, login'e yönlendirilir*

*---*

&#x20;

*## Konfigürasyon*

&#x20;

*### appsettings.json (production'da dolu olmalı)*

*```json*

*{*

&#x20; *"ConnectionStrings": { "DefaultConnection": "..." },*

&#x20; *"JWT": {*

&#x20;   *"Audience": "...", "Issuer": "...", "Secret": "...",*

&#x20;   *"TokenValidityMinutes": 120, "RefleshTokenValidityDays": 7*

&#x20; *}*

*}*

*```*

&#x20;

*### Nginx (nginx.conf)*

*- `/api/\\\*` → `http://72.62.114.221:5000` (proxy\_pass)*

*- `/hubs/\\\*` → WebSocket upgrade ile aynı backend*

*- Frontend: React SPA, `try\\\_files $uri /index.html`*

*### CORS (Program.cs)*

*İzin verilen originler: `localhost:5173`, `localhost:5174`, `72.62.114.221`, `75ymkt.com`*

&#x20;

*---*

&#x20;

*## Otomatik Seed (Program.cs)*

*Uygulama başlarken:*

*1. `MigrateAsync()` çalışır*

*2. `Admin` ve `User` rolleri oluşturulur (yoksa)*

*3. `admin` kullanıcısı oluşturulur (username: `admin`, şifre: ``, rol: Admin)*

*---*

&#x20;

*## Bilinen Eksikler / Notlar*

*- `Koop.Core` projesi boş*

*- `Repository` pattern başlatılmış ama henüz kullanılmıyor (controller'lar direkt DbContext kullanıyor)*

*- `\\\[Obsolete]` işareti `LoadServiceLayerExtension`'da var (Assembly.GetExecutingAssembly kullanımından)*

*- Refresh token endpoint'i yok (model var ama controller implement edilmemiş)*

*- `RouteVehicleQueue`'da `QueueTimestamp` UTC, frontend'de sıralama buna göre*

