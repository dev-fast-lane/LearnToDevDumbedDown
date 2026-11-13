import { Link } from "react-router";
import { useAuth } from "../auth/useAuth";
import { useLoad } from "../hooks/useLoad";
import { getStatus } from "../services/gameService";
import { ErrorMessage, Loading } from "../components/StateViews";

// The player's status: current job and passed exams
export default function StartPage() {
  const { user } = useAuth();
  const { data: status, loading, error, reload } = useLoad(getStatus);

  if (loading) return <Loading />;
  if (error || !status) return <ErrorMessage message={error ?? "Fant ikke status."} onRetry={reload} />;

  return (
    <>
      <h1>Hei, {user?.userName}!</h1>
      <p>Bestå eksamener på skolene for å låse opp bedre jobber.</p>

      <div className="cards">
        <section className="card">
          <h2>Jobb</h2>
          {/* R-04: different text with and without a job */}
          {status.currentJob ? <p>Du jobber som {status.currentJob}.</p> : <p>Du har ingen jobb ennå.</p>}
          <Link className="btn" to="/jobs">
            Se jobber
          </Link>
        </section>

        <section className="card">
          <h2>Beståtte eksamener</h2>
          {status.passedSchools.length === 0 ? (
            <p>Du har ikke bestått noen eksamener ennå.</p>
          ) : (
            <ul>
              {status.passedSchools.map((name) => (
                <li key={name}>{name}</li>
              ))}
            </ul>
          )}
          <Link className="btn" to="/schools">
            Gå til skolene
          </Link>
        </section>
      </div>
    </>
  );
}
