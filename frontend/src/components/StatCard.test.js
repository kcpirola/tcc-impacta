import { render, screen } from "@testing-library/react";
import StatCard from "./StatCard";

describe("StatCard", () => {
  it("renders the title and value", () => {
    render(<StatCard title="E-mails Phishing" value={12} variant="phishing" />);

    expect(screen.getByText("E-mails Phishing")).toBeInTheDocument();
    expect(screen.getByText("12")).toBeInTheDocument();
  });
});
