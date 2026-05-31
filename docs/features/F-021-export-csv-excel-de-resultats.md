# F-021 — Export CSV / Excel de résultats

> **Statut** : 🟡 MVP CSV livré (MVP 2, 29 mai 2026). Helper `Atlas.Application.Common.CsvWriter` (RFC 4180, UTF-8 + BOM, CRLF, échappement quotes/virgules/newlines). 3 endpoints d'export favoris : `GET /favorites/{companies,trademarks,patents}/export` → `text/csv` avec `Content-Disposition: attachment`. Pas de dépendance NuGet ajoutée. **Reste** : export XLSX (ClosedXML), export des résultats de recherche RNE/PI (paginé, à concevoir), export de la veille (timeline). PR #42.

**Description** : depuis une liste de résultats ou un panier, l'utilisateur exporte les données en CSV ou XLSX.

**Valeur user** : intégration avec les outils existants des utilisateurs (Excel, Google Sheets, CRM).

**Complexité** : ★★

**APIs externes** : aucune.

**Dépendances** : F-005.

**Détails techniques** : bibliothèque ClosedXML ou EPPlus pour Excel.
