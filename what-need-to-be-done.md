# Web Matcha - Vertical Slice Architecture Plan

## 🏗️ ARCHITECTURE CHOISIE
- **Vertical Slice Architecture** (sans MediatR pour gagner du temps)
- **FastEndpoints** (plus rapide que controllers)
- **Organisation par Feature** (plus logique pour ce projet)
- **Entity Framework** avec PostgreSQL
- **SignalR** pour temps réel

---

## 📁 STRUCTURE DU PROJET

```
WebMatcha/
├── Program.cs
├── AppSettings.json
├── .env
├── docker-compose.yml
├── Dockerfile
│
├── Common/
│   ├── Database/
│   │   ├── MatchaDbContext.cs
│   │   └── Entities/
│   │       ├── User.cs
│   │       ├── UserPhoto.cs
│   │       ├── Like.cs
│   │       ├── Message.cs
│   │       └── ...
│   ├── Services/
│   │   ├── EmailService.cs
│   │   ├── TokenService.cs
│   │   └── FileUploadService.cs
│   └── Security/
│       ├── PasswordHasher.cs
│       └── CurrentUser.cs
│
├── Features/
│   ├── Auth/
│   │   ├── Register/
│   │   │   ├── RegisterEndpoint.cs
│   │   │   ├── RegisterRequest.cs
│   │   │   ├── RegisterValidator.cs
│   │   │   └── RegisterPage.razor
│   │   ├── Login/
│   │   │   ├── LoginEndpoint.cs
│   │   │   ├── LoginRequest.cs
│   │   │   └── LoginPage.razor
│   │   ├── VerifyEmail/
│   │   ├── ResetPassword/
│   │   └── Logout/
│   │
│   ├── Profile/
│   │   ├── ViewProfile/
│   │   ├── EditProfile/
│   │   ├── UploadPhoto/
│   │   ├── UpdateLocation/
│   │   └── ManageTags/
│   │
│   ├── Browsing/
│   │   ├── GetSuggestions/
│   │   ├── SearchProfiles/
│   │   ├── FilterProfiles/
│   │   └── Components/
│   │       └── ProfileCard.razor
│   │
│   ├── Matching/
│   │   ├── LikeProfile/
│   │   ├── UnlikeProfile/
│   │   ├── GetMatches/
│   │   ├── BlockUser/
│   │   └── ReportUser/
│   │
│   ├── Chat/
│   │   ├── SendMessage/
│   │   ├── GetConversations/
│   │   ├── GetMessages/
│   │   └── Hubs/
│   │       └── ChatHub.cs
│   │
│   └── Notifications/
│       ├── GetNotifications/
│       ├── MarkAsRead/
│       └── Hubs/
│           └── NotificationHub.cs
│
├── Pages/
│   └── _Host.cshtml
│
└── wwwroot/
    ├── css/
    └── uploads/
```

---

## 🚀 ÉTAPES D'IMPLÉMENTATION PAR FEATURE

## ÉTAPE 1: SETUP INITIAL (30 min)

### 1.1 Créer le projet
```bash
dotnet new blazor -n WebMatcha --interactivity Server
cd WebMatcha
```

### 1.2 Installer les packages
```bash
# Core packages
dotnet add package FastEndpoints
dotnet add package FastEndpoints.Security
dotnet add package Npgsql.EntityFrameworkCore.PostgreSQL
dotnet add package EFCore.NamingConventions

# Utils
dotnet add package BCrypt.Net-Next
dotnet add package MailKit
dotnet add package FluentValidation
```

### 1.3 Créer la structure de dossiers
- Créer tous les dossiers Features/, Common/, etc.
- Créer docker-compose.yml et .env

### 1.4 Configurer Program.cs
- Ajouter FastEndpoints
- Configurer DbContext
- Ajouter Authentication JWT
- Configurer SignalR

---

## ÉTAPE 2: DATABASE & COMMON (1h)

### 2.1 Common/Database/Entities/
Créer toutes les entités :
- User.cs (avec toutes les propriétés)
- UserPhoto.cs
- Like.cs
- Message.cs
- ProfileView.cs
- Notification.cs
- Tag.cs
- UserTag.cs
- Block.cs
- Report.cs

### 2.2 Common/Database/MatchaDbContext.cs
- Configurer toutes les relations
- Ajouter les index
- Créer la migration initiale

### 2.3 Common/Services/
- EmailService.cs (MailKit)
- TokenService.cs (JWT)
- FileUploadService.cs
- PasswordHasher.cs (BCrypt)

### 2.4 Data Seeding
- Créer SeedData.cs
- Générer 500+ profils automatiquement

---

## ÉTAPE 3: FEATURE AUTH (1h30)

### 3.1 Features/Auth/Register/
```
RegisterEndpoint.cs     → POST /api/auth/register
RegisterRequest.cs       → DTO avec validation
RegisterValidator.cs     → FluentValidation rules
RegisterPage.razor       → UI Blazor
```

### 3.2 Features/Auth/Login/
```
LoginEndpoint.cs        → POST /api/auth/login
LoginRequest.cs         → Username + Password
LoginPage.razor         → UI avec cookie/JWT
```

### 3.3 Features/Auth/VerifyEmail/
```
VerifyEmailEndpoint.cs  → GET /api/auth/verify?token=xxx
```

### 3.4 Features/Auth/ResetPassword/
```
RequestResetEndpoint.cs → POST /api/auth/request-reset
ResetPasswordEndpoint.cs → POST /api/auth/reset
```

---

## ÉTAPE 4: FEATURE PROFILE (1h30)

### 4.1 Features/Profile/EditProfile/
```
EditProfileEndpoint.cs  → PUT /api/profile
EditProfileRequest.cs   → Tous les champs du profil
EditProfilePage.razor   → Formulaire complet
```

### 4.2 Features/Profile/UploadPhoto/
```
UploadPhotoEndpoint.cs  → POST /api/profile/photo
SetProfilePhotoEndpoint.cs → PUT /api/profile/photo/{id}/main
DeletePhotoEndpoint.cs  → DELETE /api/profile/photo/{id}
```

### 4.3 Features/Profile/UpdateLocation/
```
UpdateLocationEndpoint.cs → PUT /api/profile/location
LocationComponent.razor   → GPS ou IP fallback
```

### 4.4 Features/Profile/ManageTags/
```
AddTagEndpoint.cs       → POST /api/profile/tags
RemoveTagEndpoint.cs    → DELETE /api/profile/tags/{id}
GetTagsEndpoint.cs      → GET /api/tags (autocomplete)
```

### 4.5 Features/Profile/ViewProfile/
```
GetProfileEndpoint.cs   → GET /api/profile/{username}
ProfilePage.razor       → Affichage complet
RecordViewEndpoint.cs   → POST /api/profile/{id}/view
```

---

## ÉTAPE 5: FEATURE BROWSING (1h30)

### 5.1 Features/Browsing/GetSuggestions/
```
GetSuggestionsEndpoint.cs → GET /api/browse/suggestions
SuggestionsAlgorithm.cs   → Logique de matching
BrowsePage.razor          → Liste de ProfileCards
```

### 5.2 Features/Browsing/SearchProfiles/
```
SearchEndpoint.cs        → POST /api/browse/search
SearchRequest.cs         → Critères multiples
SearchPage.razor         → Formulaire avancé
```

### 5.3 Features/Browsing/FilterProfiles/
```
FilterHelper.cs          → Logique de filtrage
SortHelper.cs            → Logique de tri
```

---

## ÉTAPE 6: FEATURE MATCHING (1h)

### 6.1 Features/Matching/LikeProfile/
```
LikeEndpoint.cs         → POST /api/match/like/{userId}
CheckMatchLogic.cs      → Vérifier si match mutuel
```

### 6.2 Features/Matching/UnlikeProfile/
```
UnlikeEndpoint.cs       → DELETE /api/match/like/{userId}
```

### 6.3 Features/Matching/GetMatches/
```
GetMatchesEndpoint.cs   → GET /api/match/matches
MatchesPage.razor       → Liste des matchs
```

### 6.4 Features/Matching/BlockUser/
```
BlockUserEndpoint.cs    → POST /api/match/block/{userId}
```

### 6.5 Features/Matching/ReportUser/
```
ReportUserEndpoint.cs   → POST /api/match/report/{userId}
```

---

## ÉTAPE 7: FEATURE CHAT (1h30)

### 7.1 Features/Chat/Hubs/ChatHub.cs
```
- OnConnectedAsync()
- OnDisconnectedAsync()
- SendMessage(int toUserId, string message)
- TypingIndicator(int toUserId)
```

### 7.2 Features/Chat/SendMessage/
```
SendMessageEndpoint.cs  → POST /api/chat/send
StoreMessageLogic.cs    → Sauvegarder en DB
```

### 7.3 Features/Chat/GetConversations/
```
GetConversationsEndpoint.cs → GET /api/chat/conversations
ConversationsPage.razor      → Liste des chats
```

### 7.4 Features/Chat/GetMessages/
```
GetMessagesEndpoint.cs  → GET /api/chat/messages/{userId}
ChatWindow.razor        → Interface de chat
```

---

## ÉTAPE 8: FEATURE NOTIFICATIONS (1h)

### 8.1 Features/Notifications/Hubs/NotificationHub.cs
```
- SendNotification(int userId, NotificationType type)
- Real-time push
```

### 8.2 Features/Notifications/GetNotifications/
```
GetNotificationsEndpoint.cs → GET /api/notifications
NotificationDropdown.razor   → UI component
```

### 8.3 Features/Notifications/MarkAsRead/
```
MarkAsReadEndpoint.cs   → PUT /api/notifications/{id}/read
```

---

## ÉTAPE 9: SÉCURITÉ & TESTS (1h)

### 9.1 Sécurité globale
- Ajouter Authorization sur tous les endpoints
- Valider tous les inputs
- Rate limiting sur les endpoints sensibles
- CORS configuration

### 9.2 Tests essentiels
- Tester flow complet d'inscription
- Tester le matching
- Tester le chat temps réel
- Tester les notifications

---

## ÉTAPE 10: UI & DÉPLOIEMENT (1h)

### 10.1 Layout principal
- MainLayout.razor avec header/footer
- Navigation responsive
- Badge notifications

### 10.2 Docker/Podman
- Finaliser docker-compose.yml
- Tester le déploiement complet

---

## 💡 AVANTAGES DE CETTE ARCHITECTURE

1. **Isolation** : Chaque feature est indépendante
2. **Rapidité** : FastEndpoints = moins de boilerplate
3. **Clarté** : Un endpoint = un fichier = une responsabilité
4. **Testabilité** : Chaque slice peut être testé isolément
5. **Parallélisation** : Tu peux développer plusieurs features en même temps

## 🎯 CONSEILS POUR CLAUDE CODE

Pour chaque feature, demande à Claude Code de :
1. Créer l'endpoint avec FastEndpoints
2. Ajouter la validation avec FluentValidation
3. Créer la page Blazor correspondante
4. Tester immédiatement

Exemple de commande pour Claude Code :
```
"Crée la feature complète Auth/Register avec :
- RegisterEndpoint.cs utilisant FastEndpoints
- Validation du mot de passe (pas de mots communs)
- Hash BCrypt
- Envoi email avec MailKit
- Page Blazor avec formulaire"
```

## ⚡ ORDRE DE PRIORITÉ

1. **Setup + Database** (obligatoire en premier)
2. **Auth/Register + Login** (base de tout)
3. **Profile/EditProfile** (nécessaire pour la suite)
4. **Matching/LikeProfile** (coeur du projet)
5. **Chat** (temps réel critique)
6. **Notifications** (temps réel critique)
7. **Browsing** (peut être basique)
8. **Reste** (si temps)