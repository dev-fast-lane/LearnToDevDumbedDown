# Funksjonsoversikt

LearnToDev er et lite læringsspill. Spilleren tar eksamener på skoler, og beståtte eksamener låser opp jobber. Tall og regler er bestemt i [beslutningsloggen](beslutningslogg.md) og [spilltall](spilltall.md). Hvis noe her er uklart, er det de som gjelder.

Prosjektet ble forenklet 13.11.2026 (ADR 7 i beslutningsloggen). Penger, bolig, timer, nivåer, Fullstack, kompendium, øving, arbeidsdager, achievements, klokke og leaderboard er kuttet.

## Roller

| Rolle | Kan |
| --- | --- |
| Gjest | Logge inn og registrere seg |
| Spiller | Se skolene, ta eksamen, se jobbene, ta en åpen jobb og si opp |
| Admin | Alt spilleren kan, og i tillegg CRUD for skoler, spørsmål og jobber, og søk i spørsmål |

## Spiller

- En spiller (`Player`) opprettes automatisk første gang brukeren åpner spillet. Én bruker har én spiller.
- Startsiden viser nåværende jobb og beståtte eksamener.

## Skoler og eksamen

- Det finnes tre skoler: Frontend, Backend og Database. Admin kan legge til flere.
- Hver skole har flervalgsspørsmål med 2–4 svaralternativer, der nøyaktig ett er riktig.
- En eksamen trekker 5 tilfeldige spørsmål fra skolen. 4 eller 5 riktige gir bestått.
- Serveren retter eksamen, og riktig svar sendes ikke til nettleseren før eksamen er levert.
- Etter levering ser spilleren resultatet, riktig svar og en forklaring for hvert spørsmål.
- Man kan ta eksamen så mange ganger man vil. En bestått eksamen lagres én gang (`PassedExam`).

## Jobber

| Jobb | Krever bestått |
| --- | --- |
| IT Help | Ingenting |
| Frontend-utvikler | Frontend |
| Backend-utvikler | Backend |
| Databaseutvikler | Database |
| Fullstack-utvikler | Frontend, Backend og Database |

- En jobb er åpen når spilleren har bestått alle skolene den krever. Ellers er den låst, med en årsak som «Mangler eksamen i Backend og Frontend».
- Spilleren kan ha én jobb om gangen, og kan si den opp.
- Admin bestemmer hvilke skoler en jobb krever (N:M mellom `Job` og `School`).

## Admin

- CRUD for skoler (`/admin/schools`), spørsmål (`/admin/questions`) og jobber (`/admin/jobs`).
- Søk i spørsmål på tekst og filter på skole.
- Validering på både klient og server, med samme regler.
