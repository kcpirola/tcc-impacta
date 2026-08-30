import { NavLink, useNavigate } from "react-router-dom";
import { useAuth } from "../context/AuthContext";
import "./Navbar.css";

export default function Navbar() {
  const { user, logout } = useAuth();
  const navigate = useNavigate();

  function handleLogout() {
    logout();
    navigate("/login");
  }

  return (
    <nav className="navbar">
      <div className="navbar-brand">🛡️ Phishing Detection</div>
      <div className="navbar-links">
        <NavLink to="/dashboard" className={({ isActive }) => (isActive ? "active" : "")}>
          Dashboard
        </NavLink>
        <NavLink to="/emails" className={({ isActive }) => (isActive ? "active" : "")}>
          E-mails
        </NavLink>
        <NavLink to="/classify" className={({ isActive }) => (isActive ? "active" : "")}>
          Classificar
        </NavLink>
      </div>
      <div className="navbar-user">
        {user && <span className="navbar-username">{user.fullName || user.username}</span>}
        <button type="button" className="btn-logout" onClick={handleLogout}>
          Sair
        </button>
      </div>
    </nav>
  );
}
