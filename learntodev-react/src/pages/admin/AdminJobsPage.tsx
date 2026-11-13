import { useState, type SubmitEvent } from "react";
import { useLoad } from "../../hooks/useLoad";
import { getSchools } from "../../services/schoolService";
import { createJob, deleteJob, getJobs, updateJob } from "../../services/jobService";
import { ApiError } from "../../services/apiError";
import { useToast } from "../../components/toast/useToast";
import FormField from "../../components/FormField";
import { EmptyState, ErrorMessage, Loading } from "../../components/StateViews";
import { fieldErrorsFrom, type FieldErrors } from "../../utils/problemDetails";
import type { JobDto, SaveJobDto } from "../../types/job";

const emptyForm: SaveJobDto = { name: "", description: "", requiredSchoolIds: [] };

// Admin: create, read, update and delete jobs, and choose the schools they require (R-01, R-02)
export default function AdminJobsPage() {
  const { data: schools } = useLoad(getSchools);
  const { data: jobs, loading, error, reload } = useLoad(getJobs);
  const [form, setForm] = useState<SaveJobDto>(emptyForm);
  const [editingId, setEditingId] = useState<number | null>(null);
  const [errors, setErrors] = useState<FieldErrors>({});
  const [saving, setSaving] = useState(false);
  const toast = useToast();

  function startEdit(job: JobDto) {
    setEditingId(job.id);
    setForm({
      name: job.name,
      description: job.description,
      requiredSchoolIds: job.requiredSchools.map((s) => s.id),
    });
    setErrors({});
  }

  function resetForm() {
    setEditingId(null);
    setForm(emptyForm);
    setErrors({});
  }

  function toggleSchool(schoolId: number) {
    const ids = form.requiredSchoolIds;
    setForm({
      ...form,
      requiredSchoolIds: ids.includes(schoolId) ? ids.filter((id) => id !== schoolId) : [...ids, schoolId],
    });
  }

  async function handleSubmit(event: SubmitEvent<HTMLFormElement>) {
    event.preventDefault();

    // Client validation, same rules as SaveJobDto in the API
    const clientErrors: FieldErrors = {};
    if (!form.name.trim()) clientErrors.name = "Skriv inn et navn.";
    else if (form.name.length > 100) clientErrors.name = "Navnet kan være maks 100 tegn.";
    if (form.description.length > 500) clientErrors.description = "Beskrivelsen kan være maks 500 tegn.";
    setErrors(clientErrors);
    if (Object.keys(clientErrors).length > 0 || saving) return;

    setSaving(true);
    try {
      if (editingId === null) {
        await createJob(form);
        toast("Jobben er opprettet.", "success");
      } else {
        await updateJob(editingId, form);
        toast("Jobben er oppdatert.", "success");
      }
      resetForm();
      reload();
    } catch (err) {
      if (err instanceof ApiError && Object.keys(err.fieldErrors).length > 0) {
        setErrors(fieldErrorsFrom(err.fieldErrors));
      } else {
        toast(err instanceof ApiError ? err.message : "Kunne ikke lagre jobben.", "error");
      }
    } finally {
      setSaving(false);
    }
  }

  async function handleDelete(job: JobDto) {
    if (!window.confirm(`Vil du slette ${job.name}?`)) return;
    try {
      await deleteJob(job.id);
      toast(`${job.name} er slettet.`, "success");
      if (editingId === job.id) resetForm();
      reload();
    } catch (err) {
      toast(err instanceof ApiError ? err.message : "Kunne ikke slette jobben.", "error");
    }
  }

  return (
    <>
      <h1>Jobber</h1>

      <form className="card" onSubmit={handleSubmit} noValidate>
        <h2>{editingId === null ? "Ny jobb" : "Rediger jobb"}</h2>
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
        <fieldset className="options field">
          <legend>
            <strong>Krever eksamen i</strong>
          </legend>
          {schools?.map((school) => (
            <label key={school.id}>
              <input
                type="checkbox"
                checked={form.requiredSchoolIds.includes(school.id)}
                onChange={() => toggleSchool(school.id)}
              />{" "}
              {school.name}
            </label>
          ))}
          {errors.requiredSchoolIds && <span className="field-error">{errors.requiredSchoolIds}</span>}
        </fieldset>
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
      ) : error || !jobs ? (
        <ErrorMessage message={error ?? "Fant ikke jobbene."} onRetry={reload} />
      ) : jobs.length === 0 ? (
        <EmptyState text="Ingen jobber ennå." />
      ) : (
        <div className="table-wrap">
          <table className="table">
            <thead>
              <tr>
                <th>Navn</th>
                <th>Krever</th>
                <th></th>
              </tr>
            </thead>
            <tbody>
              {jobs.map((job) => (
                <tr key={job.id}>
                  <td>{job.name}</td>
                  <td>{job.requiredSchools.map((s) => s.name).join(", ") || "Ingenting"}</td>
                  <td>
                    <div className="row">
                      <button className="btn btn-secondary" onClick={() => startEdit(job)}>
                        Rediger
                      </button>
                      <button className="btn btn-secondary" onClick={() => handleDelete(job)}>
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
