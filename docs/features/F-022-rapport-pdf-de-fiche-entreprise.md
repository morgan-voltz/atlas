# F-022 — Rapport PDF de fiche entreprise

> **Statut** : ✅ MVP implémenté (MVP 2, 29 mai 2026). Endpoint `GET /companies/{siren}/report.pdf` → PDF A4 (identité, NAF, adresse, dirigeants, table actes/bilans). Powered by **QuestPDF community edition** (licence engagée au démarrage). `CompanyReportRenderer` dans `Atlas.Api/Reports/` (couche présentation, QuestPDF référencé uniquement par Atlas.Api). Attachments best-effort (PDF généré même si la liste échoue). 3 smoke tests dans `Atlas.Api.IntegrationTests` (signature `%PDF`, payloads minimal/complet/vide). PR #43. **Reste** : enrichissement (logo, historique des modifications via snapshot F-019, bilans intégrés via F-013 download).

**Description** : génération d'un PDF imprimable rassemblant les informations principales d'une entreprise + historique des modifications + bilans récents.

**Valeur user** : documentation client pour les cabinets, présentation à un comité d'investissement.

**Complexité** : ★★★

**APIs externes** : INPI RNE.

**Dépendances** : F-004, F-013.

**Détails techniques** : QuestPDF (excellente lib C# moderne et performante pour la génération PDF, sous licence MIT pour usage non-commercial).

---

## Cluster Veille (intégré au MVP 2)

> **Décision stratégique structurante** (cf. ADR-009) : la veille devient une **feature majeure et différenciante** du produit, exploitant le trou de marché identifié (aucun concurrent ne combine données entreprises et veille agrégée).
> Le cluster F-041 à F-050 forme un bloc cohérent à livrer ensemble en MVP 2 pour offrir une expérience complète dès le lancement de la veille.
