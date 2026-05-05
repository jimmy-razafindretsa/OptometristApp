# OptometristApp Developer & Workflow Guide

Welcome to the development guide for the **OptometristApp**. This document explains the system's full end-to-end architecture, database schema, data access layer (C# ADO.NET), and the Blazor frontend workflow (including the recent additions for sorting, search filtering, and detail views).

---

## 1. High-Level Architecture Overview

The application is built on **C# .NET 10.0** using **Blazor Server** with PostgreSQL as the database.

```
Browser (Blazor UI) <==== WebSocket / SignalR ====> Blazor Server App <==== Npgsql ADO.NET ====> PostgreSQL DB
```

- **Frontend (Blazor Server)**: Component-based UI. Interactive components run with `@rendermode InteractiveServer`, maintaining a real-time WebSocket connection (SignalR) to the server.
- **Database Access (ADO.NET)**: Instead of an ORM like Entity Framework, the app uses raw SQL queries with **Npgsql** for high-performance, predictable database interactions.
- **Environment & Hosting**: Orchestrated via **Docker Compose** (`compose.yaml`). It maps environment variables to configure the database credentials dynamically.

---

## 2. Database Schema & Auto-IDs

The schema is defined in `sql/init.sql`. Primary entities use auto-incrementing keys (`SERIAL PRIMARY KEY`). 

### Core Tables Diagram
```
 VILLE (id, nom, province)
   ▲
   │ (located_in)
 CLINIQUE (id, nom, id_ville)
   ▲
   │ (associated_with)
 DOCTEUR (id, licence, nom_complet, id_clinique)
   ▲
   │ (referred_by)
 PATIENT (id, prenom, nom, id_ville, id_docteur)
   ▲
   │ (performs_on)
 EXAMEN (id, date_examen, id_patient, id_type) <── (is_of_type) ── TYPE_EXAMEN (id, nom)
```

### Auto-ID Management
Entities like `Patient` previously required entering IDs manually. They have been migrated to database-native auto-incrementing serials:
1. **Schema Update**: Changed the patient `id` column to `SERIAL PRIMARY KEY`.
2. **Sequence Sync**: Because seed data has manual IDs, we synchronize the underlying PostgreSQL sequence during startup using:
   ```sql
   SELECT setval('patient_id_seq', (SELECT MAX(id) FROM patient));
   ```
3. **C# Service Update**: Removed `id` from `INSERT` queries, letting the database generate it automatically.

---

## 3. Data Access Layer (C# & SQL Queries)

All database operations are managed via scoped service classes inside the `Data/` directory.

### Key Queries & Join Logic

#### A. Patient Detail View (`GetPatientDetailAsync`)
Retrieves comprehensive details for a single patient by joining across four different tables (`patient`, `ville`, `docteur`, and `clinique`), plus fetching associated phone numbers and exams.

```csharp
// 1. Fetch Patient, City, and referring Doctor
await using var cmdPat = new NpgsqlCommand(@"
    SELECT p.*, v.nom as ville_nom, v.province as ville_prov, d.nom_complet as doc_nom, d.licence as doc_lic, d.id_clinique as doc_clin_id
    FROM patient p
    LEFT JOIN ville v ON p.id_ville = v.id
    LEFT JOIN docteur d ON p.id_docteur = d.id
    WHERE p.id = @id", conn);
cmdPat.Parameters.AddWithValue("id", id);

// 2. Fetch all exams with their types
await using var cmdExams = new NpgsqlCommand(@"
    SELECT e.*, te.nom as type_nom 
    FROM examen e
    JOIN type_examen te ON e.id_type = te.id
    WHERE e.id_patient = @id", conn);
```

#### B. Doctor Detail View (`GetDoctorDetailAsync`)
Retrieves doctor details, their assigned clinic, and all patients assigned to them:

```csharp
// 1. Fetch Doctor and Clinic
await using var cmdDoc = new NpgsqlCommand(@"
    SELECT d.*, c.id as clin_id, c.nom as clin_nom, c.id_ville as clin_ville_id
    FROM docteur d
    LEFT JOIN clinique c ON d.id_clinique = c.id
    WHERE d.id = @id", conn);

// 2. Fetch all Patients referred to this Doctor
await using var cmdPats = new NpgsqlCommand(@"
    SELECT * FROM patient WHERE id_docteur = @id", conn);
```

---

## 4. Frontend Layer & UI Workflow

All UI pages reside in `Components/Pages/`. The core layout is driven by reactive Blazor component lifecycles.

### Lifecycle of a Page (e.g., `DoctorsPatients.razor`)
1. **Initialization (`OnInitializedAsync`)**:
   Asynchronously fetches the initial collections (`Patients`, `Doctors`, `Cities`, `Clinics`) from the C# services:
   ```csharp
   protected override async Task OnInitializedAsync()
   {
       _patients = await PatientService.GetPatientsAsync();
       _doctors = await DoctorService.GetDoctorsAsync();
   }
   ```
2. **Interactive Search & Sorting (Dynamic LINQ)**:
   Rather than hammering the database with a SQL `LIKE` or `ORDER BY` query on every keypress, search filtering and sorting are handled in **C# memory** using computed properties. This keeps the app incredibly fast:

   ```csharp
   private string _searchQueryPatients = "";
   private string _sortColumnPatients = "Profession";
   private bool _sortAscendingPatients = true;

   private IEnumerable<Patient>? SortedPatients
   {
       get
       {
           if (_patients == null) return null;

           // 1. Filter by search input
           var filtered = string.IsNullOrWhiteSpace(_searchQueryPatients)
               ? _patients
               : _patients.Where(p => p.FullName.Contains(_searchQueryPatients, StringComparison.OrdinalIgnoreCase));

           // 2. Sort the filtered subset
           if (_sortColumnPatients == "Profession")
               return _sortAscendingPatients ? filtered.OrderBy(p => p.Profession) : filtered.OrderByDescending(p => p.Profession);
           
           return filtered;
       }
   }
   ```

3. **Reactivity & Rendering**:
   - The `@bind:event="oninput"` attribute binds input fields instantly as the user types, refreshing the computed list in real-time.
   - Clickable table headers (`@onclick='() => SortPatients("Profession")'`) change the sorting state variables, instantly re-evaluating the computed list and updating the screen without a full page reload.

---

## 5. Summary of Recent Improvements
- **Serial Auto-incrementing IDs**: Zero manual ID input required. Forms are cleaner, safer, and database-driven.
- **Interactive UI Sorting**: Sort exams by date/type, patients by job profession, and doctors by license ID directly in memory.
- **Dynamic Search Bars**: Real-time filtering by doctor and patient names.
- **Broken Navigation Fixed**: Added `@rendermode InteractiveServer` to detail pages, restoring all button events and page transition flows.
