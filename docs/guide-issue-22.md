# Guide: Issue #22 – CRUD for StudyQuestion med svaralternativer

Målet er at admin kan **opprette, vise, redigere og slette** spørsmål, og hvert spørsmål har 2–4 svaralternativer der nøyaktig ett er riktig.

Guiden gir deg *retning og forklaring*, ikke ferdig kode. Der det står **Prøv selv** skal du skrive koden før du leser hintet. Der det står **Sjekkpunkt** stopper du og tester før du går videre. Da finner du feil mens de fortsatt er små.

> Den gamle løsningen ligger på `origin/23-crud-for-studyquestion-med-svaralternativer`, men den bruker den gamle modellen (`SchoolLevelId`). Bruk den bare som fasit **etter** at du har prøvd selv: `git show origin/23-crud-for-studyquestion-med-svaralternativer:<filsti>`.

---

## 0. Forstå hva som allerede finnes

Før du skriver noe, les disse filene og svar på spørsmålene for deg selv:

| Fil | Spørsmål å svare på |
|---|---|
| `LearnToDev.Api/Models/StudyQuestion.cs` | Hvilke felt har et spørsmål? Hva er forskjellen på `SchoolId` og `School`? |
| `LearnToDev.Api/Models/AnswerOption.cs` | Hvordan vet et svaralternativ hvilket spørsmål det hører til? |
| `LearnToDev.Api/Repositories/StudyQuestionRepository.cs` | Hva gjør `Include`? Hva skjer med de gamle svaralternativene i `UpdateAsync`? |
| `LearnToDev.Api/Controllers/AuthController.cs` | Hvordan returneres 400 med feltfeil (`ValidationProblem`)? |
| `LearnToDev.Api/Controllers/AdminController.cs` | Hvor står rollesjekken, og hvorfor gjelder den alle actions? |

**Viktig innsikt:** Repositoryet er allerede ferdig. Jobben din er laget *over* det (DTO-er, validering, controller) og *under* det i React (service, typer, skjema).

```
React (AdminPage → QuestionForm)
   │  questionService.ts  ── fetch via apiClient
   ▼
QuestionsController  ── tar imot SaveQuestionDto, sender QuestionDto
   │  IStudyQuestionRepository (finnes)
   ▼
ApplicationDbContext → SQLite
```

Lag branch først (navnet følger konvensjonen dere bruker):

```bash
git switch -c 22-crud-for-studyquestion-med-svaralternativer
```

---

## 1. DTO-er – hvorfor ikke bare sende modellen?

Du kunne returnert `StudyQuestion` rett fra controlleren. Men det gir tre problemer:

1. **Sykler i JSON:** `StudyQuestion.AnswerOptions[0].StudyQuestion.AnswerOptions...` → serialiseringen krasjer eller blir enorm.
2. **Overposting:** Hvis du tar imot modellen i POST, kan klienten sende med `Id` eller felt du ikke ville at de skulle styre.
3. **Kontrakt:** API-et blir låst til databasen. Endrer du modellen, brekker React.

Derfor lager vi **to sett** DTO-er (krav R-09):

- **Lese-DTO** (det API-et *sender*): `QuestionDto` og `AnswerOptionDto`. Inneholder `Id`, og gjerne `SchoolName` så React slipper et ekstra kall.
- **Skrive-DTO** (det API-et *tar imot*): `SaveQuestionDto` og `SaveAnswerOptionDto`. **Ingen `Id`** – id-en kommer fra URL-en (`PUT api/questions/5`), og svaralternativene får nye id-er av databasen.

**Prøv selv:** Lag `LearnToDev.Api/DTOs/QuestionDtos.cs` med de fire typene.

<details><summary>Hint</summary>

- Lese-DTO-ene kan være `record` på én linje: `public record AnswerOptionDto(int Id, string Text, bool IsCorrect);`
- Skrive-DTO-ene bør være `class` med `{ get; set; }`, fordi validering med attributter fungerer best der.
- Se `RegisterDto.cs` for hvordan dere skriver norske feilmeldinger i attributtene.
</details>

---

## 2. Validering – tre nivåer

Tenk på hvilke regler som gjelder og *hvor* de hører hjemme:

| Regel | Hvor | Verktøy |
|---|---|---|
| Teksten må fylles inn, maks 1000 tegn | Ett felt | `[Required]`, `[StringLength]` |
| Det må være 2–4 svaralternativer | Ett felt (listen) | `[MinLength(2)]`, `[MaxLength(4)]` |
| Nøyaktig **ett** alternativ er riktig | Flere felt sammen | `IValidatableObject` eller egen validator |
| Skolen må finnes i databasen | Trenger databasen | I controlleren |

Lengdene bør matche modellen (`StudyQuestion.Text` er `StringLength(1000)` osv.), ellers kan API-et godta noe databasen ikke vil lagre.

**"Nøyaktig ett riktig"** – issuet nevner en `StudyQuestionValidator`. Du har to gode valg:

- **A) `IValidatableObject` på `SaveQuestionDto`:** `[ApiController]` kjører den automatisk og svarer 400. Minst kode.
- **B) Egen statisk klasse `StudyQuestionValidator`:** En metode som tar inn DTO-en og returnerer feilmeldinger. Lettere å teste isolert, og kan gjenbrukes.

Jeg anbefaler **A, og at `Validate` kaller B**. Da får du både automatikken og en klasse som er lett å teste.

**Prøv selv:** Skriv regelen. Tenk over: hva skjer hvis `AnswerOptions` er tom? Hvis to er riktige? Hvis ingen er riktige?

<details><summary>Hint</summary>

`AnswerOptions.Count(o => o.IsCorrect) != 1` dekker både "ingen" og "flere". Bruk `new ValidationResult("...", new[] { nameof(AnswerOptions) })` så feilen havner på riktig felt i responsen.
</details>

**Sjekkpunkt:** `dotnet build` uten advarsler.

---

## 3. Mapping – fra modell til DTO og tilbake

Du trenger to oversettelser:

- `StudyQuestion` → `QuestionDto` (når du sender)
- `SaveQuestionDto` → `StudyQuestion` (når du lagrer)

Lag dem som små statiske metoder, for eksempel i `DTOs/QuestionMapper.cs` som extension-metoder (`question.ToDto()`).

**Fallgruve:** Når du mapper `SaveQuestionDto` → `StudyQuestion`, skal svaralternativene **ikke** få `Id`. Se på `UpdateAsync` i repositoryet: det sletter de gamle alternativene og legger inn de nye. Hvis de nye hadde en gammel id, ville EF prøvd å oppdatere en rad som akkurat ble slettet.

**Spørsmål til deg selv:** Hvorfor krever `ToDto()` at `School` er lastet med `Include`? (Hint: hva er `question.School.Name` hvis den ikke er det?)

---

## 4. QuestionsController

Lag `LearnToDev.Api/Controllers/QuestionsController.cs`. Bruk `AuthController` og `AdminController` som mal for oppsettet:

- `[ApiController]`, `[Route("api/questions")]`, `[Authorize(Roles = Roles.Admin)]` **på klassen**
- Injiser `IStudyQuestionRepository`, `ISchoolRepository` (for å sjekke at skolen finnes) og `ILogger<QuestionsController>`

### Hvilke statuskoder skal hver action gi?

Dette er pensum-stoff. Prøv å fylle ut tabellen selv før du ser fasiten.

| Action | Suksess | Feil |
|---|---|---|
| `GET api/questions` | ? | – |
| `GET api/questions/{id}` | ? | ? |
| `POST api/questions` | ? | ? |
| `PUT api/questions/{id}` | ? | ? |
| `DELETE api/questions/{id}` | ? | ? |

<details><summary>Fasit</summary>

| Action | Suksess | Feil |
|---|---|---|
| GET liste | `200 Ok(liste)` | – |
| GET én | `200 Ok(dto)` | `404 NotFound()` |
| POST | `201 CreatedAtAction(nameof(GetById), new { id }, dto)` | `400` ugyldig input eller skole finnes ikke |
| PUT | `204 NoContent()` (eller `200` med dto) | `400`, `404` |
| DELETE | `204 NoContent()` | `404` |

`201 Created` setter en `Location`-header som peker på den nye ressursen. Det er REST-måten å si "her er den du lagde".
</details>

### Skolen finnes ikke → 400, ikke 500

Hvis `SchoolId = 999` sendes, feiler fremmednøkkelen i databasen og du får en 500-feil. Det er klientens feil, så det skal være 400. Sjekk først:

```csharp
// Skisse – fyll inn selv
if (await _schools.GetByIdAsync(dto.SchoolId) == null)
{
    ModelState.AddModelError(nameof(dto.SchoolId), "Skolen finnes ikke.");
    return ValidationProblem(ModelState);
}
```

Da kommer feilen i samme format som de andre valideringsfeilene, og React kan vise den under riktig felt.

### Logging (R-10)

Logg det som *endrer* data, med strukturerte parametere (ikke string-interpolering):

```csharp
_logger.LogInformation("Spørsmål {QuestionId} opprettet i skole {SchoolId}", created.Id, created.SchoolId);
```

Hvorfor `{QuestionId}` og ikke `$"...{created.Id}"`? Fordi loggverktøy da kan søke på `QuestionId` som et felt.

### Fallgruve i POST

Etter `AddAsync` er `created.School` *ikke* lastet (du satte bare `SchoolId`). Hent spørsmålet på nytt med `GetByIdAsync(created.Id)` før du mapper til DTO, ellers får du en null-feil på `School.Name`.

**Sjekkpunkt – test i Swagger:**

1. `dotnet run --project LearnToDev.Api` og åpne `/swagger`
2. Kall `GET /api/questions` uten å være logget inn → skal gi **401**
3. Logg inn med `POST /api/auth/login` som admin (testbrukerne står i `Data/IdentitySeeder.cs`). Cookien blir lagret i nettleseren.
4. `GET /api/questions` → **200** med seed-spørsmålene
5. `POST` med to riktige svar → **400** med feilmeldingen din
6. `POST` med `schoolId: 999` → **400**, ikke 500
7. Logg inn som spiller og prøv igjen → **403**

Får du alle disse, er backend ferdig.

---

## 5. Tester (R-12)

Lag `LearnToDev.Tests/DTOs/StudyQuestionValidatorTests.cs` (eller under `Validators/`). Følg mønsteret i `JobServiceTests.cs`: én test per regel, med navn på formen `Metode_Situasjon_Forventet`.

Testene du minst bør ha:

- ✅ Gyldig spørsmål med ett riktig svar → ingen feil
- ❌ Ingen riktige svar → feil
- ❌ To riktige svar → feil
- ❌ Bare ett svaralternativ → feil (hvis du tester `MinLength` også)

<details><summary>Hint: teste data-annotasjoner uten controller</summary>

```csharp
var results = new List<ValidationResult>();
var ok = Validator.TryValidateObject(dto, new ValidationContext(dto), results, validateAllProperties: true);
```

Dette kjører både attributtene og `IValidatableObject.Validate`. OBS: `Validate` kjøres bare hvis attributtene først går gjennom.
</details>

**Bonus:** En controllertest med Moq (`Tests/Controllers/` er klar) som sjekker at `GetById` gir `NotFoundResult` når repositoryet returnerer `null`.

**Sjekkpunkt:** `dotnet test` er grønn.

---

## 6. React – type, service, skjema

Jobb deg fra bunnen og opp, akkurat som i backend.

### 6a. Typer – `src/types/question.ts`

Speil DTO-ene. API-et sender **camelCase** (`schoolName`, ikke `SchoolName`). Lag både `QuestionDto` (det du får) og `SaveQuestionDto` (det du sender).

### 6b. Service – `src/services/questionService.ts`

Se på `jobService.ts`. Du trenger fem funksjoner: `getQuestions`, `getQuestion(id)`, `createQuestion(dto)`, `updateQuestion(id, dto)`, `deleteQuestion(id)`. Bruk `get`, `post`, `put` og `del` fra `apiClient`.

**Hvorfor dette laget?** Komponentene skal aldri kalle `fetch` direkte (R-08). Da er det ett sted å endre hvis URL-en endres, og feilhåndteringen i `apiClient` gjelder overalt.

### 6c. Liste i AdminPage

Bruk **nøyaktig samme mønster som `JobsPage.tsx`**: `loading` / `error` / tom liste / tabell, med `cancelled`-flagget og `retry`. Les kommentarene der – de forklarer hvorfor hver del finnes.

Legg til en "Rediger"- og "Slett"-knapp per rad. For sletting: spør med `window.confirm(...)` først, og vis en toast (`useToast`) etterpå.

### 6d. QuestionForm – den vanskeligste delen

Lag `src/components/QuestionForm.tsx`. Det samme skjemaet brukes til både *ny* og *rediger*: ta inn et valgfritt `question`-prop. Finnes det, fyll feltene og bruk `updateQuestion`. Ellers bruker du `createQuestion`.

Utfordringer du må løse, og tips:

1. **Liste i state.** Svaralternativene er en `array` i state. Endre aldri arrayen direkte. Lag en ny:
   ```ts
   setOptions(prev => prev.map((o, i) => i === index ? { ...o, text: value } : o));
   ```
   Spør deg selv: hvorfor trigger ikke `options[index].text = value` en ny render?
2. **Riktig svar.** Bruk radioknapper med samme `name`. Da kan bare ett velges, og regelen "nøyaktig ett riktig" håndheves allerede i UI-et. (API-et sjekker det likevel. Stol aldri bare på klienten.)
3. **Legg til / fjern alternativ.** Deaktiver "Legg til" ved 4 og "Fjern" ved 2, så UI-et matcher valideringen.
4. **Skolevalg.** Det finnes ikke noe `GET /api/schools` ennå. Det kommer med #24 (CRUD for School). Snakk med den som har #24. Frem til da kan du bruke et tallfelt for `schoolId`, eller lage dropdownen når endepunktet er på plass.
5. **Feltfeil fra API-et.** Fang `ApiError` og bruk `fieldErrorsFrom(err.fieldErrors)` fra `utils/problemDetails.ts`. Se hvordan `RegisterPage.tsx` gjør det. Vis feilene med `FormField` sin `error`-prop.
6. **Etter lagring.** Kall en `onSaved`-callback-prop så `AdminPage` kan hente listen på nytt (øk `attempt`, akkurat som `retry`).

**Sjekkpunkt – test i nettleseren** (`npm run dev` i `learntodev-react`, API-et må kjøre):

- Opprett et spørsmål → dukker opp i listen, toast vises
- Rediger og bytt riktig svar → lagres
- Slett → forsvinner
- Tøm spørsmålsteksten og lagre → feilmeldingen vises under feltet
- Logg inn som spiller og gå til `/admin` → stoppes av `RequireAdmin`

---

## 7. Før du lager PR

- [ ] `dotnet build` og `npm run build` uten advarsler (issue #44)
- [ ] `dotnet test` grønn
- [ ] `npm run lint` grønn
- [ ] Huk av sjekklisten i issue #22. Det siste punktet ("tilpasset den nye modellen") er oppfylt fordi du brukte `SchoolId`
- [ ] PR-beskrivelse med `Closes #22`, og fyll ut PR-malen
- [ ] Skriv i `docs/KI-logg.md` at du brukte denne guiden

---

## Spørsmål å kunne svare på etterpå (eksamensforberedelse)

1. Hvorfor har vi separate DTO-er for lesing og skriving?
2. Hva er forskjellen på 400, 401, 403 og 404, og hvilken av dem gir controlleren din når?
3. Hvorfor sjekker vi `SchoolId` i controlleren i stedet for å la databasen feile?
4. Hvorfor validerer vi både i React og i API-et?
5. Hva gjør `[ApiController]` automatisk for deg?
6. Hvorfor må state i React oppdateres med en ny array i stedet for å endre den eksisterende?
