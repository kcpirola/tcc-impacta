import { Navigate } from "react-router-dom";
import { useAuth } from "../context/AuthContext";

/** Protege rotas que exigem autenticação, redirecionando para o login caso necessário. */
export default function PrivateRoute({ children }) {
  const { isAuthenticated } = useAuth();
  return isAuthenticated ? children : <Navigate to="/login" replace />;
}
