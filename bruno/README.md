# Collection Bruno — Atlas API

Tests d'API versionnés avec [Bruno](https://www.usebruno.com/) (fichiers `.bru` en clair,
git-friendly). Couvre tout le parcours backend de la V1 : auth, 2FA, connexion INPI,
entreprises, marques, historique, compte/RGPD.

## Prérequis : démarrer le harness local

Depuis la racine du dépôt :

```bash
# 1. PostgreSQL de dev (conteneur sur le port hôte 5433, pour ne pas heurter un PG natif sur 5432)
docker compose up -d

# 2. Appliquer le schéma
dotnet ef database update --project src/Atlas.Infrastructure.Persistence

# 3. Lancer l'API (profil http → http://localhost:5023)
dotnet run --project src/Atlas.Api --launch-profile http
```

L'API démarre en environnement `Development` :
- clés JWT (RS256) et clé de chiffrement générées automatiquement ;
- emails **non envoyés** — le lien de vérification est écrit dans les logs (`LoggingEmailSender`) ;
- cache en mémoire (pas de Redis requis).

## Utilisation dans l'app Bruno

1. Ouvrir le dossier `bruno/` dans Bruno (*Open Collection*).
2. Sélectionner l'environnement **Local** (en haut à droite).
3. Jouer les requêtes dans l'ordre des dossiers (`01-Auth` → `07-Account`).

### Étape manuelle : vérification de l'email

`Register` n'envoie pas d'email en dev. Après l'avoir joué, récupérer le lien dans les
logs de l'API :

```
[DEV] Email de vérification ... ?userId=<GUID>&token=<TOKEN>
```

Copier `<GUID>` dans la variable d'env `userId` et `<TOKEN>` dans `verificationToken`,
puis jouer `Verify Email`. `Login` capture ensuite l'`accessToken` dans une variable
runtime réutilisée par toutes les requêtes protégées.

### Variables d'environnement (`environments/Local.bru`)

| Variable | Rôle |
|---|---|
| `baseUrl` | URL de l'API (`http://localhost:5023`) |
| `email` / `password` | Compte de test |
| `userId` / `verificationToken` | Vérification email (copiés depuis les logs) |
| `challengeToken` / `totpCode` | Parcours 2FA (TOTP à générer depuis le `secret` de Setup) |
| `siren` | SIREN pour la fiche entreprise (défaut RENAULT) |
| `depositNumber` | Numéro de dépôt pour la notice de marque |
| `inpiUsername` / `inpiPassword` | **Identifiants techniques API INPI** — lus depuis `bruno/.env` (cf. ci-dessous) |

### Secrets : `bruno/.env` (jamais commité)

Les identifiants INPI ne vivent **pas** dans le fichier d'environnement versionné. Copier
`bruno/.env.example` en `bruno/.env` (gitignoré) et y renseigner :

```dotenv
INPI_USERNAME=...
INPI_PASSWORD='...'   # quotes simples si caractères spéciaux ($ % !)
```

`environments/Local.bru` les référence via `{{process.env.INPI_USERNAME}}` /
`{{process.env.INPI_PASSWORD}}`. Bruno (app et CLI) charge automatiquement le `.env`.

> **Accès API requis.** Un compte portail inpi.fr standard n'est **pas** habilité à l'API :
> `Connect` renvoie alors **403 `inpi.api_access_not_allowed`** (l'INPI répond
> `connection_type_not_allowed`). Activer l'accès via data.inpi.fr → « Mes accès API / SFTP »
> et utiliser les identifiants techniques fournis.

> Tant qu'aucune connexion INPI n'est configurée, `04-Companies` et `05-Trademarks`
> renvoient **409 `inpi.not_connected`** : c'est le comportement attendu.

## Automatisation en ligne de commande

Avec le CLI Bruno (`@usebruno/cli`) :

```bash
# Un seul dossier
npx @usebruno/cli run 01-Auth --env Local

# Scénarios de sécurité / validation / RGPD (ne nécessitent PAS d'accès INPI)
npx @usebruno/cli run 08-Scenarios --env Local
npx @usebruno/cli run 08-Scenarios/Lockout --env Local   # verrouillage : 5 échecs → 423

# Injecter des variables (ex. token déjà obtenu) et un rapport JSON
npx @usebruno/cli run "06-Search History" --env Local \
  --env-var accessToken=<JWT> --reporter-json results.json
```

> **Flag `-r`** : `bru run <dossier>` ne descend PAS dans les sous-dossiers par défaut.
> Ajouter `-r` pour inclure les sous-dossiers (ex. `08-Scenarios/Lockout`). Éviter de jouer
> toute la collection d'un coup avec `-r` : elle contient `07-Account/Delete (RGPD)` (destructif)
> et `03-INPI/Connect` (qui appelle l'INPI réel).

### Le dossier `08-Scenarios`

Tests négatifs et de sécurité **jouables sans accès INPI** (idéal en attendant l'habilitation) :
validation (mot de passe faible, email invalide), login non vérifié (403) / mauvais mot de passe
(401), JWT absent/trafiqué (401), SIREN mal formé ou clé de Luhn invalide (400), blocage INPI
(`connected:false`, 409), et sous-dossier `Lockout` (verrouillage après 5 échecs → 423). Chaque
scénario crée ses propres comptes jetables (emails horodatés), sans toucher au compte `e2e`.

> ⚠️ `07-Account/Delete (RGPD)` est **destructif** (supprime le compte de test). Il est
> exclu d'un run complet automatisé sauf intention explicite : le rejouer impose de
> recréer le compte via `Register` + `Verify Email`.
