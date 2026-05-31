# F-046 — Filtres et règles de surveillance personnalisées

> **Statut** : ✅ Backend implémenté (MVP 2, 29 mai 2026). Entité `FeedRule` (UserId + critères AND : `KeywordPattern` / `SourceId` / `MentionedSiren` (Siren) + actions : `NotifyEmail` / `NotifyPush`), invariants `Create` / `Update` (au moins un critère, au moins une action, nom ≤ 200, keyword ≤ 200), `RegisterEvaluation` / `RegisterTrigger`. Port `IFeedRuleRepository`. 5 use cases MediatR (`CreateFeedRule`, `UpdateFeedRule`, `DeleteFeedRule`, `ListMyFeedRules`, `EvaluateFeedRules`). Notification `FeedRuleMatchedNotification` + 2 handlers (`SendFeedRuleMatchedEmailHandler`, `DispatchFeedRuleMatchedPushHandler`) calqués sur F-019, gating sur `NotifyEmail` / `NotifyPush` par règle. Extension `IEmailSender.SendFeedRuleMatchedAsync` + adapters logging / capturing. 4 endpoints `/feed/rules` (POST/GET/PATCH/DELETE). Job Hangfire : `EvaluateFeedRulesCommand` chaîné dans `FeedPollingJob.PollAsync()` après `MatchFavoritesInFeedItemsCommand` (F-047) — voit donc les `FeedItemFavoriteMatch` pour évaluer `MentionedSiren`. Évaluation incrémentale par watermark `LastEvaluatedAt`. Table `feed_rule` (cascade FK user RGPD). 248 tests verts (89 domain + 155 application + 4 archi), dont 17 sur `FeedRule` (invariants + matching AND + watermark), CreateFeedRule (4), DeleteFeedRule (3), EvaluateFeedRules (4 — flux complet match/no-match/watermark). Adapter email **Brevo** livré le 29 mai 2026 (commun avec F-019) ; le `BrevoEmailSender` envoie les notifications de règle matchée via `SendFeedRuleMatchedAsync`. Reste : UI MAUI (CRUD des règles), validateurs FluentValidation câblés dans le pipeline si nécessaire.

> **Architecture liée** : `FeedRule` est le **support d'activation** des `WatchRule` futures. **F-057** étendra cette mécanique pour activer la surveillance sanctions par watchlist (« surveillance sanctions » comme `WatchRule`), réutilisant le moteur d'évaluation existant.

**Description** : l'utilisateur configure des **filtres** sur sa timeline (par mot-clé, par entreprise favorite mentionnée, par source). Crée aussi des **règles** ("alerte-moi quand un nouvel item mentionne X").

**Valeur user** : transformer la veille passive en veille active et ciblée.

**Complexité** : ★★★

**APIs externes** : aucune.

**Dépendances** : F-044.

**Détails techniques** :
- Filtres : moteur de search-as-you-type sur les items en cache
- Règles : structure { critère, action } persistée
- Action : notification email, push, ou ajout à une liste dédiée
