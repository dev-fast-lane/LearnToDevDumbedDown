import { useEffect, useState } from "react";
import { Loading, ErrorMessage, EmptyState } from "../components/StateViews";
import { useToast } from "../components/toast/useToast";
import { getPlayerJobs, quitJob, takeJob } from "../services/gameService";
import { ApiError } from "../services/apiError";
import type { PlayerJobDto } from "../types/game";

// Jobbsiden: alle jobbene, åpne eller låste. Spilleren kan ta en åpen jobb og si opp den hen har (#31)
export default function JobsPage() {
  const [jobs, setJobs] = useState<PlayerJobDto[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [attempt, setAttempt] = useState(0); // endres for å hente på nytt (prøv igjen, og etter ta/si opp)
  const [busy, setBusy] = useState(false); // hindrer dobbeltklikk mens et kall pågår
  const toast = useToast();

  useEffect(() => {
    let cancelled = false; // ignorer svaret hvis siden forlates før det kommer

    async function fetchJobs() {
      try {
        const data = await getPlayerJobs();
        if (!cancelled) setJobs(data);
      } catch (err) {
        if (!cancelled)
          setError(
            err instanceof ApiError ? err.message : "Kunne ikke hente jobbene.",
          );
      } finally {
        if (!cancelled) setLoading(false);
      }
    }

    fetchJobs();
    return () => {
      cancelled = true;
    };
  }, [attempt]);

  function reload() {
    setLoading(true);
    setError(null);
    setAttempt((prev) => prev + 1);
  }

  async function handleTake(job: PlayerJobDto) {
    if (busy) return;
    setBusy(true);
    try {
      await takeJob(job.jobId);
      toast(`Du jobber nå som ${job.jobName}.`, "success");
      reload();
    } catch (err) {
      // f.eks. 409 «Mangler eksamen i Backend»
      toast(
        err instanceof ApiError ? err.message : "Kunne ikke ta jobben.",
        "error",
      );
    } finally {
      setBusy(false);
    }
  }

  async function handleQuit() {
    if (busy || !window.confirm("Vil du si opp jobben?")) return;
    setBusy(true);
    try {
      await quitJob();
      toast("Du har sagt opp jobben.", "success");
      reload();
    } catch (err) {
      toast(
        err instanceof ApiError ? err.message : "Kunne ikke si opp jobben.",
        "error",
      );
    } finally {
      setBusy(false);
    }
  }

  if (loading) return <Loading />;
  if (error) return <ErrorMessage message={error} onRetry={reload} />;

  return (
    <>
      <h1>Jobber</h1>
      <p>
        En jobb låses opp når du har bestått eksamen på alle skolene den krever.
      </p>

      {jobs.length === 0 ? (
        <EmptyState text="Ingen jobber ennå." />
      ) : (
        jobs.map((job) => (
          <section key={job.jobId} className="card">
            <h2>{job.jobName}</h2>

            {/* R-04: merket viser om jobben er din, åpen eller låst */}
            {job.isCurrent ? (
              <span className="badge badge-success">Din jobb</span>
            ) : job.isOpen ? (
              <span className="badge">Åpen</span>
            ) : (
              <span className="badge badge-danger">Låst: {job.lockReason}</span>
            )}

            <p>{job.description}</p>
            <p>
              Krever:{" "}
              {job.requiredSchools.length > 0
                ? job.requiredSchools.join(", ")
                : "ingenting"}
            </p>

            {/* R-04: bare knappen som gjelder, vises */}
            {job.isCurrent ? (
              <button
                className="btn btn-secondary"
                disabled={busy}
                onClick={handleQuit}
              >
                Si opp
              </button>
            ) : (
              job.isOpen && (
                <button
                  className="btn"
                  disabled={busy}
                  onClick={() => handleTake(job)}
                >
                  Ta jobben
                </button>
              )
            )}
          </section>
        ))
      )}
    </>
  );
}
