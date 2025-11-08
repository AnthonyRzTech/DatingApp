#!/bin/bash
# Script de configuration pour ajouter un utilisateur de test

echo "=== Configuration PostgreSQL et utilisateur de test ==="
echo ""

# Étape 1: Définir le mot de passe postgres à 'q'
echo "Étape 1: Configuration du mot de passe PostgreSQL..."
sudo -u postgres psql -c "ALTER USER postgres WITH PASSWORD 'q';"

if [ $? -eq 0 ]; then
    echo "✓ Mot de passe PostgreSQL configuré avec succès"
else
    echo "✗ Erreur lors de la configuration du mot de passe"
    exit 1
fi

echo ""

# Étape 2: Créer la base de données si elle n'existe pas
echo "Étape 2: Vérification de la base de données..."
sudo -u postgres psql -lqt | cut -d \| -f 1 | grep -qw webmatcha

if [ $? -ne 0 ]; then
    echo "Base de données 'webmatcha' introuvable, création..."
    sudo -u postgres createdb webmatcha
    echo "✓ Base de données créée"
else
    echo "✓ Base de données existe déjà"
fi

echo ""

# Étape 3: Lancer l'application pour appliquer les migrations
echo "Étape 3: Application des migrations..."
echo "Lancement de l'application (appuyez sur Ctrl+C après 10 secondes)..."
timeout 10 dotnet run > /dev/null 2>&1 || true

echo ""

# Étape 4: Ajouter l'utilisateur de test
echo "Étape 4: Ajout de l'utilisateur de test..."
PGPASSWORD=q psql -U postgres -d webmatcha -f SQL/InsertTestUser.sql

if [ $? -eq 0 ]; then
    echo ""
    echo "=========================================="
    echo "✅ CONFIGURATION TERMINÉE AVEC SUCCÈS !"
    echo "=========================================="
    echo ""
    echo "Vous pouvez maintenant vous connecter avec:"
    echo "  Username: testuser"
    echo "  Password: Test123!"
    echo ""
    echo "Lancer l'application avec: dotnet run"
    echo "Puis aller sur: https://localhost:7036"
else
    echo "✗ Erreur lors de l'ajout de l'utilisateur de test"
    exit 1
fi
