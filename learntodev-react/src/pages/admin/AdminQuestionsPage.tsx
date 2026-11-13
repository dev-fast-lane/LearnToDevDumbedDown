import { useCallback, useState, type SubmitEvent } from "react";
import { useLoad } from "../../hooks/useLoad";
import { getSchools } from "../../services/schoolService";
import { createQuestion, deleteQuestion, searchQuestions, updateQuestion } from "../../services/questionService";
import { ApiError } from "../../services/apiError";
import { useToast } from "../../components/toast/useToast";
import FormField from "../../components/FormField";
import { EmptyState, ErrorMessage, Loading } from "../../components/StateViews";
import { fieldErrorsFrom, type FieldErrors } from "../../utils/problemDetails";
import type { QuestionDto } from "../../types/question";

const OPTION_COUNT = 4;

interface QuestionForm {
  schoolId: string; // the value from <select>, "" = none chosen
  text: string;
  explanation: string;
  options: string[]; // always 4 fields, empty ones are not sent
  correctIndex: number | null;
}

const emptyForm: QuestionForm = {
  schoolId: "",
  text: "",
  explanation: "",
  options: Array(OPTION_COUNT).fill(""),
  correctIndex: null,
};

// Admin: search, create, edit and delete questions (R-01, R-05)
export default function AdminQuestionsPage() {
  const { data: schools } = useLoad(getSchools);

  // The filters that are used. They change when the search form is sent
  const [filter, setFilter] = useState<{ search: string; schoolId: number | null }>({ search: "", schoolId: null });
  const [searchInput, setSearchInput] = useState("");
  const [schoolInput, setSchoolInput] = useState("");
  const load = useCallback(() => searchQuestions(filter.search, filter.schoolId), [filter]);
  const { data: questions, loading, error, reload } = useLoad(load);

  const [form, setForm] = useState<QuestionForm>(emptyForm);
  const [editingId, setEditingId] = useState<number | null>(null);
  const [errors, setErrors] = useState<FieldErrors>({});
  const [saving, setSaving] = useState(false);
  const toast = useToast();

  function handleSearch(event: SubmitEvent<HTMLFormElement>) {
    event.preventDefault();
    setFilter({ search: searchInput, schoolId: schoolInput ? Number(schoolInput) : null });
  }

  function startEdit(question: QuestionDto) {
    const options = question.answerOptions.map((o) => o.text);
    setEditingId(question.id);
    setForm({
      schoolId: String(question.schoolId),
      text: question.text,
      explanation: question.explanation,
      options: [...options, ...Array(OPTION_COUNT - options.length).fill("")],
      correctIndex: question.answerOptions.findIndex((o) => o.isCorrect),
    });
    setErrors({});
    window.scrollTo({ top: 0, behavior: "smooth" });
  }

  function resetForm() {
    setEditingId(null);
    setForm(emptyForm);
    setErrors({});
  }

  function setOption(index: number, value: string) {
    setForm({ ...form, options: form.options.map((o, i) => (i === index ? value : o)) });
  }

  async function handleSubmit(event: SubmitEvent<HTMLFormElement>) {
    event.preventDefault();

    // Only the filled in options are sent. The correct one must be one of them
    const filled = form.options
      .map((text, index) => ({ text: text.trim(), isCorrect: index === form.correctIndex }))
      .filter((o) => o.text !== "");

    // Client validation, same rules as SaveQuestionDto in the API
    const clientErrors: FieldErrors = {};
    if (!form.schoolId) clientErrors.schoolId = "Velg en skole.";
    if (!form.text.trim()) clientErrors.text = "Skriv inn spørsmålet.";
    if (!form.explanation.trim()) clientErrors.explanation = "Skriv inn en forklaring.";
    if (filled.length < 2) clientErrors.answerOptions = "Et spørsmål må ha minst 2 svaralternativer.";
    else if (filled.filter((o) => o.isCorrect).length !== 1)
      clientErrors.answerOptions = "Velg hvilket av de utfylte svarene som er riktig.";
    setErrors(clientErrors);
    if (Object.keys(clientErrors).length > 0 || saving) return;

    const dto = {
      schoolId: Number(form.schoolId),
      text: form.text,
      explanation: form.explanation,
      answerOptions: filled,
    };

    setSaving(true);
    try {
      if (editingId === null) {
        await createQuestion(dto);
        toast("Spørsmålet er opprettet.", "success");
      } else {
        await updateQuestion(editingId, dto);
        toast("Spørsmålet er oppdatert.", "success");
      }
      resetForm();
      reload();
    } catch (err) {
      if (err instanceof ApiError && Object.keys(err.fieldErrors).length > 0) {
        setErrors(fieldErrorsFrom(err.fieldErrors));
      } else {
        toast(err instanceof ApiError ? err.message : "Kunne ikke lagre spørsmålet.", "error");
      }
    } finally {
      setSaving(false);
    }
  }

  async function handleDelete(question: QuestionDto) {
    if (!window.confirm("Vil du slette spørsmålet?")) return;
    try {
      await deleteQuestion(question.id);
      toast("Spørsmålet er slettet.", "success");
      if (editingId === question.id) resetForm();
      reload();
    } catch (err) {
      toast(err instanceof ApiError ? err.message : "Kunne ikke slette spørsmålet.", "error");
    }
  }

  return (
    <>
      <h1>Spørsmål</h1>

      <form className="card" onSubmit={handleSubmit} noValidate>
        <h2>{editingId === null ? "Nytt spørsmål" : "Rediger spørsmål"}</h2>
        <div className="field">
          <label htmlFor="schoolId">Skole</label>
          <select
            id="schoolId"
            value={form.schoolId}
            aria-invalid={!!errors.schoolId}
            onChange={(e) => setForm({ ...form, schoolId: e.target.value })}
          >
            <option value="">Velg skole</option>
            {schools?.map((s) => (
              <option key={s.id} value={s.id}>
                {s.name}
              </option>
            ))}
          </select>
          {errors.schoolId && <span className="field-error">{errors.schoolId}</span>}
        </div>
        <FormField
          label="Spørsmål"
          name="text"
          value={form.text}
          onChange={(e) => setForm({ ...form, text: e.target.value })}
          error={errors.text}
        />
        <FormField
          label="Forklaring (vises etter eksamen)"
          name="explanation"
          value={form.explanation}
          onChange={(e) => setForm({ ...form, explanation: e.target.value })}
          error={errors.explanation}
        />

        <fieldset className="options field">
          <legend>
            <strong>Svaralternativer (velg det riktige)</strong>
          </legend>
          {form.options.map((option, i) => (
            <div key={i} className="row">
              <input
                type="radio"
                name="correct"
                aria-label={`Svar ${i + 1} er riktig`}
                checked={form.correctIndex === i}
                onChange={() => setForm({ ...form, correctIndex: i })}
              />
              <FormField
                label={`Svar ${i + 1}`}
                name={`option-${i}`}
                value={option}
                onChange={(e) => setOption(i, e.target.value)}
              />
            </div>
          ))}
          {errors.answerOptions && <span className="field-error">{errors.answerOptions}</span>}
        </fieldset>

        <div className="row">
          <button className="btn" type="submit" disabled={saving}>
            {saving ? "Lagrer..." : "Lagre"}
          </button>
          {editingId !== null && (
            <button className="btn btn-secondary" type="button" onClick={resetForm}>
              Avbryt
            </button>
          )}
        </div>
      </form>

      {/* Search with two parameters: text and school (R-05) */}
      <form className="row" onSubmit={handleSearch} role="search">
        <FormField
          label="Søk i spørsmål"
          name="search"
          value={searchInput}
          onChange={(e) => setSearchInput(e.target.value)}
        />
        <div className="field">
          <label htmlFor="schoolFilter">Skole</label>
          <select id="schoolFilter" value={schoolInput} onChange={(e) => setSchoolInput(e.target.value)}>
            <option value="">Alle skoler</option>
            {schools?.map((s) => (
              <option key={s.id} value={s.id}>
                {s.name}
              </option>
            ))}
          </select>
        </div>
        <button className="btn" type="submit">
          Søk
        </button>
      </form>

      {loading ? (
        <Loading />
      ) : error || !questions ? (
        <ErrorMessage message={error ?? "Fant ikke spørsmålene."} onRetry={reload} />
      ) : questions.length === 0 ? (
        <EmptyState text="Ingen spørsmål passer søket." />
      ) : (
        <div className="table-wrap">
          <table className="table">
            <thead>
              <tr>
                <th>Skole</th>
                <th>Spørsmål</th>
                <th>Riktig svar</th>
                <th></th>
              </tr>
            </thead>
            <tbody>
              {questions.map((q) => (
                <tr key={q.id}>
                  <td>{q.schoolName}</td>
                  <td>{q.text}</td>
                  <td>{q.answerOptions.find((o) => o.isCorrect)?.text}</td>
                  <td>
                    <div className="row">
                      <button className="btn btn-secondary" onClick={() => startEdit(q)}>
                        Rediger
                      </button>
                      <button className="btn btn-secondary" onClick={() => handleDelete(q)}>
                        Slett
                      </button>
                    </div>
                  </td>
                </tr>
              ))}
            </tbody>
          </table>
        </div>
      )}
    </>
  );
}
