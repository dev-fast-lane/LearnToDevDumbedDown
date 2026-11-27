import { Link } from "react-router";

export default function AdminPage() {
  return (
    <>
      <h1>Admin</h1>
      <Link className="btn" to="/admin/schools">
        Skoler
      </Link>{" "}
      <Link className="btn" to="/admin/questions">
        Spørsmål
      </Link>
    </>
  );
}
