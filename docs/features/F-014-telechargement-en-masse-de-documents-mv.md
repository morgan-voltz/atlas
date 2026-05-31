# F-014 — Téléchargement en masse de documents — ✅ MVP 2 (29 mai 2026)

**Description** : l'utilisateur sélectionne plusieurs entreprises (via une liste de SIREN, jusqu'à 50) et lance un téléchargement groupé de tous leurs bilans/actes, livré sous forme d'archive ZIP avec une arborescence par SIREN.

**Valeur user** : automatisation d'une tâche fastidieuse pour les cabinets traitant des dizaines d'entreprises.

**Complexité** : ★★★★

**APIs externes** : INPI RNE.

**Dépendances** : F-013.

**Implémentation** :
- Endpoints `POST /downloads/bulk`, `GET /downloads/bulk/{id}`, `GET /downloads/bulk/{id}/archive`.
- Job Hangfire `BulkDownloadJob.RunAsync(jobId)` enfilé après la création du job en base. Idempotent : si le job est déjà finalisé, no-op.
- Limite : 50 SIREN par job, validés en amont (Luhn) avant enqueue.
- Stockage temporaire via port `IFileStorage` (adapter `LocalFileStorage` filesystem ; clé `bulk/{jobId:N}.zip`).
- TTL d'archive : 24h. Status renvoie `410 Gone` après expiration.
- Documents confidentiels filtrés à la source (best-effort par SIREN : un échec n'arrête pas les autres).
- Suivi par polling client (l'envoi push/email est repoussé à un lot ultérieur).

**Reste à traiter dans un lot futur** :
- Notification push/email à la finalisation du job.
- Job récurrent de purge des archives expirées + entités `BulkDownloadJob`.
- Adapter S3 (MinIO, Wasabi, AWS) pour la prod.
- Rate limiting INPI dédié aux téléchargements (aujourd'hui : limites par défaut du `RneCompanyProvider`).
