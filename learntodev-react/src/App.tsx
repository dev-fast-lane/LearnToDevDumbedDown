import Layout from "./components/Layout";
import { Routes, Route } from "react-router";
import StartPage from "./pages/StartPage";
import LoginPage from "./pages/LoginPage";
import SchoolsPage from "./pages/SchoolsPage";
import JobsPage from "./pages/JobsPage";
import AdminPage from "./pages/AdminPage";
import AdminSchoolsPage from "./pages/AdminSchoolsPage";
import AdminQuestionsPage from "./pages/AdminQuestionsPage";
import NotFoundPage from "./pages/NotFoundPage";
import RegisterPage from "./pages/RegisterPage";
import RequireAuth from "./auth/RequireAuth";
import RequireAdmin from "./auth/RequireAdmin";
import ExamPage from "./pages/ExamPage";

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
            <Route path="/admin" element={<AdminPage />} />
            <Route path="/admin/schools" element={<AdminSchoolsPage />} />
            <Route path="/admin/questions" element={<AdminQuestionsPage />} />
          </Route>
        </Route>

        <Route path="*" element={<NotFoundPage />} />
      </Route>
    </Routes>
  );
}
