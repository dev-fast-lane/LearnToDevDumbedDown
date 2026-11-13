# Prosjektplan

Planen vår ligger på GitHub-prosjektet **Kanban LearnToDevDumbedDown**, og ikke i en egen fil. Der er den alltid oppdatert, og den henger sammen med sakene vi faktisk jobber med.

## Hvor finner du hva

| Hva | Hvor |
| --- | --- |
| Alle oppgaver og status | [Kanban-brettet](https://github.com/orgs/dev-fast-lane/projects/4) |
| Fremdrift per uke | [Milestones](https://github.com/dev-fast-lane/LearnToDevDumbedDown/milestones) |
| Arbeidsprosess og regler | [README](../README.md#arbeidsprosess) |

## Tidsplan

Hver sak har feltet **Uke** og en milestone som viser når den skal være ferdig.

LearnToDev er Subapp 2: et ASP.NET Core Web API (`LearnToDev.Api`) med en React-app (Vite) som klient. Hver funksjonssak dekker både endepunktet og React-skjermen.

13.11.2026 ble prosjektet forenklet til en quiz-app med skoler, eksamen og jobber (#58, ADR 7 i [beslutningsloggen](beslutningslogg.md)). Planen under er oppdatert etter det.

| Uke | Datoer | Milestone | Hovedinnhold |
| --- | --- | --- | --- |
| U43 | 19.–25. okt | Grunnmur | Spillregler, struktur, modell, migrering, Identity, feilhåndtering, seed-format |
| U44 | 26. okt–1. nov | Backend-start | Testprosjekt og spillregler |
| U45 | 2.–8. nov | API- og React-grunnmur | API-prosjektet, innlogging, repositories, seed, React-oppsett |
| U46 | 9.–15. nov | Forenkling, admin og spill | Forenklingen, CRUD, søk, skoler, eksamen og jobber |
| U47 | 16.–22. nov | Rapport og rydding | Rapportkapitlene, kravmatrise og funksjonsoversikt |
| U48 | 23.–30. nov | Levering | Resten av rapporten, test på ren maskin, innlevering |

## Slik holder vi planen oppdatert

- Endringer i omfang eller uke gjøres på selve saken (feltet *Uke* og milestone).
- Milestonene oppdateres da av seg selv, så det finnes ikke en egen plan som kan bli utdatert.
