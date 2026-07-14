# Site Vitrine - Imprimerie & Fabrication de Panneaux (site en arabe)

## Présentation du projet

### Objectif

Développer un site vitrine moderne en arabe, responsive et optimisé pour le référencement (SEO) pour une entreprise spécialisée dans :

- Impression de panneaux publicitaires
- Fabrication de panneaux rigides
- Panneaux en relief
- Enseignes
- Signalétique intérieure et extérieure
- Impression grand format
- Découpe et personnalisation

Le site permettra aux visiteurs de découvrir les services, les réalisations de l'entreprise et de la contacter facilement.

Le site ne comporte **qu'un seul administrateur**, chargé de gérer tout le contenu.

Aucune fonctionnalité de vente en ligne ou de paiement n'est prévue.

---

# Public cible

- Entreprises
- Commerces
- Artisans
- Collectivités
- Associations
- Particuliers

---

# Fonctionnalités

## Site public

- Page d'accueil
- Présentation de l'entreprise
- Liste des services
- Galerie des réalisations
- Actualités
- Formulaire de contact
- Coordonnées
- Carte Google Maps
- Site responsive
- Optimisation SEO

---

## Administration

Un seul utilisateur administrateur.

Fonctionnalités :

- Connexion sécurisée
- Gestion des réalisations
- Gestion des services
- Gestion des actualités
- Gestion des images
- Modification des informations de contact
- Gestion de la page d'accueil
- Gestion des horaires

---

# Arborescence du site

## Accueil

Contient :

- Présentation rapide
- Image principale
- Mise en avant des services
- Réalisations récentes
- Bouton "Nous contacter"

---

## À propos

Présentation de l'entreprise :

- Historique
- Expérience
- Savoir-faire
- Valeurs

---

## Services

Une fiche par service :

- Impression numérique
- Impression grand format
- Fabrication de panneaux
- Panneaux en relief
- Enseignes
- Signalétique
- Personnalisation

Chaque fiche comprend :

- Description
- Galerie d'images
- Avantages

---

## Réalisations

Galerie des projets réalisés.

Chaque réalisation contient :

- Titre
- Description
- Catégorie
- Plusieurs photos
- Date de réalisation

---

## Actualités

Liste des actualités ou nouveautés.

Chaque actualité comprend :

- Titre
- Contenu
- Image
- Date de publication

---

## Contact

Informations :

- Adresse
- Téléphone
- Email
- Horaires
- Google Maps

Formulaire :

- Nom
- Société (optionnel)
- Téléphone
- Email
- Sujet
- Message

---

# Back-office

## Tableau de bord

Affiche :

- Nombre de réalisations
- Nombre d'actualités
- Dernières modifications

Menus :

- Tableau de bord
- Réalisations
- Services
- Actualités
- Paramètres
- Contact

---

# Modèle de données

## Réalisation

| Champ        | Type       |
| ------------ | ---------- |
| Id           | Guid       |
| Titre        | string     |
| Description  | string     |
| Catégorie    | string     |
| Images       | Collection |
| DateCréation | DateTime   |
| Visible      | bool       |

---

## Service

| Champ       | Type   |
| ----------- | ------ |
| Id          | Guid   |
| Nom         | string |
| Description | string |
| Image       | string |
| Ordre       | int    |
| Visible     | bool   |

---

## Actualité

| Champ           | Type     |
| --------------- | -------- |
| Id              | Guid     |
| Titre           | string   |
| Contenu         | string   |
| Image           | string   |
| DatePublication | DateTime |
| Visible         | bool     |

---

## Paramètres

| Champ         | Type   |
| ------------- | ------ |
| NomEntreprise | string |
| Logo          | string |
| Adresse       | string |
| Téléphone     | string |
| Email         | string |
| Horaires      | string |
| Facebook      | string |
| Instagram     | string |
| LinkedIn      | string |

---

# User Stories

## Visiteur

### Consulter les services

**En tant que** visiteur

**Je souhaite** consulter les services proposés

**Afin de** connaître les prestations de l'entreprise.

---

### Consulter les réalisations

**En tant que** visiteur

**Je souhaite** parcourir les réalisations

**Afin de** voir la qualité des travaux réalisés.

---

### Consulter les coordonnées

**En tant que** visiteur

**Je souhaite** consulter les informations de contact

**Afin de** joindre facilement l'entreprise.

---

### Envoyer un message

**En tant que** visiteur

**Je souhaite** envoyer un message via un formulaire

**Afin de** demander un devis ou des renseignements.

---

### Consulter le site sur mobile

**En tant que** visiteur

**Je souhaite** utiliser le site sur smartphone

**Afin de** naviguer facilement.

---

### Visualiser les photos

**En tant que** visiteur

**Je souhaite** agrandir les images

**Afin de** mieux apprécier les réalisations.

---

# Administrateur

### Se connecter

**En tant qu'** administrateur

**Je souhaite** me connecter

**Afin de** gérer le contenu du site.

---

### Ajouter une réalisation

**En tant qu'** administrateur

**Je souhaite** publier une nouvelle réalisation

**Afin de** présenter un nouveau projet.

---

### Modifier une réalisation

**En tant qu'** administrateur

**Je souhaite** modifier une réalisation

**Afin de** mettre son contenu à jour.

---

### Supprimer une réalisation

**En tant qu'** administrateur

**Je souhaite** supprimer une réalisation

**Afin de** retirer un ancien projet.

---

### Gérer les services

**En tant qu'** administrateur

**Je souhaite** créer, modifier ou supprimer un service

**Afin de** maintenir la liste des prestations.

---

### Publier une actualité

**En tant qu'** administrateur

**Je souhaite** publier une actualité

**Afin de** communiquer avec les visiteurs.

---

### Modifier les informations de contact

**En tant qu'** administrateur

**Je souhaite** modifier les coordonnées de l'entreprise

**Afin de** maintenir des informations exactes.

---

### Gérer les images

**En tant qu'** administrateur

**Je souhaite** ajouter plusieurs images à une réalisation

**Afin de** mieux présenter le projet.

---

### Masquer un contenu

**En tant qu'** administrateur

**Je souhaite** masquer un contenu sans le supprimer

**Afin de** pouvoir le republier ultérieurement.

---

# Contraintes techniques

- Responsive (mobile, tablette, desktop)
- Interface d'administration simple
- Authentification sécurisée
- Optimisation SEO
- Chargement rapide
- Images optimisées
- Protection du formulaire contre le spam
- Architecture évolutive
- Accessibilité (WCAG de base)
- Respect des bonnes pratiques Angular et ASP.NET

---

# Stack technique

## Frontend

- Angular 21 (Standalone)
- TypeScript
- Tailwind CSS
- primeng 21
- Angular Router
- Angular Signals

## Backend

- ASP.NET Core 10 Web API
- Entity Framework Core
- JWT Authentication

## Base de données

- PostgreSQL

## Stockage

- Images stockées sur le serveur sur un container minio

## Déploiement

- docker
- HTTPS
- Reverse Proxy (NPM)

---

# Objectif final

Obtenir un site professionnel, rapide, moderne et facilement administrable permettant :

- de présenter l'entreprise,
- de valoriser les réalisations,
- de mettre à jour les contenus simplement,
- d'améliorer la visibilité sur les moteurs de recherche,
- et de permettre aux prospects de contacter facilement l'entreprise.
