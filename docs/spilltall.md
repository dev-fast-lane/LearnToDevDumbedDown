# Spilltall

Tallene ligger i `GameRules` i `LearnToDev.Api/Models/GameRules.cs`. Hvorfor de er valgt, står i [beslutningsloggen](beslutningslogg.md).

## Eksamen

| Regel | Verdi | Konstant |
| --- | --- | --- |
| Spørsmål per eksamen | 5 | `GameRules.ExamQuestionCount` |
| Riktige for bestått | 4 | `GameRules.ExamPassMark` |

- 4–5 av 5 riktige: bestått
- 0–3 av 5 riktige: ikke bestått
- En skole må ha minst 5 spørsmål før eksamen kan tas.
- Nye forsøk er gratis og ubegrensede.

## Jobber

| Jobb | Krever bestått |
| --- | --- |
| IT Help | Ingenting |
| Frontend-utvikler | Frontend |
| Backend-utvikler | Backend |
| Databaseutvikler | Database |
| Fullstack-utvikler | Frontend, Backend og Database |

## Kuttet

Penger, lønn, husleie, boliger, timer per dag, nivå 1–3, eksamenspris og eksamenstid ble kuttet 13.11.2026 (ADR 7).
