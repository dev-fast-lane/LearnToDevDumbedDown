import { useEffect, useState } from "react";
import QuestionForm from "../components/QuestionForm";
import { Loading, ErrorMessage, EmptyState } from "../components/StateViews";
import { useToast } from "../components/toast/useToast";
import { ApiError } from "../services/apiError";
import { deleteQuestion, getQuestions } from "../services/questionService";
import type { QuestionDto } from "../types/question";

// Which form is open: none, a new question, or editing one
type FormMode = { kind: "closed" } | { kind: "new" } | { kind: "edit"; question: QuestionDto };

export default function AdminPage() {
  const [questions, setQuestions] = useState<QuestionDto[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [attempt, setAttempt] = useState(0); // Changing it fetches the list again
  const [form, setForm] = useState<FormMode>({ kind: "closed" });
  const toast = useToast();

  // Same pattern as JobsPage: loading / error / empty / table
  useEffect(() => {
    let cancelled = false; // ignore the answer if the page is left before it arrives

    async function fetchQuestions() {
      try {
        const data = await getQuestions();
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
  }, [attempt]);

  function reload() {
    setLoading(true);
    setError(null);
    setAttempt((prev) => prev + 1);
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
    if (questions.length === 0) return <EmptyState text="Ingen spørsmål enda." />;

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
      <h1>Admin</h1>
      <h2>Spørsmål</h2>

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
