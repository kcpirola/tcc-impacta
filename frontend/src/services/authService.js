import api from "./api";

/**
 * Autentica o usuário e persiste o token/dados no localStorage.
 * @param {string} username
 * @param {string} password
 * @returns {Promise<object>} Dados do usuário autenticado
 */
export async function login(username, password) {
  const { data } = await api.post("/auth/login", { username, password });
  localStorage.setItem("token", data.token);
  localStorage.setItem("user", JSON.stringify(data.user));
  return data.user;
}

/** Remove as credenciais armazenadas localmente. */
export function logout() {
  localStorage.removeItem("token");
  localStorage.removeItem("user");
}

/** Retorna o usuário autenticado salvo no localStorage, ou null. */
export function getCurrentUser() {
  const stored = localStorage.getItem("user");
  return stored ? JSON.parse(stored) : null;
}

/** Verifica se existe um token JWT armazenado. */
export function isAuthenticated() {
  return Boolean(localStorage.getItem("token"));
}
