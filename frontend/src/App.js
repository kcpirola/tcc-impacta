import { Navigate, Route, Routes } from "react-router-dom";
import { AuthProvider, useAuth } from "./context/AuthContext";
import Navbar from "./components/Navbar";
import PrivateRoute from "./components/PrivateRoute";
import LoginPage from "./pages/LoginPage";
import DashboardPage from "./pages/DashboardPage";
import EmailsPage from "./pages/EmailsPage";
import ClassifyEmailPage from "./pages/ClassifyEmailPage";
import "./App.css";

function AppLayout({ children }) {
  const { isAuthenticated } = useAuth();
  return (
    <>
      {isAuthenticated && <Navbar />}
      <main>{children}</main>
    </>
  );
}

function AppRoutes() {
  return (
    <AppLayout>
      <Routes>
        <Route path="/login" element={<LoginPage />} />
        <Route
          path="/dashboard"
          element={
            <PrivateRoute>
              <DashboardPage />
            </PrivateRoute>
          }
        />
        <Route
          path="/emails"
          element={
            <PrivateRoute>
              <EmailsPage />
            </PrivateRoute>
          }
        />
        <Route
          path="/classify"
          element={
            <PrivateRoute>
              <ClassifyEmailPage />
            </PrivateRoute>
          }
        />
        <Route path="/" element={<Navigate to="/dashboard" replace />} />
        <Route path="*" element={<Navigate to="/dashboard" replace />} />
      </Routes>
    </AppLayout>
  );
}

export default function App() {
  return (
    <AuthProvider>
      <AppRoutes />
    </AuthProvider>
  );
}
