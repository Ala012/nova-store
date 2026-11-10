# NOVA — Boutique en ligne (ASP.NET Core MVC · .NET 10 · MySQL)

Application e-commerce complète réalisée dans le cadre du **TP N°4** (qui regroupe les acquis des TP2 et TP3).
Projet **ASP.NET Core MVC** ciblant **.NET 10**, base de données **MySQL / MariaDB** (provider Oracle
`MySql.EntityFrameworkCore`, **sans Pomelo**), avec une identité visuelle et une expérience d'achat modernes.

---

## 1. Démonstration vidéo

Une démonstration d'exécution complète (≈ 2 min, 1280×720) est disponible ici :

**`demo/nova-demo.mp4`**

Elle parcourt : accueil, catalogue et filtres, fiche produit (favoris + panier), panier,
connexion client, commande et confirmation, puis le back-office administrateur
(dashboard, produits, commandes, utilisateurs et attribution des rôles).

Aperçu (première image) : `demo/nova-demo-poster.jpg`

---

## 2. Lancer le projet

### Prérequis

| Outil | Version conseillée |
| --- | --- |
| .NET SDK | 10.0 |
| MySQL ou MariaDB | 8+ / 10.4+ (XAMPP suffit) |
| Navigateur | Chrome / Edge / Firefox récent |

### Étapes

1. **Démarrer MySQL** (ex. XAMPP → *MySQL → Start*), puis créer la base :

   ```sql
   CREATE DATABASE IF NOT EXISTS nova_store
     CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci;
   ```

2. **Vérifier la chaîne de connexion** dans `appsettings.json` :

   ```json
   "ProductDBConnection": "Server=localhost;Port=3306;Database=nova_store;User=root;Password=;CharSet=utf8mb4;"
   ```

   Adaptez `User` / `Password` à votre installation (XAMPP par défaut : `root` sans mot de passe).

3. **Restaurer et lancer** :

   ```bash
   dotnet restore
   dotnet run --urls "http://localhost:5080"
   ```

4. Ouvrir <http://localhost:5080>.

Au premier démarrage, le schéma est créé et la base est alimentée automatiquement
(rôles, utilisateurs, 6 catégories, 20 produits, avis et commandes de démonstration).

### Comptes de démonstration

| Rôle | E-mail | Mot de passe |
| --- | --- | --- |
| Admin | `admin@nova.tn` | `Admin@123` |
| Manager | `manager@nova.tn` | `Manager@123` |
| Client | `client@nova.tn` | `Client@123` |
| Client | `sami@nova.tn` | `Sami@123` |

---

## 3. Fonctionnalités

### Base TP2 / TP3

- **Produits** : liste, recherche, détails, création, modification, suppression, upload d'image,
  gestion du stock, prix barré / promotion, badges « Nouveau », « Promo », « Stock faible ».
- **Catégories** : CRUD complet, icône, couleur d'accent, description, compteur de produits.
- **Authentification Identity** : inscription, connexion, déconnexion,
  profil utilisateur, changement de mot de passe.
- **Rôles** : `Admin`, `Manager`, `User` avec autorisations par contrôleur/action,
  gestion des rôles (création, édition, suppression) et affectation des utilisateurs aux rôles.
- **Sécurité** : mots de passe hachés, anti-forgery sur les POST, validation serveur + client,
  page « Accès refusé », gestion des erreurs 404 / 500.

### TP4 — Parcours d'achat

- **Catalogue dynamique** : sidebar des catégories avec compteurs, filtres (catégorie, prix min/max,
  disponibilité, promotions), tri (prix, nouveauté, popularité, note) et **pagination côté serveur**
  conservant les filtres.
- **Panier** : ajout depuis la fiche produit ou les cartes, quantités (+/−), suppression,
  sous-total, frais de livraison (offerts dès 299 DT), badge panier temps réel, panier par session.
- **Commande** : formulaire de livraison, choix du paiement (livraison, carte, virement),
  validation, calcul du total, **décrémentation du stock**, page de **confirmation** avec récapitulatif.
- **Commandes** : historique client, détails avec timeline, **facture imprimable**, gestion admin
  (filtre par statut, changement de statut, restauration du stock en cas d'annulation).

### Fonctionnalités bonus

- **Wishlist** (favoris) avec ajax, badge et page dédiée.
- **Avis clients** : note 1–5, commentaire, note moyenne, achat vérifié, avis unique par utilisateur.
- **Tableau de bord admin** : KPI (CA, commandes, produits, clients), graphique des ventes,
  répartition des statuts, top produits, stock faible, actions rapides.
- **Gestion des utilisateurs** (liste, rôles) réservée aux admins.
- **Thème clair / sombre** persistant (`localStorage`).
- **Ajax** : panier, favoris, toasts, sans rechargement de page.
- **Design** : identité NOVA (Sora / Manrope, dégradés violet-pink), illustrations SVG produits,
  animations d'apparition, responsive mobile/tablette/desktop, accessibilité (skip link, focus, ARIA).
- **Recherche globale** avec raccourci clavier `/`.

---

## 4. Architecture

```text
TP_Product/
├─ Controllers/        Home, Product, Category, Panier, Orders, Wishlist,
│                      Account, Admin, Dashboard
├─ Data/               AppDbContext, DbSeeder (rôles, users, données de démo)
├─ Helpers/            Money (format fr-FR + DT), Shipping, DisplayText
├─ Models/             Product, Category, Order, OrderItem, Review, WishlistItem
│  ├─ Help/            CartItem
│  └─ Query/           PagedResult<T>
├─ Repositories/       Interfaces + implémentations EF Core
├─ Services/           CartService (panier en session)
├─ ViewComponents/     Categories, CartBadge, WishlistBadge
├─ ViewModels/         Formulaires, listes, dashboard
├─ Views/              Razor (layout partagé, partiels, pages)
└─ wwwroot/
   ├─ css/nova.css     Design system complet
   ├─ js/nova.js       Thème, ajax, toasts, reveals, formulaires
   ├─ lib/             Bootstrap 5, jQuery, validation (local)
   └─ images/products/ 20 illustrations SVG + image par défaut
```

### Choix techniques

- **Provider MySQL Oracle** : `MySql.EntityFrameworkCore` 10.0.12 + EF Core / Identity 10.0.14.
- **Panier en session** : `CartService` stocke les lignes en `ISession`, ce qui isole les paniers
  par navigateur et évite de créer un panier en base pour les visiteurs anonymes.
- **Création du schéma via `EnsureCreatedAsync()`** : le provider Oracle échoue sur MariaDB lors de
  `MigrateAsync()` (`MySQLHistoryRepository.AcquireDatabaseLock`, `DBNull` → `Int64`).
  Les migrations ont donc été retirées : le schéma est généré au démarrage, puis `DbSeeder`
  insère les données si la base est vide.
- **Sécurité** : Identity + rôles, `[Authorize]` sur les espaces privés, anti-forgery sur les POST,
  upload limité aux extensions image (`.jpg`, `.jpeg`, `.png`, `.webp`, `.gif`, `.svg`).

---

## 5. Parcours de test rapide

1. Accueil → « Explorer la boutique ».
2. Filtrer par catégorie / prix, changer de page, trier.
3. Ouvrir un produit → ajouter au panier → ajouter aux favoris.
4. Panier → modifier les quantités (Ajax) → commander.
5. Se connecter avec `client@nova.tn` / `Client@123` → « Mes commandes » → facture.
6. Se connecter avec `admin@nova.tn` / `Admin@123` → tableau de bord, gestion produits,
   catégories, commandes, rôles et utilisateurs.

---

## 6. Remarques

- Le projet cible `net10.0` ; un SDK preview peut afficher l'avertissement `NETSDK1057`
  (sans impact sur le fonctionnement).
- Les montants sont formatés en français (`fr-FR`) avec trois décimales et le suffixe `DT`.
- Les images produits sont des SVG générés localement : aucun appel réseau n'est nécessaire
  pour le catalogue. Seules les polices Google Fonts et Bootstrap Icons proviennent d'un CDN.

---

© 2026 — Projet académique NOVA · ASP.NET Core MVC · MySQL
