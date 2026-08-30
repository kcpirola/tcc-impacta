import { render, screen, fireEvent, waitFor } from "@testing-library/react";
import { MemoryRouter } from "react-router-dom";
import LoginPage from "./LoginPage";
import { useAuth } from "../context/AuthContext";

jest.mock("../context/AuthContext");

const mockNavigate = jest.fn();
jest.mock("react-router-dom", () => ({
  ...jest.requireActual("react-router-dom"),
  useNavigate: () => mockNavigate,
}));

function renderLoginPage() {
  return render(
    <MemoryRouter>
      <LoginPage />
    </MemoryRouter>
  );
}

describe("LoginPage", () => {
  afterEach(() => {
    jest.clearAllMocks();
  });

  it("navigates to the dashboard after a successful login", async () => {
    const login = jest.fn().mockResolvedValue({ username: "john" });
    useAuth.mockReturnValue({ login });
    renderLoginPage();

    fireEvent.change(screen.getByLabelText("Usuário"), { target: { value: "john" } });
    fireEvent.change(screen.getByLabelText("Senha"), { target: { value: "secret" } });
    fireEvent.click(screen.getByRole("button", { name: "Entrar" }));

    await waitFor(() => expect(login).toHaveBeenCalledWith("john", "secret"));
    await waitFor(() => expect(mockNavigate).toHaveBeenCalledWith("/dashboard"));
  });

  it("shows an error message when login fails", async () => {
    const login = jest.fn().mockRejectedValue({
      response: { data: { message: "Credenciais inválidas" } },
    });
    useAuth.mockReturnValue({ login });
    renderLoginPage();

    fireEvent.change(screen.getByLabelText("Usuário"), { target: { value: "john" } });
    fireEvent.change(screen.getByLabelText("Senha"), { target: { value: "wrong" } });
    fireEvent.click(screen.getByRole("button", { name: "Entrar" }));

    expect(await screen.findByText("Credenciais inválidas")).toBeInTheDocument();
    expect(mockNavigate).not.toHaveBeenCalled();
  });
});
