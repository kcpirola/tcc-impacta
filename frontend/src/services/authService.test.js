import api from "./api";
import * as authService from "./authService";

jest.mock("./api");

describe("authService", () => {
  afterEach(() => {
    localStorage.clear();
    jest.clearAllMocks();
  });

  describe("login", () => {
    it("stores the token and user, and returns the user", async () => {
      const fakeUser = { id: 1, username: "john", role: "User" };
      api.post.mockResolvedValue({ data: { token: "fake-jwt", user: fakeUser } });

      const result = await authService.login("john", "secret");

      expect(api.post).toHaveBeenCalledWith("/auth/login", { username: "john", password: "secret" });
      expect(localStorage.getItem("token")).toBe("fake-jwt");
      expect(JSON.parse(localStorage.getItem("user"))).toEqual(fakeUser);
      expect(result).toEqual(fakeUser);
    });

    it("propagates errors from the API", async () => {
      api.post.mockRejectedValue(new Error("Invalid credentials"));

      await expect(authService.login("john", "wrong")).rejects.toThrow("Invalid credentials");
      expect(localStorage.getItem("token")).toBeNull();
    });
  });

  describe("logout", () => {
    it("removes token and user from localStorage", () => {
      localStorage.setItem("token", "fake-jwt");
      localStorage.setItem("user", JSON.stringify({ id: 1 }));

      authService.logout();

      expect(localStorage.getItem("token")).toBeNull();
      expect(localStorage.getItem("user")).toBeNull();
    });
  });

  describe("getCurrentUser", () => {
    it("returns null when there is no stored user", () => {
      expect(authService.getCurrentUser()).toBeNull();
    });

    it("returns the parsed user when present", () => {
      const fakeUser = { id: 1, username: "john" };
      localStorage.setItem("user", JSON.stringify(fakeUser));

      expect(authService.getCurrentUser()).toEqual(fakeUser);
    });
  });

  describe("isAuthenticated", () => {
    it("returns false when there is no token", () => {
      expect(authService.isAuthenticated()).toBe(false);
    });

    it("returns true when a token is stored", () => {
      localStorage.setItem("token", "fake-jwt");
      expect(authService.isAuthenticated()).toBe(true);
    });
  });
});
