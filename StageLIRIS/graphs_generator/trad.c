#include <stdio.h>
#include <stdlib.h>
#include <string.h>
#include <ctype.h>
#include <sys/stat.h>
#include <stdbool.h>

/*
 * Génère un fichier pour chaque graphe présent dans le fichier output.txt
 * Code généré par IA
*/

// Gestion de la création de dossiers (Cross-platform)
#ifdef _WIN32
#include <direct.h>
#define MKDIR(path) _mkdir(path)
#else
#define MKDIR(path) mkdir(path, 0777)
#endif

#define MAX_LINE_LENGTH 65536 // Taille max d'une ligne lue

// ==========================================
// FONCTIONS UTILITAIRES
// ==========================================
void strip(char *str) {
    int len = strlen(str);
    while (len > 0 && isspace((unsigned char)str[len - 1])) {
        str[len - 1] = '\0';
        len--;
    }
    char *start = str;
    while (*start && isspace((unsigned char)*start)) {
        start++;
    }
    if (start != str) {
        memmove(str, start, strlen(start) + 1);
    }
}

bool is_digits_only(const char *str) {
    if (*str == '\0') return false;
    while (*str) {
        if (!isdigit((unsigned char)*str)) return false;
        str++;
    }
    return true;
}

// Créer un dossier s'il n'existe pas
void create_dir_if_not_exists(const char *dir) {
    struct stat st = {0};
    if (stat(dir, &st) == -1) {
        MKDIR(dir);
    }
}

// ==========================================
// FONCTION PRINCIPALE
// ==========================================

void convertir_graphes_geant(const char *fichier_entree, const char *dossier_sortie) {
    /*
     * Lit un fichier de graphes géant ligne par ligne et génère à la volée :
     * - Un fichier de listes d'adjacence (graphe_X.txt)
     * - Un fichier de paires d'arêtes (graphe_X_aretes.txt)
     * Consommation mémoire RAM : Proche de zéro.
     */

    // 1. Création des dossiers de sortie s'ils n'existent pas
    char dossier_aretes[256];
    snprintf(dossier_aretes, sizeof(dossier_aretes), "%s_aretes", dossier_sortie);

    create_dir_if_not_exists(dossier_sortie);
    create_dir_if_not_exists(dossier_aretes);

    int graphe_idx = 0;
    FILE *f_adj = NULL;
    FILE *f_aretes = NULL;

    // 2. Lecture et traitement "Stream" (ligne par ligne)
    FILE *f_in = fopen(fichier_entree, "r");
    if (!f_in) {
        printf("Erreur : Le fichier '%s' est introuvable.\n", fichier_entree);
        return;
    }

    char ligne[MAX_LINE_LENGTH];
    
    // Cette boucle lit le fichier ligne par ligne sans TOUT charger en RAM
    while (fgets(ligne, sizeof(ligne), f_in)) {
        strip(ligne);
        
        if (ligne[0] == '\0') {
            continue; // Ligne vide
        }
        
        // Détection d'un nouveau graphe
        if (is_digits_only(ligne)) {
            // Si des fichiers étaient ouverts pour le graphe précédent, on les ferme
            if (f_adj && f_aretes) {
                fclose(f_adj);
                fclose(f_aretes);
                f_adj = NULL;
                f_aretes = NULL;
            }
            continue; // On passe à la ligne suivante
        }
        
        // Traitement des données du graphe actuel
        char *colon_pos = strchr(ligne, ':');
        if (colon_pos != NULL) {
            // Si aucun fichier n'est ouvert pour ce graphe, on les ouvre à la volée
            if (f_adj == NULL) {
                graphe_idx++;
                char chemin_adj[512];
                char chemin_aretes[512];
                
                snprintf(chemin_adj, sizeof(chemin_adj), "%s/graphe_%d.txt", dossier_sortie, graphe_idx);
                snprintf(chemin_aretes, sizeof(chemin_aretes), "%s/graphe_%d.txt", dossier_aretes, graphe_idx);
                
                f_adj = fopen(chemin_adj, "w");
                f_aretes = fopen(chemin_aretes, "w");
            }
            
            // Extraction des données de la ligne (split par ':')
            *colon_pos = '\0'; 
            char *gauche = ligne;
            char *droite = colon_pos + 1;
            
            // Équivalent de replace(';', '')
            char *semi_pos = strchr(droite, ';');
            if (semi_pos) *semi_pos = ' '; // On remplace par un espace pour le parsing
            
            int sommet_id = atoi(gauche);
            int nouvel_id = sommet_id + 1; // Passage à l'indexation commençant par 1
            
            // ÉCRITURE IMMÉDIATE (Format Adjacence)
            fprintf(f_adj, "%d:", nouvel_id);
            
            // Extraction des voisins et ÉCRITURE IMMÉDIATE (Format Arêtes)
            char *token = strtok(droite, " \t");
            while (token != NULL) {
                int voisin = atoi(token);
                int nouveau_voisin = voisin + 1;
                
                fprintf(f_adj, " %d", nouveau_voisin);
                
                if (nouvel_id < nouveau_voisin) { // Évite les doublons (1 5 et 5 1)
                    fprintf(f_aretes, "%d %d\n", nouvel_id, nouveau_voisin);
                }
                
                token = strtok(NULL, " \t");
            }
            fprintf(f_adj, "\n");
        }
    }
    
    // Fin du fichier : on ferme les derniers fichiers restés ouverts
    if (f_adj && f_aretes) {
        fclose(f_adj);
        fclose(f_aretes);
    }
    
    fclose(f_in);
    
    printf("Succès ! %d graphes traités.\n", graphe_idx);
}

// ==========================================
// UTILISATION
// ==========================================
int main() {
    const char *fichier_source = "output.txt";  // Modifié selon ton exemple
    const char *dossier_destination = "graphes";
    
    convertir_graphes_geant(fichier_source, dossier_destination);
    
    return 0;
}