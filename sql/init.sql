-- ==============================================================================
-- PROJET H26 - 4R3 : CLINIQUE MAGOO
-- SCRIPT D'INITIALISATION DE LA BASE DE DONNÉES (POSTGRESQL)
-- schéma PUBLIC
-- ==============================================================================

-- 1. NETTOYAGE (Pour permettre de relancer le script)
DROP TABLE IF EXISTS examen CASCADE;
DROP TABLE IF EXISTS liste_examen CASCADE;
DROP TABLE IF EXISTS telephone CASCADE;
DROP TABLE IF EXISTS patient CASCADE;
DROP TABLE IF EXISTS docteur CASCADE;
DROP TABLE IF EXISTS clinique CASCADE;
DROP TABLE IF EXISTS ville CASCADE;

-- ==============================================================================
-- 2. CRÉATION DES TABLES (DDL)
-- ==============================================================================

CREATE TABLE ville (
                       id SERIAL PRIMARY KEY,
                       nom VARCHAR(100) NOT NULL,
                       province VARCHAR(50) DEFAULT 'Québec',
                       pays VARCHAR(50) DEFAULT 'Canada',
                       CONSTRAINT uq_ville_prov UNIQUE (nom, province)
);

CREATE TABLE clinique (
                          id SERIAL PRIMARY KEY,
                          nom VARCHAR(150) NOT NULL UNIQUE,
                          id_ville INT,
                          CONSTRAINT fk_ville_clinique FOREIGN KEY (id_ville)
                              REFERENCES ville (id) ON DELETE SET NULL
);

CREATE TABLE docteur (
                         id SERIAL PRIMARY KEY,
                         licence VARCHAR(50) UNIQUE NOT NULL,
                         nom_complet VARCHAR(150) NOT NULL,
                         id_clinique INT,
                         CONSTRAINT fk_clinique FOREIGN KEY (id_clinique)
                             REFERENCES clinique (id) ON DELETE SET NULL
);

CREATE TABLE patient (
                         id SERIAL PRIMARY KEY, -- Conserve l'ID unique provenant de l'Excel d'origine
                         prenom VARCHAR(100) NOT NULL,
                         nom VARCHAR(100) NOT NULL,
                         sexe VARCHAR(20),
                         date_naissance DATE,
                         langue VARCHAR(50) DEFAULT 'Français',
                         courriel VARCHAR(150),
                         adresse_rue VARCHAR(200),
                         adresse_appart VARCHAR(50),
                         code_postal VARCHAR(20),
                         ramq_date_exp DATE,
                         dossier_no INT,
                         profession VARCHAR(150),
                         date_creation DATE DEFAULT CURRENT_DATE,
                         ne_pas_rappeler BOOLEAN DEFAULT FALSE,
                         est_decede BOOLEAN DEFAULT FALSE,
                         id_docteur INT,
                         id_ville INT,
                         CONSTRAINT fk_docteur FOREIGN KEY (id_docteur)
                             REFERENCES docteur (id) ON DELETE SET NULL,
                         CONSTRAINT fk_ville_patient FOREIGN KEY (id_ville)
                             REFERENCES ville (id) ON DELETE SET NULL
);

CREATE TABLE telephone (
                           id SERIAL PRIMARY KEY,
                           id_patient INT NOT NULL,
                           numero VARCHAR(20) NOT NULL,
                           type_tel VARCHAR(50),
                           CONSTRAINT fk_patient_tel FOREIGN KEY (id_patient)
                               REFERENCES patient (id) ON DELETE CASCADE
);

CREATE TABLE liste_examen (
                              id SERIAL PRIMARY KEY,
                              nom VARCHAR(100) NOT NULL UNIQUE
);

CREATE TABLE examen (
                        id SERIAL PRIMARY KEY,
                        id_patient INT NOT NULL,
                        id_liste_examen INT NOT NULL,
                        date_examen DATE NOT NULL,
                        CONSTRAINT fk_patient_exam FOREIGN KEY (id_patient)
                            REFERENCES patient (id) ON DELETE CASCADE,
                        CONSTRAINT fk_liste_examen FOREIGN KEY (id_liste_examen)
                            REFERENCES liste_examen (id) ON DELETE RESTRICT
);

-- ==============================================================================
-- 3. INSERTION DES DONNÉES (DML)
-- ==============================================================================

-- Villes
INSERT INTO ville (nom, province, pays) VALUES
                                            ('Sherbrooke', 'Québec', 'Canada'),         -- id 1
                                            ('Sorel-Tracy', 'Québec', 'Canada'),        -- id 2
                                            ('St-Charles-de-Drummond', 'Québec', 'Canada'), -- id 3
                                            ('Beauceville', 'Québec', 'Canada'),        -- id 4
                                            ('ST-GUILLAUME', 'Québec', 'Canada'),       -- id 5
                                            ('Drummondville', 'Québec', 'Canada');      -- id 6

-- Cliniques
INSERT INTO clinique (nom, id_ville) VALUES
                                         ('Clinique de l''Estrie', 1),                -- id 1
                                         ('Clinique médical d''Acton Vale', 2),       -- id 2
                                         ('Clinique de la Beauce', 4),                -- id 3
                                         ('Centre Visuel Drummond', 6);               -- id 4

-- Docteurs
INSERT INTO docteur (licence, nom_complet, id_clinique) VALUES
                                                            ('85471', 'Johnny Buck', 1),                 -- id 1
                                                            ('99001', 'Marie-Michelle Dés', 2),          -- id 2
                                                            ('34502', 'Titi Bayeur', 3),                 -- id 3
                                                            ('11223', 'Dr. Hugo Morgan', 4);             -- id 4

-- Liste des examens
INSERT INTO liste_examen (nom) VALUES
                                   ('Complet'),                                 -- id 1
                                   ('Partiel'),                                 -- id 2
                                   ('Urgence oculaire'),                        -- id 3
                                   ('Examen LC'),                               -- id 4
                                   ('Ajustement'),                              -- id 5
                                   ('Champ visuel');                            -- id 6

-- Patients (Échantillon représentatif)
INSERT INTO patient (id, prenom, nom, sexe, date_naissance, langue, courriel, adresse_rue, adresse_appart, code_postal, ramq_date_exp, dossier_no, profession, date_creation, ne_pas_rappeler, est_decede, id_docteur, id_ville) VALUES
                                                                                                                                                                                                                                     (246149, 'Eric', 'Salois-Lefebvre', 'Homme', '1993-08-27', 'Français', 'eric.sl@email.com', '304 rue Beauchesne', NULL, 'J3B0A6', '2025-05-01', 9007, 'Opérateur', '2014-03-24', FALSE, FALSE, 1, 5),
                                                                                                                                                                                                                                     (246150, 'David', 'Despins', 'Homme', '1957-08-19', 'Français', 'david.d@email.com', '3290 rue Des Rouleaux', NULL, 'J3P0A9', '2026-03-01', 106, 'Retraité', '2021-03-26', FALSE, FALSE, 2, 2),
                                                                                                                                                                                                                                     (246151, 'Lynda', 'He', 'Femme', '1959-09-24', 'Français', 'lynda.h@email.com', '423 rue Ferla', 'A', 'J3B4L9', NULL, 15105, 'Comptable', '2010-06-09', FALSE, FALSE, 3, 3),
                                                                                                                                                                                                                                     (246152, 'Diane', 'Lebel', 'Femme', '1980-10-01', 'Français', 'diane.l@email.com', '393 rue Pie IX', NULL, 'J3B1P6', '2027-08-01', 4914, 'Enseignante', '2022-02-15', FALSE, FALSE, 1, 5),
                                                                                                                                                                                                                                     (246153, 'Jacky', 'Boeuf', 'Homme', '1985-11-12', 'Français', 'j.boeuf@email.com', '12 rue de la Forge', NULL, 'J2C1A1', '2024-12-01', 1102, 'Mécanicien', '2023-01-10', FALSE, FALSE, 4, 6),
                                                                                                                                                                                                                                     (246154, 'Sophie', 'Godin', 'Femme', '1990-04-05', 'Français', 'sgodin@email.com', '45 Ave des Érables', 'App 4', 'J2B2B2', '2028-07-15', 3304, 'Infirmière', '2023-05-20', FALSE, FALSE, 4, 6),
                                                                                                                                                                                                                                     (246155, 'Marc', 'Godin', 'Homme', '1988-02-28', 'Français', 'mgodin@email.com', '45 Ave des Érables', 'App 4', 'J2B2B2', '2025-09-30', 3305, 'Technicien IT', '2023-05-20', TRUE, FALSE, 4, 6);

-- Téléphones (Permet de tester les jointures et l'affichage multiple)
INSERT INTO telephone (id_patient, numero, type_tel) VALUES
                                                         (246149, '514-111-1234', 'Cellulaire'),
                                                         (246149, '418-825-4521', 'Maison'),
                                                         (246150, '450-742-1235', 'Maison'),
                                                         (246151, '819-477-1236', 'Cellulaire'),
                                                         (246152, '819-478-1237', 'Cellulaire'),
                                                         (246152, '819-478-4524', 'Travail'),
                                                         (246153, '819-333-4444', 'Cellulaire'),
                                                         (246154, '819-555-6666', 'Cellulaire'),
                                                         (246155, '819-555-7777', 'Cellulaire');

-- Examens passés (Idéal pour tester les requêtes avancées)
INSERT INTO examen (id_patient, id_liste_examen, date_examen) VALUES
                                                                  (246149, 1, '2025-05-01'), -- Eric: Complet
                                                                  (246149, 3, '2020-05-11'), -- Eric: Urgence
                                                                  (246149, 4, '2026-05-15'), -- Eric: Examen LC
                                                                  (246150, 2, '2020-05-01'), -- David: Partiel
                                                                  (246150, 1, '2020-05-12'), -- David: Complet
                                                                  (246151, 4, '2020-05-09'), -- Lynda: Examen LC
                                                                  (246152, 5, '2020-05-10'), -- Diane: Ajustement
                                                                  (246153, 1, '2023-02-15'), -- Jacky: Complet
                                                                  (246154, 6, '2023-06-01'), -- Sophie: Champ visuel
                                                                  (246155, 1, '2023-06-01'); -- Marc: Complet

-- Sync the primary key sequence for the patient table since we inserted explicit IDs
SELECT setval('patient_id_seq', COALESCE((SELECT MAX(id)+1 FROM patient), 1), false);