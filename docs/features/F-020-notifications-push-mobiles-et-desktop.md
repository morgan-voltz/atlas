# F-020 — Notifications push mobiles (et desktop)

> **Statut** : ✅ Backend complet (29 mai 2026), client MAUI restant. **Toutes les plateformes push** sont livrées : **FCM** (Android + Web Push) JWT RS256, **APNs** (iOS + macOS) JWT ES256 HTTP/2, **WNS** (Windows desktop) OAuth2 `client_credentials` + Toast XML. Architecture : interface interne `IPlatformPushDispatcher` + `CompositeNotificationDispatcher` qui fan-out vers tous les adapters configurés en parallèle, avec isolation des erreurs. Cleanup auto des tokens morts (FCM : 404 / UNREGISTERED ; APNs : 410 / BadDeviceToken ; WNS : 410 / 404). Bascule DI : ≥ 1 plateforme configurée → composite ; aucune → `LoggingNotificationDispatcher` (dev). Ports + endpoints (28 mai) : entité `DeviceRegistration`, port `INotificationDispatcher`, API `POST/DELETE/GET /devices`. **Reste** : client MAUI (récupération du token natif par plateforme + `POST /devices` au démarrage).

**Description** : alertes envoyées en push sur l'app mobile MAUI en complément des emails. Étendu pour couvrir aussi le desktop (Windows / macOS).

**Valeur user** : immédiateté de l'information.

**Complexité** : ★★★

**APIs externes** : Firebase Cloud Messaging (Android), APNs (iOS).

**Dépendances** : F-009, F-019.
