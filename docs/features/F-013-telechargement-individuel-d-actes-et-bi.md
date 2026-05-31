# F-013 — Téléchargement individuel d'actes et bilans

> **Statut** : 🟡 Backend implémenté (MVP 2, 29 mai 2026). Extension `ICompanyDataProvider` avec `GetAttachmentsAsync` + `DownloadAttachmentAsync` (auth Bearer + retry 401 unifié dans `ExecuteAsync<T>` du `RneCompanyProvider`). Endpoints `GET /companies/{siren}/attachments` et `/attachments/{id}/download` (proxy binaire, `Results.Stream`). Entité `CompanyAttachment` (Id, Type enum `Acte`/`Bilan`/`Other`, Name, DepositedAt?, SizeBytes?, IsConfidential). Mapping JSON **défensif** : accepte tableau direct OU objet avec sous-collections `actes`/`comptesAnnuels`/`bilans`. Bilans confidentiels (champ `confidentialite`) → 403 (`companies.attachment_confidential`). **Reste** : confirmation par un **appel authentifié réel** (compte INPI requis) sur la structure JSON exacte renvoyée par l'INPI ; ajouter Bruno coverage pour les 2 endpoints.

**Description** : depuis la fiche d'une entreprise, l'utilisateur voit la liste des actes (statuts, modifications) et bilans déposés, et peut télécharger chaque document en PDF.

**Valeur user** : usage central pour les experts-comptables et avocats — accéder rapidement aux documents juridiques d'une boîte.

**Complexité** : ★★

**APIs externes** : INPI RNE (`/companies/{siren}/attachments` + endpoint de téléchargement).

**Dépendances** : F-004.
