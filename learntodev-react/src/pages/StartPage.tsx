import { useEffect, useState } from "react";
import { Link } from "react-router";
import { useAuth } from "../auth/useAuth";
import { Loading, ErrorMessage } from "../components/StateViews";
import { getStatus } from "../services/gameService";
import { ApiError } from "../services/apiError";
import type { PlayerStatusDto } from "../types/game";

// Startskjermen etter innlogging: viser spillerens nåværende jobb og beståtte eksamener
export default function StartPage() {
  const { user } = useAuth();
  // null til svaret fra API-et har kommet, så siden vet at den fortsatt laster
  const [status, setStatus] = useState<PlayerStatusDto | null>(null);
  const [error, setError] = useState<string | null>(null);

  // Henter statusen én gang når siden åpnes. API-et lager spilleren første gang
  useEffect(() => {
    // Hindrer at en side som allerede er lukket blir oppdatert når svaret kommer
    let cancelled = false;

    async function fetchStatus() {
      try {
        const data = await getStatus();
        if (!cancelled) setStatus(data);
      } catch (err) {
        if (!cancelled) {
          // ApiError har allerede en norsk melding fra apiClient
          setError(err instanceof ApiError ? err.message : "Kunne ikke hente spillet.");
        }
      }
    }

    fetchStatus();
    return () => {
      cancelled = true;
    };
  }, []);

  // Bare én tilstand vises om gangen: feil, laster eller selve siden
  if (error) return <ErrorMessage message={error} />;
  if (!status) return <Loading />;

  return (
      <>
        <h1>Hei, {user?.userName}!</h1>

        {/* currentJobName er null når spilleren ikke har jobb */}
        <section className="card">
          <h2>Jobb</h2>
          {status.currentJobName ? (
              <p>Du jobber som <strong>{status.currentJobName}</strong>.</p>
          ) : (
              <p>Du har ingen jobb ennå. <Link to="/jobs">Se hvilke jobber du kan ta</Link>.</p>
          )}
        </section>

        {/* Viser listen når spilleren har bestått noe, ellers en tekst som viser veien videre */}
        <section className="card">
          <h2>Beståtte eksamener</h2>
          {status.passedSchools.length > 0 ? (
              <ul>
                {/* Skolenavnene er unike i listen, så de kan brukes som key */}
                {status.passedSchools.map((school) => (
                    <li key={school}>{school}</li>
                ))}
              </ul>
          ) : (
              <p>Du har ikke bestått noen eksamener ennå. <Link to="/schools">Velg en skole</Link> for å starte.</p>
          )}
        </section>
      </>
  );
}
