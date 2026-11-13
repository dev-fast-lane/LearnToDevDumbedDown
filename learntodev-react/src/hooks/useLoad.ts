import { useCallback, useEffect, useState } from "react";
import { ApiError } from "../services/apiError";

interface LoadState<T> {
  data: T | null;
  loading: boolean;
  error: string | null;
}

// Fetches data when the page opens, and keeps track of loading and errors.
// `load` must be stable (a service function, or wrapped in useCallback), or it fetches on every render
export function useLoad<T>(load: () => Promise<T>) {
  const [state, setState] = useState<LoadState<T>>({ data: null, loading: true, error: null });
  const [attempt, setAttempt] = useState(0); // changed by reload to fetch again

  useEffect(() => {
    let cancelled = false; // ignore the answer if the page is left before it arrives

    load()
      .then((data) => {
        if (!cancelled) setState({ data, loading: false, error: null });
      })
      .catch((err: unknown) => {
        if (!cancelled) {
          const message = err instanceof ApiError ? err.message : "Noe gikk galt. Prøv igjen.";
          setState({ data: null, loading: false, error: message });
        }
      });

    return () => {
      cancelled = true;
    };
  }, [load, attempt]);

  const reload = useCallback(() => {
    setState((prev) => ({ ...prev, loading: true, error: null }));
    setAttempt((prev) => prev + 1);
  }, []);

  return { ...state, reload };
}
