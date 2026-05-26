# Politique de securite — Atlas

## Signalement responsable

Si vous decouvrez une vulnerabilite de securite dans Atlas, **ne pas ouvrir une issue publique**. Merci de respecter le processus de divulgation responsable suivant :

1. Envoyer un email a **security@atlas.invalid** (a remplacer par l'adresse reelle au moment de l'ouverture du repo public) en decrivant :
   - La nature de la vulnerabilite
   - Les etapes pour reproduire
   - L'impact potentiel
   - Une eventuelle suggestion de remediation
2. Vous recevrez un accuse de reception sous 72 heures.
3. Un correctif sera prepare en interne, puis publie en coordination avec vous.
4. Vous serez credite dans le changelog si vous le souhaitez.

## Engagements

- **Hashage des mots de passe** : Argon2id exclusivement. Voir [`docs/04-securite-rgpd.md`](docs/04-securite-rgpd.md).
- **Credentials INPI** : chiffres au repos en AES-256-GCM avec une cle geree par le KMS.
- **JWT** : duree de vie courte (15 min), refresh token rotatif cote serveur, signature asymetrique.
- **Logs** : aucun secret, mot de passe, token ou credential INPI ne doit apparaitre dans les logs.
- **RGPD** : minimisation des donnees personnelles, droit a l'effacement, anonymisation des logs.

## Perimetres

Sont consideres comme des vulnerabilites de securite :

- Toute exposition de credentials INPI (en logs, reponses API, dumps memoire)
- Toute exposition non chiffree de donnees personnelles
- Tout contournement de l'authentification ou des permissions
- Toute injection (SQL, commande, NoSQL, etc.)
- Tout dysfonctionnement crypto (signature, chiffrement, hashage)
- Toute fuite via les API publiques (BeneficiaireEffectif notamment, regime CJUE Sovim)

## Hors perimetre

- Les outils de developpement (Swagger, etc.) non actifs en production
- Les vulnerabilites supposees dans des dependances tierces non exploitables dans le contexte Atlas
- Les attaques necessitant un acces physique a la machine d'un utilisateur authentifie
