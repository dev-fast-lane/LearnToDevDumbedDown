import { useCallback, useState, type SubmitEvent } from "react";
import { Link, useParams } from "react-router";
import { useLoad } from "../hooks/useLoad";
import { startExam, submitExam } from "../services/gameService";
import { ApiError } from "../services/apiError";
import { useToast } from "../components/toast/useToast";
import { ErrorMessage, Loading } from "../components/StateViews";
import type { ExamResultDto } from "../types/game";

// One exam: answer 5 questions, hand in, and see the result. The server grades it (R-03)
export default function ExamPage() {
  const schoolId = Number(useParams().schoolId);
  const load = useCallback(() => startExam(schoolId), [schoolId]);
  const { data: exam, loading, error, reload } = useLoad(load);

  // questionId -> chosen answerOptionId
  const [answers, setAnswers] = useState<Record<number, number>>({});
  const [result, setResult] = useState<ExamResultDto | null>(null);
  const [submitting, setSubmitting] = useState(false);
  const toast = useToast();

  if (loading) return <Loading text="Trekker spørsmål..." />;
  if (error || !exam) return <ErrorMessage message={error ?? "Fant ikke eksamen."} onRetry={reload} />;

  const allAnswered = exam.questions.every((q) => answers[q.id] !== undefined);

  async function handleSubmit(event: SubmitEvent<HTMLFormElement>) {
    event.preventDefault();
    if (!exam || !allAnswered || submitting) return; // client validation: every question must be answered

    setSubmitting(true);
    try {
      const dto = {
        answers: exam.questions.map((q) => ({ questionId: q.id, answerOptionId: answers[q.id] })),
      };
      setResult(await submitExam(exam.schoolId, dto));
      window.scrollTo({ top: 0 }); // the result starts at the top of the page
    } catch (err) {
      toast(err instanceof ApiError ? err.message : "Kunne ikke levere eksamen.", "error");
    } finally {
      setSubmitting(false);
    }
  }

  function newExam() {
    setAnswers({});
    setResult(null);
    reload();
  }

  // R-04: the result replaces the questions after hand-in
  if (result) {
    return (
      <>
        <h1>Resultat: {exam.schoolName}</h1>
        <div className="card" role="status">
          <h2>
            {result.correct} av {result.total} riktige
          </h2>
          {result.passed ? (
            <span className="badge badge-success">Bestått</span>
          ) : (
            <span className="badge badge-danger">Ikke bestått</span>
          )}
        </div>

        {result.feedback.map((f, i) => (
          <section key={f.questionId} className="card">
            <h3>
              {i + 1}. {f.text}
            </h3>
            <p>
              {f.correct ? (
                <span className="badge badge-success">Riktig</span>
              ) : (
                <span className="badge badge-danger">Feil. Riktig svar: {f.correctAnswer}</span>
              )}
            </p>
            <p className="muted">{f.explanation}</p>
          </section>
        ))}

        <div className="row">
          <button className="btn" onClick={newExam}>
            Ny eksamen
          </button>
          <Link className="btn btn-secondary" to="/schools">
            Tilbake til skolene
          </Link>
        </div>
      </>
    );
  }

  return (
    <form onSubmit={handleSubmit} noValidate>
      <h1>Eksamen: {exam.schoolName}</h1>

      {exam.questions.map((question, i) => (
        <fieldset key={question.id} className="card options">
          <legend>
            <strong>
              {i + 1}. {question.text}
            </strong>
          </legend>
          {question.options.map((option) => (
            <label key={option.id}>
              <input
                type="radio"
                name={`question-${question.id}`}
                checked={answers[question.id] === option.id}
                onChange={() => setAnswers((prev) => ({ ...prev, [question.id]: option.id }))}
              />{" "}
              {option.text}
            </label>
          ))}
        </fieldset>
      ))}

      {!allAnswered && <p className="muted">Svar på alle spørsmålene før du leverer.</p>}
      <button className="btn" type="submit" disabled={!allAnswered || submitting}>
        {submitting ? "Leverer..." : "Lever eksamen"}
      </button>
    </form>
  );
}
