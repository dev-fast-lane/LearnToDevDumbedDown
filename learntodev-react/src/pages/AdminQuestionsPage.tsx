import { useEffect, useState, type FormEvent } from "react";
import FormField from "../components/FormField";
import QuestionForm from "../components/QuestionForm";
import { Loading, ErrorMessage, EmptyState } from "../components/StateViews";
import { useToast } from "../components/toast/useToast";
import { ApiError } from "../services/apiError";
import { deleteQuestion, getQuestions } from "../services/questionService";
import { getSchools } from "../services/schoolService";
import type { QuestionDto } from "../types/question";
import type { SchoolDto } from "../types/school";

// Which form is open: none, a new question, or editing one
type FormMode = { kind: "closed" } | { kind: "new" } | { kind: "edit"; question: QuestionDto };

// The search that has been sent (#25). Changes only when admin presses Søk, not on every key
type Filters = { search: string; schoolId?: number };

// Admin: create, read, update, delete (#22, R-01) and search (#25, R-05) study questions
export default function AdminQuestionsPage() {
  const [questions, setQuestions] = useState<QuestionDto[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [attempt, setAttempt] = useState(0); // Changing it fetches the list again
  const [form, setForm] = useState<FormMode>({ kind: "closed" });
  const [schools, setSchools] = useState<SchoolDto[]>([]);
  // What is in the search fields right now. "" in schoolFilter means all schools
  const [searchText, setSearchText] = useState("");
  const [schoolFilter, setSchoolFilter] = useState("");
  const [filters, setFilters] = useState<Filters>({ search: "" });
  const toast = useToast();

  // Same pattern as JobsPage: loading / error / empty / table
  useEffect(() => {
    let cancelled = false; // ignore the answer if the page is left before it arrives

    async function fetchQuestions() {
      try {
        const data = await getQuestions(filters.search, filters.schoolId);
        if (!cancelled) setQuestions(data);
      } catch (err) {
        if (!cancelled) {
          setError(err instanceof ApiError ? err.message : "Kunne ikke hente spørsmålene.");
        }
      } finally {
        if (!cancelled) setLoading(false);
      }
    }

    fetchQuestions();
    return () => {
      cancelled = true;
    };
  }, [attempt, filters]); // a new search fetches again

  // The schools for the dropdown, fetched once. Same as in QuestionForm
  useEffect(() => {
    let cancelled = false;

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

  function reload() {
    setLoading(true);
    setError(null);
    setAttempt((prev) => prev + 1);
  }

  function handleSearch(e: FormEvent) {
    e.preventDefault(); // otherwise the browser reloads the page
    setLoading(true);
    setError(null);
    setFilters({
      search: searchText.trim(),
      schoolId: schoolFilter === "" ? undefined : Number(schoolFilter),
    });
  }

  function handleReset() {
    setSearchText("");
    setSchoolFilter("");
    setLoading(true);
    setError(null);
    setFilters({ search: "" });
  }

  function handleSaved() {
    setForm({ kind: "closed" });
    reload();
  }

  async function handleDelete(question: QuestionDto) {
    if (!window.confirm(`Slette spørsmålet «${question.text}»?`)) return;
    try {
      await deleteQuestion(question.id);
      toast("Spørsmålet er slettet.", "success");
      if (form.kind === "edit" && form.question.id === question.id) setForm({ kind: "closed" });
      reload();
    } catch (err) {
      toast(err instanceof ApiError ? err.message : "Kunne ikke slette spørsmålet.", "error");
    }
  }

  function renderList() {
    if (loading) return <Loading />;
    if (error) return <ErrorMessage message={error} onRetry={reload} />;
    if (questions.length === 0) {
      // An empty search result is not the same as no questions at all
      const isFiltered = filters.search !== "" || filters.schoolId !== undefined;
      return <EmptyState text={isFiltered ? "Ingen spørsmål passer søket" : "Ingen spørsmål enda."} />;
    }

    return (
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
                <td style={{ whiteSpace: "nowrap" }}>
                  <button className="btn btn-secondary" onClick={() => setForm({ kind: "edit", question: q })}>
                    Rediger
                  </button>{" "}
                  <button className="btn btn-secondary" onClick={() => handleDelete(q)}>
                    Slett
                  </button>
                </td>
              </tr>
            ))}
          </tbody>
        </table>
      </div>
    );
  }

  return (
    <>
      <h1>Spørsmål</h1>

      <form className="card" onSubmit={handleSearch} role="search" aria-label="Søk i spørsmål">
        <FormField
          label="Søk i tekst"
          name="search"
          type="search"
          value={searchText}
          onChange={(e) => setSearchText(e.target.value)}
        />
        <div className="field">
          <label htmlFor="schoolFilter">Skole</label>
          <select id="schoolFilter" value={schoolFilter} onChange={(e) => setSchoolFilter(e.target.value)}>
            <option value="">Alle skoler</option>
            {schools.map((school) => (
              <option key={school.id} value={school.id}>
                {school.name}
              </option>
            ))}
          </select>
        </div>
        <button type="submit" className="btn">
          Søk
        </button>{" "}
        <button type="button" className="btn btn-secondary" onClick={handleReset}>
          Nullstill
        </button>
      </form>

      {form.kind === "closed" ? (
        <p>
          <button className="btn" onClick={() => setForm({ kind: "new" })}>
            Nytt spørsmål
          </button>
        </p>
      ) : (
        // key makes React create a fresh form when switching question, so the fields are filled from the new one
        <QuestionForm
          key={form.kind === "edit" ? form.question.id : "new"}
          question={form.kind === "edit" ? form.question : undefined}
          onSaved={handleSaved}
          onCancel={() => setForm({ kind: "closed" })}
        />
      )}

      {renderList()}
    </>
  );
}
