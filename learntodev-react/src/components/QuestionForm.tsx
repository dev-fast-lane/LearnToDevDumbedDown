import { useEffect, useState, type SubmitEvent } from "react";
import FormField from "./FormField";
import { useToast } from "./toast/useToast";
import { ApiError } from "../services/apiError";
import { createQuestion, updateQuestion } from "../services/questionService";
import { getSchools } from "../services/schoolService";
import { fieldErrorsFrom, type FieldErrors } from "../utils/problemDetails";
import type { QuestionDto, SaveAnswerOptionDto } from "../types/question";
import type { SchoolDto } from "../types/school";

// Same limits as SaveQuestionDto in the API
const MIN_OPTIONS = 2;
const MAX_OPTIONS = 4;

type Props = {
  // Given -> edit that question. Missing -> create a new one
  question?: QuestionDto;
  onSaved: () => void;
  onCancel?: () => void;
};

const emptyOptions = (): SaveAnswerOptionDto[] => [
  { text: "", isCorrect: true },
  { text: "", isCorrect: false },
];

// The parent gives this a new key when it switches question, so the state below starts fresh from the props
export default function QuestionForm({ question, onSaved, onCancel }: Props) {
  const isEdit = question !== undefined;
  const [schoolId, setSchoolId] = useState(question ? String(question.schoolId) : "");
  const [text, setText] = useState(question?.text ?? "");
  const [explanation, setExplanation] = useState(question?.explanation ?? "");
  const [options, setOptions] = useState<SaveAnswerOptionDto[]>(
    question
      ? question.answerOptions.map((o) => ({ text: o.text, isCorrect: o.isCorrect }))
      : emptyOptions(),
  );
  const [errors, setErrors] = useState<FieldErrors>({});
  const [submitting, setSubmitting] = useState(false);
  const [schools, setSchools] = useState<SchoolDto[]>([]);
  const toast = useToast();

  // The schools for the dropdown (GET api/schools from #24)
  useEffect(() => {
    let cancelled = false; // ignore the answer if the form is closed before it arrives

    async function fetchSchools() {
      try {
        const data = await getSchools();
        if (!cancelled) setSchools(data);
      } catch {
        if (!cancelled) toast("Kunne ikke hente skolene.", "error");
      }
    }

    fetchSchools();
    return () => {
      cancelled = true;
    };
  }, [toast]);

  // Never change the array in place: React compares the reference, so a mutated array gives no re-render
  function setOptionText(index: number, value: string) {
    setOptions((prev) => prev.map((o, i) => (i === index ? { ...o, text: value } : o)));
  }

  // Radio buttons: exactly one is correct, so the rule is kept in the UI too (the API still checks it)
  function setCorrect(index: number) {
    setOptions((prev) => prev.map((o, i) => ({ ...o, isCorrect: i === index })));
  }

  function addOption() {
    setOptions((prev) => [...prev, { text: "", isCorrect: false }]);
  }

  function removeOption(index: number) {
    setOptions((prev) => {
      const next = prev.filter((_, i) => i !== index);
      // If the correct one was removed, mark the first as correct so there is still exactly one
      if (!next.some((o) => o.isCorrect)) next[0] = { ...next[0], isCorrect: true };
      return next;
    });
  }

  async function handleSubmit(event: SubmitEvent<HTMLFormElement>) {
    event.preventDefault();

    // Client validation first, same rules as SaveQuestionDto in the API
    const clientErrors: FieldErrors = {};
    if (!schoolId || Number(schoolId) < 1) clientErrors.schoolId = "Velg en skole.";
    if (!text.trim()) clientErrors.text = "Skriv inn spørsmålet.";
    if (!explanation.trim()) clientErrors.explanation = "Skriv inn en forklaring.";
    options.forEach((o, i) => {
      if (!o.text.trim()) clientErrors[`answerOptions[${i}].Text`] = "Skriv inn svaralternativet.";
    });
    if (options.filter((o) => o.isCorrect).length !== 1)
      clientErrors.answerOptions = "Nøyaktig ett svar må være riktig.";
    setErrors(clientErrors);
    if (Object.keys(clientErrors).length > 0) return;
    if (submitting) return; // guard against double submit

    setSubmitting(true);
    const dto = { schoolId: Number(schoolId), text, explanation, answerOptions: options };
    try {
      if (isEdit) {
        await updateQuestion(question.id, dto);
        toast("Spørsmålet er oppdatert.", "success");
      } else {
        await createQuestion(dto);
        toast("Spørsmålet er opprettet.", "success");
      }
      onSaved();
    } catch (error) {
      if (error instanceof ApiError && Object.keys(error.fieldErrors).length > 0) {
        setErrors(fieldErrorsFrom(error.fieldErrors)); // errors at each field
      } else {
        toast(error instanceof ApiError ? error.message : "Noe gikk galt. Prøv igjen.", "error");
      }
    } finally {
      setSubmitting(false); // always re-enable, also after an error
    }
  }

  return (
    <form className="card" onSubmit={handleSubmit} noValidate>
      <h2>{isEdit ? "Rediger spørsmål" : "Nytt spørsmål"}</h2>

      <div className="field">
        <label htmlFor="schoolId">Skole</label>
        <select
          id="schoolId"
          name="schoolId"
          value={schoolId}
          onChange={(e) => setSchoolId(e.target.value)}
          aria-invalid={!!errors.schoolId}
          aria-describedby={errors.schoolId ? "schoolId-error" : undefined}
        >
          <option value="">Velg skole</option>
          {schools.map((school) => (
            <option key={school.id} value={school.id}>
              {school.name}
            </option>
          ))}
        </select>
        {errors.schoolId && (
          <span id="schoolId-error" className="field-error">
            {errors.schoolId}
          </span>
        )}
      </div>
      <FormField
        label="Spørsmål"
        name="text"
        maxLength={1000}
        value={text}
        onChange={(e) => setText(e.target.value)}
        error={errors.text}
      />

      <div className="field">
        <label htmlFor="explanation">Forklaring (vises etter eksamen)</label>
        <textarea
          id="explanation"
          name="explanation"
          rows={3}
          maxLength={2000}
          value={explanation}
          onChange={(e) => setExplanation(e.target.value)}
          aria-invalid={!!errors.explanation}
          aria-describedby={errors.explanation ? "explanation-error" : undefined}
        />
        {errors.explanation && (
          <span id="explanation-error" className="field-error">
            {errors.explanation}
          </span>
        )}
      </div>

      <fieldset className="field">
        <legend>Svaralternativer (velg det riktige)</legend>
        {options.map((option, index) => {
          const optionError = errors[`answerOptions[${index}].Text`];
          return (
            <div key={index}>
              <div style={{ display: "flex", gap: "0.5rem", alignItems: "center" }}>
                <input
                  type="radio"
                  name="correct"
                  aria-label={`Alternativ ${index + 1} er riktig`}
                  checked={option.isCorrect}
                  onChange={() => setCorrect(index)}
                />
                <input
                  aria-label={`Alternativ ${index + 1}`}
                  maxLength={1000}
                  style={{ flex: 1 }}
                  value={option.text}
                  onChange={(e) => setOptionText(index, e.target.value)}
                  aria-invalid={!!optionError}
                />
                <button
                  type="button"
                  className="btn btn-secondary"
                  onClick={() => removeOption(index)}
                  disabled={options.length <= MIN_OPTIONS}
                >
                  Fjern
                </button>
              </div>
              {optionError && <span className="field-error">{optionError}</span>}
            </div>
          );
        })}
        {errors.answerOptions && <span className="field-error">{errors.answerOptions}</span>}
        <div>
          <button
            type="button"
            className="btn btn-secondary"
            onClick={addOption}
            disabled={options.length >= MAX_OPTIONS}
          >
            Legg til alternativ
          </button>
        </div>
      </fieldset>

      <div style={{ display: "flex", gap: "0.5rem" }}>
        <button className="btn" type="submit" disabled={submitting}>
          {submitting ? "Lagrer..." : "Lagre"}
        </button>
        {onCancel && (
          <button type="button" className="btn btn-secondary" onClick={onCancel}>
            Avbryt
          </button>
        )}
      </div>
    </form>
  );
}
