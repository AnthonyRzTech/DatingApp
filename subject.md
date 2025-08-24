# Web Matcha - Checklist des Fonctionnalités Obligatoires

## ✅ TOUTES LES FONCTIONNALITÉS MANDATORY À IMPLÉMENTER

---

## 📝 1. INSCRIPTION ET CONNEXION

### Inscription
- [ ] Formulaire d'inscription avec :
  - [ ] Email obligatoire
  - [ ] Username obligatoire
  - [ ] Nom obligatoire
  - [ ] Prénom obligatoire
  - [ ] Mot de passe sécurisé obligatoire
- [ ] Validation du mot de passe (rejeter les mots anglais courants)
- [ ] Hashage du mot de passe (BCrypt ou équivalent)
- [ ] Envoi d'email de vérification avec lien unique
- [ ] Activation du compte via le lien de vérification

### Connexion
- [ ] Connexion avec username + mot de passe
- [ ] Génération de token/session
- [ ] Mise à jour de "dernière activité"

### Récupération de mot de passe
- [ ] Formulaire "mot de passe oublié"
- [ ] Envoi d'email avec lien de réinitialisation
- [ ] Page de réinitialisation avec nouveau mot de passe

### Déconnexion
- [ ] Bouton de déconnexion accessible depuis toutes les pages
- [ ] Déconnexion en un clic
- [ ] Suppression de la session/token

---

## 👤 2. PROFIL UTILISATEUR

### Complétion du profil
- [ ] Genre (sélection obligatoire)
- [ ] Préférences sexuelles (sélection obligatoire)
- [ ] Biographie (texte)
- [ ] Tags d'intérêts (ex: #vegan, #geek, #piercing)
  - [ ] Tags réutilisables
  - [ ] Système d'autocomplétion
- [ ] Upload de photos (maximum 5)
  - [ ] Désignation d'une photo de profil
  - [ ] Validation du type de fichier (images uniquement)

### Modification du profil
- [ ] Modification du nom
- [ ] Modification du prénom
- [ ] Modification de l'email
- [ ] Modification du genre
- [ ] Modification des préférences sexuelles
- [ ] Modification de la biographie
- [ ] Modification des tags
- [ ] Ajout/suppression de photos
- [ ] Changement de photo de profil

### Localisation
- [ ] Géolocalisation GPS (précision au quartier)
- [ ] Si GPS refusé : localisation alternative (IP ou autre)
- [ ] Possibilité d'ajuster manuellement sa position

### Fame Rating
- [ ] Calcul automatique du score de popularité
- [ ] Affichage public du fame rating
- [ ] Critères cohérents pour le calcul

### Historiques visibles
- [ ] Liste des personnes qui ont vu mon profil
- [ ] Liste des personnes qui m'ont liké

---

## 🔍 3. BROWSING (NAVIGATION)

### Liste de suggestions
- [ ] Suggestions basées sur :
  - [ ] Orientation sexuelle compatible
  - [ ] Proximité géographique (prioritaire)
  - [ ] Tags en commun
  - [ ] Fame rating
- [ ] Gestion de la bisexualité
- [ ] Si orientation non spécifiée → bisexuel par défaut

### Tri de la liste
- [ ] Tri par âge
- [ ] Tri par localisation
- [ ] Tri par fame rating
- [ ] Tri par tags communs

### Filtrage de la liste
- [ ] Filtre par âge
- [ ] Filtre par localisation
- [ ] Filtre par fame rating
- [ ] Filtre par tags communs

---

## 🔎 4. RECHERCHE AVANCÉE

### Critères de recherche
- [ ] Intervalle d'âge (min-max)
- [ ] Intervalle de fame rating (min-max)
- [ ] Localisation spécifique
- [ ] Un ou plusieurs tags d'intérêts

### Résultats
- [ ] Tri par âge
- [ ] Tri par localisation
- [ ] Tri par fame rating
- [ ] Tri par tags
- [ ] Filtrage des résultats

---

## 👁️ 5. CONSULTATION DE PROFIL

### Affichage
- [ ] Toutes les infos sauf email et mot de passe
- [ ] Photos du profil
- [ ] Tags d'intérêts
- [ ] Fame rating
- [ ] Statut en ligne/hors ligne
- [ ] Dernière connexion si hors ligne

### Enregistrement
- [ ] Chaque visite est enregistrée dans l'historique

### Actions disponibles
- [ ] Liker la photo de profil
  - [ ] Impossible si pas de photo de profil
  - [ ] Création de "connection" si like mutuel
- [ ] Retirer un like précédent
  - [ ] Suppression de la connection
  - [ ] Arrêt des notifications
- [ ] Signaler comme faux compte
- [ ] Bloquer l'utilisateur
  - [ ] N'apparaît plus dans les recherches
  - [ ] Plus de notifications
  - [ ] Chat impossible

### Indicateurs visuels
- [ ] Voir si la personne m'a liké
- [ ] Voir si on est connectés
- [ ] Option pour unliker/déconnecter

---

## 💬 6. CHAT

### Conditions
- [ ] Chat uniquement entre utilisateurs connectés (like mutuel)

### Fonctionnalités
- [ ] Messages en temps réel (max 10 secondes de délai)
- [ ] Historique des conversations
- [ ] Indicateur de nouveau message visible depuis n'importe quelle page
- [ ] Liste des conversations actives

---

## 🔔 7. NOTIFICATIONS

### Types de notifications (temps réel, max 10 sec)
- [ ] Quand quelqu'un like mon profil
- [ ] Quand quelqu'un visite mon profil
- [ ] Quand je reçois un message
- [ ] Quand un like devient mutuel (match)
- [ ] Quand quelqu'un retire son like

### Système de notifications
- [ ] Badge/compteur de notifications non lues
- [ ] Visible depuis toutes les pages
- [ ] Marquer comme lu

---

## 🔒 8. SÉCURITÉ (OBLIGATOIRE - 0% si non respecté)

### Protections essentielles
- [ ] Mots de passe hashés (jamais en clair dans la BD)
- [ ] Protection contre les injections SQL
- [ ] Protection contre les injections HTML/JavaScript (XSS)
- [ ] Validation de tous les formulaires
- [ ] Validation des uploads (type, taille)
- [ ] Protection CSRF sur les formulaires
- [ ] Authentification requise pour les actions sensibles

### Credentials
- [ ] Fichier .env pour tous les secrets
- [ ] .env exclu de Git
- [ ] Pas de credentials dans le code

---

## 🎨 9. INTERFACE & COMPATIBILITÉ

### Structure
- [ ] Header sur toutes les pages
- [ ] Section principale
- [ ] Footer sur toutes les pages

### Responsive
- [ ] Compatible mobile
- [ ] Layout acceptable sur petits écrans

### Compatibilité navigateurs
- [ ] Fonctionne sur Firefox (dernière version)
- [ ] Fonctionne sur Chrome (dernière version)

### Qualité
- [ ] Aucune erreur dans la console JavaScript
- [ ] Aucune erreur côté serveur
- [ ] Aucun warning

---

## 📊 10. BASE DE DONNÉES

### Requirements
- [ ] Base de données relationnelle ou orientée graphe
- [ ] Minimum 500 profils distincts pour l'évaluation
- [ ] Requêtes manuelles (pas d'ORM complet autorisé)

---

## 🚀 11. DÉPLOIEMENT

### Configuration
- [ ] Serveur web (Apache, Nginx ou built-in)
- [ ] Podman configuration
- [ ] Instructions de déploiement claires

---

## ⚠️ POINTS CRITIQUES POUR LA DÉFENSE

1. **Sécurité** : Une seule faille = 0%
2. **500 profils minimum** dans la base
3. **Temps réel** : Notifications et chat (max 10 sec)
4. **Pas d'erreurs** dans la console
5. **Mobile responsive** obligatoire
6. **Matching intelligent** avec plusieurs critères

---

## 🎯 STRATÉGIE POUR RÉUSSIR

### Priorité HAUTE (faire en premier)
1. Authentification complète avec email
2. Profils avec photos
3. Système de like/match
4. Chat temps réel
5. Notifications temps réel
6. Sécurité

### Priorité MOYENNE
1. Browsing avec filtres
2. Recherche avancée
3. Fame rating
4. Géolocalisation


### Priorité BASSE (si temps restant)
1. Design élaboré
2. Animations
3. Features supplémentaires

---

## ❌ NE PAS FAIRE (Bonus - pas nécessaire pour 100%)
- OAuth/OmniAuth
- Galerie photo avec drag-and-drop
- Édition d'images
- Carte interactive
- Chat vidéo/audio
- Organisation de dates/événements