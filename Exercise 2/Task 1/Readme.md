# Exercise 2 - Task 1: Diagnosis Units

## LATENCY ALERT!
If text is displayed poorly, modify "Thread.Sleep(1)" at the end of MonitorPatientStatus() and change it to "*500*". One millisecond was used as in my computer it prompts non-blinking visuals.

## Requisites
All previous excercise requisites, plus:
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
### 1. Did you choose to visualize extra information? Why? Elaborate on what other information could be useful.
I actually decided to show the information in the following fashion:
   
* While `Awaiting`, add how many seconds the patient has waited
* While `In consultancy`, add the expected consultancy time and the time elapsed
* While `Discharged`, add how much time it took and add a counter to say in how much time the patient will be removed from the list

This extra information is extremely useful to snapshot a more accurate picture of the current status as it provides rich information on everything that's going on at the moment.

## Output
![alt text](workingOutput.png)
![alt text](endingOutput.png)