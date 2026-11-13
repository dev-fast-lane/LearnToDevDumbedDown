# Seed-format for skoler og spørsmål

Spørsmålene i spillet skrives i JSON-filer som `DbInitializer` (#12) leser inn første gang appen starter.

## Mal

Kopier [`sporsmal-mal.json`](sporsmal-mal.json) når du skal skrive spørsmål for en ny skole. Malen har plassholdere og skal ikke seedes. De ekte filene ligger i `LearnToDev.Api/Data/Seed/`.

## Én fil per skole

Hver skole har sin egen fil, for eksempel `frontend.json` og `backend.json`. Da kan flere skrive spørsmål samtidig uten merge-konflikter.

## Struktur

```
School
└── studyQuestions[]
    └── answerOptions[]
```

Strukturen følger modellene `School` → `StudyQuestion` → `AnswerOption`. Relasjonene kommer av nestingen, så filen har ingen `id`- eller `…Id`-felter. Databasen lager dem.

## Felter

Feltnavnene er de samme som i modellklassene, i camelCase.

| Felt | Type | Regel |
|---|---|---|
| `name` | tekst | Påkrevd, maks 100 tegn |
| `description` | tekst | Maks 500 tegn |
| `studyQuestions[].text` | tekst | Påkrevd, maks 1000 tegn |
| `studyQuestions[].explanation` | tekst | Påkrevd, maks 2000 tegn. Vises etter eksamen |
| `answerOptions[].text` | tekst | Påkrevd, maks 1000 tegn |
| `answerOptions[].isCorrect` | true/false | Se reglene under |

## Regler

- Hver skole har minst 5 spørsmål, ellers kan ikke eksamen tas (`GameRules.ExamQuestionCount`).
- Hvert spørsmål har 2–4 svaralternativer.
- Hvert spørsmål har nøyaktig ett riktig svar (`isCorrect: true`).

## Jobber

Jobbene og skolene de krever, seedes i koden i `DbInitializer`, ikke i JSON. De refererer til skolene med navn, så `Frontend`, `Backend` og `Database` må finnes.

## Innlesing

`System.Text.Json` skiller som standard mellom store og små bokstaver, så `studyQuestions` blir ikke koblet til `StudyQuestions`. Les filene med `new JsonSerializerOptions(JsonSerializerDefaults.Web)`.
