#!/usr/bin/env python3
"""
Script pour générer automatiquement la navigation MkDocs
à partir du fichier navigation.yml
"""

import yaml
import sys
from pathlib import Path

def load_navigation_config():
    """Charge la configuration de navigation depuis navigation.yml"""
    try:
        with open('navigation.yml', 'r', encoding='utf-8') as file:
            return yaml.safe_load(file)
    except FileNotFoundError:
        print("ERREUR: Fichier navigation.yml non trouve")
        sys.exit(1)
    except yaml.YAMLError as e:
        print(f"ERREUR YAML: {e}")
        sys.exit(1)

def generate_navigation(config):
    """Génère la structure de navigation pour MkDocs"""
    nav = []
    
    # Ajouter la page d'accueil
    nav.extend(config['base_structure'])
    
    # Ajouter les sections dans l'ordre spécifié
    for section_key in config['section_order']:
        if section_key in config['sections']:
            section = config['sections'][section_key]
            nav.append({section['title']: section['items']})
    
    return nav

def update_mkdocs_config(nav):
    """Met à jour le fichier mkdocs-simple.yml avec la nouvelle navigation"""
    mkdocs_file = Path('mkdocs-simple.yml')
    
    if not mkdocs_file.exists():
        print("ERREUR: Fichier mkdocs-simple.yml non trouve")
        sys.exit(1)
    
    # Lire le fichier MkDocs
    with open(mkdocs_file, 'r', encoding='utf-8') as file:
        content = file.read()
    
    # Trouver la section nav et la remplacer
    start_marker = "# Navigation Flexible et Modulaire\nnav:"
    end_marker = "\n# Copyright"
    
    start_index = content.find(start_marker)
    end_index = content.find(end_marker)
    
    if start_index == -1 or end_index == -1:
        print("ERREUR: Impossible de trouver les marqueurs de navigation")
        sys.exit(1)
    
    # Générer le nouveau contenu de navigation
    nav_yaml = yaml.dump(nav, default_flow_style=False, allow_unicode=True, sort_keys=False)
    nav_content = f"# Navigation Flexible et Modulaire\nnav:\n{nav_yaml}"
    
    # Remplacer la section
    new_content = content[:start_index] + nav_content + content[end_index:]
    
    # Écrire le fichier mis à jour
    with open(mkdocs_file, 'w', encoding='utf-8') as file:
        file.write(new_content)
    
    print("Navigation mise a jour dans mkdocs-simple.yml")

def main():
    """Fonction principale"""
    print("Generation de la navigation MkDocs...")
    
    # Charger la configuration
    config = load_navigation_config()
    print("Configuration chargee depuis navigation.yml")
    
    # Générer la navigation
    nav = generate_navigation(config)
    print("Structure de navigation generee")
    
    # Mettre à jour le fichier MkDocs
    update_mkdocs_config(nav)
    print("Fichier mkdocs-simple.yml mis a jour")
    
    print("\nNavigation generee:")
    for item in nav:
        if isinstance(item, dict):
            for title, pages in item.items():
                print(f"  [SECTION] {title}")
                if isinstance(pages, list):
                    for page in pages:
                        if isinstance(page, dict):
                            for page_title, page_path in page.items():
                                print(f"    [PAGE] {page_title}: {page_path}")
                        else:
                            print(f"    [PAGE] {page}")
        else:
            print(f"  [PAGE] {item}")
    
    print("\nPret pour le deploiement!")

if __name__ == "__main__":
    main()
