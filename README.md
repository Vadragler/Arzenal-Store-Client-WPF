# Arzenal-Store-Client-WPF

![.NET 8](https://img.shields.io/badge/.NET-8-blue)
![Platform](https://img.shields.io/badge/Platform-Windows-lightgrey)
![UI](https://img.shields.io/badge/UI-WPF-purple)
![Language](https://img.shields.io/badge/Language-C%23-blue)

Client WPF de la plateforme **Arzenal Store**, développé en C# avec .NET 8.0.

Le client WPF communique avec l’API Arzenal-Store-Api et nécessite une authentification via le client Angular. Une fois connecté, le token est récupéré automatiquement pour permettre l’accès aux fonctionnalités de gestion des applications.

---

## ✨ Fonctionnalités

- 🔐 Connexion avec token JWT  
- 🔍 Consultation de la liste des applications  
- ➕ Ajout d’une application avec upload de fichier  
- ✏️ Modification et suppression des applications existantes  
- 🏷️ Gestion des **catégories**, des **tags**, des **systèmes d’exploitation** et des **langues** associées à chaque application  
- 📂 Affichage des fichiers liés aux applications  
- 🌐 Communication avec l’API REST : [Arzenal-Store-Api](https://github.com/Vadragler/Arzenal-Store-Api)  
- 🔄 Communication avec le client Angular pour la gestion de l’authentification (création de compte et connexion)  

---

## 🛠️ Technologies utilisées

- [.NET 8.0](https://dotnet.microsoft.com/)
- WPF (Windows Presentation Foundation)
- C#
- MVVM
- JWT
- HttpClient pour l’appel à l’API

---

## 📦 Dépendances principales

- FluentValidation (validation côté client)
- Newtonsoft.Json

---

## Pré-requis

- API Arzenal-Store-Api fonctionnelle et accessible
- Client Angular pour l’authentification (création de compte et connexion)

---

## ⚙️ Lancement du projet

1. Cloner ce dépôt
2. Ouvrir la solution avec Visual Studio 2022 ou plus récent.
3. L’URL de l’API est codée en dur dans la classe **ServiceLocator.cs**. Modifier si nécessaire.
4. Exécuter le projet.

ℹ️ L'API doit être disponible localement ou dans Docker au moment de l'exécution.

## 📂 Organisation
- **Views/** – Interfaces utilisateur (XAML)  
- **Models/** – Objets de données (DTO)  
- **Services/** – Communication avec l’API et logique métier  
- **Ressources/** – Ressources pour l’interface (images, styles, etc.)  
- **Fonts/** – Polices d’écriture 

## 🔧 À venir
- Gestion automatique du rafraîchissement du token JWT
- Réorganisation de l’application en créant un dossier dédié à la logique métier, afin de mieux structurer l'application.  
  D'autres ajustements dans l’organisation du projet sont également prévus.
