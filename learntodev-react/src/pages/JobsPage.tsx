import { useState } from "react";
import { useLoad } from "../hooks/useLoad";
import { getPlayerJobs, quitJob, takeJob } from "../services/gameService";
import { ApiError } from "../services/apiError";
import { useToast } from "../components/toast/useToast";
import { EmptyState, ErrorMessage, Loading } from "../components/StateViews";
import type { PlayerJobDto } from "../types/game";

// Every job, open or locked. The player can take an open job or quit the current one
export default function JobsPage() {
  const { data: jobs, loading, error, reload } = useLoad(getPlayerJobs);
  const [busy, setBusy] = useState(false);
  const toast = useToast();

  async function run(action: () => Promise<unknown>, success: string) {
    if (busy) return;
    setBusy(true);
    try {
      await action();
      toast(success, "success");
      reload();
    } catch (err) {
      // e.g. 409 "Mangler eksamen i Backend"
      toast(err instanceof ApiError ? err.message : "Noe gikk galt. Prøv igjen.", "error");
    } finally {
      setBusy(false);
    }
  }

  if (loading) return <Loading />;
  if (error || !jobs) return <ErrorMessage message={error ?? "Fant ikke jobbene."} onRetry={reload} />;

  return (
    <>
      <h1>Jobber</h1>
      <p>En jobb låses opp når du har bestått eksamen på alle skolene den krever.</p>

      {jobs.length === 0 ? (
        <EmptyState text="Ingen jobber ennå." />
      ) : (
        <div className="cards">
          {jobs.map((job) => (
            <section key={job.id} className="card">
              <h2>{job.name}</h2>
              <JobBadge job={job} />
              <p>{job.description}</p>
              <p className="muted">
                Krever: {job.requiredSchools.length > 0 ? job.requiredSchools.join(", ") : "ingenting"}
              </p>

              {/* R-04: the button depends on the job's state */}
              {job.isCurrent ? (
                <button className="btn btn-secondary" disabled={busy} onClick={() => run(quitJob, "Du sa opp jobben.")}>
                  Si opp
                </button>
              ) : (
                job.isOpen && (
                  <button
                    className="btn"
                    disabled={busy}
                    onClick={() => run(() => takeJob(job.id), `Du jobber nå som ${job.name}.`)}
                  >
                    Ta jobben
                  </button>
                )
              )}
            </section>
          ))}
        </div>
      )}
    </>
  );
}

function JobBadge({ job }: { job: PlayerJobDto }) {
  if (job.isCurrent) return <span className="badge badge-success">Din jobb</span>;
  if (job.isOpen) return <span className="badge">Åpen</span>;
  return <span className="badge badge-danger">Låst: {job.lockReason}</span>;
}
