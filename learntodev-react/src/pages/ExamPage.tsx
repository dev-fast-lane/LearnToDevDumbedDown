import { useEffect, useState, type SubmitEvent } from "react";
import { Link, useParams } from "react-router";
import { Loading, ErrorMessage } from "../components/StateViews";
import { startExam, submitExam } from "../services/gameService";
import { ApiError } from "../services/apiError";
import type { ExamDto, ExamResultDto } from "../types/game";

// Eksamen for én skole: 5 spørsmål, lever når alle er besvart, og se resultatet med forklaringer
export default function ExamPage() {
  // schoolId kommer fra URL-en (/schools/:schoolId/exam) som tekst, så vi gjør den om til et tall
  const { schoolId } = useParams();
  const id = Number(schoolId);

  const [exam, setExam] = useState<ExamDto | null>(null);
  const [error, setError] = useState<string | null>(null);
  const [attempt, setAttempt] = useState(0); // økes for å trekke en ny eksamen

  // Valgt svar per spørsmål: spørsmåls-id -> svaralternativ-id
  const [answers, setAnswers] = useState<Record<number, number>>({});
  const [result, setResult] = useState<ExamResultDto | null>(null);
  const [submitting, setSubmitting] = useState(false);
  const [submitError, setSubmitError] = useState<string | null>(null);

  // Trekker eksamen når siden åpnes, og på nytt når attempt endres («Ny eksamen»)
  useEffect(() => {
    if (Number.isNaN(id)) return;
    let cancelled = false; // ignorerer svaret hvis siden er lukket før det kommer

    async function fetchExam() {
      try {
        const data = await startExam(id);
        if (!cancelled) setExam(data);
      } catch (err) {
        if (!cancelled) {
          // 404 (ukjent skole) og 409 (for få spørsmål) har en norsk melding i detail fra API-et
          setError(
            err instanceof ApiError ? err.message : "Kunne ikke hente eksamen.",
          );
        }
      }
    }

    fetchExam();
    return () => {
      cancelled = true;
    };
  }, [id, attempt]);

  // Nullstiller alt og trekker en ny eksamen
  function newExam() {
    setExam(null);
    setError(null);
    setAnswers({});
    setResult(null);
    setSubmitError(null);
    setAttempt((prev) => prev + 1);
  }

  function chooseAnswer(questionId: number, optionId: number) {
    setAnswers((prev) => ({ ...prev, [questionId]: optionId }));
  }

  async function handleSubmit(event: SubmitEvent<HTMLFormElement>) {
    event.preventDefault();
    if (!exam) return;

    setSubmitting(true);
    setSubmitError(null);
    try {
      const data = await submitExam(id, {
        answers: exam.questions.map((q) => ({
          questionId: q.id,
          answerOptionId: answers[q.id],
        })),
      });
      setResult(data);
    } catch (err) {
      // Feilen vises over knappen, så svarene ikke forsvinner
      setSubmitError(
        err instanceof ApiError ? err.message : "Kunne ikke levere eksamen.",
      );
    } finally {
      setSubmitting(false);
    }
  }

  if (Number.isNaN(id))
    return <ErrorMessage message="Ugyldig skole i adressen." />;
  if (error) return <ErrorMessage message={error} onRetry={newExam} />;
  if (!exam) return <Loading text="Trekker spørsmål..." />;

  // Resultatet: bestått eller ikke, og riktig svar med forklaring for hvert spørsmål
  if (result) {
    return (
      <>
        <h1>{result.passed ? "Bestått!" : "Ikke bestått"}</h1>
        <p>
          Du fikk{" "}
          <strong>
            {result.correct} av {result.total}
          </strong>{" "}
          riktige i {exam.schoolName}.
        </p>
        {result.feedback.map((f) => (
          <section className="card" key={f.questionId}>
            <h2>
              {f.correct ? "✔" : "✘"} {f.text}
            </h2>
            <p>
              Riktig svar: <strong>{f.correctAnswer}</strong>
            </p>
            <p>{f.explanation}</p>
          </section>
        ))}
        <button className="btn" type="button" onClick={newExam}>
          Ny eksamen
        </button>{" "}
        <Link to="/">Til startsiden</Link>
      </>
    );
  }

  // Lever-knappen er bare aktiv når alle spørsmålene har et svar
  const allAnswered = exam.questions.every((q) => answers[q.id] !== undefined);

  return (
    <>
      <h1>Eksamen: {exam.schoolName}</h1>
      <p>
        Svar på alle {exam.questions.length} spørsmålene. Du trenger 4 riktige
        for å bestå.
      </p>

      <form onSubmit={handleSubmit}>
        {exam.questions.map((q, index) => (
          // fieldset + legend gjør at skjermlesere leser spørsmålet sammen med svarene
          <fieldset className="card" key={q.id}>
            <legend>
              {index + 1}. {q.text}
            </legend>
            {q.options.map((o) => (
              <label key={o.id} style={{ display: "block" }}>
                <input
                  type="radio"
                  name={`question-${q.id}`}
                  checked={answers[q.id] === o.id}
                  onChange={() => chooseAnswer(q.id, o.id)}
                />{" "}
                {o.text}
              </label>
            ))}
          </fieldset>
        ))}

        {submitError && <p className="field-error">{submitError}</p>}
        <button
          className="btn"
          type="submit"
          disabled={!allAnswered || submitting}
        >
          {submitting ? "Leverer..." : "Lever eksamen"}
        </button>
      </form>
    </>
  );
}
