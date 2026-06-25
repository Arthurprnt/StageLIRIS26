#include <stdio.h>
#include <stdlib.h>
#include <string.h>
#include <ctype.h>
#include <sys/stat.h>
#include <stdbool.h>
#include <omp.h> // Indispensable pour le multi-threading

#ifdef _WIN32
#include <direct.h>
#define MKDIR(path) _mkdir(path)
#define strtok_r strtok_s // Compatibilité Windows
#else
#define MKDIR(path) mkdir(path, 0777)
#endif

#define MAX_LINE_LENGTH 65536
#define INITIAL_GRAPH_BUFFER_SIZE 1048576 // 1 Mo par graphe en mémoire

// ... (Garder les fonctions strip, is_digits_only, create_dir_if_not_exists) ...

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

void create_dir_if_not_exists(const char *dir) {
    struct stat st = {0};
    if (stat(dir, &st) == -1) {
        MKDIR(dir);
    }
}

// ==========================================
// FONCTION EXÉCUTÉE PAR CHAQUE THREAD
// ==========================================
void traiter_et_ecrire_graphe(char *donnees_graphe, int id_graphe, const char *dossier_sortie, const char *dossier_aretes) {
    char chemin_adj[512], chemin_aretes[512];
    snprintf(chemin_adj, sizeof(chemin_adj), "%s/graphe_%d.txt", dossier_sortie, id_graphe);
    snprintf(chemin_aretes, sizeof(chemin_aretes), "%s/graphe_%d.txt", dossier_aretes, id_graphe);

    FILE *f_adj = fopen(chemin_adj, "w");
    FILE *f_aretes = fopen(chemin_aretes, "w");

    if (!f_adj || !f_aretes) {
        if(f_adj) fclose(f_adj);
        if(f_aretes) fclose(f_aretes);
        return;
    }

    char *saveptr_ligne;
    char *ligne = strtok_r(donnees_graphe, "\n", &saveptr_ligne);

    while (ligne != NULL) {
        char *colon_pos = strchr(ligne, ':');
        if (colon_pos != NULL) {
            *colon_pos = '\0';
            char *gauche = ligne;
            char *droite = colon_pos + 1;

            char *semi_pos = strchr(droite, ';');
            if (semi_pos) *semi_pos = ' ';

            int sommet_id = atoi(gauche);
            int nouvel_id = sommet_id + 1;

            fprintf(f_adj, "%d:", nouvel_id);

            char *saveptr_token;
            char *token = strtok_r(droite, " \t", &saveptr_token);
            while (token != NULL) {
                int voisin = atoi(token);
                int nouveau_voisin = voisin + 1;

                fprintf(f_adj, " %d", nouveau_voisin);

                if (nouvel_id < nouveau_voisin) {
                    fprintf(f_aretes, "%d %d\n", nouvel_id, nouveau_voisin);
                }
                token = strtok_r(NULL, " \t", &saveptr_token);
            }
            fprintf(f_adj, "\n");
        }
        ligne = strtok_r(NULL, "\n", &saveptr_ligne);
    }

    fclose(f_adj);
    fclose(f_aretes);
}

// ==========================================
// FONCTION PRINCIPALE MULTI-THREAD
// ==========================================
void convertir_graphes_geant_mt(const char *fichier_entree, const char *dossier_sortie) {
    char dossier_aretes[256];
    snprintf(dossier_aretes, sizeof(dossier_aretes), "%s_aretes", dossier_sortie);

    create_dir_if_not_exists(dossier_sortie);
    create_dir_if_not_exists(dossier_aretes);

    FILE *f_in = fopen(fichier_entree, "r");
    if (!f_in) {
        printf("Erreur : Le fichier '%s' est introuvable.\n", fichier_entree);
        return;
    }

    char ligne[MAX_LINE_LENGTH];
    int graphe_idx = 0;
    
    // Tampon dynamique pour accumuler un graphe
    size_t buffer_size = INITIAL_GRAPH_BUFFER_SIZE;
    char *graph_buffer = malloc(buffer_size);
    graph_buffer[0] = '\0';
    size_t current_len = 0;

    // Début de la zone parallèle OpenMP
    #pragma omp parallel
    {
        // Un seul thread lit le fichier (Le Producteur)
        #pragma omp single
        {
            while (fgets(ligne, sizeof(ligne), f_in)) {
                strip(ligne);
                if (ligne[0] == '\0') continue;

                if (is_digits_only(ligne)) {
                    // Si le buffer n'est pas vide, on a un graphe complet
                    if (current_len > 0) {
                        graphe_idx++;
                        char *donnees_a_traiter = strdup(graph_buffer); 
                        
                        // Création d'une tâche asynchrone pour un thread (Le Consommateur)
                        #pragma omp task firstprivate(donnees_a_traiter, graphe_idx)
                        {
                            traiter_et_ecrire_graphe(donnees_a_traiter, graphe_idx, dossier_sortie, dossier_aretes);
                            free(donnees_a_traiter); // Libération locale au thread
                        }
                        
                        // Reset du buffer pour le prochain graphe
                        graph_buffer[0] = '\0';
                        current_len = 0;
                    }
                    continue;
                }

                // Ajout de la ligne au buffer du graphe en cours
                size_t line_len = strlen(ligne);
                if (current_len + line_len + 2 > buffer_size) {
                    buffer_size *= 2;
                    graph_buffer = realloc(graph_buffer, buffer_size);
                }
                strcat(graph_buffer, ligne);
                strcat(graph_buffer, "\n");
                current_len += line_len + 1;
            }

            // Traitement du tout dernier graphe (fin de fichier)
            if (current_len > 0) {
                graphe_idx++;
                char *donnees_a_traiter = strdup(graph_buffer);
                #pragma omp task firstprivate(donnees_a_traiter, graphe_idx)
                {
                    traiter_et_ecrire_graphe(donnees_a_traiter, graphe_idx, dossier_sortie, dossier_aretes);
                    free(donnees_a_traiter);
                }
            }
        } // Fin du #pragma omp single (et attente implicite des tâches)
    } // Fin du #pragma omp parallel

    free(graph_buffer);
    fclose(f_in);
    
    printf("Succès ! %d graphes traités en parallèle.\n", graphe_idx);
}

int main() {
    // Mesure du temps (facultatif)
    double debut = omp_get_wtime();
    
    convertir_graphes_geant_mt("output.txt", "graphes");
    
    double fin = omp_get_wtime();
    printf("Temps d'exécution : %f secondes.\n", fin - debut);
    
    return 0;
}