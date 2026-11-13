import Layout from "./components/Layout";
import { Routes, Route } from "react-router";
import StartPage from "./pages/StartPage";
import LoginPage from "./pages/LoginPage";
import RegisterPage from "./pages/RegisterPage";
import SchoolsPage from "./pages/SchoolsPage";
import ExamPage from "./pages/ExamPage";
import JobsPage from "./pages/JobsPage";
import AdminSchoolsPage from "./pages/admin/AdminSchoolsPage";
import AdminQuestionsPage from "./pages/admin/AdminQuestionsPage";
import AdminJobsPage from "./pages/admin/AdminJobsPage";
import NotFoundPage from "./pages/NotFoundPage";
import RequireAuth from "./auth/RequireAuth";
import RequireAdmin from "./auth/RequireAdmin";

// Which page to show for each URL
export default function App() {
  return (
    <Routes>
      <Route element={<Layout />}>
        {/* Open to everyone */}
        <Route path="/login" element={<LoginPage />} />
        <Route path="/register" element={<RegisterPage />} />

        {/* Logged in users only */}
        <Route element={<RequireAuth />}>
          <Route path="/" element={<StartPage />} />
          <Route path="/schools" element={<SchoolsPage />} />
          <Route path="/schools/:schoolId/exam" element={<ExamPage />} />
          <Route path="/jobs" element={<JobsPage />} />

          {/* Admins only */}
          <Route element={<RequireAdmin />}>
            <Route path="/admin/schools" element={<AdminSchoolsPage />} />
            <Route path="/admin/questions" element={<AdminQuestionsPage />} />
            <Route path="/admin/jobs" element={<AdminJobsPage />} />
          </Route>
        </Route>

        <Route path="*" element={<NotFoundPage />} />
      </Route>
    </Routes>
  );
}
