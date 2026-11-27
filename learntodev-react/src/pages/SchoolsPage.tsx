import { useEffect, useState } from "react";
import { Link } from "react-router";
import { Loading, ErrorMessage, EmptyState } from "../components/StateViews";
import { getGameSchools } from "../services/gameService";
import { ApiError } from "../services/apiError";
import type { PlayerSchoolDto } from "../types/game";

// Skoleoversikten (#27, R-04): ett kort per skole med om eksamen er bestått, og vei videre til eksamen
export default function SchoolsPage() {
  // null til svaret fra API-et har kommet, så siden vet at den fortsatt laster
  const [schools, setSchools] = useState<PlayerSchoolDto[] | null>(null);
  const [error, setError] = useState<string | null>(null);

  // Henter skolene én gang når siden åpnes
  useEffect(() => {
    let cancelled = false; // ignorerer svaret hvis siden er lukket før det kommer

    async function fetchSchools() {
      try {
        const data = await getGameSchools();
        if (!cancelled) setSchools(data);
      } catch (err) {
        if (!cancelled)
          setError(
            err instanceof ApiError ? err.message : "Kunne ikke hente skolene.",
          );
      }
    }

    fetchSchools();
    return () => {
      cancelled = true;
    };
  }, []);

  if (error) return <ErrorMessage message={error} />;
  if (!schools) return <Loading />;

  return (
    <>
      <h1>Skoler</h1>
      {schools.length === 0 ? (
        <EmptyState text="Det finnes ingen skoler ennå." />
      ) : (
        schools.map((school) => (
          <section className="card" key={school.id}>
            <h2>{school.name}</h2>

            {/* Betinget rendering (R-04): merket og knappeteksten avhenger av om eksamen er bestått */}
            <p>
              <span className={school.passed ? "badge badge-success" : "badge"}>
                {school.passed ? "Bestått" : "Ikke bestått"}
              </span>
            </p>

            <p>{school.description}</p>
            <Link className="btn" to={`/schools/${school.id}/exam`}>
              {school.passed ? "Ta eksamen igjen" : "Ta eksamen"}
            </Link>
          </section>
        ))
      )}
    </>
  );
}
