import { useEffect, useState, type SubmitEvent } from "react";
import { Loading, ErrorMessage, EmptyState } from "../components/StateViews";
import FormField from "../components/FormField";
import { useToast } from "../components/toast/useToast";
import {
  createSchool,
  deleteSchool,
  getSchools,
  updateSchool,
} from "../services/schoolService";
import { ApiError } from "../services/apiError";
import { fieldErrorsFrom, type FieldErrors } from "../utils/problemDetails";
import type { SaveSchoolDto, SchoolDto } from "../types/school";

// Admin: create, read, update and delete schools (#24, R-01)
export default function AdminSchoolsPage() {
  // The list
  const [schools, setSchools] = useState<SchoolDto[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [attempt, setAttempt] = useState(0); // changed to fetch again (retry, and after saving)
  const toast = useToast();

  // The form
  const [form, setForm] = useState<SaveSchoolDto>({
    name: "",
    description: "",
  });
  const [editingId, setEditingId] = useState<number | null>(null); // null = creating a new school
  const [errors, setErrors] = useState<FieldErrors>({});
  const [saving, setSaving] = useState(false);

  useEffect(() => {
    let cancelled = false; // ignore the answer if the page is left before it arrives

    async function fetchSchools() {
      try {
        const data = await getSchools();
        if (!cancelled) setSchools(data);
      } catch (err) {
        if (!cancelled)
          setError(
            err instanceof ApiError ? err.message : "Kunne ikke hente skolene.",
          );
      } finally {
        if (!cancelled) setLoading(false);
      }
    }

    fetchSchools();
    return () => {
      cancelled = true;
    };
  }, [attempt]);

  function reload() {
    setLoading(true);
    setError(null);
    setAttempt((prev) => prev + 1);
  }

  async function handleDelete(school: SchoolDto) {
    // Deleting a school also deletes its questions, job requirements and passed exams (cascade)
    if (
      !window.confirm(
        `Vil du slette ${school.name}? Spørsmålene i skolen blir også slettet.`,
      )
    )
      return;

    try {
      await deleteSchool(school.id);
      // Remove it from the list without fetching everything again
      setSchools((prev) => prev.filter((s) => s.id !== school.id));
      if (editingId === school.id) resetForm();
      toast(`${school.name} er slettet.`, "success");
    } catch (err) {
      toast(
        err instanceof ApiError ? err.message : "Kunne ikke slette skolen.",
        "error",
      );
    }
  }

  // Fills the form with the school, so the next save is an update
  function startEdit(school: SchoolDto) {
    setEditingId(school.id);
    setForm({ name: school.name, description: school.description });
    setErrors({});
  }

  // Empties the form, so the next save creates a new school
  function resetForm() {
    setEditingId(null);
    setForm({ name: "", description: "" });
    setErrors({});
  }

  async function handleSubmit(event: SubmitEvent<HTMLFormElement>) {
    event.preventDefault();

    // Client validation, same rules and messages as SaveSchoolDto in the API
    const clientErrors: FieldErrors = {};
    if (!form.name.trim()) clientErrors.name = "Skriv inn et navn.";
    else if (form.name.length > 100)
      clientErrors.name = "Navnet kan være maks 100 tegn.";
    if (form.description.length > 500)
      clientErrors.description = "Beskrivelsen kan være maks 500 tegn.";
    setErrors(clientErrors);
    if (Object.keys(clientErrors).length > 0 || saving) return;

    setSaving(true);
    try {
      if (editingId === null) {
        await createSchool(form);
        toast("Skolen er opprettet.", "success");
      } else {
        await updateSchool(editingId, form);
        toast("Skolen er oppdatert.", "success");
      }
      resetForm();
      reload(); // fetch the list again so it shows the change
    } catch (err) {
      if (err instanceof ApiError && Object.keys(err.fieldErrors).length > 0) {
        setErrors(fieldErrorsFrom(err.fieldErrors)); // server validation errors at the fields
      } else {
        toast(
          err instanceof ApiError ? err.message : "Kunne ikke lagre skolen.",
          "error",
        );
      }
    } finally {
      setSaving(false);
    }
  }

  if (loading) return <Loading />;
  if (error) return <ErrorMessage message={error} onRetry={reload} />;

  return (
    <>
      <h1>Skoler</h1>

      {/* The same form is used for new and edit. The heading shows which one (R-04) */}
      <form className="card" onSubmit={handleSubmit} noValidate>
        <h2>{editingId === null ? "Ny skole" : "Rediger skole"}</h2>
        <FormField
          label="Navn"
          name="name"
          value={form.name}
          onChange={(e) => setForm({ ...form, name: e.target.value })}
          error={errors.name}
        />
        <FormField
          label="Beskrivelse"
          name="description"
          value={form.description}
          onChange={(e) => setForm({ ...form, description: e.target.value })}
          error={errors.description}
        />
        <button className="btn" type="submit" disabled={saving}>
          {saving ? "Lagrer..." : "Lagre"}
        </button>
        {editingId !== null && (
          <button
            className="btn btn-secondary"
            type="button"
            onClick={resetForm}
          >
            Avbryt
          </button>
        )}
      </form>

      {schools.length === 0 ? (
        <EmptyState text="Ingen skoler ennå." />
      ) : (
        <div className="table-wrap">
          <table className="table">
            <thead>
              <tr>
                <th>Navn</th>
                <th>Beskrivelse</th>
                <th></th>
              </tr>
            </thead>
            <tbody>
              {schools.map((school) => (
                <tr key={school.id}>
                  <td>{school.name}</td>
                  <td>{school.description}</td>
                  <td>
                    <button
                      className="btn btn-secondary"
                      onClick={() => startEdit(school)}
                    >
                      Rediger
                    </button>
                    <button
                      className="btn btn-secondary"
                      onClick={() => handleDelete(school)}
                    >
                      Slett
                    </button>
                  </td>
                </tr>
              ))}
            </tbody>
          </table>
        </div>
      )}
    </>
  );
}
