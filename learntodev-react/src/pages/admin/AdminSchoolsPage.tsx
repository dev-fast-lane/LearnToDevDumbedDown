import { useState, type SubmitEvent } from "react";
import { useLoad } from "../../hooks/useLoad";
import { createSchool, deleteSchool, getSchools, updateSchool } from "../../services/schoolService";
import { ApiError } from "../../services/apiError";
import { useToast } from "../../components/toast/useToast";
import FormField from "../../components/FormField";
import { EmptyState, ErrorMessage, Loading } from "../../components/StateViews";
import { fieldErrorsFrom, type FieldErrors } from "../../utils/problemDetails";
import type { SaveSchoolDto, SchoolDto } from "../../types/school";

const emptyForm: SaveSchoolDto = { name: "", description: "" };

// Admin: create, read, update and delete schools (R-01)
export default function AdminSchoolsPage() {
  const { data: schools, loading, error, reload } = useLoad(getSchools);
  const [form, setForm] = useState<SaveSchoolDto>(emptyForm);
  const [editingId, setEditingId] = useState<number | null>(null); // null = creating a new one
  const [errors, setErrors] = useState<FieldErrors>({});
  const [saving, setSaving] = useState(false);
  const toast = useToast();

  function startEdit(school: SchoolDto) {
    setEditingId(school.id);
    setForm({ name: school.name, description: school.description });
    setErrors({});
  }

  function resetForm() {
    setEditingId(null);
    setForm(emptyForm);
    setErrors({});
  }

  async function handleSubmit(event: SubmitEvent<HTMLFormElement>) {
    event.preventDefault();

    // Client validation, same rules as SaveSchoolDto in the API
    const clientErrors: FieldErrors = {};
    if (!form.name.trim()) clientErrors.name = "Skriv inn et navn.";
    else if (form.name.length > 100) clientErrors.name = "Navnet kan være maks 100 tegn.";
    if (form.description.length > 500) clientErrors.description = "Beskrivelsen kan være maks 500 tegn.";
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
      reload();
    } catch (err) {
      if (err instanceof ApiError && Object.keys(err.fieldErrors).length > 0) {
        setErrors(fieldErrorsFrom(err.fieldErrors)); // server validation errors at the fields
      } else {
        toast(err instanceof ApiError ? err.message : "Kunne ikke lagre skolen.", "error");
      }
    } finally {
      setSaving(false);
    }
  }

  async function handleDelete(school: SchoolDto) {
    if (!window.confirm(`Vil du slette ${school.name}? Spørsmålene i skolen blir også slettet.`)) return;
    try {
      await deleteSchool(school.id);
      toast(`${school.name} er slettet.`, "success");
      if (editingId === school.id) resetForm();
      reload();
    } catch (err) {
      toast(err instanceof ApiError ? err.message : "Kunne ikke slette skolen.", "error");
    }
  }

  return (
    <>
      <h1>Skoler</h1>

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
        <div className="row">
          <button className="btn" type="submit" disabled={saving}>
            {saving ? "Lagrer..." : "Lagre"}
          </button>
          {editingId !== null && (
            <button className="btn btn-secondary" type="button" onClick={resetForm}>
              Avbryt
            </button>
          )}
        </div>
      </form>

      {loading ? (
        <Loading />
      ) : error || !schools ? (
        <ErrorMessage message={error ?? "Fant ikke skolene."} onRetry={reload} />
      ) : schools.length === 0 ? (
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
                    <div className="row">
                      <button className="btn btn-secondary" onClick={() => startEdit(school)}>
                        Rediger
                      </button>
                      <button className="btn btn-secondary" onClick={() => handleDelete(school)}>
                        Slett
                      </button>
                    </div>
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
