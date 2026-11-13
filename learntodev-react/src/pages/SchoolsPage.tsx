import { Link } from "react-router";
import { useLoad } from "../hooks/useLoad";
import { getPlayerSchools } from "../services/gameService";
import { EmptyState, ErrorMessage, Loading } from "../components/StateViews";

// The schools, and which exams the player has passed
export default function SchoolsPage() {
  const { data: schools, loading, error, reload } = useLoad(getPlayerSchools);

  if (loading) return <Loading />;
  if (error || !schools) return <ErrorMessage message={error ?? "Fant ikke skolene."} onRetry={reload} />;

  return (
    <>
      <h1>Skoler</h1>
      <p>Eksamen har 5 spørsmål. Du må ha minst 4 riktige for å bestå.</p>

      {schools.length === 0 ? (
        <EmptyState text="Ingen skoler ennå." />
      ) : (
        <div className="cards">
          {schools.map((school) => (
            <section key={school.id} className="card">
              <h2>{school.name}</h2>
              {/* R-04: passed or not passed */}
              {school.passed ? (
                <span className="badge badge-success">Bestått</span>
              ) : (
                <span className="badge">Ikke bestått</span>
              )}
              <p>{school.description}</p>
              <Link className={school.passed ? "btn btn-secondary" : "btn"} to={`/schools/${school.id}/exam`}>
                {school.passed ? "Ta eksamen igjen" : "Ta eksamen"}
              </Link>
            </section>
          ))}
        </div>
      )}
    </>
  );
}
