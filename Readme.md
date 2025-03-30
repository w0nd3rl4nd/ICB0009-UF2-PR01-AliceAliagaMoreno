# 🏥 ICB0009-UF2-PR01 - Hospital Simulation System
by Alice Aliaga Moreno

## 📋 Project Overview
This C# simulation models a hospital's patient flow with priority-based resource allocation. Key features include:

* __4 medical consultancy rooms__ with doctor availability
* __2 diagnostic machines__ for priority testing
* __Smart patient prioritization__ (Emergency > Urgency > General)
* Real-time monitoring dashboard
* Statistical analysis of operations

Built with `.NET 6.0` using multi-threading and synchronization primitives for realistic resource management.

---

## 🧮 Core Components

### 🚨 Priority Management System
* __Three tier priority levels:__
**`1 - Emergency`** | **`2 - Urgency`** | **`3 - General`**
* __Dual sorting:__
    1. Priority level
    2. Arrival time

    Implemented with `SortedSet<T>` with custom `IComparer<Patient>`

    ```
    // Custom comparer for priority queue
    private class PatientPriorityComparer : IComparer<Patient> {
        public int Compare(Patient? x, Patient? y) {
            if (x is null || y is null) throw new ArgumentException("Invalid patients");
            return x.Priority == y.Priority 
                ? x.ArrivalNumber.CompareTo(y.ArrivalNumber) 
                : x.Priority.CompareTo(y.Priority);
        }
    }
    ```

---

### 📊 Key Metrics Tracked

| Metric                | Calculation Method |
| --------------------- | ------------------ |
| Patient Wait Times    | From arrival to consultancy start |
| Diagnostic Usage      | (Total diagnosis time) / (Simulation duration × Machines) × 100 |
| Priority Distribution | Count per priority level |

---

### 🚦 Execution Workflow
1. **Patient Registration**
    * Auto-generates every 2 seconds
    * Random: ID(1-100), Priority(1-3), ConsultancyTime(10-20s)
2. **Diagnosis Phase** (50% patients)
    * 15-second fixed diagnostic time
3. **Consultancy Phase**
    * Priority-based queue management
4. **Discharge**
    * Automatic removal after 10-second cooldown

---

### 📈 Statistics Output Example 

```
=== Hospital Simulation Statistics ===
Patients Treated:
- Emergency: 12
- Urgency: 18 
- General: 20

Average Wait Times:
- Emergency: 4.7s
- Urgency: 8.2s
- General: 12.5s

Diagnostic Machine Utilization: 68.3%
```

---

### 🚀 Getting Started

#### ⚙️ Requirements
* .NET 9.0 SDK
* Git version control

#### 📥 Installation

```
git clone https://github.com/alicealiagamoreno/hospital-sim.git
cd hospital-sim/Exercise2/Task4
dotnet run
```

#### 🎚️ Runtime Controls

**🔧 Modify quantity of patients**

* At Program.cs, search for ```Hospital.SimulatePatientArrival(50);```
* Adjust 50 to the desired number of random patients

```Hospital.SimulatePatientArrival(50); // Default 50 for proper simulation```

**🔧 Modify refresh rate**

* At Hospital.cs, search for ```Thread.Sleep(1);```
* Adjust to 500 if there is visual glitching. Mind 500 millis will also glitch but it might remain more consistent

```Thread.Sleep(1); // Default 1 for virtually no blinking```