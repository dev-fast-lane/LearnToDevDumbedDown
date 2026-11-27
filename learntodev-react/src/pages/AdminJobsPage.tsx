import { useEffect, useState, type SubmitEvent } from "react";
import { Loading, ErrorMessage, EmptyState } from "../components/StateViews";
import FormField from "../components/FormField";
import { useToast } from "../components/toast/useToast";
import { createJob, deleteJob, getJobs, updateJob } from "../services/jobService";
import { getSchools } from "../services/schoolService";
import { ApiError } from "../services/apiError";
import { fieldErrorsFrom, type FieldErrors } from "../utils/problemDetails";
import type { JobDto, SaveJobDto } from "../types/job";
import type { SchoolDto } from "../types/school";

// Tomt skjema, brukes når en ny jobb lages
const emptyForm: SaveJobDto = { name: "", description: "", requiredSchoolIds: [] };

// Admin: lage, vise, endre og slette jobber, og velge skolene hver jobb krever (R-01, R-02)
export default function AdminJobsPage() {
    // Jobbene i tabellen, og skolene som kan krysses av i skjemaet
    const [jobs, setJobs] = useState<JobDto[]>([]);
    const [schools, setSchools] = useState<SchoolDto[]>([]);
    const [loading, setLoading] = useState(true);
    const [error, setError] = useState<string | null>(null);
    const [attempt, setAttempt] = useState(0); // Endres for å hente på nytt, etter «Prøv igjen» og etter lagring
    const toast = useToast();

    // Skjemaet
    const [form, setForm] = useState<SaveJobDto>(emptyForm);
    const [editingId, setEditingId] = useState<number | null>(null); // null betyr at vi lager en ny jobb
    const [errors, setErrors] = useState<FieldErrors>({});
    const [saving, setSaving] = useState(false);

    useEffect(() => {
        let cancelled = false; // Ignorer svaret hvis siden lukkes før det kommer

        async function fetchData() {
            try {
                // Jobbene og skolene avhenger ikke av hverandre, så de hentes samtidig
                const [jobData, schoolData] = await Promise.all([getJobs(), getSchools()]);
                if (!cancelled) {
                    setJobs(jobData);
                    setSchools(schoolData);
                }
            } catch (err) {
                if (!cancelled) {
                    setError(err instanceof ApiError ? err.message : "Kunne ikke hente jobbene.");
                }
            } finally {
                if (!cancelled) setLoading(false);
            }
        }

        fetchData();
        return () => {
            cancelled = true;
        };
    }, [attempt]);

    function reload() {
        setLoading(true);
        setError(null);
        setAttempt((prev) => prev + 1);
    }

    // Legger skolen til i kravene hvis den ikke er der, og fjerner den hvis den er det
    function toggleSchool(schoolId: number) {
        setForm((prev) => ({
            ...prev,
            requiredSchoolIds: prev.requiredSchoolIds.includes(schoolId)
                ? prev.requiredSchoolIds.filter((id) => id !== schoolId)
                : [...prev.requiredSchoolIds, schoolId],
        }));
    }

    // Fyller skjemaet med jobben, så neste lagring blir en oppdatering
    function startEdit(job: JobDto) {
        setEditingId(job.id);
        setForm({
            name: job.name,
            description: job.description,
            requiredSchoolIds: job.requiredSchools.map((school) => school.id),
        });
        setErrors({});
    }

    // Tømmer skjemaet, så neste lagring lager en ny jobb
    function resetForm() {
        setEditingId(null);
        setForm(emptyForm);
        setErrors({});
    }

    async function handleDelete(job: JobDto) {
        // Spillere med jobben blir arbeidsledige, så admin må bekrefte først
        if (!window.confirm(`Vil du slette ${job.name}? Spillere med denne jobben blir arbeidsledige.`)) return;

        try {
            await deleteJob(job.id);
            // Fjerner jobben fra listen uten å hente alt på nytt
            setJobs((prev) => prev.filter((j) => j.id !== job.id));
            if (editingId === job.id) resetForm();
            toast(`${job.name} er slettet.`, "success");
        } catch (err) {
            toast(err instanceof ApiError ? err.message : "Kunne ikke slette jobben.", "error");
        }
    }

    async function handleSubmit(event: SubmitEvent<HTMLFormElement>) {
        event.preventDefault();

        // Samme regler og meldinger som SaveJobDto i API-et, så brukeren slipper å vente på serveren
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
            reload(); // Henter listen på nytt, så den viser endringen
        } catch (err) {
            if (err instanceof ApiError && Object.keys(err.fieldErrors).length > 0) {
                // Feil fra serveren vises ved feltet de gjelder, også en skole som ikke finnes
                setErrors(fieldErrorsFrom(err.fieldErrors));
            } else {
                toast(err instanceof ApiError ? err.message : "Kunne ikke lagre jobben.", "error");
            }
        } finally {
            setSaving(false);
        }
    }

    if (loading) return <Loading />;
    if (error) return <ErrorMessage message={error} onRetry={reload} />;

    return (
        <>
            <h1>Jobber</h1>

            {/* Samme skjema brukes til ny og endring. Overskriften viser hvilken */}
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

                {/* Skolene jobben krever. Ingen avkrysning betyr at jobben er åpen for alle */}
                <fieldset className="field">
                    <legend>Krever eksamen i</legend>
                    {schools.map((school) => (
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

                <button className="btn" type="submit" disabled={saving}>
                    {saving ? "Lagrer..." : "Lagre"}
                </button>
                {editingId !== null && (
                    <button className="btn btn-secondary" type="button" onClick={resetForm}>
                        Avbryt
                    </button>
                )}
            </form>

            {jobs.length === 0 ? (
                <EmptyState text="Ingen jobber ennå." />
            ) : (
                <div className="table-wrap">
                    <table className="table">
                        <thead>
                        <tr>
                            <th>Navn</th>
                            <th>Beskrivelse</th>
                            <th>Krever</th>
                            <th></th>
                        </tr>
                        </thead>
                        <tbody>
                        {jobs.map((job) => (
                            <tr key={job.id}>
                                <td>{job.name}</td>
                                <td>{job.description}</td>
                                <td>
                                    {job.requiredSchools.length > 0
                                        ? job.requiredSchools.map((school) => school.name).join(", ")
                                        : "Ingen"}
                                </td>
                                <td>
                                    <button className="btn btn-secondary" onClick={() => startEdit(job)}>
                                        Rediger
                                    </button>
                                    <button className="btn btn-secondary" onClick={() => handleDelete(job)}>
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