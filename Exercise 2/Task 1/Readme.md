# Exercise 2 - Task 1: Diagnosis Units

## LATENCY ALERT!
If text is displayed poorly, modify "Thread.Sleep(1)" at the end of MonitorPatientStatus() and change it to "*500*". One millisecond was used as in my computer it prompts non-blinking visuals.

## Requisites
All previous excercises requisites, plus:
* Patients must have a 50% chance of requiring diagnosis
* If diagnosis is required, it will occur in 15 seconds each
* Only two diagnosis rooms are available
* Monitoring should show proper updated statuses

## Classes and methods
### Class Hospital
* Create ManagePatient class that will send patients to `Diagnosis` if required, and then send them to `Consulting`
* Monitoring update to reflect proper statuses
* RegisterNewPatient now creates an RNG(50%) boolean to choose if a patien needs diagnosis or not
* RegisterNewPatient sets initial Status based on wether diagnosis is required
* Added DiagnosisRooms as a similar class to ConsultingRooms:
    * Includes a set of maximum available rooms (2 available diagnosis rooms)
    * Includes AttendPatient method which will diagnose a patient and instead of discharging them, sets them as "Awaiting consult"
* [BUGFIX] Monitoring tabs corrected so properties have an adequate spacing

### Class Patient
* Added StartDiagnosisTime to track elapsed diagnosis time
* Added FinishDiagnosisTime to track when diagnosis finished

## Q&A
### 1. Do patients that require consultancy enter to the consultancy by arrival order? Explain what type of tests you realised to check this behaviour.
Patients that arrived before new ones will NOT have preference for consultancy if they have just finished a diagnosis. In fact they will wait until no new patients can enter a diagnosis room to be assigned one. That means that virtually a patient could wait until every other patient has been given a consult if this patient had a diagnosis beforehand.

I checked this via making the consultancy periods longer and simulating 20 patients, this way I could observe how "Awaiting consult" patients that didn't diagnose entered before "Awaiting consult" patiens that were diagnosed.

## Output
![alt text](image-1.png)