import { useEffect, useState } from "react";
import { Link } from "react-router";
import { Loading, ErrorMessage, EmptyState } from "../components/StateViews";
import { getSchools } from "../services/schoolService";
import { ApiError } from "../services/apiError";
import type { SchoolDto } from "../types/school";

// Midlertidig skoleliste med vei inn til eksamen (#28). Den ordentlige skoleoversikten kommer i #27
export default function SchoolsPage() {
  // null til svaret fra API-et har kommet, så siden vet at den fortsatt laster
  const [schools, setSchools] = useState<SchoolDto[] | null>(null);
  const [error, setError] = useState<string | null>(null);

  // Henter skolene én gang når siden åpnes
  useEffect(() => {
    let cancelled = false; // ignorerer svaret hvis siden er lukket før det kommer

    async function fetchSchools() {
      try {
        const data = await getSchools();
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
            <p>{school.description}</p>
            <Link className="btn" to={`/schools/${school.id}/exam`}>
              Ta eksamen
            </Link>
          </section>
        ))
      )}
    </>
  );
}
